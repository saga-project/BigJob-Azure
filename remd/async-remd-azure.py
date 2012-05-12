""" The main script for REMD on Azure 
"""

import sys
import os
import random
import time
import optparse
import logging
import re
import math
import threading
import traceback
import pdb
import ConfigParser

# import bigjob implementation (azure based)
sys.path.append('../')
sys.path.append('/home/saisaripalli/Bigjob-Azure/winazurestorage')
from bigjob_azure import *

class ReManager():
    """ 
    This class holds information about the application and replicas remotely running via Azure
    """    
    def __init__(self, config_filename):
        
        self.stage_in_file_list = []
        self.exchange_count = 0
        self.arguments = []
        
        # lists for variables of each replica (Note that these variable should have n variables where n is self.replica_count
        self.replica_count = 0
        self.temperatures = []
        
        # instant variable for replica exchange
        self.replica_jobs = []   # saga jobs
        
        # file staging
        # contains ids of staged files
        # <glidein_url, [replica_id1, ...]
        self.glidein_file_dict = {}

        self.read_config(config_filename)
        # init random seed
        random.seed(time.time()/10.)
  
        
    def read_config(self, conf_file):
        # read config file
        config = ConfigParser.ConfigParser()
        #pdb.set_trace()
        print ("read configfile: " + conf_file)
        config.read(conf_file)
        # RE configuration
        default_dict = config.defaults()        
        self.arguments = default_dict["arguments"].split()       
        self.exchange_count = config.getint("DEFAULT", "exchange_count")
        self.total_number_replica = config.getint("DEFAULT", "total_number_replica")
        self.number_of_nodes = config.getint("DEFAULT", "number_of_nodes")
        """ Config parameters (will be moved to config file in the future) """
        self.adaptive_sampling  =  config.getboolean("DEFAULT", "adaptive_sampling") 
        self.adaptive_replica_size  = config.getboolean("DEFAULT", "adaptive_replica_size") 
        
        self.temperatures = default_dict["temperature"].split()
        self.stage_in_file_list = default_dict["stage_in_file"].split()
        self.executable = default_dict["executable"]
        self.working_directory = default_dict["working_directory"]
        
  
    #####################################
    #  Elementary Functions
    ########################################################
    def get_job_description(self, replica_id):        
        jd = description()  
        jd.executable = "approot\\resources\\namd\\namd2.exe"
        #jd.number_of_processes = self.number_of_processes
        jd.spmd_variation = "single"
        jd.arguments = self.arguments
        jd.working_directory = "$TEMP"
        jd.output = "stdout"
        jd.error = "stderr"
        
        # file staging
        transfer = {}
        transfer ["source"] = os.getcwd() + "/NPT.conf" # source for staging in (see JSDL spec) 
        transfer ["target"] = jd.working_directory + "NPT.conf"     # target for staging out (see JSDL spec)   
        jd.filetransfer = [transfer]         
        return jd

    
    def submit_job(self, dest_url_string, jd):
        error_string = ""
        js = saga.job.service(saga.url(dest_url_string))
        new_job = js.create_job(jd)
        new_job.run()
        return error_string, new_job
   

    def prepare_NAMD_config(self, replica_id):
        # The idea behind this is that we can simply modify NPT.conf before submit a job to set temp and other variables
        ifile = open("NPT.conf")   # should be changed if a different name is going to be used
        lines = ifile.readlines()
        for line in lines:
            if line.find("desired_temp") >= 0 and line.find("set") >= 0:
                items = line.split()
                temp = items[2]
                if eval(temp) != self.temperatures[replica_id]:
                    print "\n (DEBUG) temperature is changing to " + str(self.temperatures[replica_id]) + " from " + temp + " for rep" + str(replica_id)
                    lines[lines.index(line)] = "set desired_temp %s \n"%(str(self.temperatures[replica_id]))
        ifile.close() 
        ofile = open("NPT.conf","w")
        for line in lines:    
            ofile.write(line)
        ofile.close()

    def get_energy(self, replica_id):
        """ parse energy out of stdout """
        #pdb.set_trace()
        stdout = self.replica_jobs[replica_id].get_stdout()
        for line in stdout.split("\n"):
            items = line.split()
            if len(items) > 0:
                if items[0] in ("ENERGY:"):
                    en = items[11]  
        #print "(DEBUG) energy : " + str(en) + " from replica " + str(replica_id) 
        return eval(en)

    def do_exchange(self, energy, irep, jrep):
        iflag = False
        en_a = energy[irep]
        en_b = energy[jrep]
        
        factor = 0.0019872  # from R = 1.9872 cal/mol
        delta = (1./int(self.temperatures[irep])/factor - 1./int(self.temperatures[jrep])/factor)*(en_b-en_a)
        if delta < 0:
            iflag = True
        else :
            if math.exp(-delta) > random.random() :
                iflag = True
    
        if iflag is True:
            tmpNum = self.temperatures[jrep]
            self.temperatures[jrep] = self.temperatures[irep]
            self.temperatures[irep] = tmpNum
    
        print "(DEBUG) delta = %f"%delta + " en_a = %f"%en_a + " from rep " + str(irep) + " en_b = %f"%en_b +" from rep " + str(jrep)


    def submit_subjob(self,  jd):
        """ submit job via pilot job"""       
        sj = subjob(bigjob=self.bj)
        sj.submit_job(jd)
        return sj
      
    def start_bigjob(self, nodes):
        """start pilot jobs (advert_job.py) at every unique machine specified in RE_info"""  
        start = time.time()
        self.bj = bigjob_azure()
        self.bj.start_pilot_job(number_nodes=nodes)
        logging.debug("BigJob Azure Initiation Time: " + str(time.time()-start))
        return self.bj
    
  
    def stop_bigjob(self):
        """ stop pilot job """
        #pdb.set_trace()
        self.bj.cancel()
  
    
    def gcd(a, b):

        '''Returns the Greatest Common Divisor,
           implementing Euclid's algorithm.
           Input arguments must be integers;
           return value is an integer.'''
        while a:
            a, b = b%a, a
        return b


    #########################################################
    #  run_REMDg
    #########################################################
    def run_REMDg(self):
        
        """ Main loop running replica-exchange """
        #pdb.set_trace()
        REMD_start = time.time()
        numEX = self.exchange_count    
        ofilename = "async-remd-temp-4R-4M-16c.out"
        print "Start Bigjob"
        self.bj = self.start_bigjob(self.number_of_nodes)
        if self.bj==None or self.bj.get_state_detail()=="Failed":
            return       
        iEX = 0
       
        print "\n"
        # reset replica number
                     
        print "############# spawn jobs ################"
        self.replica_jobs = []            
        start_time = time.time()
        replica_id = 0            
        state = self.bj.get_state_detail()  
        pilot_url = self.bj.pilot_url 
        print "Pilot: " + pilot_url + " state: " + state
 
        if state.lower()== "running":
           logging.debug("pilot job running - start " + str(self.total_number_replica) + " jobs.")
           for i in range (0, self.total_number_replica):
                    self.prepare_NAMD_config(replica_id)

                    ############## Tfile: Time to Transfer Files and submit job #########################
                    Tfile=time.time()
                    jd = self.get_job_description(replica_id)
                    new_job = self.submit_subjob(jd)
                    print "(INFO) Time to stage files : " + str(time.time()-Tfile)

                    #pdb.set_trace()
                    self.replica_jobs.insert(replica_id, new_job)
                    replica_id = replica_id + 1
                    print "(INFO) Replica " + "%d"%replica_id + " started (Num of Exchange Done = %d)"%(iEX)

        end_time = time.time()        
        # contains number of started replicas
        numReplica = len(self.replica_jobs)
    
        print "started " + "%d"%numReplica + " of " + str(self.total_number_replica) + " in this round." 
        print "Time for spawning " + "%d"%numReplica + " replica: " + str(end_time-start_time) + " s"

        print  ####################################### async-job monitoring step ###############################
        energy = [0 for i in range(0, numReplica)]
        #flagJobDone = [ False for i in range(0, numReplica)]
        #flagExchangeDone = [ False for i in range(0, numReplica)]
        #flagJobCount = [ False for i in range(0, numReplica)]
        #numJobDone = 0
        print "\n" 
        total_number_of_namd_jobs = 0
        times={}
        Rtimes={}
        TState=[]
        TFind=[]
        Tmd=[]
        while (iEX < numEX):
            print "\n (EXCHANGE COUNT) INFO : " + str(iEX)
            print "\n##################### Replica State Check at: " + time.asctime(time.localtime(time.time())) + " ########################"
            for irep in range(0, numReplica):
                running_job = self.replica_jobs[irep]
                try: 
                   state = running_job.get_state()
                except:
                   pass
                print "replica_id: " + str(irep) + " job: " + str(running_job) + " received state: " + str(state)\
                                     + " Time since launch: " + str(time.time()-REMD_start) + " sec"
                if(str(state) == "Running"):
                   if Rtimes.has_key(irep):
                      pass
                   else:
                      Rtimes[irep]=time.time()

                if ((str(state) == "Done") and (iEX < numEX)):   

                   ################ Tfind: Time to find a partner after reaching Done state ##########
                   if times.has_key(irep):
                      pass 
                   else:
                      times[irep]=time.time()
                      if Rtimes.has_key(irep): 
                        print "\n $$$$$$$$$$ (Run Time INFO) of " + str(irep) + " is : " + str(times[irep]-Rtimes[irep])
                        del Rtimes[irep]
                      else:
                        print "\n $$$$$$$$$$ (Run Time INFO) of" +str(irep) + " is : " + str(times[irep]-0)
                        
                   energy[irep] = self.get_energy(irep) ##todo get energy from right host
                   total_number_of_namd_jobs = total_number_of_namd_jobs + 1
                   ####################################### Replica Exchange ##################################    
                   j=irep
                   frep=0
                   list=[]
                   for frep in range(0,numReplica):
                       running_job_frep = self.replica_jobs[frep] 
                       try:
                          state = running_job_frep.get_state()
                       except:
                          pass
                       if(str(state) == "Running"):
                           if Rtimes.has_key(frep):
                              pass
                           else:
                              Rtimes[frep]=time.time()

                       if(str(state) == "Done" and (frep!=j)):
                          estfrep=time.time()
                          if times.has_key(frep):
                             times[frep]=estfrep-times[frep]
                             if Rtimes.has_key(frep):
                                print "\n $$$$$$$$$$ (Run Time INFO) of " + str(frep) + " is : " + str(estfrep-Rtimes[frep])
                                Tmd.append(estfrep-Rtimes[frep])
                                del Rtimes[frep]
                             else:
                                print "\n $$$$$$$$$$ (Run Time INFO) of " + str(frep) + " is : " + str(estfrep-0)
                                Tmd.append(estfrep-0)
                          else:
                             times[frep]=0
                             if Rtimes.has_key(frep):
                                print "\n $$$$$$$$$$ (Run Time INFO) of " + str(frep) + " is : " + str(estfrep-Rtimes[frep])
                                Tmd.append(estfrep-Rtimes[frep])
                                del Rtimes[frep]
                             else:
                                print "\n $$$$$$$$$$ (Run Time INFO) of " + str(frep) + " is : " + str(estfrep-0)
                                Tmd.append(estfrep-0)

                          times[j]=estfrep-times[j]

                          print "\n (INFO) Tfind Time for replica_id: "+ str(j) +" is " + str(times[j])
                          print "\n (INFO) Tfind Time for replica_id: "+ str(frep) +" is " + str(times[frep])
                          TFind.append(times[j])
                          TFind.append(times[frep])
                          del times[j]
                          del times[frep]

                          energy[frep] = self.get_energy(frep) ##todo get energy from right host
                          total_number_of_namd_jobs = total_number_of_namd_jobs + 1
                          en_a = energy[frep]
                          en_b = energy[irep]
                          self.do_exchange(energy,frep, irep)
                          #print "\n(INFO) replica_id:" + str(irep) + " exchanged temperature with " + "replica_id: " + str(frep) + "\n" 
                          iEX=iEX + 1        
                          #print "\n (Replica Exchange INFO) iEX=" + str(iEX)
                          if(iEX< numEX):

                               ############# Tstate: Time to Update states in Azure Blob and Queue #############
                               Tstate=time.time()
                               self.prepare_NAMD_config(frep)
                               jd = self.get_job_description(frep)
                               new_job = self.submit_subjob(jd)
                               self.replica_jobs[frep]= new_job
                               print "\n (INFO) Replica " + "%d"%frep + " started "
                               self.prepare_NAMD_config(irep)
                               jd = self.get_job_description(irep)
                               new_job = self.submit_subjob(jd)
                               self.replica_jobs[irep]= new_job
                               print "\n (INFO) Replica " + "%d"%irep + " started"  
                               print "(Tstate INFO) " + str(time.time()-Tstate)
                               TState.append(time.time()-Tstate)
                          else:
                               pass
                          break
                       elif(frep==j):
                          pass
                       elif(iEX>=numEX):
                          break
                       else:
                          pass

                elif(str(state)=="Failed"):
                  self.stop_glidin_jobs()
                  sys.exit(1)
                elif(iEX>=numEX):
                  break
                else:
                  pass
            
        print "REMD Runtime: " + str(time.time()-REMD_start) + " sec; Pilot URL: " + str(self.bj.pilot_url) \
                + "; number replica: " + str(self.total_number_replica) \
                + "; number namd jobs: " + str(total_number_of_namd_jobs)
        # stop gliding job     
        print "\n (Tmd) : " + str(Tmd)
        print "\n (TFind) : " + str(TFind)
        print "\n (TState) : " + str(TState)   
        self.stop_bigjob()

    
#########################################################
#  main
#########################################################
if __name__ == "__main__" :
    #pdb.set_trace()
    start = time.time()
    op = optparse.OptionParser()
    op.add_option('--type','-t', default='REMD')
    op.add_option('--configfile','-c')
    op.add_option('--numreplica','-n', default='2')
    options, arguments = op.parse_args()
    
    if options != None and options.configfile!=None and options.type != None and options.type in ("REMD"):
        re_manager = ReManager(options.configfile)
        try:
            re_manager.run_REMDg() 
        except:
            traceback.print_exc(file=sys.stdout)
            print "Stop Glide-Ins"
            re_manager.stop_bigjob()
    else:
        print "Usage : \n python " + sys.argv[0] + " --type=<REMD> --configfile=<configfile> \n"
        print "Example: \n python " + sys.argv[0] + " --type=REMD --configfile=remd.conf"
        sys.exit(1)      
        
    #print "REMDgManager Total Runtime: " + str(time.time()-start) + " s"

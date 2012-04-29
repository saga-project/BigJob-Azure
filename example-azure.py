""" Example application demonstrating job submission via bigjob 
    Azure implementation of BigJob is used
"""

import os
from bigjob_azure import *
import time
import pdb

NUMBER_JOBS=4


""" Test Job Submission via Azure BigJob """
if __name__ == "__main__":

    ##########################################################################################
    # Start BigJob
    # Parameter for BigJob 
    nodes = 2 # number nodes for agent
    # current_directory=os.getcwd() +"/agent"  # working directory for agent
    pdb.set_trace();  
    # start pilot job (bigjob_agent)
    print "Start Pilot Job/BigJob in the cloud. "
    start = time.time()
    bj = bigjob_azure()
    bj.start_pilot_job(number_nodes=nodes)
    print "Pilot Job/BigJob URL: " + bj.pilot_url + " State: " + str(bj.get_state())\
        + " Launch Time: " + str(time.time()-start) + " sec"    
    if(bj.get_state()=="Failed"):
        print "Start of BigJob failed."
        sys.exit(-1)

    ##########################################################################################
    # Submit SubJob through BigJob
    jd = description() #TODO: move to SAGA JD
    jd.executable = "approot\\resources\\namd\\Replica_Agent"
    jd.number_of_processes = "1"
    jd.spmd_variation = "single"
    #jd.arguments = ["+p1", "NPT.conf"]
    jd.arguments = [""]
    jd.working_directory = "approot\\resources\\namd\\"
    jd.output = "stdout"
    jd.error = "stderr"
    
  
    
    jobs = []
    for i in range (0, NUMBER_JOBS):
        print "Start job no.: " + str(i)
        sj = subjob(bigjob=bj)
        sj.submit_job(jd)        
        jobs.append(sj)
    
    # busy wait for completion
    while 1:
        try:
            number_done = 0
            for i in jobs:
                state = str(i.get_state())
                print "job: " + str(i) + " state: " + str(state) + " Time passed: " + str(time.time()-start) + " sec"    
                if(state=="Failed" or state=="Done"):
                    number_done = number_done + 1                    
                
            if (number_done == len(jobs)):
                break
            time.sleep(20)
        except KeyboardInterrupt:
            break

    ##########################################################################################
    # Cleanup - stop BigJob
    pdb.set_trace()
    bj.cancel()

#!/usr/bin/env python

"""Module bigjob_azure.

This Module is used to launch a set of jobs via a defined set of Azure worker roles. 

Expects configuration file: bigjob_azure.conf in directory of python executable
Use template: bigjob_azure.conf.template for reference
cp bigjob_azure.conf.template bigjob_azure.conf

"""

import sys
import os
import pdb
print os.getcwd()
sys.path.append(os.getcwd() + '/winazurestorage/')
sys.path.append('/home/saisaripalli/BigJob-Azure/winazurestorage')
import getopt
import time
import uuid
import socket
import traceback
import json
import ConfigParser

# multiprocessing
import threading
#from multiprocessing import Process, Pool, Lock
#from multiprocessing.sharedctypes import Value, Array


# for logging
import logging
logging.basicConfig(level=logging.DEBUG)

# azure storage and service lib
from winazurestorage import *
from winazureservice import *

import xml.etree.ElementTree
from  xml.etree.ElementTree import *

""" Config parameters (will move to config file in future) """
""" Used as blob storage keys """
APPLICATION_NAME="bigjob"
STATE = "state"
JOB_DESCRIPTION = "jd"
NODE_FILE = "nodefile"
CONFIG_FILE="bigjob_azure.conf"
COUNT = "count"

class bigjob_azure():
    
    def __init__(self, database_host=None):
        
        # read config file
        conf_file =  os.path.join(os.path.dirname( __file__ ),CONFIG_FILE)
        logging.debug("read config file: " + conf_file)
        config = ConfigParser.ConfigParser()
        config.read(conf_file)
        default_dict = config.defaults()
        self.account_name_storage = default_dict["account_name_storage"]
        logging.debug(self.account_name_storage)
        self.account_names_compute = default_dict["account_name_compute"]
        logging.debug(self.account_names_compute)
        self.slot = default_dict["slot"]
        self.secret_key = default_dict["secret_key"]
        self.user_certificate =os.path.join(os.path.dirname(__file__),default_dict["user_certificate"])
        logging.debug(self.user_certificate)
        self.subscription_id = default_dict["subscription_id"]
        logging.debug(self.subscription_id)
        self.pilot_url = "http://localhost"
        
        self.service_package = default_dict["service_package"]
        logging.debug(self.service_package)
        self.service_configuration =os.path.join(os.path.dirname(__file__),default_dict["service_configuration"])
        logging.debug(self.service_configuration)        
        logging.debug("init azure storage: blob and queue") 
        self.uuid = str(uuid.uuid1())        
        self.app_id = APPLICATION_NAME
        logging.debug("Self.app_id is Application Name: " + self.app_id)
        self.blob = BlobStorage(host = "blob.core.windows.net", 
                             account_name = self.account_name_storage, 
                             secret_key = self.secret_key)
        """self.blob = BlobStorage(host = "127.0.0.1:10000", 
                             account_name = self.account_name_storage, 
                             secret_key = self.secret_key) """
        result = self.blob.create_container(self.app_id)
        logging.debug("Result of pilot job blob container creation: " + str(result))
        
        self.queue = QueueStorage(host = "queue.core.windows.net", 
                             account_name = self.account_name_storage, 
                             secret_key = self.secret_key)
        
        result = self.queue.create_queue(self.app_id)
        logging.debug("Result of pilot job queue creation: " + str(result))
        
        self.app_url=self.blob.get_base_url()+"/"+self.app_id
        logging.debug("created azure blob: " + self.app_url)
        
        self.stopped = False
    
    def start_azure_worker_roles(self, number=4):
        self.stopped = False
        service_names = self.account_names_compute.split()
        results = []
        for i in service_names:
            r = self.start_single_azure_worker_role(i, number, self.slot, self.service_package, self.service_configuration)
            results.append(r)
        return all(results)
        #return True
        
    
        #threads = []
        #for i in service_names:
        #    thread=threading.Thread(target=self.start_single_azure_worker_role, 
        #                            args=(i, number, self.slot, self.service_package, self.service_configuration))
        #    thread.start()      
        #    threads.append(thread)
        #    time.sleep(20)
             
        # join threads
        #for t in threads:
        #    t.join()     
              
           
    def start_single_azure_worker_role(self, service_name, number, slot, service_package, service_configuration):
        logging.debug("Initiate service: " + service_name + " number instances: " + str(number))
        hostedService = HostedService(self.subscription_id, self.user_certificate); 
        requestId = hostedService.createDeployment(service_name, APPLICATION_NAME, slot, 
                       service_package, 
                       service_configuration, 
                       number)
        # wait for deployment to be done
        status = hostedService.waitForRequest(requestId);
    
        if status != "Succeeded":
            logging.debug("Deployment Failed")
            return False
            
        # change status to running    
        logging.debug("Setting deployment status to Running")
        requestId = hostedService.updateDeploymentStatus(service_name, self.slot, "Running")
        status = hostedService.waitForRequest(requestId);
        if status != "Succeeded":
            logging.debug("Update Deployment Failed")
            return False
        self.stopped = False
        return True    
        
        
        #for service_name in :
        #    self.start_single_azure_worker_role(service_name, number)
                    
        
    def stop_azure_worker_roles(self):        
        if self.stopped == False:
            for service_name in self.account_names_compute.split():
                logging.debug("Deleting deployment for service: " + service_name)
                hostedService = HostedService(self.subscription_id, self.user_certificate); 
                requestId = hostedService.updateDeploymentStatus(service_name, self.slot, "Suspended")
                status = hostedService.waitForRequest(requestId)
    
                requestId = hostedService.deleteDeployment(service_name, self.slot)    
                status = hostedService.waitForRequest(requestId)     
            
            self.stopped = True
        else:
            logging.debug("BigJob already stopped")
        
    
    def start_pilot_job(self, 
                 lrms_url=None,                     # in future version one can specify a URL for a cloud (ec2:// vs. nimbus:// vs. eu://)
                 bigjob_agent_executable=None,      # n/a
                 number_nodes=4,                    # number of images requested    
                 queue=None,                        # n/a
                 project=None,                      # n/a
                 working_directory=None,            # working directory
                 userproxy=None,                    # optional: path to user credential (X509 cert or proxy cert)
                 walltime=None,                     # optional: walltime
                 cloud_type=None,
                 image_name=None):   
     
        self.pilot_url = self.app_url
        #update state blob
        #self.blob.put_blob(self.app_id, STATE, str(state.Unknown), "text/plain")
        self.set_state(str(state.Unknown))
        logging.debug("set pilot state to: " + str(state.Unknown))

        ########## Set Count ###############
        self.set_count(str(0)) 
        logging.debug("set initial count to: " + str(0))

        # use service management api to spawn azure images
        logging.debug("init azure worker roles") 
        if self.start_azure_worker_roles(number_nodes):
             self.set_state(str(state.Running))
        else:
             self.set_state(state.Failed) 
        self.set_state(str(state.Running))
        
    def set_count(self,cnt):
        self.blob.put_blob(self.app_id, COUNT, cnt, "text/plain")

    def set_state(self, new_state):
        self.blob.put_blob(self.app_id, STATE, new_state, "text/plain")

    def get_count(self):
        return self.blob.get_blob(self.app_id,COUNT) 
    
    def get_state(self):        
        return self.blob.get_blob(self.app_id, STATE)
    
    def get_state_detail(self): 
        return self.blob.get_blob(self.app_id, STATE)
    
    def cancel(self):        
        logging.debug("Cancel Pilot Job")     
        #self.queue.put_message(self.app_id, "STOP")
        self.stop_azure_worker_roles()
        self.blob.delete_container(self.app_id)
        self.queue.delete_queue(self.app_id);
        
        
    def add_subjob(self, jd):
        logging.debug("add subjob to queue")
        job_id = "subjob-" + str(uuid.uuid1())
        # handle file staging
        if (len(jd.filetransfer)>0):
            self.stage_in_files(job_id, jd.filetransfer)
        
        json_jd = json.dumps(self.create_jd_json(jd))
        logging.debug(json_jd)
        # create subjob blob
        result1 = self.blob.put_blob(self.app_id, job_id, json_jd, "text/plain")
        # create queue message for subjob
        result2 = self.queue.put_message(self.app_id, job_id)
        logging.debug ("Results: subjob blob creation: " +str(result1) 
                       + " subjob queue message: " + str(result2))
        return job_id 

    
    def stage_in_files(self, job_id, file_list):
        """ Upload file into Azure Blob storage so that they can later be retrieved by the BigJob Agent """
        for ifile in file_list:
            ifile_basename = os.path.basename(ifile["source"])           
            if not os.path.isfile(ifile["source"]):
                error_msg = "Input file %s does not exist in %s"%(ifile_basename, os.path.dirname(ifile["source"]))
                logging.error(error_msg)
            else:
                fd = open (ifile["source"], "r")
                self.blob.put_blob(self.app_id, job_id + "/" + ifile_basename, fd.read(), "text/plain")
                fd.close()     
       
     
    def get_subjob_state(self, job_id):
        json_jd = self.blob.get_blob(self.app_id, job_id)  
        jd_dict = json.loads(json_jd)
        return jd_dict["state"]

    def get_blob_as_string(self, blob_name):
        return self.blob.get_blob(self.app_id, blob_name)
 
    def delete_subjob(self, job_id):
        # winazurestorage currently does not support the deletion of blobs
        pass
    
    def create_jd_json(self, jd):
        jd_dict = {}
        for i in dir(jd):          
              if not str(i).startswith("__"):
                  logging.debug("Add attribute: " + str(i) + " Value: " + str(getattr(jd, i)))              
                  jd_dict[i]=getattr(jd, i)
        
        #attributes = jd.list_attributes()                
        #for i in attributes:          
              #if jd.attribute_is_vector(i):
              #     jd_dict[i]=jd.get_vector_attribute(i)
              # else:
              #     logging.debug("Add attribute: " + str(i) + " Value: " + jd.get_attribute(i))
              #     jd_dict[i] = jd.get_attribute(i)
        # state should be stored as metadata to avoid that the entire blob must
        # be read (not supported by winazurestorage yet)
        jd_dict["state"] = str(state.Unknown)       
        return jd_dict

    def __repr__(self):
        return self.pilot_url 

    def __del__(self):
        self.cancel()

                   
                    
class subjob():
    
    def __init__(self, 
                 database_host=None,
                 bigjob=None):
        """Constructor"""
        self.bigjob=bigjob
        self.job_url=None
        self.job_id=None
        self.job_jd = None
 
    def submit_job(self, jd):
        """ submit job via Azure queue and Azure agent
            dest_url - url reference to advert job or host on which the advert job is going to run"""
        print "submit job: " + str(self.bigjob)
        #queue subjob add bigjob
        self.job_jd=jd
        self.job_id=self.bigjob.add_subjob(jd)
        self.job_url=self.bigjob.pilot_url + "/"+ str(self.job_id)
        
    def get_state(self):        
        """ duck typing for get_state of saga.cpr.job and saga.job.job  """
        return self.bigjob.get_subjob_state(self.job_id)
    
    def cancel(self):
        print "delete job: " + self.job_url
        try:
            self.bigjob.delete_subjob(self.job_id)
        except:
            pass
        
    def get_stdout(self):
        """ return stdout of subjob as string """
        return self.bigjob.get_blob_as_string(self.job_jd.output + "-" +self.job_id)

    def __del__(self):
        self.bigjob.delete_subjob(self.job_id)
    
    def __repr__(self):        
        if(self.job_url==None):
            return "None"
        else:
            return self.job_url

class description():
    def __init__(self):
        self.executable = ""
        self.number_of_processes = ""
        self.spmd_variation = ""
        self.arguments = []
        self.working_directory = ""
        self.output = ""
        self.error = ""
        self.filetransfer = []

                    
class state():
   Unknown = "Unknown"
   Failed = "Failed"
   New = "New"
   Running = "Running"
   Done = "Done"
   Suspended = "Suspended"
   Canceled = "Canceled"
        

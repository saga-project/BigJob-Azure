import httplib
import base64
from xml.dom import minidom
import time
import logging

from xml.etree.ElementTree import ElementTree, QName, tostring, dump

#logging.basicConfig(level=logging.Info)

# Azure Configuration
SERVICE_MANAGEMENT_HOST = 'management.core.windows.net'

# User Configuration
USER_CERTIFICATE = 'azure-certificate.pem'
SUBSCRIPTION_ID='c86aebd3-affe-4feb-aaa1-e69e7ee65d10'
            

class HostedService():
    
        def __init__(self, subscription_id=SUBSCRIPTION_ID, user_certificate=USER_CERTIFICATE):
            logging.debug("init hosted service")
            self.subscription_id = subscription_id
            self.user_certificate = user_certificate
            
            self.h = httplib.HTTPSConnection(SERVICE_MANAGEMENT_HOST, 443, cert_file=user_certificate, timeout=120)
            #if(logging.getLogger('').isEnabledFor(logging.DEBUG)):
            #   self.h.set_debuglevel(7)
                
        def list(self):        
            """ list all hosted services """    
            self.h.request('GET', '/' + self.subscription_id +'/services/hostedservices', headers={"x-ms-version":"2009-10-01"})
            response = self.h.getresponse()
            logging.debug("HTTP Response: " + str(response.status) + " " + str(response.reason) + "\n"+response.read())
            
        def createDeployment(self, serviceName, deploymentName, slotName, serviceLocation, serviceConfiguration, numberNodes):
           
            configuration = self.prepareConfiguration(serviceConfiguration, numberNodes)
            #create payload string
            data = """<?xml version="1.0" encoding="utf-8"?>
            <CreateDeployment xmlns="http://schemas.microsoft.com/windowsazure">
                <Name>%s</Name>
                <PackageUrl>%s</PackageUrl>
                <Label>%s</Label>
                <Configuration>%s</Configuration>
            </CreateDeployment>""" % (deploymentName, serviceLocation, base64.encodestring(deploymentName), base64.encodestring(configuration))
            
            logging.debug(data)
            url = "/" + self.subscription_id +"/services/hostedservices/" + serviceName + "/deploymentslots/"+ slotName
            logging.debug("URL: " + url)
            self.h.request('POST', url, body=data, 
                           headers={"x-ms-version":"2009-10-01", "Content-Type": "application/xml"})
            response = self.h.getresponse()
            requestId = response.getheader("x-ms-request-id")
            responseData = response.read()
            logging.debug("HTTP Response: " + str(response.status) + " " + str(response.reason) 
                          + "\nRequest-Id: " + requestId)
            return requestId        
        
        def prepareConfiguration(self, serviceConfiguration, numberNodes):
            """ read configuration and adjust number of nodes """
            tree = ElementTree(file=serviceConfiguration)
            instanceElement = tree.find("*/{http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration}Instances") 
            instanceElement.set("count", str(numberNodes))            
            configuration = tostring(tree.getroot())
            logging.debug("Configuration: " + configuration)
            return configuration
            
        
        def deleteDeployment(self, serviceName, slotName):
            """ delete Azure deployment """
            url = "/" + self.subscription_id +"/services/hostedservices/" + serviceName + "/deploymentslots/"+ slotName
            self.h.request('DELETE', url, headers={"x-ms-version":"2009-10-01"})
            response = self.h.getresponse()
            requestId = response.getheader("x-ms-request-id")  
            responseData = response.read()          
            logging.debug("HTTP Response: " + str(response.status) + " " + str(response.reason) 
                          + "\nRequest-Id: " + requestId)     
            return requestId      
            
        def getOperationStatus(self, requestId):
            url = "/" + self.subscription_id +"/operations/" + requestId
            self.h.request('GET', url, headers={"x-ms-version":"2009-10-01"})
            response = self.h.getresponse()
            responseData = response.read()
            logging.debug("HTTP Response: " + str(response.status) + " " + str(response.reason) 
                          + "\n" + responseData)  
            dom = minidom.parseString(responseData)
            statusElement = dom.getElementsByTagName("Status")[0]
            status = statusElement.firstChild.toxml()
            return status
        
        def waitForRequest(self, requestId):
            while True:
                status = self.getOperationStatus(requestId)
                logging.debug("Status: " + str(status))
                if(status == "Succeeded" or status == "Failed"):
                    return status
                time.sleep(10)
        
        def updateDeploymentStatus(self, serviceName, slotName, status):
            """updates delployment status: new status should be either Running or Suspended"""
            if (status != "Running" and status != "Suspended"):
                logging.error("Invalid status: " + status + " (should be either Running or Suspended)")
                return
            #build URL and payload
            url = "/" + self.subscription_id +"/services/hostedservices/" + serviceName + "/deploymentslots/"+ slotName  + "/?comp=status"
            data ="""<?xml version="1.0" encoding="utf-8"?>
                    <UpdateDeploymentStatus xmlns="http://schemas.microsoft.com/windowsazure">
                          <Status>%s</Status>
                    </UpdateDeploymentStatus>""" % (status)
                    
            self.h.request('POST', url, body=data, 
                           headers={"x-ms-version":"2009-10-01", "Content-Type": "application/xml"})
            response = self.h.getresponse()
            responseData = response.read()
            requestId = response.getheader("x-ms-request-id")
            logging.debug("HTTP Response: " + str(response.status) + " " + str(response.reason) 
                          + "\nRequest-Id: " + str(requestId))
            return requestId   
        
        def __del__(self):
            self.h.close()
        
        
def main():
    h = HostedService(subscription_id="2ffebf74-01b0-4ee8-a4e7-3a9178002908") 
    h.list()
    
    requestId = h.createDeployment( "drelu", "bigjob", "staging", 
                       "http://drelu.blob.core.windows.net/namd-service/BigJobService.cspkg",
                       "BigJobService/deploy/ServiceConfiguration.cscfg", 1)
    # wait for deployment to be done
    status = h.waitForRequest(requestId);
    
    if status != "Succeeded":
        logging.debug("Deployment Failed")
        return
    
    # change status to running    
    logging.debug("Setting deployment status to Running")
    requestId = h.updateDeploymentStatus("drelu", "staging", "Running")
    status = h.waitForRequest(requestId);
        
    # do what ever you need to do with your Azure instances
    time.sleep(300)
    
    # delete deployment
    logging.debug("Deleting deployment")
    requestId = h.updateDeploymentStatus("drelu", "staging", "Suspended")
    status = h.waitForRequest(requestId)
    
    requestId = h.deleteDeployment("drelu", "staging")    
    status = h.waitForRequest(requestId)
    
       
   
if __name__ == '__main__':
    main()
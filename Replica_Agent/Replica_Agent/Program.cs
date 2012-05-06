using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using System.Globalization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Replica_Agent
{
    class Program
    {
        private System.Object lockThis = new System.Object();
        CloudStorageAccount storageAccount;
        String applicationName = "bigjob";
        CloudBlobContainer blobContainer = null;
        CloudBlobClient blobClient = null;
        CloudQueueClient queueClient1 = null;
        string STATE = "state";
        string namdoutput = "namdoutput";
        string namderror = "namderror";
        string jobId = null;
        int temp_a;
        int temp_b;
        double energy;
        string COUNT = "0";
        CloudQueue q;
        Dictionary<string, object> jobDict_current;
        Dictionary<string, object> jobDict_Foreign;

        static void Main(string[] args)
        {
            Program ob = new Program();
            Trace.WriteLine("############## Before Job ID Arguments are ################# :" + ob.jobId);

            /* Get the JobId, replica_id, temperature, total_number_replica into Items List */
            try
            {
                
                ob.jobId = args[0];
                ob.jobDict_current = ob.getJobDictFromBlob(ob.jobId);
                int replica_id = Convert.ToInt32(args[1]);
                ob.temp_a= Convert.ToInt32(args[2]);
                
                //Trace.WriteLine("############## After Job ID Arguments are ################# :" + ob.jobId);
                Trace.WriteLine("replica_id is:" + args[1]);
                Trace.WriteLine("temperature temp_a is :" + ob.temp_a);
                Trace.WriteLine("total_number_replica is :" + args[3]);
            }
            catch(Exception ex)
            {
                Trace.WriteLine(" CLoud Exception found" + ex);
                Trace.WriteLine(" CLoud Exception found ","Information");
            }
           /* Run the Namd For first time */
            if (ob.runNamd(ob))
            {
                   //Trace.WriteLine(" *********** Successfully Namd Ran For First Time ************** ","Information");
                /* Perform Replica Exchange */
                   Trace.WriteLine(" *********** Calling Replica Exchange Method **************** ", "Information");
                   ob.callReplicaExchange(ob);
            }
            else
                Trace.WriteLine("\n Oops!!!!! Namd Failed  ");

        }

        
        public bool runNamd(Program ob)
        {
            if (ob.executesubjob(jobId))
            {
                /* if done Update the status in the queue after writing the blob file */
                UpdateState_Current(jobId, "Done");
                ob.q = ob.callQueue();
                CloudQueueMessage message= new CloudQueueMessage(jobId);
                ob.q.AddMessage(message);
                Trace.WriteLine(" ########## Namd Ran for First Time ########### added message to queue.........Now UpdatedState : " + jobId);
                
                return true;
            }
            else
                return false;
        }

        public void callReplicaExchange(Program ob)
        {
            //Trace.WriteLine(" *********** I'm in Replica Exchange Method **************** ", "Information");
            CloudQueue queue1 = ob.callQueue();
            //Trace.WriteLine(" queuq1 value is: " + queue1.ToString());
           
            Trace.WriteLine("############## Job ID Arguments in Replica-Exchange ################# :" + jobId);
            jobDict_current = getJobDictFromBlob(jobId);
                               
            if (queue1.PeekMessage() == null)
            {
                Trace.WriteLine(" ******** Peek Message Expired ********: ", "Information");
            }
            int i = 0;
            while (true)
            {
                //while (queue1.PeekMessage() != null)
                foreach ( CloudQueueMessage queueMessage1 in queue1.GetMessages(4, TimeSpan.FromSeconds(10)))                                                                 
                {
                    Trace.WriteLine(" ");

                    Trace.WriteLine("Counter Value of While is : " + i);
                  //  queueMessage1 = queue1.GetMessage(new TimeSpan(1, 0, 0));
                    int freshMessageCount = queue1.RetrieveApproximateMessageCount();
                    Trace.WriteLine(" FreshMessageCount is : " + freshMessageCount);
                    int? cachedMessageCount = queue1.ApproximateMessageCount;

                    //queueMessage1 = queue1.GetMessages(2,TimeSpan.FromMinutes(15));
                    //Trace.WriteLine("Peek Message : " + queueMessage1.AsString);
                    Trace.WriteLine(" Before State is : " + (string)jobDict_current[STATE]);
                    if (ob.checkStateDone(ob, jobId, jobDict_current))
                    {
                        //string checkstate = (string)jobDict_current[STATE];
                        Trace.WriteLine("In Returned CheckDone State method", "Information");
                        //Trace.WriteLine(" Before State is : " + (string)jobDict_current[STATE]);
                        UpdateState_Current(jobId, "Free");

                        string checkstate = (string)jobDict_current[STATE];
                        Trace.WriteLine(" After State is : " + checkstate);
                        Trace.WriteLine("UpdatedState : Free " + jobId);
                    }
                    if (ob.checkStateReady(ob, jobId, jobDict_current))
                    {
                        //string checkstate = (string)jobDict_current[STATE];
                        Trace.WriteLine("In CheckReady State method", "Information");

                        UpdateState_Current(jobId, "Running");
                        string checkstate = (string)jobDict_current[STATE];
                        Trace.WriteLine(" After State is : " + checkstate);
                        Trace.WriteLine("UpdatedState of Current JobID : Running " + jobId);

                        if (ob.executesubjob(jobId))
                        {
                            //checkstate = (string)jobDict_current[STATE];
                            UpdateState_Current(jobId, "Free");

                            checkstate = (string)jobDict_current[STATE];
                            Trace.WriteLine(" Before State is : " + checkstate);
                            Trace.WriteLine("UpdatedState : Free " + jobId);
                        }
                    }

                    if (jobId == queueMessage1.AsString)
                    {
                        Trace.WriteLine(" ");
                        Trace.WriteLine(" What the hell.............I'm comapring " + jobId + "with same f_JobID: " + queueMessage1.AsString);
                        //ob.q.DeleteMessage(queueMessage1);
                        //CloudQueueMessage message = new CloudQueueMessage(jobId);
                        //ob.q.AddMessage(message);
                        //Trace.WriteLine(" Deleted and Added the JobId : " + jobId);
                    }

                    if (jobId != queueMessage1.AsString)
                    {
                        Trace.WriteLine("ReplicaAgent got queue message: " + queueMessage1.AsString + " MsgId: "
                                     + queueMessage1.Id + " Pop Receipt: " + queueMessage1.PopReceipt
                                     + " Visibility: " + queueMessage1.NextVisibleTime, "Information");

                        string f_jobId = queueMessage1.AsString;
                        Trace.WriteLine(" ++++++++++++++++++++ Got Foreign JobId in the Queue ++++++++++++++++++++ " + f_jobId);

                        jobDict_Foreign = getJobDictFromBlob(queueMessage1.AsString);
                        if (ob.checkStateFree(ob, queueMessage1.AsString, jobDict_Foreign))
                        {
                            // Change the state of present jobId and post it to blob 
                            //string checkstate = (string)jobDict_Foreign[STATE];
                            UpdateState_Current(jobId, "Pending");
                            string checkstate = (string)jobDict_current[STATE];
                            Trace.WriteLine(" After State is : " + checkstate);
                            Trace.WriteLine("UpdatedState of the Current JobID: Pending" + jobId);
                            // Change the state of other jobId and post it to blob 
                            UpdateState_Foreign(queueMessage1.AsString, "Pending");
                            Trace.WriteLine(" Before State is : " + (string)jobDict_Foreign[STATE]);
                            Trace.WriteLine("UpdatedState of the foreign JobID : Pending" + queueMessage1.AsString);



                            // Call checkJobDictArguments to get temperature 
                            ob.temp_b = ob.queueMessageTemperature(ob, queueMessage1.AsString, jobDict_Foreign);  // doubt
                            Trace.WriteLine("Temperature of QueueMessage1 JobId is : " + ob.temp_b);

                            // get the energy values for both jobId from the stdout in the blob and make a exchange attempt 
                            double en_a = ob.getEnergy(ob, jobId);
                            Trace.WriteLine("Energy of JobId is : " + en_a);
                            double en_b = ob.getEnergy(ob, queueMessage1.AsString);
                            Trace.WriteLine("Energy of QueueMessage1 JobId is : " + en_b);

                            // Call the Exchange Method to Swap temperatures if they Satify Metroplois Scheme 
                            ob.doExchange(ob, en_a, en_b);
                            Trace.WriteLine("Exchange attempt is Done :");

                            // Increment BlobCount 
                            int count = ob.incrementBlobCount();
                            Trace.WriteLine("Exchange Value is :" + count);

                            // Restart both Replicas after Updating the States and increase the count variable in blob 


                            UpdateState_Foreign(queueMessage1.AsString, "Ready");
                            Trace.WriteLine("UpdatedState : Ready" + queueMessage1.AsString);
                            UpdateState_Current(jobId, "Running");
                            Trace.WriteLine("UpdatedState : Running " + jobId);
                            if (ob.executesubjob(jobId))
                            {
                                UpdateState_Current(jobId, "Done");
                                Trace.WriteLine("UpdatedState : Done " + jobId);
                            }

                        }
                    }

                    i++;
                }
            }
        }
        private CloudQueue callQueue()
        {
            // Get Reference to Queue Storage 
            queueClient1 = new CloudQueueClient(storageAccount.QueueEndpoint, storageAccount.Credentials);
            CloudQueue queue1 = queueClient1.GetQueueReference("replicaagent");
            queue1.CreateIfNotExist();
            string q= queue1.ToString();
            //Trace.WriteLine(" %%%%%%%%%%%  Queuename is %%%%%%%% : " + q);
            return queue1;
        }

        public int queueMessageTemperature(Program ob, String jobId_P, Dictionary<string, object> jobDict_P)
        {
            Trace.WriteLine(" Before Temperature of Message Queue Job Id is: " + ob.temp_b);
            if (jobDict_P.ContainsKey("arguments"))
            {
                string jqueuemessageArray = jobDict_P["arguments"].ToString();
                string[] ar = jqueuemessageArray.Split(' ');
                int res;
                bool check = int.TryParse(ar[1],out res);
                if(check==true )
                ob.temp_b = Convert.ToInt32(ar[2]);

                Trace.WriteLine(" After Temperature of Message Queue Job Id is: " + ob.temp_b);
            }
            return ob.temp_b;
        }

        public bool checkStateDone(Program ob, String jobId_P, Dictionary<string, object> jobDict_P)
        {
            string checkstate = (string)jobDict_P[STATE];
            if ( checkstate == "Done")
            {
                Trace.WriteLine(" ######### Yes In done state ##########", "Information");
                Trace.WriteLine(" job id_P is : " + jobId_P);
                Trace.WriteLine(" ");
                return true;
            }
             
            else
                return false;
        }

        public bool checkStateReady(Program ob, String jobId_P, Dictionary<string, object> jobDict_P)
        {
            string checkstate = (string)jobDict_P[STATE];
            if (checkstate == "Ready")
            {
                Trace.WriteLine(" ######### In CheckReady State method ##########", "Information");
                Trace.WriteLine(" $$$$$$$$$ Will Restart the current JobID soon $$$$$$$$$$$$ ", jobId_P);
                return true;
            }
            else
                return false;
        }

        public bool checkStateFree(Program ob, String jobId_P, Dictionary<string, object> jobDict_P)
        {

            string checkstate = (string)jobDict_P[STATE];
                if (checkstate == "Free")
                {
                    Trace.WriteLine(" ######### In CheckFree State method ##########", "Information");
                    return true;
                }
                else
                    return false;
            
        }

        public int incrementBlobCount()
        {
            int blobcount = 0;
            try
            {
                //string count1= null;
                
                CloudBlob countBlob = getCloudBlobContainer().GetBlobReference("count");
                blobcount = Convert.ToInt32(countBlob.DownloadText());
                blobcount += 1;
                //Upload to Blob /
                countBlob.UploadText(blobcount.ToString());
                Trace.WriteLine("IncrementBlobCount value is: " + blobcount);
                
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception in IncrementBlobCount" + e);
            }
            return blobcount;
            
        }


        public double getEnergy(Program ob, String jobId_P)
        {
            CloudBlob stdoutBlob = getCloudBlobContainer().GetBlobReference(namdoutput + "-" + jobId_P);
            int counter = 0;
            var output1 = stdoutBlob.DownloadText();

            string[] line1 = output1.Split('\n');
            List<string> items1 = new List<string>();
            //double result;
            try
            {

                foreach (string l in line1)
                {
                    items1.Add(l);
                    counter++;
                    if (items1[0].ToString() == "Energy")
                    {
                        if (counter == line1.Length)
                            //   ob.energy= double.TryParse(items1[11],out result);
                            ob.energy = Convert.ToDouble(items1[11]);
                    }
                }
               Trace.WriteLine("Energy of Jobid in QueueMessage " + ob.energy);
               return ob.energy;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(" I'm in Energy exception" + ex);
            }
            return ob.energy;
        }

        public void doExchange(Program ob, double en_a, double en_b)
        {
            Boolean iflag = false;
            // from R= 1.9872 cal per mol 
            double factor = 0.0019872;
            double delta = ((1 / ob.temp_a) / (factor) - (1 / ob.temp_b) / (factor)) * (en_b - en_a);
            Trace.WriteLine(" Delta value is : " + delta);
            if (delta < 0)
                iflag = true;

            else
            {
                Random rm = new Random();
                if (Math.Exp(-delta) > rm.Next())
                    iflag = true;
            }
            if (iflag == true)
            {
                int tmpNum = ob.temp_b;
                Trace.WriteLine(" TmpNum value is : " + tmpNum);
                ob.temp_b = ob.temp_a;
                Trace.WriteLine(" temp_b value is : " + ob.temp_b);
                ob.temp_a = tmpNum;
                Trace.WriteLine(" temp_a value is : " + ob.temp_a);
            }
        }
        
        public void UpdateState_Current(String jobId_P, string newState)
        {
            jobDict_current = getJobDictFromBlob(jobId_P);
            jobDict_current[STATE] = newState;

            uploadJobDictToBlob(jobId_P, jobDict_current);
        }

        public void UpdateState_Foreign(String jobId_P, string newState)
        {
            jobDict_Foreign = getJobDictFromBlob(jobId_P);
            jobDict_Foreign[STATE] = newState;

            uploadJobDictToBlob(jobId_P, jobDict_Foreign);
        }
        private void uploadJobDictToBlob(String jobId_P, Dictionary<string, object> jobDict_P)
        {
            CloudBlob jobBlob = getCloudBlobContainer().GetBlobReference(jobId_P);
            String jobString = JsonConvert.SerializeObject(jobDict_P);
            jobBlob.UploadText(jobString);
        }

        private Dictionary<string, object> getJobDictFromBlob(String jobId_P)
        {
            CloudBlob jobBlob = getCloudBlobContainer().GetBlobReference(jobId_P);
            String jobString = jobBlob.DownloadText();
            Dictionary<string, object> jobDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobString);
            return jobDict;
        }

        public bool executesubjob(String jobId_P)
        {
            try
            {
                // + @"\", @"approot\resources\namd\");
                //String workingdirectory = Environment.GetEnvironmentVariable("RoleRoot");
                //Trace.WriteLine(" &&&&&&&&&&&&&&&&&& I'm in Execute Job Method &&&&&&&&& ", "Information");
                string path = @"E:\approot\resources\namd\namd2.exe";
                string configpath = @"E:\approot\resources\namd\NPT.conf";
                string path2 = "+p1 " + configpath;
                //ProcessStartInfo info = new ProcessStartInfo("namd2.exe", "+p1 NPT.conf");
                ProcessStartInfo info = new ProcessStartInfo(path, path2);
                UpdateState_Current(jobId, "Running");
                info.UseShellExecute = false;
                info.ErrorDialog = false;
                //  info.WorkingDirectory = "J:\\DrJhaProject\\Backup\\BigjobAzureAgent\\resources\\namd";
                info.CreateNoWindow = true;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                Process applicationProcess = Process.Start(info);
                //Trace.WriteLine(" &&&&&&&&&&&&&&&&&& After Execution &&&&&&&&& ", "Information");
                //Console.Read();
                // log a message that can be viewed in the diagnostics tables called WADLogsTable
                //  System.Diagnostics.Trace.WriteLine("Added blob to Windows Azure Storage");
                StreamReader reader = applicationProcess.StandardOutput;
                string completeOutput = string.Empty;
                string line = string.Empty;
                line = reader.ReadLine();
                Trace.WriteLine(line, "Information");
                while (line != null)
                {
                    line = reader.ReadLine();
                    completeOutput += line != null ? line : string.Empty;
                    completeOutput += "\n";
                    Trace.WriteLine(line != null ? line : "<EOL>", "Information");
                }
                applicationProcess.WaitForExit();
                Stopwatch swComputeTime = Stopwatch.StartNew();
                //Trace.WriteLine("Runtime: " + swComputeTime.ElapsedMilliseconds + " msec");

                //#region get output and store in subjob blob
                //string outputString = applicationProcess.StandardOutput.ReadToEnd();

                string outputString = completeOutput;
                string errorString = applicationProcess.StandardError.ReadToEnd();
                Random rn = new Random();
                //int jobId = rn.Next(1000);
                Trace.WriteLine("############## Execute Method Job ID Arguments are ################# :" + jobId);
                DateTime date = DateTime.Now;
                string dateString = date.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
                CloudBlob stdoutBlob = getCloudBlobContainer().GetBlobReference(namdoutput + "-" + jobId);
                stdoutBlob.UploadText("VMSize: " + "n/a" + "\n" + "Runtime: "
                                       + swComputeTime.ElapsedMilliseconds + " ms\n\n"
                                       + "******************************************************************************************"
                                       + "\nOutput:\n" + outputString);
                CloudBlob stderrBlob = getCloudBlobContainer().GetBlobReference(namderror + "-" + jobId);
                stderrBlob.UploadText(errorString);
                //Program ob = new Program();
                //ob.createqueue();
                //UpdateState(jobId, "Done");
                //Program ob1 = new Program();
                //ob1.Start1();
                //ob1.Run1();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n Exception ------\n{0}", ex.InnerException.StackTrace);
                return false;

            }

        }

        private CloudBlobContainer getCloudBlobContainer()
        {
            if (blobContainer == null)
            {
                applicationName = "bigjob";
                blobContainer = getCloudBlobClient().GetContainerReference(applicationName);
            }
            return blobContainer;
        }

        private CloudBlobClient getCloudBlobClient()
        {
            if (blobClient == null)
            {
                storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=saga;AccountKey=cjOGY5SjG9uRNuvK4W0Yjzb9J3BMIPFs3an6pgAq8Wdj3xdJ85kXuo6zPfMArJ+ZIYhlgtRuoZ9FKsy5S2e4Fw==");
                blobClient = new CloudBlobClient(storageAccount.BlobEndpoint, storageAccount.Credentials);
            }
            return blobClient;
        }
    }
}

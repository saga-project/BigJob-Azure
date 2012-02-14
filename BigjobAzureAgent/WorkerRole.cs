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

namespace BigjobAzureAgent
{

    public class WorkerRole : RoleEntryPoint
    {
        CloudStorageAccount storageAccount;
        String applicationName;
        String STATE = "state";

        CloudBlobClient blobClient = null;
        CloudQueueClient queueClient = null;
        CloudBlobContainer blobContainer = null;


        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("BigjobAzureAgent entry point called", "Information");// information warning and errors.
            System.Diagnostics.Trace.TraceInformation("Change in Entry Point");
            //GET reference to Queue Storage
            queueClient = new CloudQueueClient(storageAccount.QueueEndpoint, storageAccount.Credentials);

            CloudQueue queue = queueClient.GetQueueReference(applicationName);
            queue.CreateIfNotExist();
            while (true)
            {
                //IAsyncResult result = queue.BeginGetMessage(null, null);
                //CloudQueueMessage jobId = queue.EndGetMessage(result);
                Trace.WriteLine("In WHile Loop", "Warning");
                CloudQueueMessage queueMessage = queue.GetMessage(new TimeSpan(1,0,0));
                
                if (queueMessage == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    Trace.WriteLine("BigjobAzureAgent got queue message: " + queueMessage.AsString + " MsgId: " 
                        + queueMessage.Id + " Pop Receipt: " + queueMessage.PopReceipt
                        + " Visibility: " + queueMessage.NextVisibleTime, "Information");
                }
                if (queueMessage.AsString == "STOP")
                {
                    //put STOP message back in queue
                    queue.AddMessage(queueMessage);
                    break; //exit while loop and worker role
                }
                else
                {
                    string jobId = queueMessage.AsString; //queue message contains id of subjob to execute
                    if (ExecuteSubJob(jobId)) 
                    {
                        //success - delete message
                        UpdateState(jobId, "Done");
                        queue.DeleteMessage(queueMessage);
                        //queue.DeleteMessage(queueMessage.Id, queueMessage.PopReceipt);
                        
                        //getCloudBlobContainer().GetBlobReference(jobId).Delete();
                    }
                    else
                    {
                        //failure
                        UpdateState(jobId, "New");
                        queue.AddMessage(queueMessage); //put message back in queue 
                    }
                    
                }
            } //end while loop
        }

        public void StageInFiles(string jobId, Dictionary<string, object> jobDict, string workingDirectory)
        {
            if (jobDict.ContainsKey("filetransfer"))
            {
                JArray sourceArray = (JArray)jobDict["filetransfer"];
                //Dictionary<string, object> transfers 
                //    = (Dictionary<string, object>)jobDict["filetransfer"];
                //JArray sourceArray = (JArray) jobject.SelectToken("source");
                foreach (JObject a in sourceArray)
                {
                    try
                    {
                        string source = (string)a.SelectToken("source");
                        source = Path.GetFileName(source);
                        string sourceFileName = jobId + "/" + source;

                        //string localPath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot"));
                        string localPath = Path.Combine(Environment.GetEnvironmentVariable("temp"));
                        //string targetFileName = Path.Combine(localPath, workingDirectory);
                        string targetFileName = Path.Combine(localPath, source);
                        CloudBlobContainer blobContainer = getCloudBlobContainer();
                        Trace.WriteLine("Download from Blob: " + sourceFileName + " to: " + targetFileName);
                        blobContainer.GetBlobReference(sourceFileName).DownloadToFile(targetFileName);
                    }                   
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.StackTrace);
                    }
                }
            }
        }

        public void UpdateState(string jobId, string newState)
        {
            Dictionary<string, object> jobDict = getJobDictFromBlob(jobId);
            jobDict[STATE] = newState;
            uploadJobDictToBlob(jobId, jobDict);
        }

        public bool ExecuteSubJob(String jobId)
        {
            //get reference to blob client
            Dictionary<string, object> jobDict = getJobDictFromBlob(jobId);
            
            string state = (string) jobDict[STATE];
            if (state == "New" || state == "Unknown")
            {
                jobDict[STATE] = "New";
                Trace.WriteLine(jobDict, "Information");
                String numberOfProcesses = "1";
                if (jobDict.ContainsKey("number_of_processes"))
                {
                    numberOfProcesses = (string)jobDict["number_of_processes"];
                }

                String spmdvariation = "single";
                if (jobDict.ContainsKey("spmd_variation"))
                {
                    spmdvariation = (string)jobDict["spmd_variation"];
                }
                String arguments = "";
                if (jobDict.ContainsKey("arguments"))
                {
                    JArray argumentArray = (JArray)jobDict["arguments"];
                    foreach (JValue a in argumentArray)
                    {
                        string newArg = a.ToString();
                        newArg = substituteTempDir(newArg);
                        arguments = arguments + " " + newArg;
                    }
                }
                String executable = (string)jobDict["executable"];

                String workingdirectory = Environment.GetEnvironmentVariable("RoleRoot");
                if (jobDict.ContainsKey("working_directory"))
                {
                    workingdirectory = (string)jobDict["working_directory"];
                    workingdirectory = substituteTempDir(workingdirectory);
                    copyDirectory(Environment.GetEnvironmentVariable("RoleRoot") + @"\approot\resources\namd\",
                                  workingdirectory);
                }
                String output = "stdout";
                if (jobDict.ContainsKey("output"))
                {
                    output = (string)jobDict["output"];
                }

                String error = "stderr";
                if (jobDict.ContainsKey("error"))
                {
                    error = (string)jobDict["error"];
                }

                //stage files
                StageInFiles(jobId, jobDict, workingdirectory);

                try
                {
                    #region execute subjob
                    //String command = executable + " " + arguments;
                    string localPath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot")); // + @"\", @"approot\resources\namd\");
                    //string exeFileName = "namd2.exe";
                    //ProcessStartInfo info = new ProcessStartInfo(localPath + exeFileName, "+p8 NPT.conf");

                    Trace.WriteLine("Executable: " + localPath + @"\" + executable + " Arguments: " + arguments
                        + " WorkingDirectory: " + Path.Combine(localPath + @"\", workingdirectory), "Information");
                    ProcessStartInfo info = new ProcessStartInfo(localPath + @"\" + executable, arguments);
                    info.UseShellExecute = false;
                    info.ErrorDialog = false;
                    info.WorkingDirectory = Path.Combine(localPath + @"\", workingdirectory);                    
                    info.CreateNoWindow = true;
                    info.RedirectStandardOutput = true;
                    info.RedirectStandardError = true;
                    Stopwatch swComputeTime = Stopwatch.StartNew();
                    UpdateState(jobId, "Running");
                    Trace.WriteLine("Starting .exe in directory " + Path.Combine(localPath + @"\", workingdirectory), "Information");
                    Process applicationProcess = Process.Start(info);
                    Trace.WriteLine("Started .exe on host " + applicationProcess.MachineName, "Information");

                    #region Grap Output
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
                    #endregion

                    
                    applicationProcess.WaitForExit();
                    swComputeTime.Stop();
                    Trace.WriteLine("Runtime: " + swComputeTime.ElapsedMilliseconds + " msec");
                    #endregion

                    #region get output and store in subjob blob

                    //string outputString = applicationProcess.StandardOutput.ReadToEnd();

                    string outputString = completeOutput;
                    string errorString = applicationProcess.StandardError.ReadToEnd();

                    DateTime date = DateTime.Now;
                    string dateString = date.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
                    CloudBlob stdoutBlob = getCloudBlobContainer().GetBlobReference(output + "-" + jobId);
                    stdoutBlob.UploadText("VMSize: " + "n/a" + "\n" + "Runtime: "
                        + swComputeTime.ElapsedMilliseconds + " ms\n\n"
                        + "******************************************************************************************"
                        + "\nOutput:\n" + outputString);
                    CloudBlob stderrBlob = getCloudBlobContainer().GetBlobReference(error + "-" + jobId);
                    stderrBlob.UploadText(errorString);                    
                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nMessage ---\n{0}", ex.Message);
                    Console.WriteLine(
                        "\nHelpLink ---\n{0}", ex.HelpLink);
                    Console.WriteLine("\nSource ---\n{0}", ex.Source);
                    Console.WriteLine(
                        "\nStackTrace ---\n{0}", ex.StackTrace);
                    Console.WriteLine(
                        "\nTargetSite ---\n{0}", ex.TargetSite);
                    Console.WriteLine(
                      "\nInner Exception ---\n{0}", ex.InnerException.StackTrace);
                    return false;
                }
                return true;
            }
            return false;
        }

        public static void copyDirectory(string Src, string Dst)
        {
            Trace.WriteLine("Copy resources from: " + Src + " to: " + Dst);
            try
            {
                String[] Files;
                if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                    Dst += Path.DirectorySeparatorChar;
                if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
                Files = Directory.GetFileSystemEntries(Src);
                foreach (string Element in Files)
                {
                    // Sub directories
                    if (Directory.Exists(Element))
                    {
                        copyDirectory(Element, Dst + Path.GetFileName(Element));
                        // Files in directory
                    }
                    else
                    {
                        string destinationFile = Dst + @"\" + Path.GetFileName(Element);
                        Trace.WriteLine("Copy: " + Element + " to: " + destinationFile);
                        File.Copy(Element, destinationFile, true);
                    }
                }
            }
            catch (DirectoryNotFoundException e)
            {
                Trace.WriteLine(e.StackTrace);                
            }
       }

       private static string substituteTempDir(string newArg)
       {
            if (newArg.Contains("$TEMP"))
            {
                string temp = Environment.GetEnvironmentVariable("temp");
                Trace.WriteLine("Set $TEMP in: " + newArg + " to " + temp);
                newArg = newArg.Replace("$TEMP", temp);
            }
            return newArg;
        }

     
        private Dictionary<string, object> getJobDictFromBlob(String jobId)
        {
            CloudBlob jobBlob = getCloudBlobContainer().GetBlobReference(jobId);
            String jobString = jobBlob.DownloadText();
            Dictionary<string, object> jobDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobString);
            return jobDict;
        }

        private void uploadJobDictToBlob(String jobId, Dictionary<string,object> jobDict)
        {
            CloudBlob jobBlob = getCloudBlobContainer().GetBlobReference(jobId);
            String jobString = JsonConvert.SerializeObject(jobDict);            
            jobBlob.UploadText(jobString);            
        }
        
        private CloudBlobClient getCloudBlobClient()
        {
            if (blobClient == null)
            {
                blobClient = new CloudBlobClient(storageAccount.BlobEndpoint, storageAccount.Credentials);
            }
            return blobClient;
        }

        private CloudBlobContainer getCloudBlobContainer()
        {
            if (blobContainer == null)
            {
                blobContainer = getCloudBlobClient().GetContainerReference(applicationName); 
            }
            return blobContainer;
        
        }

        public override bool OnStart()
        {

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;


            //Get the default initial configuration for Windows Azure Diagnostics
            DiagnosticMonitorConfiguration diagnosticMonitorConfiguration = DiagnosticMonitor.GetDefaultInitialConfiguration();
                     
            //Specify the scheduled transfer
            diagnosticMonitorConfiguration.WindowsEventLog.ScheduledTransferPeriod = System.TimeSpan.FromMinutes(1.0);

            diagnosticMonitorConfiguration.Directories.ScheduledTransferPeriod = TimeSpan.FromMinutes(5.0);
            diagnosticMonitorConfiguration.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1.0);
            diagnosticMonitorConfiguration.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            diagnosticMonitorConfiguration.WindowsEventLog.ScheduledTransferPeriod = TimeSpan.FromMinutes(5.0);
            diagnosticMonitorConfiguration.PerformanceCounters.ScheduledTransferPeriod = TimeSpan.FromMinutes(1.0);

            diagnosticMonitorConfiguration.WindowsEventLog.DataSources.Add("Application!*");
            diagnosticMonitorConfiguration.WindowsEventLog.DataSources.Add("System!*");

            PerformanceCounterConfiguration performanceCounterConfiguration = new PerformanceCounterConfiguration();
            performanceCounterConfiguration.CounterSpecifier = @"\Processor(*)\% Processor Time";
            performanceCounterConfiguration.SampleRate = System.TimeSpan.FromSeconds(1.0);
            diagnosticMonitorConfiguration.PerformanceCounters.DataSources.Add(performanceCounterConfiguration);

            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", diagnosticMonitorConfiguration);

            // Config Change Handling
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));

                RoleEnvironment.Changed += (sender, arg) =>
                {
                    if (arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any((change) => (change.ConfigurationSettingName == configName)))
                    {
                        if (!configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)))
                        {
                            RoleEnvironment.RequestRecycle();
                        }
                    }
                };
            });

            // create storage account
            storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            applicationName = RoleEnvironment.GetConfigurationSettingValue("ApplicationName");


            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += RoleEnvironmentChanging;

            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}

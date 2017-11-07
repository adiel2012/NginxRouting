using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class ProcessMonitor
    {
        private int portFrom;
        private int portTo;

        public ProcessMonitor(int portFrom, int portTo)
        {
            this.portFrom = portFrom;
            this.portTo = portTo;
            this.StartMonitoring();
        }

        private void StartMonitoring()
        {
            int miliseconds = 10000;
            while (true)
            {
                Thread.Sleep(miliseconds);

                MonitorNginx();

                Process[] processes = Process.GetProcessesByName("ServerNET");
                HashSet<int> ports = new HashSet<int>();
                for (int p = 0; p < processes.Length; p++)
                {
                    String[] arguments = getCommandLinesParsed(processes[p]);
                    if (arguments != null && arguments.Length > 0)
                    {
                        if (int.TryParse(arguments[0], out int i))
                        {
                            ports.Add(i);
                        }
                    }
                }
                for(int i = portFrom; i <= portTo; i++)
                {
                    if(!ports.Contains(i))
                    {
                        CreateProcess(i);
                    }
                }
                // TODO REMOVE IT
               // break;

            }

        }

        private void MonitorNginx()
        {
            Process[] processes = Process.GetProcessesByName("nginx");
            if(processes == null || processes.Length == 0)
            {
                string absPathContainingHrefs = Directory.GetCurrentDirectory(); // Get the "base" path
                ReplaceConfig(Path.Combine(absPathContainingHrefs, @".\nginx-1.12.2\conf\nginx.conf"));
                string fullPath = Path.Combine(absPathContainingHrefs, @".\nginx-1.12.2\nginx.exe");
                fullPath = Path.GetFullPath(fullPath);  // Will turn the above into a proper abs path

                ProcessStartInfo procInfo = new ProcessStartInfo(fullPath);
                //procInfo.UseShellExecute = true;
                //procInfo.Verb = "runas";
                procInfo.WorkingDirectory = absPathContainingHrefs+ @"\nginx-1.12.2\";
                Process.Start(procInfo);

                Thread.Sleep(5000);
            }

        }

        private void ReplaceConfig(string aurl)
        {

            /*
             * server 127.0.0.1:2000;
		server 127.0.0.1:2001;
		server 127.0.0.1:2002;
		server 127.0.0.1:2003;
             * */
            string servers = "\n ";
            for (int i = portFrom; i <= portTo; i++)
            {
                servers += String.Format("server 127.0.0.1:{0};\n",i);
            }
                string content = @"#user  nobody;
worker_processes  1;

#error_log  logs/error.log;
#error_log  logs/error.log  notice;
#error_log  logs/error.log  info;

#pid        logs/nginx.pid;

events {
    worker_connections  1024;
}

stream {
	upstream stream_backend {
		
		"+ servers + @"
	}
	
	server {
        listen 3107;
        proxy_pass stream_backend;
    }
}";
            if (File.Exists(aurl))
            {
                File.Delete(aurl);
            }
            System.IO.File.WriteAllText(aurl, content);
        }

        private void CreateProcess(int i)
        {
            Process process = Process.Start(new ProcessStartInfo( "ServerNET.exe", i.ToString()));
        }

        private bool ProcessRuning(int i)
        {
            return false;
        }

        public static String getCommandLines(Process processs)
        {
            ManagementObjectSearcher commandLineSearcher = new ManagementObjectSearcher(
                "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + processs.Id);
            String commandLine = "";
            foreach (ManagementObject commandLineObject in commandLineSearcher.Get())
            {
                commandLine += (String)commandLineObject["CommandLine"];
            }

            return commandLine;
        }

        public static String[] getCommandLinesParsed(Process process)
        {
            return (parseCommandLine(getCommandLines(process)));
        }

        /// <summary>
        /// This routine parses a command line to an array of strings
        /// Element zero is the program name
        /// Command line arguments fill the remainder of the array
        /// In all cases the values are stripped of the enclosing quotation marks
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns>String array</returns>
        public static String[] parseCommandLine(String commandLine)
        {
            List<String> arguments = new List<String>();

            /* Boolean stringIsQuoted = false;
             String argString = "";
             for (int c = 0; c < commandLine.Length; c++)  //process string one character at a tie
             {
                 if (commandLine.Substring(c, 1) == "\"")
                 {
                     if (stringIsQuoted)  //end quote so populate next element of list with constructed argument
                     {
                         arguments.Add(argString);
                         argString = "";
                     }
                     else
                     {
                         stringIsQuoted = true; //beginning quote so flag and scip
                     }
                 }
                 else if (commandLine.Substring(c, 1) == "".PadRight(1))
                 {
                     if (stringIsQuoted)
                     {
                         argString += commandLine.Substring(c, 1); //blank is embedded in quotes, so preserve it
                     }
                     else if (argString.Length > 0)
                     {
                         arguments.Add(argString);  //non-quoted blank so add to list if the first consecutive blank
                     }
                 }
                 else
                 {
                     argString += commandLine.Substring(c, 1);  //non-blan character:  add it to the element being constructed
                 }
             }*/

            string[] splitted = commandLine.Split(' ');
            for (int i = 1; i < splitted.Length; i++)
                arguments.Add(splitted[i]);

            return arguments.ToArray();

        }
    }
}

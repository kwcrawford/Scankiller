using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scankill {
	internal class Program {
        public enum SimpleServiceCustomCommands
        { StopWorker = 128, RestartWorker, CheckWorker };
		static bool bDetail=false;

		static void Main(string[] args) {
			System.Timers.Timer timer = new System.Timers.Timer();
			if(args != null && args.Length > 0 && args[0] == "/D")
				bDetail=true;
			if(bDetail)
				Console.WriteLine("Starting scankill with logging detail");
			timer.Elapsed += (source, srgs) => {
				if(bDetail)
					Console.WriteLine("Running scan ...");
				GetWUpStatus();
				//Process[] processes = Process.GetProcessesByName("wuauserv");
				//if (processes.Length > 0) {
				//	Console.WriteLine("Found "+processes.Length+" matching processes");

				//	foreach (Process process in processes) {
				//		process.Kill();
				//		process.WaitForExit();
				//	}
				//}
				//Start again.
				//This makes sure that we wait 10 seconds after
				//we are done killing the processes
				timer.Start();
			};

			//Run every 5 seconds
			timer.Interval = 5000;

			//This causes the timer to run only once
			//But will be restarted after processing. See comments above
			timer.AutoReset = false;
			GetWUpStatus();

			timer.Start();

			Console.WriteLine("Scanning started, press Return key to exit program .....");
			Console.ReadLine();
		}

		static bool GetWUpStatus()
        {
            ServiceController[] scServices;
			bool isDisabled = false;

            scServices = ServiceController.GetServices();

            foreach (ServiceController scTemp in scServices)
            {
                if (scTemp.ServiceName == "wuauserv")
                {
					// Display properties for the Simple Service sample
					// from the ServiceBase example.
					//Console.WriteLine("Service name: "+scTemp.ServiceName);
					ServiceController sc = new ServiceController("wuauserv");
					//               Console.WriteLine("Status = " + sc.Status);
					//               Console.WriteLine("Can Pause and Continue = " + sc.CanPauseAndContinue);
					//               Console.WriteLine("Can ShutDown = " + sc.CanShutdown);
					//               Console.WriteLine("Can Stop = " + sc.CanStop);
					//Console.WriteLine("Start Type = "+sc.StartType);
					if (sc.StartType == ServiceStartMode.Disabled && sc.Status != ServiceControllerStatus.Running)
						isDisabled = true;
					else if(sc.Status == ServiceControllerStatus.Running)
					{
						Console.WriteLine("Stopping and disabling service name: "+scTemp.ServiceName);
						try
						{
							sc.Stop();
						}
						catch(Exception ex) {
							Console.WriteLine("Call to stop service failed: "+ex.Message);
							Console.WriteLine("Scankill must be executed in Administrator mode");
						}
						while (sc.Status == ServiceControllerStatus.Running) {
							Thread.Sleep(1000);
							sc.Refresh();
						}
						//sc.ExecuteCommand((int)SimpleServiceCustomCommands.StopWorker);
						try
						{
							ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Disabled); 
						}
						catch(Exception ex) {
							Console.WriteLine("Call to ChangeStartMode failed: "+ex.Message);
							Console.WriteLine("Scankill must be executed in Administrator mode");
						}
					}
					if(isDisabled == false) {
						Console.WriteLine("Disabling service name: "+scTemp.ServiceName);
						try
						{
							ServiceHelper.ChangeStartMode(sc, ServiceStartMode.Disabled); 
						}
						catch(Exception ex) {
							Console.WriteLine("Call to ChangeStartMode failed: "+ex.Message);
							Console.WriteLine("Scankill must be executed in Administrator mode");
						}
					}
					sc.Close();
					sc.Dispose();
                }
            }
			return isDisabled;
		}
	}
}

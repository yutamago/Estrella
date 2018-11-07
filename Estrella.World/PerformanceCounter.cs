using System;
using System.Diagnostics;
using System.Threading;

namespace Estrella.World
{
    public class PerformCounter
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _performanceCounterReceived;
        private PerformanceCounter _performanceCounterSent;
        private PerformanceCounter _ramCounter;

        public PerformCounter()
        {
            SetupConsole();
            SetupCounters();
            var performanceCounter = new Thread(SetConsoleTitle);
            performanceCounter.Start();
        }

        private string AvailableRam => _ramCounter.NextValue().ToString("N2") + " Mb";
        private string CurrentCpuUsage => _cpuCounter.NextValue().ToString("N2");


        private void SetupCounters()
        {
            _cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };

            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            var performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
            var instance = performanceCounterCategory.GetInstanceNames()[0]; // 1st NIC !
            _performanceCounterSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
            _performanceCounterReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
        }

        private static void SetupConsole()
        {
            Console.WindowWidth = 90;
        }

        private void SetConsoleTitle()
        {
            Thread.Sleep(2000);
            while (true)
            {
                try
                {
                    var memory = AvailableRam;
                    var cpu = CurrentCpuUsage;
                    Console.Title = "World[" + Settings.Instance.Id + "] TickPerSecond : " +
                                    Worker.Instance.TicksPerSecond + " Free Memory : " + memory + " CPU: " + cpu +
                                    " Network : bytes send: " +
                                    (_performanceCounterSent.NextValue() / 1024).ToString("N2") + " bytes received: " +
                                    (_performanceCounterReceived.NextValue() / 1024).ToString("N2") + " ";
                }
                finally
                {
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
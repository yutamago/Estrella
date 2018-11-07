using System;
using System.Diagnostics;
using System.Threading;

namespace Estrella.Zone
{
    public class PerformCounter
    {
        protected PerformanceCounter cpuCounter;
        protected PerformanceCounter performanceCounterReceived;
        protected PerformanceCounter performanceCounterSent;
        protected PerformanceCounter ramCounter;

        public PerformCounter()
        {
            SetupCounters();
            var PerfomanceCounter = new Thread(SetConsoleTitel);
            PerfomanceCounter.Start();
        }

        public string getAvailableRAM()
        {
            return ramCounter.NextValue().ToString("N2") + " Mb";
        }

        public string getCurrentCpuUsage()
        {
            return cpuCounter.NextValue().ToString("N2");
        }

        public void SetupCounters()
        {
            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            var performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
            var instance = performanceCounterCategory.GetInstanceNames()[0]; // 1st NIC !
            performanceCounterSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instance);
            performanceCounterReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", instance);
        }

        public void SetConsoleTitel()
        {
            while (true)
            {
                var memory = getAvailableRAM();
                var cpu = getCurrentCpuUsage();
                Console.Title = "Zone[" + Program.ServiceInfo.ID + "] TickPerSecont : " +
                                Worker.Instance.TicksPerSecond + " Free Memory : " + memory + " CPU: " + cpu +
                                " Network : bytes send: " + (performanceCounterSent.NextValue() / 1024).ToString("N2") +
                                " bytes received: " + (performanceCounterReceived.NextValue() / 1024).ToString("N2") +
                                " ";
                Thread.Sleep(2000);
            }
        }
    }
}
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitorNET
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                int PortFrom = int.Parse(args[0]);
                int PortTo = int.Parse(args[1]);
                new ProcessMonitor(PortFrom, PortTo);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wrong Format");
            }
        }
    }
}

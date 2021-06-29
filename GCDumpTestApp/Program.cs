using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace GCDumpTestApp
{
    class Program
    {
        public static int numberOfExceptions = 0;
        public static DateTime startTime;
        public static int minutes;

        static void Main(string[] args)
        {
            minutes = 15;

            Thread mathThread = new Thread(DoSomeMath)
            {
                Name = "Math Thread",
            };
            Thread arrayExcetionThread = new Thread(ArrayException)
            {
                Name = "Array Exception Thread"
            };
            Thread listThread = new Thread(CreateList)
            {
                Name = "List Thread"
            };
            Thread objectThread = new Thread(PopulateList)
            {
                Name = "Object Thread"
            };

            Thread gcDumpThread = new Thread(GC_Collect)
            {
                Name = "gcdump Thread"
            };

            Console.WriteLine("Run program with gcdump?  (y) / (n)");
            string response = Console.ReadLine();

            startTime = DateTime.UtcNow;

            objectThread.Start();
            arrayExcetionThread.Start();
            mathThread.Start();
            listThread.Start();
            if (response.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Running with gcdump");
                gcDumpThread.Start();
            }


            Console.ReadKey();

        }

        public static void LongLoop()
        {
            while ((DateTime.UtcNow - startTime) < TimeSpan.FromMinutes(minutes))
            {
                for (int i = 0; i < 1000000; i++)
                {
                    for (int j = 0; j < 10000000; j++)
                    {
                        double t = Math.Cosh(0.45);
                    }
                }

            }
        }

        public static void PopulateList()
        {
            List<object> list = new List<object>();
            while ((DateTime.UtcNow - startTime) < TimeSpan.FromMinutes(minutes))
            {
                object t = new object();
                list.Add(t);
            }
            Console.WriteLine("Total number of Objects in list: " + list.Count);
        }

        public static void ArrayException()
        {
            int run = 0;
            while ((DateTime.UtcNow - startTime) < TimeSpan.FromMinutes(minutes))
            {
                int[] randomArray = { 1, 2, 3, 4, 5 };
                Random ran = new Random();
                int randomNum = ran.Next(6);

                try
                {
                    int getNum = randomArray[randomNum];
                }
                catch (Exception)
                {
                    numberOfExceptions++;
                }
                run++;
            }
            Console.WriteLine("Total number of exceptions: " + numberOfExceptions);
            Console.WriteLine("Exception ran:" + run);


        }
        public static void CreateList()
        {
            int run = 0;
            while ((DateTime.UtcNow - startTime) < TimeSpan.FromMinutes(minutes))
            {
                for (int i = 0; i < 50; i++)
                {
                    List<int> nList = new List<int>();
                    nList.Add(i);
                }
                run++;
            }
            Console.WriteLine("List ran:" + run);
        }

        public static void DoSomeMath()
        {
            int run = 0;
            while ((DateTime.UtcNow - startTime) < TimeSpan.FromMinutes(minutes))
            {
                double r = 0.4;

                double ans = Math.Acos(r);
                run++;
            }
            Console.WriteLine("Math ran:" + run);
        }

        public static void GC_Collect()
        {
            while ((DateTime.UtcNow - startTime) < TimeSpan.FromMinutes(minutes))
            {
                Process currentProcess = Process.GetCurrentProcess();

                Thread.Sleep(3000);
                Console.WriteLine("Current application memory: " + currentProcess.WorkingSet64 / Math.Pow(1024, 2));
                Console.WriteLine("Collecting gcdump");
                string text = $"/C dotnet-gcdump collect -p {currentProcess.Id} -v --timeout 45";
                Process cmd = Process.Start("CMD.exe", text);
                cmd.WaitForExit();
                List<string> gcFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.gcdump").ToList();

                try
                {
                    File.Delete(gcFile.Single());
                    Console.WriteLine($"{gcFile.Single()} deleted");
                    Thread.Sleep(2000);
                    Console.Clear();
                }
                catch(Exception e) when (e is ArgumentNullException || e is InvalidOperationException)
                {
                    Console.WriteLine("Failed to collect a dump file for the application");
                    double totalMB = currentProcess.WorkingSet64 / Math.Pow(1024, 2);
                    Console.WriteLine($"Application memory: {totalMB}");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
        }

    }
}

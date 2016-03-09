using System;

namespace TastyDomainDriven.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Type: S to: RunSample();");
                Console.WriteLine("Type: P to: RunPerf();");

                var k = Console.ReadKey();

                switch ((int)k.Key)
{
                    case (int)ConsoleKey.S:
                        RunSample();
                        break;
                    case (int)ConsoleKey.P:
                        RunPerformance();
                        break;
                }
            }
            catch (AggregateException aex)
            {
                Console.WriteLine("===== ERRORS: =====");

                foreach (var ex in aex.InnerExceptions)
                {
                    Console.WriteLine(ex);
                }

                throw;
            }
        }

        private static void RunSample()
        {
            SayHelloSampleStartup.Run().Wait();
        }

        private static void RunPerformance()
        {
            var performance = new PerformanceRun();
            performance.Run().Wait();
        }
    }
}

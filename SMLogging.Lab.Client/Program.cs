using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SMLogging.Lab.Client
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Press any key to perform operations");
            Console.ReadKey();
            var sw = Stopwatch.StartNew();

            Action action = () =>
            {
                using (var service = new LabServiceProxy())
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        var sw1 = Stopwatch.StartNew();
                        
                        service.GetData(1);
                        
                        sw1.Stop();
                        Console.WriteLine($"Time taken: {sw1.ElapsedMilliseconds}");
                    }
                }
            };

            Task.WaitAll(
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                Task.Factory.StartNew(action)
            );

            sw.Stop();
            Console.WriteLine($"Total time taken: {sw.ElapsedMilliseconds}");
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
    }
}

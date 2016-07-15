using System;
using System.Diagnostics;
using System.IO;
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
                //using (var service = new StreamingLabServiceProxy())
                using (var service = new LabServiceProxy())
                {
                    while (true)
                    {
                        var sw1 = Stopwatch.StartNew();

                        //var r = service.GetData(new StreamingRequest() {Value = 1});
                        service.DoSomething(1);


                        sw1.Stop();
                        Console.WriteLine($"Time taken: {sw1.ElapsedMilliseconds}");
                        Console.ReadLine();
                    }
                }
            };

            //Task.WaitAll(
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action),
                //Task.Factory.StartNew(action)
            //);

            action();

            sw.Stop();
            Console.WriteLine($"Total time taken: {sw.ElapsedMilliseconds}");
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }
    }
}

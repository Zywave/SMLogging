using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SMLogging.Lab.Client
{
    class Program
    {
        static void Main()
        {
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();

            Console.Write("Basic service... ");
            using (var service = new LabServiceProxy())
            {
                var r = service.GetData(1);
            }
            Console.WriteLine("done");

            Console.Write("Enumerable service... ");
            using (var service = new LabServiceProxy())
            {
                var r = service.GetDatas(new[] { 1, 2, 3 });
            }
            Console.WriteLine("done");

            Console.Write("Async service... ");
            using (var service = new LabServiceProxy())
            {
                var r = service.GetData2(1).Result;
            }
            Console.WriteLine("done");

            Console.Write("One-way service... ");
            using (var service = new LabServiceProxy())
            {
                service.DoSomething(1);
            }
            Console.WriteLine("done");

            Console.Write("Faulting service... ");
            using (var service = new LabServiceProxy())
            {
                try
                {
                    service.Fail();
                }
                catch { }
            }
            Console.WriteLine("done");

            //Console.Write("Streaming service... ");
            //using (var service = new StreamingLabServiceProxy())
            //{
            //    var r = service.GetData(new StreamingRequest() {Value = 1});
            //}
            //Console.WriteLine("done");

            double average, total;
            Console.Write("Performance... ");
            using (var service = new LabServiceProxy())
            {
                var times = new List<long>();
                for (var i = 0; i < 500; i++)
                {
                    var sw = Stopwatch.StartNew();
                    var r = service.GetData2(1).Result;
                    sw.Stop();

                    times.Add(sw.ElapsedMilliseconds);
                }
                average = times.Average();
                total = times.Sum();
            }
            Console.WriteLine("done");
            Console.WriteLine($"  Average:\t{average}");
            Console.WriteLine($"  Total:\t{total}");

            Console.ReadKey();
        }
    }
}

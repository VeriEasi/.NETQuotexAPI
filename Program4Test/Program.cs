using System;
using System.Runtime.Loader;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Program4Test
{
    class Program
    {
        private static readonly ManualResetEvent ExitEvent = new(false);

        static async Task Main(string[] _)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            // Connect to Broker
            QuotexAPI.QuotexAPI q = new("email", "password", true);
            bool con = await Task.Run(() => q.Connect());
            Console.WriteLine("Conenct: " + con);

            // Subscribe Order Messages
            await Task.Run(() => q.SubscribeAccountUpdates(OnOrderMessage));

            // Open Order
            JsonObject oo = await Task.Run(() => q.PlaceOrder("EURUSD", 20, 60, "put"));
            Console.WriteLine(oo.ToString());
            await Task.Delay(3000);

            // Cancel Oredr
            JsonObject co = await Task.Run(() => q.CancelOrder(oo["id"].ToString()));
            Console.WriteLine(co.ToString());

            /*// Open Pending Order by Price
            JsonObject opp = await Task.Run(() => q.PlacePendingOrderByPrice("EURUSD", 20, 1.02, 60, "put"));
            Console.WriteLine(opp.ToString());
            await Task.Delay(3000);

            // Open Pending Order by Time
            JsonObject opt = await Task.Run(() => q.PlacePendingOrderByTime("EURUSD", 20, new DateTime(2022, 07, 11, 20, 10, 00), 60, "put"));
            Console.WriteLine(opt.ToString());
            //await Task.Delay(3000);

            // Cancel Pending Order
            JsonObject cpo = await Task.Run(() => q.CancelPendingOrder(opp["pending"]["ticket"].ToString()));
            Console.WriteLine(cpo.ToString());*/

            ExitEvent.WaitOne();
        }

        static void OnOrderMessage(JsonObject Order)
        {
            Console.WriteLine("Oreder Message:");
            Console.WriteLine(Order.ToString());
            Console.WriteLine("____________________________________");
        }

        private static void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            ExitEvent.Set();
        }

        private static void DefaultOnUnloading(AssemblyLoadContext assemblyLoadContext)
        {
            ExitEvent.Set();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            ExitEvent.Set();
        }
    }
}

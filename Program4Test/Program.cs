using System;
using System.Threading.Tasks;
using QuotexAPI;
using Newtonsoft.Json.Linq;

namespace Program4Test
{
    class Program
    {
        static void OnOrderMessage(JObject Order)
        {
            Console.WriteLine("Oreder Message:");
            Console.WriteLine(Order.ToString());
            Console.WriteLine("____________________________________");
        }

        static async Task Main(string[] _)
        {
            // Connect to Broker
            QuotexAPI.QuotexAPI q = new("aliabtahi.v@gmail.com", "AghaAli1372", AccountType.Demo);
            bool con = await Task.Run(() => q.Connect());
            Console.WriteLine("Conenct: " + con);

            // Subscribe Order Messages
            //await Task.Run(() => q.SubscribeOrders(OnOrderMessage));

            // Open Order
            JObject oo = await Task.Run(() => q.OpenOrder("EURUSD", 20, 60, "put"));
            Console.WriteLine(oo.ToString());
            await Task.Delay(3000);

            // Cancel Oredr
            JObject co = await Task.Run(() => q.CancelOrder(oo["id"].ToString()));
            Console.WriteLine(co.ToString());

            // Open Pending Order by Price
            JObject opp = await Task.Run(() => q.OpenPendingOrderByPrice("EURUSD", 20, 1.02, 60, "put"));
            Console.WriteLine(opp.ToString());
            await Task.Delay(3000);

            // Open Pending Order by Time
            JObject opt = await Task.Run(() => q.OpenPendingOrderByTime("EURUSD", 20, new DateTime(2022,07,11,20,10,00), 60, "put"));
            Console.WriteLine(opt.ToString());
            //await Task.Delay(3000);

            // Cancel Pending Order
            JObject cpo = await Task.Run(() => q.CancelPendingOrder(opp["pending"]["ticket"].ToString()));
            Console.WriteLine(cpo.ToString());

            await Task.Delay(100000);
        }
    }
}

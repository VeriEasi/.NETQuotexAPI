using Newtonsoft.Json.Linq;
using System.Threading;

namespace QuotexAPI
{
    class OpenOrderRequest
    {
        public ManualResetEventSlim ResetEvent;
        public long RequestID { get; set; }
        public JObject ResponseData { get; set; }
        public OpenOrderRequest()
        {
            ResetEvent = new();
        }
    }

    class OpenPendingRequest
    {
        public ManualResetEventSlim ResetEvent;
        public JObject ResponseData { get; set; }
        public OpenPendingRequest()
        {
            ResetEvent = new();
        }
    }

    class CancelOrderRequest
    {
        public ManualResetEventSlim ResetEvent;
        public string OrderTicket { get; set; }
        public JObject ResponseData { get; set; }
        public CancelOrderRequest()
        {
            ResetEvent = new();
        }
    }
}
using System.Text.Json.Nodes;
using System.Threading;

namespace QuotexAPI.WebSocket
{
    class OpenOrderRequest
    {
        public ManualResetEventSlim ResetEvent;
        public long RequestID { get; set; }
        public JsonObject ResponseData { get; set; }
        public OpenOrderRequest()
        {
            ResetEvent = new();
        }
    }

    class OpenPendingRequest
    {
        public ManualResetEventSlim ResetEvent;
        public JsonObject ResponseData { get; set; }
        public OpenPendingRequest()
        {
            ResetEvent = new();
        }
    }

    class CancelOrderRequest
    {
        public ManualResetEventSlim ResetEvent;
        public string OrderTicket { get; set; }
        public JsonObject ResponseData { get; set; }
        public CancelOrderRequest()
        {
            ResetEvent = new();
        }
    }
}
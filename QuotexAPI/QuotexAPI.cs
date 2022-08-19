using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace QuotexAPI
{
    public class QuotexAPI
    {
        // Private Fields:
        private QuotexWebsocketClient WebsocketClient { get; }
        private QuotexHTTPClient HTTPClient { get; }
        private AccountType IsDemo { get; }

        // Constructor:
        public QuotexAPI(string Username, string Password, AccountType IsDemo)
        {
            WebsocketClient = new(Username, IsDemo);
            HTTPClient = new(Username, Password);
            this.IsDemo = IsDemo;
        }

        // Public Methods:
        public async Task<bool> Connect()
        {
            string SSID = HTTPClient.GetSSID();
            WebsocketClient.SetSSID(SSID);
            WebsocketClient.IsConnect = false;

            await Task.Run(() => WebsocketClient.Start());
            WebsocketClient.ConnectEvent.Wait(10000);

            if (WebsocketClient.ConnectEvent.IsSet) return true;
            else return false;
        }

        public async Task<JObject> OpenOrder(string Asset, double Amount, int Time, string Action)
        {
            WebsocketClient.OpenOrderReq.ResetEvent.Reset();
            long reqID =new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            WebsocketClient.OpenOrderReq.RequestID = reqID;

            string NewOrder = @"42[""orders/open"",{""asset"":""" + Asset +
                @""",""amount"":" + Amount +
                @",""time"":" + Time +
                @",""action"":""" + Action +
                @""",""isDemo"":" + (int)IsDemo +
                @",""requestId"":" + reqID +
                @",""optionType"":100}]";
            await Task.Run(() => WebsocketClient.SendWebsocketMessage(NewOrder));

            WebsocketClient.OpenOrderReq.ResetEvent.Wait(5000);
            if (WebsocketClient.OpenOrderReq.ResetEvent.IsSet)
                return WebsocketClient.OpenOrderReq.ResponseData;
            else
            {
                WebsocketClient.OpenOrderReq.ResetEvent.Set();
                return new JObject { { "error", "Timeout" } };
            }
        }

        public async Task<JObject> CancelOrder(string Ticket)
        {
            WebsocketClient.CancelTradeReq.ResetEvent.Reset();
            WebsocketClient.CancelTradeReq.OrderTicket = Ticket;

            string NewOrder = @"42[""orders/cancel"",{""ticket"":""" + Ticket + @"""}]";
            await Task.Run(() => WebsocketClient.SendWebsocketMessage(NewOrder));

            WebsocketClient.CancelTradeReq.ResetEvent.Wait(5000);
            if (WebsocketClient.CancelTradeReq.ResetEvent.IsSet)
                return WebsocketClient.CancelTradeReq.ResponseData;
            else
            {
                WebsocketClient.CancelTradeReq.ResetEvent.Set();
                return new JObject { { "error", "Timeout" } };
            }
        }

        public async Task<JObject> OpenPendingOrderByPrice(string Asset, double Amount, double OpenPrice, int Timeframe, string Action)
        {
            WebsocketClient.OpenPendingReq.ResetEvent.Reset();

            string NewOrder = @"42[""pending/create"",{""openType"":" + 1 +
                @",""asset"":""" + Asset +
                @""",""openPrice"":""" + OpenPrice +
                @""",""timeframe"":" + Timeframe +
                @",""command"":""" + Action +
                @""",""amount"":" + Amount + @"}]";
            await Task.Run(() => WebsocketClient.SendWebsocketMessage(NewOrder));

            WebsocketClient.OpenPendingReq.ResetEvent.Wait(5000);
            if (WebsocketClient.OpenPendingReq.ResetEvent.IsSet)
                return WebsocketClient.OpenPendingReq.ResponseData;
            else
            {
                WebsocketClient.OpenPendingReq.ResetEvent.Set();
                return new JObject { { "error", "Timeout" } };
            }
        }

        public async Task<JObject> OpenPendingOrderByTime(string Asset, double Amount, DateTime OpenTime, int Timeframe, string Action)
        {
            WebsocketClient.OpenPendingReq.ResetEvent.Reset();

            string NewOrder = @"42[""pending/create"",{""openType"":" + 0 +
                @",""asset"":""" + Asset +
                @""",""openPrice"":""" + OpenTime +
                @""",""timeframe"":" + Timeframe +
                @",""command"":""" + Action +
                @""",""amount"":" + Amount + @"}]";
            await Task.Run(() => WebsocketClient.SendWebsocketMessage(NewOrder));

            WebsocketClient.OpenPendingReq.ResetEvent.Wait(5000);
            if (WebsocketClient.OpenPendingReq.ResetEvent.IsSet)
                return WebsocketClient.OpenPendingReq.ResponseData;
            else
            {
                WebsocketClient.OpenPendingReq.ResetEvent.Set();
                return new JObject { { "error", "Timeout" } };
            }
        }

        public async Task<JObject> CancelPendingOrder(string Ticket)
        {
            WebsocketClient.CancelPendingReq.ResetEvent.Reset();
            WebsocketClient.CancelPendingReq.OrderTicket = Ticket;

            string NewOrder = @"42[""pending/cancel"",{""ticket"":""" + Ticket + @"""}]";
            await Task.Run(() => WebsocketClient.SendWebsocketMessage(NewOrder));

            WebsocketClient.CancelPendingReq.ResetEvent.Wait(5000);
            if (WebsocketClient.CancelPendingReq.ResetEvent.IsSet)
                return WebsocketClient.CancelPendingReq.ResponseData;
            else
            {
                WebsocketClient.CancelPendingReq.ResetEvent.Set();
                return new JObject { { "error", "Timeout" } };
            }
        }

        public async Task SubscribeOrders(Action<JObject> OnOrderMessage)
        {
            await Task.Run(() => WebsocketClient.SubscribeOrders(OnOrderMessage));
        }
    }
}
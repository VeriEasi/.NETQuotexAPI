using QuotexAPI.Enum;
using QuotexAPI.HTTP;
using QuotexAPI.WebSocket;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace QuotexAPI
{
    public class QuotexAPI
    {
        // Private Fields:
        private QuotexHTTPClient HTTPClient { get; }
        private QuotexWebsocketClient WebSocketClient { get; }
        private AccountType Type { get; }

        // Constructor:
        public QuotexAPI(string Username, string Password, bool IsDemo)
        {
            Type = IsDemo ? AccountType.Demo : AccountType.Real;
            WebSocketClient = new(Username, Type);
            HTTPClient = new(Username, Password);
        }

        // Public Methods:
        public async Task<bool> Connect()
        {
            string SSID = HTTPClient.GetSSID().Result;
            WebSocketClient.SetSSID(SSID);
            WebSocketClient.IsConnect = false;

            await Task.Run(() => WebSocketClient.Start());
            WebSocketClient.ConnectEvent.Wait(10000);

            if (WebSocketClient.ConnectEvent.IsSet) return true;
            else return false;
        }

        public async Task<JsonObject> PlaceOrder(string Asset, double Amount, int Time, string Action)
        {
            WebSocketClient.OpenOrderReq.ResetEvent.Reset();
            long reqID =new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            WebSocketClient.OpenOrderReq.RequestID = reqID;

            string NewOrder = @"42[""orders/open"",{""asset"":""" + Asset +
                @""",""amount"":" + Amount +
                @",""time"":" + Time +
                @",""action"":""" + Action +
                @""",""isDemo"":" + (int)Type +
                @",""requestId"":" + reqID +
                @",""optionType"":100}]";
            await Task.Run(() => WebSocketClient.SendWebsocketMessage(NewOrder));

            WebSocketClient.OpenOrderReq.ResetEvent.Wait(5000);
            if (WebSocketClient.OpenOrderReq.ResetEvent.IsSet)
                return WebSocketClient.OpenOrderReq.ResponseData;
            else
            {
                WebSocketClient.OpenOrderReq.ResetEvent.Set();
                return new JsonObject { { "error", "Timeout" } };
            }
        }

        public async Task<JsonObject> CancelOrder(string Ticket)
        {
            WebSocketClient.CancelTradeReq.ResetEvent.Reset();
            WebSocketClient.CancelTradeReq.OrderTicket = Ticket;

            string NewOrder = @"42[""orders/cancel"",{""ticket"":""" + Ticket + @"""}]";
            await Task.Run(() => WebSocketClient.SendWebsocketMessage(NewOrder));

            WebSocketClient.CancelTradeReq.ResetEvent.Wait(5000);
            if (WebSocketClient.CancelTradeReq.ResetEvent.IsSet)
                return WebSocketClient.CancelTradeReq.ResponseData;
            else
            {
                WebSocketClient.CancelTradeReq.ResetEvent.Set();
                return new JsonObject { { "error", "Timeout" } };
            }
        }

        public async Task<JsonObject> PlacePendingOrderByPrice(string Asset, double Amount, double OpenPrice, int Timeframe, string Action)
        {
            WebSocketClient.OpenPendingReq.ResetEvent.Reset();

            string NewOrder = @"42[""pending/create"",{""openType"":" + 1 +
                @",""asset"":""" + Asset +
                @""",""openPrice"":""" + OpenPrice +
                @""",""timeframe"":" + Timeframe +
                @",""command"":""" + Action +
                @""",""amount"":" + Amount + @"}]";
            await Task.Run(() => WebSocketClient.SendWebsocketMessage(NewOrder));

            WebSocketClient.OpenPendingReq.ResetEvent.Wait(5000);
            if (WebSocketClient.OpenPendingReq.ResetEvent.IsSet)
                return WebSocketClient.OpenPendingReq.ResponseData;
            else
            {
                WebSocketClient.OpenPendingReq.ResetEvent.Set();
                return new JsonObject { { "error", "Timeout" } };
            }
        }

        public async Task<JsonObject> PlacePendingOrderByTime(string Asset, double Amount, DateTime OpenTime, int Timeframe, string Action)
        {
            WebSocketClient.OpenPendingReq.ResetEvent.Reset();

            string NewOrder = @"42[""pending/create"",{""openType"":" + 0 +
                @",""asset"":""" + Asset +
                @""",""openPrice"":""" + OpenTime +
                @""",""timeframe"":" + Timeframe +
                @",""command"":""" + Action +
                @""",""amount"":" + Amount + @"}]";
            await Task.Run(() => WebSocketClient.SendWebsocketMessage(NewOrder));

            WebSocketClient.OpenPendingReq.ResetEvent.Wait(5000);
            if (WebSocketClient.OpenPendingReq.ResetEvent.IsSet)
                return WebSocketClient.OpenPendingReq.ResponseData;
            else
            {
                WebSocketClient.OpenPendingReq.ResetEvent.Set();
                return new JsonObject { { "error", "Timeout" } };
            }
        }

        public async Task<JsonObject> CancelPendingOrder(string Ticket)
        {
            WebSocketClient.CancelPendingReq.ResetEvent.Reset();
            WebSocketClient.CancelPendingReq.OrderTicket = Ticket;

            string NewOrder = @"42[""pending/cancel"",{""ticket"":""" + Ticket + @"""}]";
            await Task.Run(() => WebSocketClient.SendWebsocketMessage(NewOrder));

            WebSocketClient.CancelPendingReq.ResetEvent.Wait(5000);
            if (WebSocketClient.CancelPendingReq.ResetEvent.IsSet)
                return WebSocketClient.CancelPendingReq.ResponseData;
            else
            {
                WebSocketClient.CancelPendingReq.ResetEvent.Set();
                return new JsonObject { { "error", "Timeout" } };
            }
        }

        public async Task<bool> SubscribeAccountUpdates(Action<JsonObject> OnOrderUpdate)
        {
            await Task.Run(() => WebSocketClient.SubscribeOrders(OnOrderUpdate));
            return true;
        }
    }
}
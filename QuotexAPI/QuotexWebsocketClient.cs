using System.Net.WebSockets;
using Websocket.Client;
using Serilog;
using System.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace QuotexAPI
{
    class QuotexWebsocketClient
    {
        // Private Fields
        private static readonly Uri URL = new("wss://ws.qxbroker.com/socket.io/?EIO=3&transport=websocket");
        private static readonly Func<ClientWebSocket> Factory = new(() =>
        {
            var Client = new ClientWebSocket { Options = { KeepAliveInterval = TimeSpan.FromSeconds(5), } };

            Client.Options.SetRequestHeader("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:100.0) Gecko/20100101 Firefox/100.0");
            Client.Options.SetRequestHeader("Origin", "https://quotex.com");
            return Client;
        });

        // Properties
        private IWebsocketClient ClientWebsocket { get; }
        private string Name { get; }
        private AccountType IsDemo { get;  }
        private string SSIDToken { get; set; }
        private SocketState WebsocketState { get; set; }
        private Action<JObject> OnOrderMessage { get; set; }
        private Task PingTask { get; set; }
        public bool IsConnect { get; set; }
        public bool IsOrderSubscribe { get; set; }

        // Request Handlers
        public ManualResetEventSlim ConnectEvent = new(false);
        public OpenOrderRequest OpenOrderReq = new();
        public OpenPendingRequest OpenPendingReq = new();
        public CancelOrderRequest CancelTradeReq = new();
        public CancelOrderRequest CancelPendingReq = new();

        // Private Methods
        private async Task OnMessage(ResponseMessage Message)
        {
            //Console.WriteLine(Message);
            switch (WebsocketState)
            {
                case SocketState.Idle:
                    // Ready to Authorization
                    if (Message.Text == "40")
                    {
                        ConnectEvent.Reset();
                        IsConnect = false;
                        string session = @"42[""authorization"",{""session"":""" + SSIDToken + @""",""isDemo"":" + (int)IsDemo + @"}]";
                        await SendWebsocketMessage(session);
                    }

                    // Authorization Succesfull
                    else if (Message.ToString().Contains(@"42[""s_authorization""]"))
                    {
                        ConnectEvent.Reset();
                        IsConnect = false;
                        if (PingTask != null)
                        {
                            PingTask.Wait();
                            PingTask.Dispose();
                        }
                        ConnectEvent.Set();
                        IsConnect = true;
                        PingTask = Task.Run(() => StartSendingPing());
                    }

                    // Incoming Open Order Message
                    else if (Message.ToString().Contains(@"451-[""s_orders/open"",{""_placeholder"":true,""num"":0}]") ||
                        Message.ToString().Contains(@"451-[""f_orders/open"",{""_placeholder"":true,""num"":0}]"))
                        WebsocketState = SocketState.OpenOrderWait;

                    // Incoming Cancel Order Message
                    else if (Message.ToString().Contains(@"451-[""s_orders/cancel"",{""_placeholder"":true,""num"":0}]") ||
                        Message.ToString().Contains(@"451-[""f_orders/cancel"",{""_placeholder"":true,""num"":0}]"))
                        WebsocketState = SocketState.CancelOrderWait;

                    // Incoming Open Pending Order Message
                    else if (Message.ToString().Contains(@"451-[""s_pending/create"",{""_placeholder"":true,""num"":0}]") ||
                        Message.ToString().Contains(@"451-[""f_pending/create"",{""_placeholder"":true,""num"":0}]"))
                        WebsocketState = SocketState.OpenPndingOrderWait;

                    // Incoming Cancel Pending Order Message
                    else if (Message.ToString().Contains(@"451-[""s_pending/cancel"",{""_placeholder"":true,""num"":0}]") ||
                        Message.ToString().Contains(@"451-[""f_pending/cancel"",{""_placeholder"":true,""num"":0}]"))
                        WebsocketState = SocketState.CancelPendingOrderWait;

                    break;

                // Open Order Message
                case SocketState.OpenOrderWait:
                    if (Message.MessageType == WebSocketMessageType.Binary)
                    {
                        try
                        {
                            string strData = Encoding.Default.GetString(Message.Binary);
                            JObject js = JObject.Parse(strData.Remove(0, 1));
                            BroadcastOrder(js);

                            if (!OpenOrderReq.ResetEvent.IsSet)
                            {
                                if ((long)js["requestId"] == OpenOrderReq.RequestID)
                                {
                                    OpenOrderReq.ResponseData = js;
                                    OpenOrderReq.ResetEvent.Set();
                                }
                            }
                        }
                        catch { }
                    }
                    WebsocketState = SocketState.Idle;
                    break;

                // Cancel Order Message
                case SocketState.CancelOrderWait:
                    if (Message.MessageType == WebSocketMessageType.Binary)
                    {
                        try
                        {
                            string strData = Encoding.Default.GetString(Message.Binary);
                            JObject js = JObject.Parse(strData.Remove(0, 1));
                            BroadcastOrder(js);

                            if (!CancelTradeReq.ResetEvent.IsSet)
                            {
                                if ((string)js["ticket"] == CancelTradeReq.OrderTicket)
                                {
                                    CancelTradeReq.ResponseData = js;
                                    CancelTradeReq.ResetEvent.Set();
                                }
                            }
                        }
                        catch { }
                    }
                    WebsocketState = SocketState.Idle;
                    break;

                // Open Pending Order Message
                case SocketState.OpenPndingOrderWait:
                    if (Message.MessageType == WebSocketMessageType.Binary)
                    {
                        try
                        {
                            string strData = Encoding.Default.GetString(Message.Binary);
                            JObject js = JObject.Parse(strData.Remove(0, 1));
                            BroadcastOrder(js);

                            if (!OpenPendingReq.ResetEvent.IsSet)
                            {
                                OpenPendingReq.ResponseData = js;
                                OpenPendingReq.ResetEvent.Set();
                            }
                        }
                        catch { }
                    }
                    WebsocketState = SocketState.Idle;
                    break;

                // Cancel Pending Order Message
                case SocketState.CancelPendingOrderWait:
                    if (Message.MessageType == WebSocketMessageType.Binary)
                    {
                        try
                        {
                            string strData = Encoding.Default.GetString(Message.Binary);
                            JObject js = JObject.Parse(strData.Remove(0, 1));
                            BroadcastOrder(js);

                            if (!CancelPendingReq.ResetEvent.IsSet)
                            {
                                if ((string)js["ticket"] == CancelPendingReq.OrderTicket)
                                {
                                    CancelPendingReq.ResponseData = js;
                                    CancelPendingReq.ResetEvent.Set();
                                }
                            }
                        }
                        catch { }
                    }
                    WebsocketState = SocketState.Idle;
                    break;

                // Default
                default:
                    WebsocketState = SocketState.Idle;
                    break;
            }
        }

        // Constructor
        public QuotexWebsocketClient(string Name, AccountType IsDemo)
        {
            this.Name = Name;
            this.IsDemo = IsDemo;
            WebsocketState = SocketState.Idle;

            ClientWebsocket = new WebsocketClient(URL, Factory)
            {
                Name = this.Name,
                ReconnectTimeout = TimeSpan.FromSeconds(30),
                ErrorReconnectTimeout = TimeSpan.FromSeconds(30)
            };
            ClientWebsocket.ReconnectionHappened.Subscribe(info =>
                Log.Information($"Reconnection happened " + Name + $", Type: {info.Type}, URL: {ClientWebsocket.Url}"));
            ClientWebsocket.DisconnectionHappened.Subscribe(info =>
                Log.Warning($"Disconnection happened " + Name + $", Type: {info.Type}"));
            ClientWebsocket.MessageReceived.Subscribe(async Message => await OnMessage(Message));
        }

        // Public Methods
        public void Start()
        {
            ClientWebsocket.Start();
        }

        public void SetSSID(string Token)
        {
            SSIDToken = Token;
        }

        public void SubscribeOrders(Action<JObject> OnOrderMessage)
        {
            IsOrderSubscribe = true;
            this.OnOrderMessage = OnOrderMessage;
        }

        public async Task SendWebsocketMessage(string Message)
        {
            if (ClientWebsocket != null) await Task.Run(() => ClientWebsocket.Send(Message));
        }

        // Private Methods
        private void BroadcastOrder(JObject Order)
        {
            if (IsOrderSubscribe) OnOrderMessage(Order);
        }

        private async Task StartSendingPing()
        {
            while (IsConnect)
            {
                await Task.Delay(5000);
                if (!ClientWebsocket.IsRunning) continue;
                await SendWebsocketMessage(@"42[""tick""]");
            }
        }
    }
}
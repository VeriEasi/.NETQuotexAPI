using HtmlAgilityPack;
using RestSharp;
using System;

namespace QuotexAPI
{
    class QuotexHTTPClient
    {
        // Private Fields
        readonly RestClient client = new("https://qxbroker.com/en");
        private string Username { get; }
        private string Password { get; }

        // Constructor
        public QuotexHTTPClient(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }

        // Public Methods
        public string GetSSID()
        {
            // Get Site Token
            var request1 = new RestRequest("/sign-in/");
            var response1 = client.Get(request1);
            HtmlDocument doc1 = new();
            doc1.LoadHtml(response1.Content);

            string _token = "";
            foreach (HtmlNode node in doc1.DocumentNode.SelectNodes("//input"))
            {
                var name = node.Attributes["name"].Value;
                if (name == "_token")
                {
                    _token = node.Attributes["value"].Value;
                    break;
                }
            }

            // Login
            var request2 = new RestRequest("/sign-in/");
            request2.AddParameter("email", Username);
            request2.AddParameter("password", Password);
            request2.AddParameter("remember", "1");
            request2.AddParameter("_token", _token.ToString());
            var response2 = client.Post(request2);

            // Check for Keep Code
            if (response2.Content.Contains(@"""keep_code"""))
            {
                Console.WriteLine("Need to Get Verification Code From Email!");

                string _keep_code = "";
                HtmlDocument doc2 = new();
                doc2.LoadHtml(response2.Content);
                foreach (HtmlNode node in doc2.DocumentNode.SelectNodes("//input"))
                {
                    var name = node.Attributes["name"].Value;
                    if (name == "keep_code")
                    {
                        Console.WriteLine("Please Enter Verification Code: ");
                        _keep_code = Console.ReadLine();
                        break;
                    }
                }

                var request3 = new RestRequest("/sign-in/");
                request3.AddParameter("email", Username);
                request3.AddParameter("password", Password);
                request3.AddParameter("remember", "1");
                request3.AddParameter("_token", _token.ToString());
                request3.AddParameter("keep_code", "1");
                request3.AddParameter("code", _keep_code.ToString());
                var response3 = client.Post(request3);
            }

            // Get Session Token (Needed For Websocket)
            var request4 = new RestRequest("/trade");
            var response4 = client.Get(request4);
            HtmlDocument doc4 = new();
            doc4.LoadHtml(response4.Content);

            string SSID = "";
            foreach (HtmlNode node in doc4.DocumentNode.SelectNodes("//script"))
            {
                if (node.InnerHtml.Contains("token"))
                {
                    int SIn = node.InnerHtml.IndexOf("token") + 8;
                    int EIn = SIn + 40;
                    SSID = node.InnerHtml[SIn..EIn];
                    break;
                }
            }
            return SSID;
        }
    }
}

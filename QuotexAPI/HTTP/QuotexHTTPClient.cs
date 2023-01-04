using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuotexAPI.HTTP
{
    class QuotexHTTPClient
    {
        // Private Fields
        private readonly HttpClient Client = new();
        private string Username { get; }
        private string Password { get; }

        // Constructor
        public QuotexHTTPClient(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }

        // Public Methods
        public async Task<string> GetSSID()
        {
            var HTTPRequest01 = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Version = HttpVersion.Version20,
                RequestUri = new Uri("https://qxbroker.com/en/sign-in"),
                Headers =
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36" }
                }
            };
            string HTTPResponse01 = await (await Client.SendAsync(HTTPRequest01)).Content.ReadAsStringAsync();

            // Get Site Token
            string Token = String.Empty;
            HtmlDocument HTMLDoc01 = new();
            HTMLDoc01.LoadHtml(HTTPResponse01);
            foreach (HtmlNode node in HTMLDoc01.DocumentNode.SelectNodes("//input"))
            {
                var name = node.Attributes["name"].Value;
                if (name == "_token")
                {
                    Token = node.Attributes["value"].Value;
                    break;
                }
            }
            if (Token == String.Empty) return String.Empty;     // In Case of Any Error

            // Login
            var Params = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("_token", Token.ToString()),
                new KeyValuePair<string, string>("email", Username),
                new KeyValuePair<string, string>("password", Password),
                new KeyValuePair<string, string>("remember", "1")
            };
            var HTTPRequest02 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Version = HttpVersion.Version20,
                RequestUri = new Uri("https://qxbroker.com/en/sign-in/"),
                Headers =
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36" }
                },
                Content = new FormUrlEncodedContent(Params)
            };
            string HTTPResponse02 = await (await Client.SendAsync(HTTPRequest02)).Content.ReadAsStringAsync();

            // Check for Keep Code
            /*if (HTTPResponse02.Contains(@"""keep_code"""))
            {
                Console.WriteLine("Need to Get Verification Code From Email!");

                string KeepCode = "";
                HtmlDocument HTMLDoc02 = new();
                HTMLDoc02.LoadHtml(HTTPResponse02);
                foreach (HtmlNode node in HTMLDoc02.DocumentNode.SelectNodes("//input"))
                {
                    var name = node.Attributes["name"].Value;
                    if (name == "keep_code")
                    {
                        Console.WriteLine("Please Enter Verification Code: ");
                        KeepCode = Console.ReadLine();
                        break;
                    }
                }
                var Params03 = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("_token", Token.ToString()),
                    new KeyValuePair<string, string>("email", Username),
                    new KeyValuePair<string, string>("password", Password),
                    new KeyValuePair<string, string>("remember", "1"),
                    new KeyValuePair<string, string>("keep_code", "1"),
                    new KeyValuePair<string, string>("code", KeepCode)
                };
                var HTTPRequest03 = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Version = HttpVersion.Version20,
                    RequestUri = new Uri("https://qxbroker.com/en/sign-in"),
                    Headers =
                    {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36" }
                    },
                    Content = new FormUrlEncodedContent(Params03)
                };
                var _ = await Client.SendAsync(HTTPRequest03);
            }*/

            // Get Session Token (Needed For Websocket)
            string SSID = String.Empty;
            var HTTPRequest04 = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Version = HttpVersion.Version20,
                RequestUri = new Uri("https://qxbroker.com/en/trade"),
                Headers =
                {
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36" }
                }
            };
            string HTTPResponse04 = await (await Client.SendAsync(HTTPRequest04)).Content.ReadAsStringAsync();
            HtmlDocument HTMLDoc04 = new();
            HTMLDoc04.LoadHtml(HTTPResponse04);

            foreach (HtmlNode node in HTMLDoc04.DocumentNode.SelectNodes("//script"))
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

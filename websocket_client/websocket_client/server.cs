using WebSocketSharp;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace ClientServer
{
    class WebsocketClientServer
    {
        private const string HOST = "0.0.0.0:8000";
        private WebSocket ws;
        public bool IsAlive => ws.IsAlive;


        public WebsocketClientServer(Settings settings)
        {
            string url = $"ws://{HOST}/server?key={settings.ApiKey}";
            ws = new WebSocket(url);
            WebSocketSharp.Net.Cookie cookie = new("secret-key", settings.ApiSecret);
            ws.SetCookie(cookie);
        }


        public async Task Run() => await Task.Run(() => { ws.Connect(); Console.WriteLine("wefw"); });

        private static Dictionary<string, dynamic> ConvertJson(string jsonString)
        {
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
            var dic = json ?? throw new JsonException();
            int index;
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            foreach (KeyValuePair<string, string> kvp in dic)
            {
                if (dic != null)
                {
                    index = kvp.Value.IndexOf("::"); // id::int
                    if (index > 0)
                    {
                        string type = kvp.Value.Substring(index + 2);
                        if (type == "int")
                        {
                            try
                            {
                                result.Add(kvp.Key[..index], Convert.ToInt32(kvp.Value));
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine(e);
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }
                        else if (type == "float")
                        {
                            try
                            {
                                result.Add(kvp.Key[..index], Convert.ToSingle(kvp.Value));
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine(e);
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }
                        else if (type == "array")
                        {
                            try
                            {
                                result.Add(kvp.Key[..index], kvp.Value.Split(","));
                            }
                            catch
                            {
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }
                        else
                        {
                            result.Add(kvp.Key, kvp.Value);
                        }
                    }
                    else
                    {
                        result.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            return result;
        }
    }
}

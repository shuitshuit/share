
using System.Text.Json;
using System.IO;


namespace ClientServer
{
    class Settings
    {
#pragma warning disable CS8618
        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }
#pragma warning restore CS8618
    }


    class Program
    {
        static void Main(string[] args)
        {
            string jsonString = File.ReadAllText("C:\\Users\\shuitt\\ドキュメント\\programing\\share\\websocket_client\\websocket_client\\setting.json");
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString) ?? throw new Exception();
            WebsocketClientServer ws = new(settings);
            var server = ws.RunAsync();
            bool serverRun = true;
            while (serverRun)
            {
                if (server.IsCompleted)
                {
                    serverRun = false;
                }else
                {
                    Console.WriteLine("-");
                }
            }
            _ = ws.SendAsync("sfvdfbd");
            var img = File.ReadAllBytes("C:\\Users\\shuitt\\Music\\『ヴィラン』3DMVゲームサイズ公開！.flac");
            _ = ws.SendAsync(img);
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
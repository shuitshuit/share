
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
            string jsonString = File.ReadAllText("C:\\Users\\shuit\\Documents\\programing\\share\\websocket_client\\websocket_client\\setting.json");
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString)  ?? throw new Exception();
            WebsocketClientServer ws = new(settings);
            var server = ws.Run();
            bool serverRun = true;
            while (serverRun)
            {
                if (server.IsCompleted)
                {
                    serverRun = false;
                }else
                {
                    Console.WriteLine("a");
                }
            }
            Console.WriteLine(ws.IsAlive);
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
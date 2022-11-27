using Fleck;
using Npgsql;

namespace ClientServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:8000");


            server.Start(socket =>
            {
                socket.OnOpen = () => OnOpen(socket);
                socket.OnClose = () => OnClose(socket);
                socket.OnMessage = message => OnMessage(socket, message);
                socket.OnBinary = bytes => OnBinary(socket, bytes);
            });

            string? data = "";
            while (true)
            {
                data = Console.ReadLine();
                if (data == null)
                {
                    continue;
                }
                else if (data.StartsWith("exit"))
                {
                    SendMessageToAll(data);
                    break;
                }
                else
                {
                    SendMessageToAll(data);
                }
            }

            server.Dispose();
        }
    }
}

using System.Text;

namespace ClientServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //var ws = new WebsocketClientServer();
            //Console.WriteLine(ws.IsAvaible);
            var key = Encoding.UTF8.GetBytes("msncpyjsyxduxqiqoealpwmceylejckl");
            var iv = Encoding.UTF8.GetBytes("uddtweocbbrhdjhv");
            Console.WriteLine(iv.Length);
            var cry = WebsocketClientServer.Encrypt("admin", iv, key);
            foreach (var i in cry)
            {
                Console.WriteLine(Convert.ToString(i));
            }
        }
    }
}
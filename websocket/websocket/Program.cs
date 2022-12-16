using Fleck;
using Npgsql;
using System.Text;

namespace WebSocketServer
{
    class Program
    {
        private static object lockObj = new object();
        static List<IWebSocketConnection>  sockets = new();
        private static Dictionary<string, IWebSocketConnection> servers = new();
        private const string URI = "Server=localhost;Port=5433;Username=postgres;Password=2YQeEq7ZEysr5Zzm4KStKFZkuXtkPM;Database=share";
        private const string HOME = "Server=localhost;Port=5433;Username=postgres;Password=2YQeEq7ZEysr5Zzm4KStKFZkuXtkPM;Database=test";
        private static string[] paths = {"/upload", "/client", "/server"};



        static void Main(string[] args)
        {
            var server = new Fleck.WebSocketServer("ws://0.0.0.0:8000");


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
            foreach (IWebSocketConnection s in sockets)
            {
                s.Close();
            }


            server.Dispose();
        }


        static async void OnOpen(IWebSocketConnection socket)
        {
            lock (lockObj)
            {
                sockets.Add(socket);
            }
            var info = socket.ConnectionInfo; // user information
            try
            {
                var value = ParametersToDictionary(info.Path);
                (bool available, string userId) result;
                string[]? certification = { info.Cookies["secret-key"], info.Headers["secret-key"], info.Headers["Sec-WebSocket-Protocol"], "-last-" };
                foreach (var i in certification)
                {
                    result = CheckKey(value["key"], i);
                    if (result.available)
                    {
                        value.Add("user_id", result.userId);
                        break;
                    }
                    else
                    {
                        if (i == "-last-")
                        {
                            Console.WriteLine($"authentication failure: {info.ClientIpAddress}");
                            socket.Close();
                            return;

                        }else
                        {
                            continue;
                        }

                    }
                }
                if (info.Path.StartsWith("/server?"))
                {
                    /*
                        /server?key=(api key)
                    */
                    NewServer(socket, value);
                }else
                {
                    foreach (var i in paths)
                    {
                        if (info.Path.StartsWith(i))
                        {
                            Console.WriteLine($"Open: {info.ClientIpAddress} {info.ClientPort}");
                            Console.WriteLine(">> Welcome!!");
                            await socket.Send("Welcome");
                            return;
                        }
                    }
                    socket.Close();
                }
            }catch (ParametersException e)
            {
                Console.WriteLine(e);
                await socket.Send(e.Message);
                socket.Close(400);
            }
        }


        static void OnClose(IWebSocketConnection socket)
        {
            var info = socket.ConnectionInfo;
            sockets.Remove(socket);
            if (info.Path.StartsWith("/server"))
            {
                try
                {
                    Dictionary<string, string> value = ParametersToDictionary(info.Path);
                    var res = CheckKey(value["key"], info.Headers["Sec-WebSocket-Protocol"]);
                    var query = $"select server_name from api_key where key = '{value["key"]}';";
                    string name;
                    using (var connection = new NpgsqlConnection(URI))
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        reader.Read();
                        name = res.user_id + reader.GetString(0);
                    }
                    servers.Remove(name);
                }
                catch (ParametersException e)
                {
                    Console.WriteLine(e.Message);
                    socket.Close();
                }
            }
            Console.WriteLine($"Closed {info.ClientIpAddress} {info.ClientPort}");
        }


        static void OnMessage(IWebSocketConnection socket, string message)
        {
            var info = socket.ConnectionInfo;
            Console.WriteLine(info.Path);
            Console.WriteLine($"Received from {info.ClientIpAddress} {info.ClientPort}: {message}");
        }


        static async void OnBinary(IWebSocketConnection socket, byte[] bytes)
        {
            var info = socket.ConnectionInfo;
            try
            {
                Dictionary<string, string> value = ParametersToDictionary(info.Path);
                (bool available, string user_id) result = CheckKey(value["key"], info.Headers["Sec-WebSocket-Protocol"]);
                value["user_id"] = result.user_id;
                if (result.available)
                {
                    if (info.Path.StartsWith("/upload?"))
                    {
                        // /upload?key=(user key)&name=(server name)&number=(file number)&filename=(file name)
                        Upload(socket, bytes, value);
                    }//else if (info.Path.StartsWith(""))
                }
            }catch (ParametersException e)
            {
                Console.WriteLine(e.Message);
                await socket.Send(e.Message);
            }
        }



        static void SendMessageToAll(string message)
        {
            lock (lockObj)
            {
                // remove unused sockets
                List<IWebSocketConnection> soc = new List<IWebSocketConnection>();
                foreach (var s in sockets)
                {
                    if (s.IsAvailable)
                    {
                        soc.Add(s);
                    }
                    sockets = soc;
                }

                // send message
                foreach (var s in sockets)
                {
                    s.Send(message);
                }
            }
        }


        static void NewServer(IWebSocketConnection socket, Dictionary<string, string> value)
        {
            var query = $"select server_name from api_key where key = '{value["key"]}';";
            string name;
            using (var connection = new NpgsqlConnection(URI))
            using (var command = new NpgsqlCommand(query, connection))
            {
                connection.Open();
                var reader = command.ExecuteReader();
                reader.Read();
                name = value["user_id"] + reader.GetString(0);
            }

            try
            {
                servers[name].Close();
                servers[name] = socket;
            }catch (KeyNotFoundException e)
            {
                Console.WriteLine(e);
                servers.Add(name, socket);
            }catch (ArgumentNullException e)
            {
                Console.WriteLine(e);
                servers[name] = socket;
            }
            socket.Send("join!!");
        }


        static void Upload(IWebSocketConnection socket, byte[] bytes, Dictionary<string, string> value)
        {
            string name = "";
            string[] alphabet = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
            while (true) // Creation of file id
            {
                var random = new Random();
                for (int i = 0; i < 5; i++)
                {
                    var x = random.Next(0, 10);
                    name += x;
                }

                var query = $"select deta from {value["user_id"]} where name = 'share_{name}';";
                using (var connection = new NpgsqlConnection(HOME))
                using (var command = new NpgsqlCommand(query, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        break;
                    }else
                    {
                        continue;
                    }
                }
            }
            var q = $"insert into {value["user_id"]}(name, deta) values('share_{name}', '{value["filename"]}'); ";
            using (var connection = new NpgsqlConnection(HOME))
            using (var command = new NpgsqlCommand(q, connection))
            {
                connection.Open();
                int result = command.ExecuteNonQuery();
            }
            byte[] file = new byte[6 + bytes.Length];
            byte[] fileNumber = {Convert.ToByte(value["number"])};
            List<byte> Id = new();
            for (int i = 0; i < name.Length; i++)
            {
                Id.Add(Convert.ToByte(name[i]));
            }
            byte[] fileId = Id.ToArray();
            socket.Send(fileId);
            Array.Copy(bytes, file, bytes.Length);
            Array.Copy(fileNumber, file, 1);
            Array.Copy(fileId, file, fileId.Length);
            try
            {
                if (servers[value["user_id"] + value["name"]].IsAvailable)
                {
                    servers[value["user_id"]+value["name"]].Send(file);
                }else
                {
                    socket.Send("Server unavailable");
                }
            }catch
            {
                socket.Send("Server not connected");
            }
        }


        static Dictionary<string, dynamic> ConvertJson(Dictionary<string, string> dic)
        {
            int index;
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            foreach (KeyValuePair<string, string> kvp in dic)
            {
                if(dic != null)
                {
                    index = kvp.Value.IndexOf("::"); // id::int
                    if (index > 0)
                    {
                        string type = kvp.Value.Substring(index+2);
                        if (type == "int")
                        {
                            try
                            {
                                result.Add(kvp.Key[..index], Convert.ToInt32(kvp.Value));
                            }
                            catch(FormatException e)
                            {
                                Console.WriteLine(e);
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }else if (type == "float")
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
                        }else if (type == "array")
                        {
                            try
                            {
                                result.Add(kvp.Key[..index], kvp.Value.Split(","));
                            }
                            catch
                            {
                                result.Add(kvp.Key, kvp.Value);
                            }
                        }else
                        {
                            result.Add(kvp.Key, kvp.Value);
                        }
                    }else
                    {
                        result.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            return result;
        }


        static Dictionary<string, string> ParametersToDictionary(string path)
        {
            Dictionary<string, string> parameters = new();
            var s = path.Substring(path.IndexOf("?") + 1).Split("&") ?? throw new ParametersException("parameter missing.");
            // if (s is null) { await socket.Send("parameter missing; api key and server name required;"); return; }
            foreach (var i in s)
            {
                var a = i.Split("=");
                if (a == null){throw new ParametersException("The format is wrong.");}
                if (a.Length == 1){throw new ParametersException("The format is wrong."); }
                parameters.Add(a[0], a[1]);
            }
            return parameters;
        }


        static (bool available, string user_id) CheckKey(string key, string secret)
        {
            string query = $"select user_id from api_key where key = '{key}' and secret_key = '{secret}';";
            using (var connection = new NpgsqlConnection(URI))
            using (var command = new NpgsqlCommand(query, connection))
            {
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader == null)
                {
                    (bool available, string user_id) result = (false, "none");
                    return result;
                }
                reader.Read();
                string user_id = reader.GetString(0);
                if (reader.Read())
                {
                    (bool available, string user_id) res = (false, "none");
                    return res;
                }else
                {
                    (bool available, string user_id) re = (true, user_id);
                    return re;
                }
            }
        }
    }
}

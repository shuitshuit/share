using WebSocketSharp;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System.Net;

namespace ClientServer
{
    class Program
    {
        private const string HOST = "0.0.0.0:8000";


        static void Main(string[] args)
        {
            string jsonString;
            using (StreamReader sr = new StreamReader("C:\\Users\\shuitt\\ドキュメント\\programing\\share\\websocket_client\\websocket_client\\setting.json", Encoding.GetEncoding("UTF-8")))
            {
                jsonString = sr.ReadToEnd();
            }
            var json = ConvertJson(jsonString);
            byte[] aes_key = Encoding.UTF8.GetBytes("msncpyjsyxduxqiqoealpwmceylejckl");
            byte[] aes_iv = Encoding.UTF8.GetBytes("uddtweocbbrhdjhv");
            string key = Decrypt(Encoding.UTF8.GetBytes(json["key"]), aes_iv, aes_key);
            string secretKey = Decrypt(Encoding.UTF8.GetBytes(json["secret-key"]), aes_iv, aes_key);
            string url = $"ws://{HOST}/server?key={key}";
            var ws = new WebSocket(url);
            WebSocketSharp.Net.Cookie cookie = new("secret-key", secretKey);
            ws.SetCookie(cookie);
            ws.Connect();
        }


        static Dictionary<string, dynamic> ConvertJson(string jsonString)
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


        public static byte[] Encrypt(string text, byte[] iv, byte[] key)
        {
            if (text == null || text.Length <= 0) throw new ArgumentException(nameof(text));

            // CBCの場合は、「128bit / 8 = 16byte」の初期ベクトルを用意する
            if (iv.Length != 16) throw new ArgumentException(nameof(iv));

            // AES-256の場合は、「256bit / 8 = 32byte」の鍵を用意する
            if (key.Length != 32) throw new ArgumentException(nameof(key));

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }


        public static string Decrypt(byte[] cipherText, byte[] iv, byte[] key)
        {
            if (cipherText == null || cipherText.Length <= 0) throw new ArgumentException(nameof(cipherText));

            // CBCの場合は、「128bit / 8 = 16byte」の初期ベクトルを用意する
            if (iv.Length != 16) throw new ArgumentException(nameof(iv));

            // AES-256の場合は、「256bit / 8 = 32byte」の鍵を用意する
            if (key.Length != 32) throw new ArgumentException(nameof(key));

            string plaintext = "";

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 128;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;


namespace web.Data
{
    public class User : PageModel
    {
        private static string ID;
        private static string __Name;
        private static string __Email;
        public string Name { get => __Name; }
        public string Email { get => __Email; }
        private static ScriptEngine engine;
        private static ScriptScope scope;


        public User()
        {
            Task python = CreatePyEngineAsync();
            var context = new HttpContextAccessor().HttpContext;
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var client = context.Request;
            Console.WriteLine("w/"+client.Cookies["a"]);
            try
            {
                var cookieId = client.Cookies["user"] ?? throw new CookieNotFoundException(); // null合体演算子を書く
                python.Wait();
                GetUserInfo(cookieId);
            }catch(CookieNotFoundException)
            {
                __Email = "";
                __Name = "";
            }
            
        }


        private static async Task CreatePyEngineAsync()
        {
            await Task.Run(() =>
                {
                    engine = Python.CreateEngine();
                    scope = engine.CreateScope();
                    var source = engine.CreateScriptSourceFromFile("C:\\Users\\shuit\\Documents\\programing\\share\\web\\web\\script.py");
                    DateTime dt = DateTime.Now;
                    Console.WriteLine(dt.ToString("yyyyMMdd"));
                    string datetime = dt.ToString("yyyyMMdd");
                    scope.SetVariable("DateNow", datetime);
                    var libs = new[]
                    {
                        "C:\\Program Files\\IronPython 3.4\\DLLs",
                        "C:\\Program Files\\IronPython 3.4\\Lib",
                        "C:\\Program Files\\IronPython 3.4\\Lib\\site-packages",
                        "C:\\Program Files\\IronPython 3.4",
                        "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\Extensions\\Microsoft\\Python"
                    };
                    engine.SetSearchPaths(libs);
                    engine.ExecuteFile("C:\\Users\\shuit\\Documents\\programing\\share\\web\\web\\script.py");
                });
        }


        private static void GetUserInfo(string cookieId)
        {
            dynamic user = scope.GetVariable("GetUserInfo")(cookieId);
            ID = user.ID;
            __Name= user.Name;
            __Email= user.Email;
        }
    }


    public class CookieNotFoundException : Exception
    {
        public CookieNotFoundException() :base() { }


        public CookieNotFoundException(string message) : base(message) { }

    }
}

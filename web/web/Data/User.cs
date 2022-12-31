using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;


namespace web.Data
{
    public class User : PageModel
    {
        private readonly string ID;
        private readonly string __Name;
        private readonly string __Email;
        public string Name { get => __Name; }
        public string Email { get => __Email; }


        public User()
        {
            var context = new HttpContextAccessor().HttpContext;
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var client = context.Request;
            Console.WriteLine("w/"+client.Cookies["ambb"]);
        }
    }
}

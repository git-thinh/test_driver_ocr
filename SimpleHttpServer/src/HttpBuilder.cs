using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    class HttpBuilder
    {
        public static HttpResponse InternalServerError(IApp app)
        {
            //string content = File.ReadAllText("Resources/Pages/500.html"); 

            return new HttpResponse(app)
            {
                ReasonPhrase = "InternalServerError",
                StatusCode = "500",
                ContentAsUTF8 = "INTERNAL_SERVER_ERROR"
            };
        }

        public static HttpResponse NotFound(IApp app)
        {
            //string content = File.ReadAllText("Resources/Pages/404.html");

            return new HttpResponse(app)
            {
                ReasonPhrase = "NotFound",
                StatusCode = "404",
                ContentAsUTF8 = "NOT_FOUND"
            };
        }
    }
}

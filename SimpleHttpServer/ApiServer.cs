using Newtonsoft.Json;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace SimpleHttpServer
{
    public class ApiServer : ITcpClient
    {
        static string root = ConfigurationManager.AppSettings["ROOT_PATH"];

        static List<Route> routes;
        static HttpServer httpServer;

        static ManualResetEvent _EVENT = new ManualResetEvent(false);
        static OCR_BUF _RESULT = null;

        string SaveImage(string imageUrl)
        {
            try
            {
                string file = Path.GetFileName(imageUrl);
                string fileName = file.Substring(0, file.Length - 4) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

                file = Path.Combine(root, fileName);
                ImageFormat format = ImageFormat.Jpeg;

                WebClient client = new WebClient();
                Stream stream = client.OpenRead(imageUrl);
                Bitmap bitmap; bitmap = new Bitmap(stream);

                if (bitmap != null)
                {
                    bitmap.Save(file, format);
                }

                stream.Flush();
                stream.Close();
                client.Dispose();

                return fileName;
            }
            catch (Exception ex) { }

            return "";
        }

        HttpResponse ___response_ocr(HttpRequest request)
        {
            if (HandlerCallback.OcrRunning)
                return new HttpResponse(new OCR_RESULT("OCR_ENGINE is busy").getStringJson());

            if (request.Url.Contains("?") == false)
                return new HttpResponse(new OCR_RESULT("QueryString is null").getStringJson());

            string files = string.Empty,front_side = string.Empty,back_side = string.Empty;
            string queryString = request.Url.Split('?')[1];
            if (queryString[0] == '/') queryString = queryString.Substring(1);
            if (!string.IsNullOrEmpty(queryString))
            {
                var paras = System.Web.HttpUtility.ParseQueryString(queryString);
                if (paras != null && paras.HasKeys())
                {
                    HandlerCallback.OcrRunning = true;
                    front_side = paras.Get("front_side");
                    back_side = paras.Get("back_side");

                    var f1 = SaveImage(front_side);
                    var f2 = SaveImage(back_side);

                    if (string.IsNullOrEmpty(f1) || string.IsNullOrEmpty(f2))
                    {
                        HandlerCallback.OcrRunning = false;
                        return new HttpResponse(new OCR_RESULT("Cannot download images").getStringJson());
                    }

                    files = f1 + ";" + f2;

                    //Thread.Sleep(1000);

                    HandlerCallback.ocr_request_actractImage2Text(files);
                }
            }
            else
                return new HttpResponse(new OCR_RESULT("QueryString is null").getStringJson());

            _EVENT.WaitOne();

            _RESULT.urls = new string[] { front_side, back_side };

            return new HttpResponse(new OCR_RESULT(true, _RESULT).getStringJson());
        }


        public ApiServer(int port)
        {
            routes = new List<Route>() {
                //new Route {
                //    Name = "Ocr files",
                //    //UrlRegex = @"^\\/$",
                //    //UrlRegex = @"^/$",
                //    //UrlRegex = "^\\/$",
                //    //UrlRegex = @"^/Test/Example$",
                //    //UrlRegex = @"^\\/Test\\/Example\\?id=(\\d+)$",
                //    //UrlRegex = "^\\/Static\\/(.*)$",
                //    //UrlRegex = "^/(?=[^/]*$)",
                //    UrlRegex = "/?files=(.*)",
                //    Method = "GET",
                //    Callable = ___response
                //},
                new Route {
                    Name = "Ocr", 
                    //   /api/ocr?front_side=https://f88.vn/test/19.jpg&back_side=https://f88.vn/test/2.jpg
                    UrlRegex = "/api/ocr",
                    Method = "GET",
                    Callable = ___response_ocr
                },
                new Route {
                    Name = "Hook Ocr JS",
                    UrlRegex = "/vision.min.js",
                    Method = "GET",
                    Callable = (HttpRequest request) =>
                    {
                        string s = "";
                        if(File.Exists("vision.min.js")) s = File.ReadAllText("vision.min.js");
                        return new HttpResponse()
                        {
                            Headers = new Dictionary<string, string>(){
                                { "Content-Type", "application/x-javascript" }
                            },
                            ContentAsUTF8 = s,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                    }
                },
                new Route {
                    Name = "Hook Ocr JS",
                    UrlRegex = "/a.js",
                    Method = "GET",
                    Callable = (HttpRequest request) =>
                    {
                        string s = "";
                        if(File.Exists("a.js")) s = File.ReadAllText("a.js");
                        return new HttpResponse()
                        {
                            Headers = new Dictionary<string, string>(){
                                { "Content-Type", "application/x-javascript" }
                            },
                            ContentAsUTF8 = s,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                    }
                },
                new Route {
                    Name = "Hook Ocr CSS",
                    UrlRegex = "/a.css",
                    Method = "GET",
                    Callable = (HttpRequest request) =>
                    {
                        string s = "";
                        if(File.Exists("a.css")) s = File.ReadAllText("a.css");
                        return new HttpResponse()
                        {
                            Headers = new Dictionary<string, string>(){
                                { "Content-Type", "text/css" }
                            },
                            ContentAsUTF8 = s,
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                    }
                }, 
                //new Route {
                //    Name = "Stop Reload",
                //    UrlRegex = "/stop-reload",
                //    Method = "GET",
                //    Callable = (HttpRequest request) =>
                //    {
                //        HandlerCallback.OcrRunning = true;

                //        return new HttpResponse()
                //        {
                //            ContentAsUTF8 = "DONE",
                //            ReasonPhrase = "OK",
                //            StatusCode = "200"
                //        };
                //    }
                //}, 
                //new Route {   
                //    Name = "FileSystem Static Handler",
                //    UrlRegex = @"^/Static/(.*)$",
                //    Method = "GET",
                //    Callable = new FileSystemRouteHandler() { BasePath = @"C:\Tmp", ShowDirectories=true }.Handle,
                //},
            };

            httpServer = new HttpServer(port, routes);
        }

        public void Stop() {
            httpServer.Stop();
        }

        public void Start()
        {
            try
            {
                Thread thread = new Thread(new ThreadStart(httpServer.Listen));
                thread.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public IHandlerCallback HandlerCallback { get; set; }

        public void SendOcrResult(string data)
        {
            ////var cli = new System.Net.WebClient();
            ////cli.Headers[System.Net.HttpRequestHeader.ContentType] = "application/json";
            ////byte[] buf = Encoding.UTF8.GetBytes(data);
            ////string response = Encoding.UTF8.GetString(cli.UploadData(URL_OCR_API, buf));
            //////string response = cli.UploadString(URL_OCR_API, data);
            _RESULT = JsonConvert.DeserializeObject<OCR_BUF>(data);
            _EVENT.Set();
            _EVENT.Reset();
        }
    }

    

    

    
}

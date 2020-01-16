using Newtonsoft.Json;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace SimpleHttpServer
{
    static class Routes
    {
        public static List<Route> GET
        {
            get
            {
                return new List<Route>()
                {
                    new Route {
                        Name = "Ocr", 
                        //->  /api/ocr?file=https://f88.vn/test/19.jpg&side=front | back
                        UrlRegex = "/api/ocr",
                        Method = "GET",
                        Callable = ___response_api_ocr
                    },
                    new Route {
                        Name = "Token",
                        UrlRegex = "/api/token",
                        Method = "GET",
                        Callable = (HttpRequest request) => new HttpResponse(request.APP.app_getJsonToken())
                    },
                    new Route {
                        Name = "State",
                        UrlRegex = "/api/state",
                        Method = "GET",
                        Callable = (HttpRequest request) => new HttpResponse(request.APP.app_getJsonState())
                    },
                    new Route {
                        Name = "Is Busy",
                        UrlRegex = "/api/is-busy",
                        Method = "GET",
                        Callable = (HttpRequest request) => new HttpResponse(request.APP.app_checkIsBusy())
                    },
                };
            }
        }

        static string ___downloadImage(string imageUrl, IApp app)
        {
            try
            {
                string file = Path.GetFileName(imageUrl);
                string fileName = file.Substring(0, file.Length - 4) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

                file = Path.Combine(app.PATH_OCR_IMAGE, fileName);
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
            catch (Exception ex)
            {
                app.Url = imageUrl;
                app.StateOcr = STATE_OCR.OCR_FAIL_DOWNLOAD_FILE;
                app.TextError = ex.Message;
            }

            return string.Empty;
        }


        static HttpResponse ___response_api_ocr(HttpRequest request)
        {
            if (request.APP == null) 
                return new HttpResponse(JsonConvert.SerializeObject(new { Ok = false, TextError = "APP is null" }));

            request.APP.TextError = string.Empty;
            request.APP.TimeStart = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            
            if (request.APP.StateOcr == STATE_OCR.OCR_IS_BUSY) 
                return new HttpResponse(request.APP.app_getJsonResult()); 

            if (request.Url.Contains("?") == false)
            {
                request.APP.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                return new HttpResponse(request.APP.app_getJsonResult());
            }

            string queryString = request.Url.Split('?')[1];
            if (queryString[0] == '/') queryString = queryString.Substring(1);
            if (!string.IsNullOrEmpty(queryString))
            {
                var paras = System.Web.HttpUtility.ParseQueryString(queryString);
                if (paras != null && paras.HasKeys())
                {
                    string file = paras.Get("file");
                    request.APP.SideImage = paras.Get("side") == "back" ? SIDE_IMAGE.BACK : SIDE_IMAGE.FRONT;

                    if (string.IsNullOrEmpty(file))
                    {
                        request.APP.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                        return new HttpResponse(request.APP.app_getJsonResult());
                    }
                     
                    if (file.ToLower().StartsWith("http"))
                    {
                        string fileName = ___downloadImage(file, request.APP);
                        request.APP.goo_ocr_uploadFile(fileName, file);
                    }
                    else {
                        request.APP.goo_ocr_uploadFile(file);
                    }

                    //Success
                    return new HttpResponse(request.APP.app_getJsonResult());
                }
                else
                {
                    request.APP.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                    return new HttpResponse(request.APP.app_getJsonResult());
                }
            }
            else
            {
                request.APP.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                return new HttpResponse(request.APP.app_getJsonResult());
            }
        }
    }

}

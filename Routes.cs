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
                        //   /api/ocr?front_side=https://f88.vn/test/19.jpg&back_side=https://f88.vn/test/2.jpg
                        UrlRegex = "/api/ocr",
                        Method = "GET",
                        Callable = ___response_api_ocr
                    }
                };
            }
        }

        static string SaveImage(string imageUrl)
        {
            try
            {
                string file = Path.GetFileName(imageUrl);
                string fileName = file.Substring(0, file.Length - 4) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

                file = Path.Combine(@"c:\ocr-images\", fileName);
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


        static HttpResponse ___response_api_ocr(HttpRequest request)
        {
            request.APP.TimeStart = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));

            if (request.APP.StateOcr == STATE_OCR.OCR_IS_BUSY)
            { 
                return new HttpResponse(request.APP.app_getJsonResult());
            }

            if (request.Url.Contains("?") == false)
            {
                request.APP.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                return new HttpResponse(request.APP.app_getJsonResult());
            }

            //string files = string.Empty, front_side = string.Empty, back_side = string.Empty;
            //string queryString = request.Url.Split('?')[1];
            //if (queryString[0] == '/') queryString = queryString.Substring(1);
            //if (!string.IsNullOrEmpty(queryString))
            //{
            //    var paras = System.Web.HttpUtility.ParseQueryString(queryString);
            //    if (paras != null && paras.HasKeys())
            //    {
            //        HandlerCallback.OcrRunning = true;
            //        front_side = paras.Get("front_side");
            //        back_side = paras.Get("back_side");

            //        var f1 = SaveImage(front_side);
            //        var f2 = SaveImage(back_side);

            //        if (string.IsNullOrEmpty(f1) || string.IsNullOrEmpty(f2))
            //        {
            //            HandlerCallback.OcrRunning = false;
            //            return new HttpResponse(new OCR_RESULT("Cannot download images").getStringJson());
            //        }

            //        files = f1 + ";" + f2;

            //        //Thread.Sleep(1000);

            //        HandlerCallback.ocr_request_actractImage2Text(files);
            //    }
            //}
            //else
            //    return new HttpResponse(new OCR_RESULT("QueryString is null").getStringJson());


            return new HttpResponse(request.APP.app_getJsonResult());
        }
    }
}

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
                        Callable = (HttpRequest request) => new HttpResponse(request.APP.app_getState())
                    }
                };
            }
        }

        static OcrImageInfo ___downloadImage(OcrImageInfo ocr, IApp app)
        {
            try
            {
                string file = Path.GetFileName(ocr.Url);
                ocr.FileName = file.Substring(0, file.Length - 4) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

                file = Path.Combine(app.PATH_OCR_IMAGE, ocr.FileName);
                ImageFormat format = ImageFormat.Jpeg;

                WebClient client = new WebClient();
                Stream stream = client.OpenRead(ocr.Url);
                Bitmap bitmap = new Bitmap(stream);

                if (bitmap != null)
                {
                    bitmap.Save(file, format);
                }

                stream.Flush();
                stream.Close();
                client.Dispose();

                ocr.DownloadSuccess = true;
            }
            catch (Exception ex)
            {
                ocr.DownloadSuccess = false;
                ocr.StateOcr = STATE_OCR.OCR_FAIL_DOWNLOAD_FILE;
                ocr.TextError = ex.Message;
            }

            return ocr;
        }


        static HttpResponse ___response_api_ocr(HttpRequest request)
        {
            if (request.APP == null)
                return new HttpResponse(JsonConvert.SerializeObject(new { Ok = false, TextError = "APP is null" }));

            if (request.APP.StateGooService == STATE_GOO_SERVICE.GOO_AUTHEN_PROCESSING)
                return new HttpResponse(JsonConvert.SerializeObject(new { Ok = false, TextError = "Please wait, APP is authenting ..." }));

            OcrImageInfo ocr = new OcrImageInfo();

            ocr.TextError = string.Empty;
            ocr.TimeStart = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));

            //if (request.APP.StateOcr == STATE_OCR.OCR_IS_BUSY) 
            //    return new HttpResponse(request.APP.app_getJsonResult()); 

            if (request.Url.Contains("?") == false)
            {
                ocr.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                return new HttpResponse(ocr.app_getJsonResult(request.APP));
            }

            string queryString = request.Url.Split('?')[1];
            if (queryString[0] == '/') queryString = queryString.Substring(1);
            if (!string.IsNullOrEmpty(queryString))
            {
                var paras = System.Web.HttpUtility.ParseQueryString(queryString);
                if (paras != null && paras.HasKeys())
                {
                    ocr.SideImage = paras.Get("side") == "back" ? SIDE_IMAGE.BACK : SIDE_IMAGE.FRONT;
                    string file = paras.Get("file");
                    if (string.IsNullOrEmpty(file))
                    {
                        ocr.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                        return new HttpResponse(ocr.app_getJsonResult(request.APP));
                    }

                    ocr.IsUrl = file.ToLower().StartsWith("http");
                    if (ocr.IsUrl) ocr.Url = file; else ocr.FileName = file;

                    if (ocr.IsUrl)
                        ocr = ___downloadImage(ocr, request.APP);

                    ocr = request.APP.goo_ocr_uploadFile(ocr);

                    //Success
                    return new HttpResponse(ocr.app_getJsonResult(request.APP));
                }
                else
                {
                    ocr.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                    return new HttpResponse(ocr.app_getJsonResult(request.APP));
                }
            }
            else
            {
                ocr.StateOcr = STATE_OCR.OCR_FAIL_MISS_QUERY_STRING;
                return new HttpResponse(ocr.app_getJsonResult(request.APP));
            }
        }
    }

}

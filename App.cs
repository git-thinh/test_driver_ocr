﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using SimpleHttpServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using DataFile = Google.Apis.Drive.v2.Data.File;
using Newtonsoft.Json;

namespace test_driver_ocr
{
    public class App : IApp
    {
        #region [ DISABLE CLOSE BUTTON ]

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        static void ___disable_close_button()
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
        }

        #endregion

        #region [ APP ]

        const string _TITLE_FORMAT = "OCR.{0}: {1}";

        static int _PORT = 0;
        static HttpServer httpServer;
        static Thread thread;
        static IApp _app = null;

        public string setTitleMessage(string message = "") => Console.Title = string.Format(_TITLE_FORMAT, _PORT, message);

        public void Start(string[] args)
        {
            if (args.Length > 0) int.TryParse(args[0], out _PORT);

            if (_PORT == 0)
            {
                TcpListener l = new TcpListener(IPAddress.Loopback, 0);
                l.Start();
                _PORT = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
            }
            httpServer = new HttpServer(_PORT, Routes.GET, this);
            thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
            setTitleMessage();
            //------------------------------------------------
            ___app_Init();
            //------------------------------------------------
            Console.Write("\nPlease input command: ");
            string cmd = Console.ReadLine();
            cmd = cmd.ToUpper().Trim();

            while (cmd != "EXIT")
            {
                switch (cmd)
                {
                    case "CLS":
                    case "CLEAR":
                        Console.Clear();
                        break;
                }

                Console.Write("\nPlease input command: ");
                cmd = Console.ReadLine();
                cmd = cmd.ToUpper().Trim();
            }

            Stop();
        }

        void Stop()
        {
            // Exit application ...
            httpServer.Stop();
            thread.Abort(100);
            thread.Join();
        }

        public string app_getJsonResult()
        {
            string json = JsonConvert.SerializeObject(new
            {
                Ok = StateOcr == STATE_OCR.OCR_SUCCESS,
                Token = gooCredential,
                ServiceState = StateGooService,
                State = StateOcr,
                Request = new { 
                    File = FileName, 
                    Url = Url 
                },
                Result = TextResult,
                Error = TextError,
                TimeStart = TimeStart 
            }, Formatting.Indented);

            long timeComplete = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            json = json.Substring(0, json.Length - 1) + @",\n""TimeComplete"": " + timeComplete + "}";

            return json;
        }

        #endregion

        #region [ GOOGLE SERVICE ]

        public STATE_GOO_SERVICE StateGooService { get; set; }
        public STATE_OCR StateOcr { get; set; }
        public long TimeStart { get; set; }
        public long TimeComplete { get; set; }
        public string TextError { get; set; }
        public string TextResult { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }


        const string fileKey = "key.json";
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Ocr-Image-Driver-Vision";
        static UserCredential gooCredential = null;
        static DriveService gooService = null;

        static void goo_initCredentialService()
        {

            if (!File.Exists(fileKey))
            {
                _app.StateGooService = STATE_GOO_SERVICE.GOO_AUTHEN_FAIL_MISS_KEY;
                _app.setTitleMessage("ERROR: Cannot find file key.json");
                return;
            }

            try
            {
                using (var stream = new FileStream(fileKey, FileMode.Open, FileAccess.Read))
                {
                    //string credPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    string credPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    //credPath = Path.Combine(credPath, ".credentials/" + ApplicationName);
                    var fileStore = new FileDataStore(credPath, true);

                    gooCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes, "user", CancellationToken.None, fileStore).Result;
                }

                ////Create Drive API service.
                gooService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = gooCredential,
                    ApplicationName = ApplicationName,
                });

                _app.StateGooService = STATE_GOO_SERVICE.GOO_AUTHEN_SUCCESS;
            }
            catch (Exception ex)
            {
                _app.StateGooService = STATE_GOO_SERVICE.GOO_AUTHEN_FAIL_INVALID_KEY;
                _app.setTitleMessage("ERROR: " + ex.Message);
            }
        }

        public void goo_ocr_uploadFile(string fileName = "1.jpg", string url = "")
        {
            string file = Path.Combine(@"c:\ocr-images\", fileName);
            if (File.Exists(fileName) == false)
            {
                StateOcr = STATE_OCR.OCR_FAIL_MISS_FILE;
                return;
            }

            DataFile body = new DataFile()
            {
                Title = fileName,
                Description = url,
                //body.MimeType = "application/vnd.ms-excel";
                MimeType = "image/jpeg"
            };

            TextError = string.Empty;
            FileName = fileName;
            Url = url;

            byte[] byteArray = System.IO.File.ReadAllBytes(fileName);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                try
                {
                    //FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "application/vnd.google-apps.spreadsheet");
                    FilesResource.InsertMediaUpload request = gooService.Files.Insert(body, stream, "application/vnd.google-apps.photo");
                    request.Ocr = true;
                    request.OcrLanguage = "vi";
                    request.Convert = true;

                    request.Upload();
                    DataFile imgFile = request.ResponseBody;
                    string fileId = imgFile.Id;

                    // Copy image and paste as document
                    var textMetadata = new DataFile();
                    //textMetadata.Name = inputFile.Name;
                    //textMetadata.Parents = new List<string> { folderId };
                    textMetadata.MimeType = "application/vnd.google-apps.document";
                    FilesResource.CopyRequest requestCopy = gooService.Files.Copy(textMetadata, fileId);
                    requestCopy.Fields = "id";
                    requestCopy.OcrLanguage = "vi";
                    var textFile = requestCopy.Execute();

                    // Now we export document as plain text
                    FilesResource.ExportRequest requestExport = gooService.Files.Export(textFile.Id, "text/plain");
                    string output = requestExport.Execute();

                    TextResult = output;
                    StateOcr = STATE_OCR.OCR_SUCCESS;
                }
                catch (Exception e)
                {
                    TextError = e.Message;
                    StateOcr = STATE_OCR.OCR_FAIL_THROW_ERROR;
                }
            }
        }

        #endregion

        static void ___app_Init()
        {
            goo_initCredentialService();
            //// For test
            //_app.goo_ocr_uploadFile();
            //string result = _app.app_getJsonResult();
        }

        static void Main(string[] args)
        {
            if (Directory.Exists(@"c:\ocr-images\") == false) 
                Directory.CreateDirectory(@"c:\ocr-images\");

            ___disable_close_button();
            var app = new App();
            _app = (IApp)app;
            app.Start(args);
        }
    }
}

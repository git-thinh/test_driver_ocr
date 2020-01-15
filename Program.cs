using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using File = Google.Apis.Drive.v2.Data.File;

namespace test_driver_ocr
{
    class Program
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "GoogleDriveAPIDemoApp";

        static void Main(string[] args)
        {
            UserCredential credential = null;

            using (var stream = new FileStream("client_secret_1061643460639-qcoftl51kdesicoa5ifd0a057g0cmql4.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-Demo");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            ////Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            //var credential = new BaseClientService.Initializer();
            //credential.ApiKey = "AIzaSyAPLIGPa7xxqJPJf_Qh_nZ8tOeR092grj4";
            //credential.ApplicationName = "sampleApp";
            //var service = new DriveService(credential);


            /////////////////////////////// UPLOAD FILE /////////////////////////////////////
            UploadFile(service);

            ////////////////////////////// LIST FILES //////////////////////////////////////
            ListFiles(service);

            Console.WriteLine("DOME ...");
            Console.ReadKey();
        }

        private static void UploadFile(DriveService service)
        {
            File body = new File();
            body.Title = "test image ocr - " + DateTime.Now.ToString(" - yyyyMMdd - HHmmss");
            body.Description = "test image ocr - " + DateTime.Now.ToString(" - yyyyMMdd - HHmmss");
            //body.MimeType = "application/vnd.ms-excel";
            body.MimeType = "image/jpeg";


            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes("1.jpg");
            MemoryStream stream = new MemoryStream(byteArray);
            try
            {
                //FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "application/vnd.google-apps.spreadsheet");
                FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "application/vnd.google-apps.photo");
                request.Ocr = true;
                request.OcrLanguage = "vi";
                request.Convert = true;

                request.Upload();
                File imgFile = request.ResponseBody;


                // Copy image and paste as document
                var textMetadata = new File();
                //textMetadata.Name = inputFile.Name;
                //textMetadata.Parents = new List<string> { folderId };
                textMetadata.MimeType = "application/vnd.google-apps.document";
                FilesResource.CopyRequest requestCopy = service.Files.Copy(textMetadata, imgFile.Id);
                requestCopy.Fields = "id";
                requestCopy.OcrLanguage = "vi";
                var textFile = requestCopy.Execute();

                // Now we export document as plain text
                FilesResource.ExportRequest requestExport = service.Files.Export(textFile.Id, "text/plain");
                string output = requestExport.Execute();

                // Uncomment the following line to print the File ID.
                // Console.WriteLine("File ID: " + file.Id);

            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }


        private static void ListFiles(DriveService service)
        {
            try
            {
                // Define parameters of request.
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.MaxResults = 100;

                // List files.
                var files = listRequest.Execute().Items;
                Console.WriteLine("Files:");
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        Console.WriteLine("{0} ({1})", file.Title, file.Id);
                        Debug.WriteLine("{0} ({1})", file.Title, file.Id);
                    }
                }
                else
                {
                    Console.WriteLine("No files found.");
                }
            }
            catch (Exception ex) { 
            
            }

            Console.Read();
        }
    }
}
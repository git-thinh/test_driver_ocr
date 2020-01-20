using Newtonsoft.Json;
using System;

namespace SimpleHttpServer
{
    public enum STATE_GOO_SERVICE
    {
        NONE,
        GOO_AUTHEN_PROCESSING,
        GOO_AUTHEN_SUCCESS,
        GOO_AUTHEN_FAIL_MISS_KEY,
        GOO_AUTHEN_FAIL_INVALID_KEY,
    }

    public enum STATE_OCR
    {
        NONE,
        OCR_IS_BUSY,
        OCR_FAIL_AUTHEN,
        OCR_FAIL_MISS_FILE,
        OCR_FAIL_MISS_QUERY_STRING,
        OCR_FAIL_READ_FILE,
        OCR_FAIL_DOWNLOAD_FILE,
        OCR_FAIL_ROTATE_IMAGE,
        OCR_FAIL_THROW_ERROR,
        OCR_FAIL_ENGINE_ERROR,
        OCR_SUCCESS
    }

    public enum SIDE_IMAGE
    {
        FRONT,
        BACK
    }

    public class OcrImageInfo {
        public bool WriteToFile { get; set; }
        public bool IsUrl { get; set; }
        public bool DownloadSuccess { get; set; }
        public long TimeStart { get; set; }
        public long TimeComplete { get; set; }

        public SIDE_IMAGE SideImage { get; set; }
        public string TextError { get; set; }
        public string TextResult { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public STATE_OCR StateOcr { get; set; }
        
        public string app_getJsonResult(IApp app)
        {
            bool ok = this.StateOcr == STATE_OCR.OCR_SUCCESS;
            string json = JsonConvert.SerializeObject(new
            {
                Ok = ok,
                ServiceState = app != null ? app.StateGooService.ToString() : STATE_GOO_SERVICE.NONE.ToString(),
                State = this.StateOcr.ToString(),
                Request = new
                {
                    File = this.FileName,
                    Url = this.Url,
                    Side = this.SideImage.ToString()
                },
                Result = new
                {
                    Text = this.TextResult,
                    Item = ok ? new CMT(true, this.TextResult) : new CMT(this.TextError)
                },
                Error = this.TextError,
                TimeStart = this.TimeStart
            }, Formatting.Indented);

            long timeComplete = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            json = json.Substring(0, json.Length - 1) + Environment.NewLine +
                @", ""TimeComplete"": " + timeComplete + "}";

            return json;
        }
    }

    public interface IApp
    { 
        string PATH_OCR_IMAGE { get; }
        STATE_GOO_SERVICE StateGooService { get; set; }

        //string app_getJsonResult(OcrImageInfo ocr);
        string app_getJsonToken();
        string app_getState();

        string setTitleMessage(string message = "");
        void writeLogMessage(string message = "");

        OcrImageInfo goo_ocr_uploadFile(OcrImageInfo ocr);
    }
}

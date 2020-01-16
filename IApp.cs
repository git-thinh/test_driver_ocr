namespace SimpleHttpServer
{
    public enum STATE_GOO_SERVICE
    {
        NONE,
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
        OCR_FAIL_CANNOT_READ_FILE,
        OCR_FAIL_CANNOT_DOWNLOAD_FILE,
        OCR_FAIL_ROTATE_IMAGE,
        OCR_FAIL_THROW_ERROR,
        OCR_FAIL_ENGINE_ERROR,
        OCR_SUCCESS
    }

    public interface IApp
    {
        long TimeStart { get; set; }
        long TimeComplete { get; set; }

        string TextError { get; set; }
        string TextResult { get; set; }
        string FileName { get; set; }
        string Url { get; set; }

        string app_getJsonResult();

        STATE_OCR StateOcr { get; set; }
        STATE_GOO_SERVICE StateGooService { get; set; }

        string setTitleMessage(string message = "");

        void goo_ocr_uploadFile(string fileName = "1.jpg", string url = "");
    }
}

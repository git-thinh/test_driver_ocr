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

    public interface IApp
    {
        string PATH_OCR_IMAGE { get; }

        long TimeStart { get; set; }
        long TimeComplete { get; set; }

        SIDE_IMAGE SideImage { get; set; }
        string TextError { get; set; }
        string TextResult { get; set; }
        string FileName { get; set; }
        string Url { get; set; }

        string app_getJsonResult();
        string app_getJsonToken();
        string app_getJsonState();
        string app_checkIsBusy();

        STATE_OCR StateOcr { get; set; }
        STATE_GOO_SERVICE StateGooService { get; set; }

        string setTitleMessage(string message = "");

        void goo_ocr_uploadFile(string fileName = "1.jpg", string url = "");
    }
}

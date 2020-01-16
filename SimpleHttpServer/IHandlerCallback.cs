using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    public interface IHandlerCallback
    {
        bool OcrRunning { set; get; }
        int StepId { set; get; }

        void captcha_visbleCheckNotBeRobot();
        void captcha_visbleChooseImage();

        void browser_onFrameLoadEnd(string url);
        void browser_onIninited();
        void browser_goF5();
        
        void ocr_request_actractImage2Text(string fileImage);

        void response_tokenInfo(string data);
        void response_calbackSuccess(string url, string input, string data);
    }
}

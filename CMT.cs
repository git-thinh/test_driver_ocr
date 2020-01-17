using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleHttpServer
{
    public enum OCR_DATA_TYPE
    {
        NONE,
        DATE_TIME_BIRTHDAY,
        DATE_TIME_EXPIRY
    }

    public class CMT
    {
        public string address = "N/A";
        public string id = "N/A";
        public string fullname = "N/A";
        public string birthday = "N/A";
        public string expiry = "N/A";
        public string gender = "N/A";
        public string ethnicity = "N/A";
        public string issue_by = "N/A";
        public string issue_date = "N/A";
        public string religion = "N/A";

        public string signal_description = "N/A";
        public string date_active = "N/A";

        public int status_code = 2;
        public string status = "success";

        public bool ok = false; 
        public string ocr_error = "";
         
        public CMT(string message_error)
        {
            ocr_error = message_error;
        }

        string ___remove_startWith(string s, string startWith_ = ".")
        {
            while (s.StartsWith(startWith_)) s = s.Substring(startWith_.Length).Trim();
            return s.Trim();
        }

        string ___remove_endWith(string s, string startWith_ = ".")
        {
            while (s.EndsWith(startWith_)) s = s.Substring(0, s.Length - startWith_.Length).Trim();
            return s.Trim();
        }

        string[] ___extract_startWith(string t, string startWith_,
            int numberLineOutput = 1, int numberWordOutput = -1, OCR_DATA_TYPE type = OCR_DATA_TYPE.NONE)
        {
            if (string.IsNullOrEmpty(t)) return new string[] { string.Empty, string.Empty, string.Empty };

            string v = "", err = "";
            try
            {
                int k = t.ToLower().IndexOf(startWith_);
                if (k != -1)
                {
                    k = k + startWith_.Length;
                    t = t.Substring(k, t.Length - k).Trim();
                    t = ___remove_startWith(t, ".");
                    t = ___remove_startWith(t, ":");

                    v = string.Join(" ", t.Split('\n').Where((x, i) => i < numberLineOutput)).Trim();

                    k = v.Length;
                    t = t.Substring(k, t.Length - k).Trim();
                    t = ___remove_startWith(t, ".");
                    t = ___remove_startWith(t, ":");
                }
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }

            v = ___remove_endWith(v, ".").Trim();
            switch (type)
            {
                case OCR_DATA_TYPE.DATE_TIME_BIRTHDAY:
                case OCR_DATA_TYPE.DATE_TIME_EXPIRY:
                    //v = Regex.Replace(v, @"[^\d]", " ").Trim();
                    v = Regex.Replace(v, @"[^0-9]", " ").Trim();
                    break;
            }

            v = ___remove_endWith(v, ".").Trim();

            if (numberWordOutput > 0)
                v = string.Join(" ", v.Split(' ').Where((x, i) => i < numberWordOutput).ToArray());

            // Replace multi space
            v = Regex.Replace(v, @"\s+", " ").Trim();

            switch (type)
            {
                case OCR_DATA_TYPE.DATE_TIME_BIRTHDAY:
                case OCR_DATA_TYPE.DATE_TIME_EXPIRY:
                    v = v.Replace(' ', '-');
                    break;
            }

            return new string[] { v, t, err };
        }

        public CMT(bool ok_, string data_)
        {
            var o = this;
            o.ok = ok_; 

            string s = data_.Trim(), err = string.Empty;
            string[] a;

            // id
            a = ___extract_startWith(s, "số");
            o.id = a[0];
            if (err.Length == 0) err = a[2];

            // fullname
            a = ___extract_startWith(s, "tên");
            o.fullname = a[0];
            if (err.Length == 0) err = a[2];

            // birthday
            a = ___extract_startWith(s, "ngày", 1, -1, OCR_DATA_TYPE.DATE_TIME_BIRTHDAY);
            o.birthday = a[0];
            if (err.Length == 0) err = a[2];

            // address
            a = ___extract_startWith(s, "trú", 2);
            o.address = a[0];
            if (err.Length == 0) err = a[2];

            // expiry
            a = ___extract_startWith(s, "giá trị đến", 1, -1, OCR_DATA_TYPE.DATE_TIME_BIRTHDAY);
            o.expiry = a[0];
            if (err.Length == 0) err = a[2];

            // expiry
            a = ___extract_startWith(s, "giới tính", 1, 1);
            o.gender = a[0];
            if (err.Length == 0) err = a[2];

            //---------------------

            //////s = data_.DataBack.Trim();

            //////// signal_description
            //////a = ___extract_startWith(s, "nhận dạng", 2, -1);
            //////o.signal_description = a[0];
            //////if (err.Length == 0) err = a[2];

            //////// signal_description
            //////a = ___extract_startWith(s, "ngày", 1, -1, OCR_DATA_TYPE.DATE_TIME_BIRTHDAY);
            //////o.date_active = a[0];
            //////if (err.Length == 0) err = a[2];
            
            ocr_error = err;
        } 
    }
}

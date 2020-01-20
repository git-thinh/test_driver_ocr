using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleHttpServer
{
    public enum OCR_DATA_TYPE
    {
        NONE,
        NUMBER_0_9,
        DATE_TIME_BIRTHDAY,
        DATE_TIME_EXPIRY,
        CMT_ID,
        CMT_FULLNAME,
        CMT_BIRTHDAY,
        CMT_EXPIRY,
        CMT_ADDRESS,
    }

    public class OcrConfig
    {
        public int Page { set; get; }
        public bool Success { set; get; }
        public string Error { set; get; }
        public string Result { set; get; }
        public string TextSplit { set; get; }
        public OCR_DATA_TYPE Type { set; get; }
        public string Input { set; get; }

        public OcrConfig(string text, OCR_DATA_TYPE type, string textSplit = "")
        {
            Type = type;
            Input = text;
        }

        const string FULLNAME_VALID_UPPER = "QWERTYUIOPASDFGHJKLMNBVCXZ ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ ÉÈẸẺẼÊẾỀỆỂỄ ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ ÚÙỤỦŨƯỨỪỰỬỮ ÍÌỊỈĨ Đ ÝỲỴỶỸ";
        const string FULLNAME_VALID_LOWER = "qwertyuiopasdfghjklmnbvcxz áàạảãâấầậẩẫăắằặẳẵ éèẹẻẽêếềệểễ óòọỏõôốồộổỗơớờợởỡ úùụủũưứừựửữ íìịỉĩ đ ýỳỵỷỹ";

        public OcrConfig Execute()
        {
            string v = this.Input, textLower = v.ToLower(), s, t;
            string[] a1, a2;
            int pos = -1, pos2 = -1;

            if (textLower.Contains("đặc điểm nhận")
                || textLower.Contains("giám đốc")
                || textLower.Contains("dân tộc")
                || textLower.Contains("tôn giáo")) this.Page = 2;
            else this.Page = 1;

            if (this.Page == 1)
            {
                // Side front

                switch (Type)
                {
                    case OCR_DATA_TYPE.CMT_ID:
                        #region

                        v = v.Replace('l', '1');
                        //v = Regex.Replace(v, @"[^\d]", " ").Trim();
                        v = Regex.Replace(v, @"[^0-9:]", " ").Trim();
                        v = Regex.Replace(v, @"\s+", " ").Trim();
                        a1 = v.Split(new char[] { ':', ' ' }).Where(x => x.Length > 0).ToArray();

                        if (a1.Length > 0)
                        {
                            a2 = a1.Where(x => x.Length > 7).ToArray();
                            if (a2.Length > 0)
                            {
                                this.Result = a2[0].Trim();
                                return this;
                            }
                        }

                        this.Error = v;

                        #endregion
                        break;
                    case OCR_DATA_TYPE.CMT_FULLNAME:
                        #region
                        s = v;

                        pos = textLower.IndexOf("tên"); 
                        if (pos == -1) pos = textLower.IndexOf("ten");
                         
                        pos2 = textLower.IndexOf("số");
                        if (pos == -1) pos = pos2;


                        if (pos != -1) s = s.Substring(pos + 3, v.Length - pos - 3); 

                        s = s.Replace('\r', ' ').Replace('\n', ' ');
                        s = Regex.Replace(s, @"[0-9]", " ").Trim();
                         
                        a1 = s.Split(' ').Where(x => x.Length > 0).ToArray();
                        for (int i = 0; i < a1.Length; i++)
                        {
                            for (int k = 0; k < a1[i].Length; k++)
                            {
                                if (FULLNAME_VALID_LOWER.IndexOf(a1[i][k]) != -1)
                                {
                                    s = s.Replace(a1[i], "  ");
                                    break;
                                }
                            }
                        }
                        s = s.Trim();

                        char[] ca = new char[s.Length];
                        for (int i = 0; i < s.Length; i++)
                        {
                            if (s[i] == ':' || s[i] == '.')
                            {
                                ca[i] = ' ';
                            }
                            else
                            {
                                if (FULLNAME_VALID_UPPER.IndexOf(s[i]) != -1)
                                {
                                    ca[i] = s[i];
                                }
                                else
                                {
                                    ca[i] = ' ';
                                }
                            }
                        }
                        t = new string(ca);
                        t = t.Trim();

                        a1 = t.Split(new string[] { "  " }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Split(' ').Length > 1).ToArray();
                        if (a1.Length > 0)
                            this.Result = a1[0];
                        else
                            this.Error = t;

                        #endregion
                        break;
                    case OCR_DATA_TYPE.CMT_BIRTHDAY:
                        #region

                        v = v.Replace('/', '-').Replace(' ', '-')
                            .Replace("-l-", "-1-").Replace("-ll-", "-11-")
                            .Replace("ll-", "-11-").Replace("l-", "1-");

                        v = Regex.Replace(v, @"[^0-9-]", " ").Trim();
                        v = Regex.Replace(v, @"\s+", " ").Trim();
                        a1 = v.Split(' ').Select((x) =>
                        {
                            string o = x.Trim();
                            if (o.Length == 0) return o;
                            o = o.Replace('-', ' ').Trim();
                            o = Regex.Replace(o, @"\s+", " ").Trim();
                            o = o.Replace(' ', '-');
                            return o;
                        }).Where(x => (x.Length >= 8 && x.Length <= 10) && x.Contains('-') && x.Split('-').Length == 3).ToArray();


                        if (a1.Length > 0)
                            this.Result = a1[0];
                        else
                            this.Error = v;

                        #endregion
                        break;
                    case OCR_DATA_TYPE.CMT_ADDRESS:
                        #region
                        s = v;

                        pos = textLower.IndexOf("trú");
                        if (pos != -1)
                        {
                            s = s.Substring(pos + 3, s.Length - pos - 3);
                            this.Result = s;
                        }
                        else
                            this.Error = s;

                        #endregion
                        break;
                }
            }
            else
            {
                // Side back
            }

            return this;
        }
    }

    public class CMT
    {
        #region

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
        public int page = 1;

        #endregion

        public CMT(string message_error) => ocr_error = message_error;

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
                case OCR_DATA_TYPE.NUMBER_0_9:
                    v = Regex.Replace(v, @"[^\d]", " ").Trim();
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

            v = v.Replace('.', ' ');
            v = Regex.Replace(v, @"\s+", " ").Trim();

            return new string[] { v, t, err };
        }



        public CMT(bool ok_, string data_)
        {
            var o = this;
            o.ok = ok_;

            if (string.IsNullOrWhiteSpace(data_)) return;

            string s = data_.Trim(), err = string.Empty;
            string[] a;

            if (s.Contains("đặc điểm nhận dạng")
                || s.Contains("đặc điểm nhận dạng"))
            {
                page = 2;

                // signal_description
                a = ___extract_startWith(s, "nhận dạng", 2, -1);
                o.signal_description = a[0];
                if (err.Length == 0) err = a[2];

                // signal_description
                a = ___extract_startWith(s, "ngày", 1, -1, OCR_DATA_TYPE.DATE_TIME_BIRTHDAY);
                o.date_active = a[0];
                if (err.Length == 0) err = a[2];
            }
            else
            {
                // id
                a = ___extract_startWith(s, "số", 1, -1, OCR_DATA_TYPE.NUMBER_0_9);
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
            }

            ocr_error = err;
        }
    }
}

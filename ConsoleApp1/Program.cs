using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string t = @"Tri Tôn
Tịnh Biên
Thoại Sơn
Tân Châu
Phú Tân
Long Xuyên
Chợ Mới
Châu Thành
Châu Phú
Châu Đốc
An Phú";

            string stFormD = t.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            
            string s = (sb.ToString().Normalize(NormalizationForm.FormD)).ToLower();

            string v = @"""" + string.Join(@""", """, s.Split('\r').Select(x => new string(x.Trim().Reverse().ToArray())).ToArray()) + @"""";

            string a = @"""" + string.Join(@""", """, t.Split('\r').Select(x => x.Trim()).ToArray()) + @"""";



        }
    }
}

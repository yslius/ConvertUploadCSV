using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;


namespace ConvertUploadCSV
{
    public class convFile
    {
        public int ConvCSV(string pathFile)
        {
            byte[] bs = File.ReadAllBytes(pathFile);
            Encoding enc = GetCode(bs);

            if(enc.CodePage == 932)
            {
                MessageBox.Show("変換不要です。",
                "ConvertUploadCSV",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
                return -1;
            }
            List<string> listCSV = OpenCSV(pathFile);

            //if (listCSV.Count <= 1)
            //{
            //    MessageBox.Show("CSVにデータがありません。",
            //   "ConvertUploadCSV",
            //   MessageBoxButtons.OK,
            //   MessageBoxIcon.Error);
            //    return -1;
            //}
            //if (listCSV[1].Substring(0, 1) != "\"")
            //{
            //    MessageBox.Show("変換不要です。",
            //    "ConvertUploadCSV",
            //    MessageBoxButtons.OK,
            //    MessageBoxIcon.Information);
            //    return -1;
            //}

            //listCSV = ConvertData(listCSV);

            if (!WriteCSV(pathFile, listCSV))
                return -1;

            return 0;
        }

        private List<string> OpenCSV(string pathFile)
        {
            List<string> listCSV = new List<string>();

            //StreamReader sr = new StreamReader(pathFile, 
            //    Encoding.GetEncoding("shift_jis"));
            StreamReader sr = new StreamReader(pathFile,
                Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                //string[] values = line.Split(',');
                
                listCSV.Add(line);
            }
            sr.Close();

            return listCSV;
        }

        private List<string> ConvertData(List<string> listCSV)
        {
            List<string> listRet = new List<string>();
            foreach (string line in listCSV)
            {
                listRet.Add(line);
            }
            string tmp;
            int cnt1 = 0;
            int cnt2 = 0;
            foreach (string line in listCSV)
            {
                if (cnt1 == 0)
                {
                    cnt1++;
                    continue;
                }
                string[] values = line.Split(',');
                cnt2 = 0;
                foreach (string value in values)
                {
                    if(value == "")
                    {
                        cnt2++;
                        continue;
                    }
                    tmp = value;
                    if (tmp.Substring(0, 1) == "\"")
                        tmp = tmp.Substring(1);
                    if (tmp.Substring(tmp.Length - 1) == "\"")
                        tmp = tmp.Substring(0, tmp.Length - 1);
                    tmp = tmp.Replace("\"\"", "");
                    tmp = tmp.Replace("\"\"", "");
                    values[cnt2] = tmp;
                    cnt2++;
                }
                listRet[cnt1] = string.Join(",", values);

                //tmp = line.Substring(1);
                //tmp = tmp.Substring(0, tmp.Length - 1);
                //tmp = tmp.Replace("\"\"", "");
                //listRet[cnt1] = tmp;
                cnt1++;
            }

            return listRet;
        }


        private bool WriteCSV(string pathFile, List<string> listCSV)
        {
            StreamWriter sw = new StreamWriter(pathFile, 
                false, Encoding.GetEncoding("shift_jis"));
            foreach (string line in listCSV)
            {
                Debug.WriteLine(line);
                sw.WriteLine(string.Format("{0}", line));
            }
            sw.Close();

            return true;
        }

        public static Encoding GetCode(byte[] bytes)
        {
            const byte bEscape = 0x1B;
            const byte bAt = 0x40;
            const byte bDollar = 0x24;
            const byte bAnd = 0x26;
            const byte bOpen = 0x28;    //'('
            const byte bB = 0x42;
            const byte bD = 0x44;
            const byte bJ = 0x4A;
            const byte bI = 0x49;

            int len = bytes.Length;
            byte b1, b2, b3, b4;

            //Encode::is_utf8 は無視

            bool isBinary = false;
            for (int i = 0; i < len; i++)
            {
                b1 = bytes[i];
                if (b1 <= 0x06 || b1 == 0x7F || b1 == 0xFF)
                {
                    //'binary'
                    isBinary = true;
                    if (b1 == 0x00 && i < len - 1 && bytes[i + 1] <= 0x7F)
                    {
                        //smells like raw unicode
                        return System.Text.Encoding.Unicode;
                    }
                }
            }
            if (isBinary)
            {
                return null;
            }

            //not Japanese
            bool notJapanese = true;
            for (int i = 0; i < len; i++)
            {
                b1 = bytes[i];
                if (b1 == bEscape || 0x80 <= b1)
                {
                    notJapanese = false;
                    break;
                }
            }
            if (notJapanese)
            {
                return System.Text.Encoding.ASCII;
            }

            for (int i = 0; i < len - 2; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                b3 = bytes[i + 2];

                if (b1 == bEscape)
                {
                    if (b2 == bDollar && b3 == bAt)
                    {
                        //JIS_0208 1978
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bDollar && b3 == bB)
                    {
                        //JIS_0208 1983
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bOpen && (b3 == bB || b3 == bJ))
                    {
                        //JIS_ASC
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    else if (b2 == bOpen && b3 == bI)
                    {
                        //JIS_KANA
                        //JIS
                        return System.Text.Encoding.GetEncoding(50220);
                    }
                    if (i < len - 3)
                    {
                        b4 = bytes[i + 3];
                        if (b2 == bDollar && b3 == bOpen && b4 == bD)
                        {
                            //JIS_0212
                            //JIS
                            return System.Text.Encoding.GetEncoding(50220);
                        }
                        if (i < len - 5 &&
                            b2 == bAnd && b3 == bAt && b4 == bEscape &&
                            bytes[i + 4] == bDollar && bytes[i + 5] == bB)
                        {
                            //JIS_0208 1990
                            //JIS
                            return System.Text.Encoding.GetEncoding(50220);
                        }
                    }
                }
            }

            //should be euc|sjis|utf8
            //use of (?:) by Hiroki Ohzaki <ohzaki@iod.ricoh.co.jp>
            int sjis = 0;
            int euc = 0;
            int utf8 = 0;
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((0x81 <= b1 && b1 <= 0x9F) || (0xE0 <= b1 && b1 <= 0xFC)) &&
                    ((0x40 <= b2 && b2 <= 0x7E) || (0x80 <= b2 && b2 <= 0xFC)))
                {
                    //SJIS_C
                    sjis += 2;
                    i++;
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if (((0xA1 <= b1 && b1 <= 0xFE) && (0xA1 <= b2 && b2 <= 0xFE)) ||
                    (b1 == 0x8E && (0xA1 <= b2 && b2 <= 0xDF)))
                {
                    //EUC_C
                    //EUC_KANA
                    euc += 2;
                    i++;
                }
                else if (i < len - 2)
                {
                    b3 = bytes[i + 2];
                    if (b1 == 0x8F && (0xA1 <= b2 && b2 <= 0xFE) &&
                        (0xA1 <= b3 && b3 <= 0xFE))
                    {
                        //EUC_0212
                        euc += 3;
                        i += 2;
                    }
                }
            }
            for (int i = 0; i < len - 1; i++)
            {
                b1 = bytes[i];
                b2 = bytes[i + 1];
                if ((0xC0 <= b1 && b1 <= 0xDF) && (0x80 <= b2 && b2 <= 0xBF))
                {
                    //UTF8
                    utf8 += 2;
                    i++;
                }
                else if (i < len - 2)
                {
                    b3 = bytes[i + 2];
                    if ((0xE0 <= b1 && b1 <= 0xEF) && (0x80 <= b2 && b2 <= 0xBF) &&
                        (0x80 <= b3 && b3 <= 0xBF))
                    {
                        //UTF8
                        utf8 += 3;
                        i += 2;
                    }
                }
            }
            //M. Takahashi's suggestion
            //utf8 += utf8 / 2;

            System.Diagnostics.Debug.WriteLine(
                string.Format("sjis = {0}, euc = {1}, utf8 = {2}", sjis, euc, utf8));
            if (euc > sjis && euc > utf8)
            {
                //EUC
                return System.Text.Encoding.GetEncoding(51932);
            }
            else if (sjis > euc && sjis > utf8)
            {
                //SJIS
                return System.Text.Encoding.GetEncoding(932);
            }
            else if (utf8 > euc && utf8 > sjis)
            {
                //UTF8
                return System.Text.Encoding.UTF8;
            }

            return null;
        }

    }
}

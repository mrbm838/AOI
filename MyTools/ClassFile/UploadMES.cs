using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace MyTools
{
    class Upload
    {
        //string S, C, I, N, P, T, O, timeS, timeE;
        //string[] F, M;
        //string FF, MM;

        StreamWriter write;

        String basepath = System.AppDomain.CurrentDomain.BaseDirectory;
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
                    string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def,
                    StringBuilder retVal, int size, string filePath);



        public void Save(String S, String C, String I, String N, String P, String T, String O, String timeS, String timeE, String FF, String MM)
        {
          //  basepath = System.AppDomain.CurrentDomain.BaseDirectory;
            //basepath += "UploadMES" + "\\" +Define.SN+"-" + DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss") + ".Tar";
            basepath = "D:\\TAR\\S3076G-FG AOI01\\SOURCE" + "\\" + Define.SN + "-" + DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd-HH-mm-ss") + ".Tar";
            write = new StreamWriter(basepath, true);

            write.Write(S + "\r\n" + C + "\r\n" + I + "\r\n" + N + "\r\n" + P + "\r\n" + T + "\r\n" + O + "\r\n"   //表头
                          + "[" + timeS + "\r\n" + "]" + timeE + FF + MM);

            write.Close();
        }
        public void save2(String str)
        {
            basepath = System.AppDomain.CurrentDomain.BaseDirectory;
            basepath += "Log" + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            /* if (!File.Exists(basepath))
             {
                 File.Create(basepath);
              
             }*/
            write = new StreamWriter(basepath, true);
            write.Write(str);
            write.Close();
            //  file = new FileStream(basepath,FileMode.Append);
            //   byte[] bytee=  System.Text.Encoding.Default.GetBytes (str);
            //  file.Write(bytee, 0, bytee.Length);
            //  file.Close();

        }

    }
}

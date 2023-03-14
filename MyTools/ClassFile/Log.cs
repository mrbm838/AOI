using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ToolTotal;

namespace MyTools.ClassFile
{
    public class Log
    {
        private static StreamWriter write;

        private IniFile myIniFile;

        private string basepath = AppDomain.CurrentDomain.BaseDirectory;

        public string SNstr = "";

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public void save(string str)
        {
            basepath = AppDomain.CurrentDomain.BaseDirectory;
            basepath = basepath + "Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            write = new StreamWriter(basepath, append: true);
            write.Write(str);
            write.Close();
        }

        public void saveSN(string str, string path, string name)
        {
            basepath = AppDomain.CurrentDomain.BaseDirectory;
            basepath = basepath + "Log\\SN-" + path + ".txt";
            write = new StreamWriter(basepath, append: true);
            write.Write(str);
            write.Close();
        }

        public void SaveSN(string sn, string str)
        {
            basepath = "D:\\Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + "\\";
            if (!Directory.Exists(basepath))
            {
                Directory.CreateDirectory(basepath);
            }

            basepath = basepath + sn + ".txt";
            write = new StreamWriter(basepath, append: true);
            write.Write(str);
            write.Close();
        }

        public int Read(string SN, string path)
        {
            basepath = AppDomain.CurrentDomain.BaseDirectory;
            basepath = basepath + "Log\\SN-" + path + ".txt";
            SNstr = File.ReadAllText(basepath);
            string[] array = SNstr.Split(',');
            int result = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (SN == array[i] || SN + "-P" == array[i])
                {
                    result++;
                }
            }

            return result;
        }

        public int ReadSNo(string SN, string path)
        {
            basepath = AppDomain.CurrentDomain.BaseDirectory;
            basepath = basepath + "Log\\SN-" + path + ".txt";
            SNstr = File.ReadAllText(basepath);
            string[] array = SNstr.Split(',');
            for (int i = 0; i < array.Length - 1; i++)
            {
                if (SN == array[i])
                {
                    return i;
                }
            }

            return 0;
        }

        public void save2(string str, string sn)
        {
            basepath = "E:\\WorkData";
            if (!File.Exists(basepath))
            {
                Directory.CreateDirectory(basepath);
            }

            basepath = basepath + "\\" + sn + ".csv";
            write = new StreamWriter(basepath, append: true);
            write.Write(str);
            write.Close();
        }

        public void save5(string str, string path)
        {
            write = new StreamWriter(path, append: true);
            write.Write(str + "\r\n");
            write.Close();
        }
    }

}

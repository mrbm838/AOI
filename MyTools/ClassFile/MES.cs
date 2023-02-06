using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MESLinkTEST
{
    class MES
    {
        //[DllImport("SajetConnect.dll", EntryPoint = "SajetTransStart")]
        //public static extern bool SajetTransStart();

        //[DllImport("SajetConnect.dll", EntryPoint = "SajetTransClose")]
        //public static extern bool SajetTransClose();

        ////[DllImport("SajetConnect.dll", EntryPoint = "SajetTransData")]
        ////public static extern byte SajetTransData(int f_iCommandNo, byte[] f_pData, int[] f_iLen);

        //[DllImport("SajetConnect.dll", EntryPoint = "SajetTransData")]
        //public static extern bool SajetTransData(short f_iCommandNo, ref byte f_pData, ref int f_pLen);

        //[DllImport("SajetConnect.dll", EntryPoint = "SajetTransData")]
        //public static extern bool SajetTransData(short f_iCommandNo, ref string f_pData, ref int f_pLen);

        [DllImport("SajetConnect.dll", EntryPoint = "SajetTransStart")]
        public static extern bool SajetTransStart();

        [DllImport("SajetConnect.dll", EntryPoint = "SajetTransClose")]
        public static extern bool SajetTransClose();

        [DllImport("SajetConnect.dll", EntryPoint = "SajetTransData")]
        public static extern bool SajetTransData(int f_iCommandNo, ref byte f_pData, ref int f_pLen);

        [DllImport("SajetConnect.dll", EntryPoint = "SajetTransData")]
        public static extern bool SajetTransData(int f_iCommandNo, ref string f_pData, ref int f_pLen);
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Shared
{
    public sealed class Core
    {
        //    public string _connectionString = @"Server=192.168.1.2; Database=GibTraining; User Id=GibUser; Password=LetMeIn1%";

        //public string FormatDate = "dd/MM/yyyy";
        //public string FormatTime = "hh:mm";
        //public string FormatDateTime = "dd/MM/yyyy hh:mm";
        //public string FormatDateTime24 = "dd/MM/yyyy HH:mm";
        //public string FormatHours = "0.##;minus 0.0;0";
        //public string sqlitedb = "userdata.db";
        //public string sqliteiddb = "iddata.db";

        public string STOR_COMMAND = "STOR";
        public string VOL_LABEL = "BK*";
        public int BTP_PORT = 47440;

        static readonly Core instance = new Core();

        static Core()
        {
        }

        Core()
        {
        }

        public static Core Instance
        {
            get { return instance; }
        }

        public string EncryptString(string decryptedString)
        {
            if (decryptedString == null)
            {
                return "";
            }
            try
            {
                int[] replaceStr = { 1, 5, 2, 3, 8, 4, 7 };
                StringBuilder output = new StringBuilder();
                int minCounter = 0;
                for (int x = 0; x < decryptedString.Length; x++)
                {
                    char ch = (char)((int)(decryptedString[x]) - replaceStr[minCounter]);
                    output.Append(ch);
                    minCounter++;
                    if (minCounter >= 7)
                    {
                        minCounter = 0;
                    }
                }
                return output.ToString();
            }
            catch
            {
            }
            return "";
        }

        public string DecryptString(string encryptedString)
        {
            if (encryptedString == null)
            {
                return "";
            }
            try
            {
                int[] replaceStr = { 1, 5, 2, 3, 8, 4, 7 };
                StringBuilder output = new StringBuilder();
                int minCounter = 0;
                for (int x = 0; x < encryptedString.Length; x++)
                {
                    char ch = (char)((int)(encryptedString[x]) + replaceStr[minCounter]);
                    output.Append(ch);
                    minCounter++;
                    if (minCounter >= 7)
                    {
                        minCounter = 0;
                    }
                }
                return output.ToString();
            }
            catch
            {
            }
            return "";
        }

        public TimeSpan RoundToSeconds(TimeSpan timespan, int seconds = 1)
        {
            long offset = (timespan.Ticks >= 0) ? TimeSpan.TicksPerSecond / 2 : TimeSpan.TicksPerSecond / -2;
            return TimeSpan.FromTicks((timespan.Ticks + offset) / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond);
        }
    }
}
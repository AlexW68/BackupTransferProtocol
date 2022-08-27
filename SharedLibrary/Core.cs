using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Shared
{
    public sealed class Core
    {
        public string STOR_COMMAND = "STOR";
        public string VOL_LABEL = "BK*";
        public int BTP_PORT = 47440;
        public string END_COMMAND = "</BTPEND>";
        public string ROOT = @"C:\DemoBack";

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

        public List<DriveInfo> LoadDrives()
        {
            var drives = new List<DriveInfo>();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    drives.Add(drive);
                }
            }
            return drives;
        }



        public string WritePacket(UploadFile uploadFile)
        {
            string output = JsonConvert.SerializeObject(uploadFile);
            output += END_COMMAND;
            return output;
        }

        public void SendFile(UploadFile uploadFile, Socket client)
        {
            client.SendFile(uploadFile.FileName);
        }

        public bool ReceiveFile(UploadFile uploadFile, Socket client)
        {
            const int arSize = 100;
            byte[] buffer = new byte[arSize];
            SocketError errorCode;
            string filePath = ROOT;
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            filePath = Path.Combine(filePath, uploadFile.ComputerName);
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            filePath = Path.Combine(filePath, uploadFile.FileName.Substring(0, 1).ToUpper());
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            string restOfPath = Path.GetDirectoryName(uploadFile.FileName).Substring(3);
            filePath = Path.Combine(filePath, restOfPath);
            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            // by now we have the correct folder structure
            string justFileName = Path.GetFileName(uploadFile.FileName);
            string newFile = Path.Combine(filePath, justFileName);

            // TODO change how this works, bring the new file in as a temp file
            // and only if the checksum matches then do all the file renaming etc
            // 
            if (File.Exists(newFile) == true)
            {
                string oldFile = newFile + "." + DateTime.Now.ToString("HH_mm_ss_dd-MM-yyyy");
                FileInfo fi = new FileInfo(newFile);
                fi.Rename(oldFile);
            }
            Stream strm = new FileStream(newFile, FileMode.CreateNew);

            //ar stream = File.Create(@"C:\path\to\file.dat");
            var receiveBuffer = new byte[2048];
            int bytesLeftToReceive = uploadFile.FileLength;
            int totalReadBytes = 0;

            while (bytesLeftToReceive > 0)
            {
                //receive
                int bytesRead = client.Receive(receiveBuffer);
                if (bytesRead == 0)
                    throw new InvalidOperationException("Remote endpoint disconnected");

                //if the socket is used for other things after the file transfer
                //we need to make sure that we do not copy that data
                //to the file
                int bytesToCopy = Math.Min(bytesRead, bytesLeftToReceive);

                // write to file
                totalReadBytes += bytesToCopy;
                strm.Write(receiveBuffer, 0, bytesToCopy);

                //update our tracker.
                bytesLeftToReceive -= bytesToCopy;
            }

            //int readBytes = -1;
            //int blockCtr = 0;
            //int totalReadBytes = 0;
            //while (readBytes != 0 && totalReadBytes != uploadFile.FileLength)
            //{
            //    readBytes = client.Receive(buffer, 0, arSize, SocketFlags.None, out errorCode);
            //    blockCtr++;
            //    totalReadBytes += readBytes;
            //    strm.Write(buffer, 0, readBytes);
            //}
            strm.Close();
            if (totalReadBytes != uploadFile.FileLength)
            {
                Console.WriteLine(totalReadBytes.ToString());
                Console.WriteLine(uploadFile.FileLength.ToString());
                return false;
            }
            MD5 md5 = new MD5();
            string newChecksum = md5.Checksum(newFile);
            if (newChecksum != uploadFile.Checksum)
            {
                Console.WriteLine(newChecksum);
                Console.WriteLine(uploadFile.Checksum);
                return false;
            }
            else
            {
                // TOdO here is where the final file needs to be renamed and the other files
                // renamed if neccessay
                Console.WriteLine("Successfully copied " + newFile + " " + uploadFile.FileLength.ToString("###,###,###,###") + " bytes");
                return true;
            }
        }

        public UploadFile ReadPacket(string content)
        {
            string output = content.Substring(0, content.IndexOf(END_COMMAND));
            UploadFile uploadFile = JsonConvert.DeserializeObject<UploadFile>(output);
            return uploadFile;
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
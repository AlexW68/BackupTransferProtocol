using Shared;
using System.Net;

string fileName = @"c:\temp\demo\domain.csv";
IPAddress addr = IPAddress.Parse("192.168.1.200");
Client client = new Client();
MD5 md5 = new MD5();    

System.IO.FileInfo fi = new FileInfo(fileName);
UploadFile uploadFile = new();
uploadFile.FileLength = (int)fi.Length;
uploadFile.FileName = fileName;
uploadFile.RelativePath = false;
uploadFile.Checksum = md5.Checksum(fileName);
uploadFile.ComputerName = "WINDOWS11";
client.StartClient(uploadFile);
string test = "";


using Shared;
using System.Net;


// TODO work on the second parameter being wildcard including subdirectories

string ipAddress = args[0];
string fileName = args[1];	
IPAddress addr = IPAddress.Parse(ipAddress);
Client client = new Client();
MD5 md5 = new MD5();    

System.IO.FileInfo fi = new FileInfo(fileName);
UploadFile uploadFile = new();
uploadFile.FileLength = (int)fi.Length;
uploadFile.FileName = fileName;
uploadFile.RelativePath = false;
uploadFile.Checksum = md5.Checksum(fileName);
//uploadFile.ComputerName = "WebServer";
uploadFile.ComputerName = System.Environment.MachineName;
client.StartClient(uploadFile);


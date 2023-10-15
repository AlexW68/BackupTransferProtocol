using Shared;
using System.Net;


// TODO work on the second parameter being wildcard including subdirectories

string ipAddress = args[0];
string fileName = args[1];
bool wildcard = false;

if (fileName.IndexOf("*", 0) > -1) {
	wildcard = true;
}

IPAddress addr = IPAddress.Parse(ipAddress);
Client client = new();
MD5 md5 = new();

System.IO.FileInfo fi = new FileInfo(fileName);
UploadFile uploadFile = new();
uploadFile.FileLength = (int)fi.Length;
uploadFile.FileName = fileName;
uploadFile.RelativePath = false;
uploadFile.Checksum = md5.Checksum(fileName);
uploadFile.ComputerName = System.Environment.MachineName;
client.StartClient(uploadFile, ipAddress);
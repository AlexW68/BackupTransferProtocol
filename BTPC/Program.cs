using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

IPAddress addr = IPAddress.Parse("192.168.1.200");

Console.ReadLine();
Console.WriteLine("Backup Transfer Protocol Client");
TcpClient tcpClient = new TcpClient();
tcpClient.Connect(addr, 47440);



int byteCount = Encoding.ASCII.GetByteCount("Helloe");
byte[] sendData = new byte[byteCount];

NetworkStream stream = tcpClient.GetStream();
stream.Write(sendData, 0, sendData.Length);
stream.Close();
tcpClient.Close();


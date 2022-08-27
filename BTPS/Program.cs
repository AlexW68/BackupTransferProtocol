using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

Console.WriteLine("Backup Transfer Protocol Server");

List<DriveInfo> list = Core.Instance.LoadDrives();

Server server = new Server(Core.Instance.BTP_PORT);
server.StartListening();

Console.WriteLine("dfddfd");
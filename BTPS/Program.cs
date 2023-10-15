using Shared;
Console.WriteLine("Backup Transfer Protocol Server Version 1.0");
Server server = new Server(Core.Instance.BTP_PORT);
server.StartListening();

using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

Console.WriteLine("Backup Transfer Protocol Server");

IPAddress addr = IPAddress.Parse("192.168.1.200");
// set up listener on that address/port
TcpListener tcpListener = new TcpListener(addr, Core.Instance.BTP_PORT);
if (tcpListener != null)
{
    // start it up
    tcpListener.Start();
    // wait for a tcp client to connect
    TcpClient tcpClient = tcpListener.AcceptTcpClient();

    byte[] bytes = new byte[1024];
    // get the client stream
    NetworkStream clientStream = tcpClient.GetStream();
    StreamReader reader = new StreamReader(clientStream, Encoding.UTF8);
    //    TcpClient client = default(TcpClient);
    try
    {
        while (true)
        {
            //      client = server.AcceptTcpClient();
            byte[] receivedBuffer = new byte[1024];
            NetworkStream stream = tcpClient.GetStream();
            stream.Read(receivedBuffer, 0, receivedBuffer.Length);
            string msg = Encoding.ASCII.GetString(receivedBuffer, 0, receivedBuffer.Length);
            Console.Write(msg);
            //Console.Read();



        }
    } catch (Exception ex)
    {

        string test = ex.Message;
    }

}


//try
//    {
//        string request = reader.ReadToEnd();
//        Console.WriteLine(request);

//        // test the packet here

////        byte[] receivedBuffer = new byte[1024];
////        NetworkStream stream = client.GetStream();

//        // just send an acknowledgement
//        bytes = Encoding.UTF8.GetBytes("Thanks for the message!");
//        clientStream.Write(bytes, 0, bytes.Length);
//    }
//    finally
//    {
//        // close the reader
//        reader.Close();
//    }

    // stop listening
    tcpListener.Stop();
//}



//IPAddress ip = new IPAddress();
//IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0]; 
//TcpListener listener = new(ipAddress,  Core.Instance.BTP_PORT);


//IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
//TcpListener listener;
//try
//{
//    listener = new TcpListener(IPAddress.Any, 13);
//}
//catch (Exception e)
//{
//    Console.WriteLine(e.ToString());
//}
//
//Console.WriteLine();
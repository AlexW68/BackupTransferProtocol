using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Shared
{

    public class ServerStateObject
    {
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
        public Socket workSocket = null;
    }

    public class Server : IDisposable
    {

        private int _port = 47440;
        private Socket listener;

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public Server(int port)
        {
            _port = port;
        }

        public void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            Console.WriteLine("Server: " + ipHostInfo.HostName);
			string hostName = Dns.GetHostName();
			string myIP = "";
			foreach(IPAddress ip in Dns.GetHostEntry(hostName).AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					myIP = ip.ToString();	
				}
			}
//			string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
			IPAddress addr = IPAddress.Parse(myIP);
            IPEndPoint localEndPoint = new IPEndPoint(addr, _port);
            listener = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("Waiting for a connection on [" + myIP + "/" + _port.ToString() + "]");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            ServerStateObject state = new ServerStateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, ServerStateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            ServerStateObject state = (ServerStateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                content = state.sb.ToString();
                if (content.IndexOf(Core.Instance.END_COMMAND) > -1)
                {
                    UploadFile uploadFile = Core.Instance.ReadPacket(content);
                    //Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    bool ok = Core.Instance.ReceiveFile(uploadFile, handler);
                    if (ok == true)
                    {
                        Send(handler, "Ok");
                    } else
                    {
                        Send(handler, "Failed");
                    }
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, ServerStateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
            else
            {
                //        handler.Close();
            }
        }
        //}

        private void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
            listener.Dispose();
//            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{

    public class ClientStateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
    public class Client
    {

        private const int port = 47440;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private static String response = String.Empty;

        public void StartClient(UploadFile uploadFile)
        {
            try
            {
                IPAddress addr = IPAddress.Parse("192.168.1.200");
                IPEndPoint remoteEP = new IPEndPoint(addr, port);
                Socket client = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.SendTimeout = 1000000;

                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                string data = Core.Instance.WritePacket(uploadFile);
                Send(client, data);
                sendDone.WaitOne();

                // Receive the response from the remote device.  
                Receive(client);
                //                receiveDone.WaitOne();

                SendFile(uploadFile, client);

                Receive(client);
                receiveDone.WaitOne();


                //                sendDone.WaitOne();

                //                Console.WriteLine("Response received : {0}", response);

                // Release the socket.  

                //client.Shutdown(SocketShutdown.Both);
                //client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }




        public void SendFile(UploadFile uploadFile, Socket client)
        {
            //  client.SendFile(uploadFile.FileName);

            int lastStatus = 1;
            Console.WriteLine("Copying " + uploadFile.FileName);

            FileStream file = new FileStream(uploadFile.FileName, FileMode.Open); ;
            long totalBytes = file.Length, bytesSoFar = 0;
            byte[] filechunk = new byte[4096];
            int numBytes;
            while ((numBytes = file.Read(filechunk, 0, 4096)) > 0)
            {
                if (client.Send(filechunk, numBytes, SocketFlags.None) != numBytes)
                {
                    throw new Exception("Error in sending the file");
                }
                bytesSoFar += numBytes;
                Byte progress = (byte)(bytesSoFar * 10 / totalBytes);
                if (progress > lastStatus && progress != 10)
                {
                    Console.WriteLine("Progress: {0}0%, {1} total bytes", lastStatus, bytesSoFar.ToString("###,###,###,###"));
                    lastStatus = progress;
                }
            }
            Console.WriteLine("Progress: 100%, {1} total bytes", lastStatus, bytesSoFar.ToString("###,###,###,###"));


            //            client.BeginSendFile(uploadFile.FileName, new AsyncCallback(FileSendCallback), client);



            //sendDone.WaitOne(3000);
            //            sendDone.
            // client.Shutdown(SocketShutdown.Both);
            // client.Close();


            //sendDone.WaitOne();
        }

        public void FileSendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            client.EndSendFile(ar);
            sendDone.Set();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                Console.WriteLine("Backup Transfer Protocol Client connected to {0}", client.RemoteEndPoint.ToString());
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                ClientStateObject state = new ClientStateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, ClientStateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                ClientStateObject state = (ClientStateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, ClientStateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

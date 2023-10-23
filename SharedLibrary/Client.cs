using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Shared {

	public class ClientStateObject {
		public Socket workSocket = null;
		public const int BufferSize = 256;
		public byte[] buffer = new byte[BufferSize];
		public StringBuilder sb = new StringBuilder();
	}
	public class Client {

		private const int port = 47440;

		// ManualResetEvent instances signal completion.  
		private static ManualResetEvent connectDone =
			new ManualResetEvent(false);
		private static ManualResetEvent sendDone =
			new ManualResetEvent(false);
		private static ManualResetEvent receiveDone =
			new ManualResetEvent(false);

		private static String response = String.Empty;

		public void StartClient(UploadFile uploadFile, string ipAddress, bool verbose) {
			try {
				IPAddress addr = IPAddress.Parse(ipAddress);
				IPEndPoint remoteEP = new IPEndPoint(addr, port);
				Socket client = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				client.SendTimeout = 1000000;

				client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
				connectDone.WaitOne();
				if (verbose == true) {
					Console.WriteLine("Connected to BTPS on {0}", client.RemoteEndPoint.ToString());
				}
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

			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		public void SendFile(UploadFile uploadFile, Socket client) {
			//  client.SendFile(uploadFile.FileName);

			int lastStatus = 1;
			Console.WriteLine("Copying " + uploadFile.FileName);

			FileStream file = new FileStream(uploadFile.FileName, FileMode.Open); ;
			long totalBytes = file.Length, bytesSoFar = 0;
			byte[] filechunk = new byte[4096];
			int numBytes;
			while (( numBytes = file.Read(filechunk, 0, 4096) ) > 0) {
				if (client.Send(filechunk, numBytes, SocketFlags.None) != numBytes) {
					throw new Exception("Error in sending the file");
				}
				bytesSoFar += numBytes;
				Byte progress = (byte)( bytesSoFar * 10 / totalBytes );
				if (progress > lastStatus && progress != 10) {
					Console.WriteLine("Progress: {0}0%, {1} total bytes", lastStatus, bytesSoFar.ToString("###,###,###,###"));
					lastStatus = progress;
				}
			}
			Console.WriteLine("Progress: 100%, {1} total bytes", lastStatus, bytesSoFar.ToString("###,###,###,###"));
		}

		public void FileSendCallback(IAsyncResult ar) {
			if (ar.AsyncState != null) {
				Socket client = (Socket)ar.AsyncState;
				if (client != null) {
					client.EndSendFile(ar);
				}
			}
			sendDone.Set();
		}

		private void ConnectCallback(IAsyncResult ar) {
			try {
				Socket client = (Socket)ar.AsyncState;
				client.EndConnect(ar);
				Console.WriteLine("Backup Transfer Protocol Client connected to {0}", client.RemoteEndPoint.ToString());
				connectDone.Set();
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private void Receive(Socket client) {
			try {
				ClientStateObject state = new ClientStateObject();
				state.workSocket = client;
				client.BeginReceive(state.buffer, 0, ClientStateObject.BufferSize, 0,
					new AsyncCallback(ReceiveCallback), state);
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private void ReceiveCallback(IAsyncResult ar) {
			try {
				ClientStateObject state = (ClientStateObject)ar.AsyncState;
				Socket client = state.workSocket;
				int bytesRead = client.EndReceive(ar);
				if (bytesRead > 0) {
					state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
					client.BeginReceive(state.buffer, 0, ClientStateObject.BufferSize, 0,
						new AsyncCallback(ReceiveCallback), state);
				} else {
					if (state.sb.Length > 1) {
						response = state.sb.ToString();
					}
					receiveDone.Set();
				}
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private void Send(Socket client, String data) {
			byte[] byteData = Encoding.ASCII.GetBytes(data);
			client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
		}

		private void SendCallback(IAsyncResult ar) {
			try {
				Socket client = (Socket)ar.AsyncState;
				int bytesSent = client.EndSend(ar);
				sendDone.Set();
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}
	}
}
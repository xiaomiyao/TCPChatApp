using System.Net;
using System.Net.Sockets;

namespace TCPChatApp.Server
{
    // responsibility to handle new connectons 


    public class ChatServer(AuthenticationHandler authHandler, ChatMessageHandler chatMessageHandler, ClientCoordinator clientCoordinator)
    {
        // 🔌 TCP listener and server state
        private TcpListener _listener;
        private bool _isRunning;

        // 🚀 Start the server on specified port
        public void Start(int port = 5000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"Server started on port {port}.");

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        // 👥 Accept and handle new client connections
        private void AcceptClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("➡️ Client connected!");

                    var handler = new ClientHandler(client, authHandler, chatMessageHandler, clientCoordinator);

                    clientCoordinator.AddClient(handler);
                    Thread clientThread = new(handler.HandleClient);
                    clientThread.Start();
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }

        // 🛑 Stop the server
        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }
    }
}

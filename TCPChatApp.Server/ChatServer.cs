﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPChatApp.Server
{
    public class ChatServer
    {
        private TcpListener _listener;
        private bool _isRunning;

        public void Start(int port = 5000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"Server started on port {port}.");

            // Accept clients on a new thread or via async
            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private void AcceptClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    Console.WriteLine("New client connected!");

                    // Hand off client to ClientHandler
                    var handler = new ClientHandler(client);
                    Thread clientThread = new Thread(handler.HandleClient);
                    clientThread.Start();
                }
                catch (SocketException)
                {
                   
                    break;
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }
    }
}

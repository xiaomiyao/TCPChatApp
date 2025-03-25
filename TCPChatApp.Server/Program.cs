namespace TCPChatApp.Server
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Starting TCP Chat Server...");

            var server = new ChatServer();
            server.Start();

            Console.WriteLine("Server is running. Press any key to stop...");
            Console.ReadKey();

            server.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}
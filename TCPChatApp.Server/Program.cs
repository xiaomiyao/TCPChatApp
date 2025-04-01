using Microsoft.Extensions.DependencyInjection;
using TCPChatApp.Server.DataAccess;


namespace TCPChatApp.Server
{
    internal static class Program
    {
        private static void Main()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var server = serviceProvider.GetRequiredService<ChatServer>();
            Console.WriteLine("Starting TCP Chat Server...");
            server.Start();

            Console.WriteLine("Server is running. Press any key to stop...");
            Console.ReadKey();

            server.Stop();
            Console.WriteLine("Server stopped.");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(provider =>
                new UserRepository("Server=MSI; Database=TCPChatApp; Trusted_Connection=True; Encrypt=True; TrustServerCertificate=True;"));

            services.AddSingleton<ChatServer>();
            services.AddSingleton<ClientCoordinator>();
            services.AddSingleton<ChatMessageHandler>();
            services.AddSingleton<AuthenticationHandler>();
            services.AddSingleton<RelationRepository>(provider =>
                new RelationRepository("Server=MSI; Database=TCPChatApp; Trusted_Connection=True; Encrypt=True; TrustServerCertificate=True;"));

        }
    }
}
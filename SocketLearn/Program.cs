using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Socket;

internal class Program
{
    private static readonly string address = "127.0.0.1";
    private static readonly int port = 8888; // порт для приема входящих запросов

    private static async Task Main()
    {
        Console.WriteLine("Choise:");
        Console.WriteLine("1. Server");
        Console.WriteLine("2. Client");
        Console.WriteLine("0. Exit");
        if (int.TryParse(Console.ReadLine(), out int choise))
        {
            try
            {
                switch (choise)
                {
                    case 1:
                    {
                        Server server = new(address, port);
                        await server.StartAsync();

                        server.Close();
                        break;
                    }
                    case 2:
                    {
                        Console.Write("Enter your user name: ");
                        string? userName = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(userName))
                        {
                            throw new Exception("User name is not null or empty");
                        }

                        User user = new(userName);

                        Client client = new(address, port, user);
                        await client.StartChatingAsync();
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Choise {choise}");
                Console.WriteLine(ex.Message);
            }
        }
        Console.WriteLine("Exit");
        Console.ReadKey();
    }

    public static async Task ClientNewAsync()
    {
        try
        {
            TcpClient client = new();
            await client.ConnectAsync(IPAddress.Parse(address), port);

            NetworkStream stream = client.GetStream();

            // Send message to server
            string message = Console.ReadLine() ?? "";
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            await stream.WriteAsync(buffer);

            // Receive message
            byte[] msgFromServerByte = new byte[256];

            StringBuilder msgFromServerBuilder = new();

            do
            {
                int count = await stream.ReadAsync(msgFromServerByte);
                msgFromServerBuilder.Append(Encoding.UTF8.GetString(msgFromServerByte, 0, count));
            } while (stream.DataAvailable);

            string msgFromServer = msgFromServerBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(msgFromServer))
            {
                Console.WriteLine($"Message from server: '{msgFromServer}'");
            }

            // Closing
            stream.Close();
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public static async Task ServerNewAsync()
    {
        TcpListener? server = null;
        try
        {
            server = new(IPAddress.Parse(address), port);
            server.Start();

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Client connected");

                NetworkStream stream = client.GetStream();

                byte[] msgFromClientByte = new byte[256];
                StringBuilder msgFromClientBuilder = new();
                do
                {
                    int count = await stream.ReadAsync(msgFromClientByte);
                    msgFromClientBuilder.Append(Encoding.UTF8.GetString(msgFromClientByte, 0, count));
                } while (stream.DataAvailable);

                string msgFromClient = msgFromClientBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(msgFromClient))
                {
                    Console.WriteLine(msgFromClient);
                }

                // Closing
                stream.Close();
                client.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (server != null)
            {
                server.Stop();
            }
        }
    }
}

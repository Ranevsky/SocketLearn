using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Socket;

public class Server
{
    private readonly TcpListener _server;
    private readonly List<ClientFromServer> _clients = new();

    public Server(string address, int port)
        : this(new IPEndPoint(IPAddress.Parse(address), port))
    {

    }
    public Server(IPEndPoint endPoint)
    {
        _server = new TcpListener(endPoint);
    }

    public void AddClient(ClientFromServer client)
    {
        _clients.Add(client);
    }
    public void RemoveClient(ClientFromServer client)
    {
        _clients.Remove(client);
    }
    public async Task StartAsync()
    {
        _server.Start(10);
        Console.WriteLine("Server started and waiting...");

        while (true)
        {
            TcpClient client = await _server.AcceptTcpClientAsync();
            ClientFromServer clientFromServer = new(client, this);
            AddClient(clientFromServer);

            // TODO: Handle in new threads
            // ...
            Thread thread = new(new ThreadStart(async () => await clientFromServer.HandleAsync()));
            thread.Start();
        }
    }
    public async Task SendAllAsync(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        foreach (ClientFromServer? client in _clients)
        {
            await client.Stream.WriteAsync(bytes);
        }
    }
    public async Task SendAllWithoutMeAsync(string message, ClientFromServer withoutClient)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        foreach (ClientFromServer? client in _clients)
        {
            if (client == withoutClient)
            {
                continue;
            }
            Console.WriteLine($"Send {client.User.Name} message: '{message}'");
            await client.Stream.WriteAsync(bytes);
        }
    }

    public void Close()
    {
        foreach (ClientFromServer? client in _clients)
        {
            client.Close();
        }
        _server.Stop();
    }
}

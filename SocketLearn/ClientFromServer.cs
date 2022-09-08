using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Socket;

public class ClientFromServer
{
    private readonly TcpClient _client;

    // Read from client | Send to client
    public readonly NetworkStream Stream;
    private readonly Server _server;

    public User User = null!;

    public ClientFromServer(TcpClient client, Server server)
    {
        _client = client;
        Stream = client.GetStream();
        _server = server;
    }

    // Entry method
    public async Task HandleAsync()
    {
        await JoinHandleAsync();

        string joinMessage = $"[{User.Id}] {User.Name} joined";
        await _server.SendAllAsync(joinMessage);

        try
        {
            while (true)
            {
                string msg = await ReceiveAsync();
                msg = $"{User.Name}: {msg}";

                await _server.SendAllWithoutMeAsync(msg, this);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _server.RemoveClient(this);
            Close();
        }
    }
    private async Task JoinHandleAsync()
    {
        string json = await ReceiveAsync();

        User = JsonSerializer.Deserialize<User>(json)
            ?? throw new Exception("Json is not convertible");
    }

    public async Task<string> ReceiveAsync()
    {
        byte[] buffer = new byte[256];
        StringBuilder msgBuilder = new();

        do
        {
            int count = await Stream.ReadAsync(buffer);
            msgBuilder.Append(Encoding.UTF8.GetString(buffer, 0, count));
        } while (Stream.DataAvailable);

        return msgBuilder.ToString();
    }
    public void Close()
    {
        if (Stream != null)
        {
            Stream.Close();
        }

        if (_client != null)
        {
            _client.Close();
        }
    }
}
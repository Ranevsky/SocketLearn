using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Socket;

public class Client
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly User _user;

    public Action<string> GetMessage = (msg) => Console.WriteLine(msg);

    public Client(string address, int port, User user)
    {
        _user = user;
        _client = new(address, port);
        _stream = _client.GetStream();
    }

    public async Task SendAsync(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await _stream.WriteAsync(bytes);
    }
    public async Task<string> RecieveAsync()
    {
        byte[] buffer = new byte[256];
        StringBuilder msgBuilder = new();

        do
        {
            int count = await _stream.ReadAsync(buffer);
            msgBuilder.Append(Encoding.UTF8.GetString(buffer, 0, count));
        } while (_stream.DataAvailable);

        return msgBuilder.ToString();
    }
    public async Task StartChatingAsync()
    {
        // First message is json User object

        string json = JsonSerializer.Serialize(_user);
        await SendAsync(json);

        Thread thread = new(
            new ThreadStart(async () =>
            {
                while (true)
                {
                    GetMessage(await RecieveAsync());
                }
            }));

        thread.Start();

        await Task.Delay(100);
        while (true)
        {
            Console.Write($"{_user.Name}: ");
            string message = Console.ReadLine() ?? "";
            await SendAsync(message);
        }
    }
}

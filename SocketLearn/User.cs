namespace Socket;


[Serializable]
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public User(string name)
    {
        Name = name;
    }
}
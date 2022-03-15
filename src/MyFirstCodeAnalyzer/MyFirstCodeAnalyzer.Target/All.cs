namespace MyFirstCodeAnalyzer.Target;

public record SessionId(string Value);

public class SessionIdUser
{
    private readonly string sessionId;

    public SessionIdUser(string sessionId)
    {
        this.sessionId = sessionId;
    }

    public string SessionId => $"Session id: {sessionId}";
}
namespace CalculatorBridge.Demo.Services;

public sealed class DemoAuthService
{
    public const string Username = "demo@nopdemo.com";
    public const string Password = "demo123";

    private readonly HashSet<string> _tokens = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public string? Login(string? username, string? password)
    {
        if (!string.Equals(username, Username, StringComparison.Ordinal) ||
            !string.Equals(password, Password, StringComparison.Ordinal))
        {
            return null;
        }

        var token = Guid.NewGuid().ToString("N");
        lock (_gate)
            _tokens.Add(token);

        return token;
    }

    public bool IsValid(string token)
    {
        lock (_gate)
            return _tokens.Contains(token);
    }

    public void Revoke(string token)
    {
        lock (_gate)
            _tokens.Remove(token);
    }
}

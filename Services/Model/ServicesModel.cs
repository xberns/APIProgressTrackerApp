public class JwtSettings
{
    public string Key { get; set; }
    public string Issuer { get; set; }
}

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Pass { get; set; }
}

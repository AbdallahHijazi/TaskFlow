namespace TaskFlow.Infrastructure.Notifications;

public class SmtpOptions
{
    public const string SectionName = "Smtp";
    public bool Enabled { get; set; }
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = "ORQIST Notifications";
    public string AppUrl { get; set; } = "http://localhost:4200";
}

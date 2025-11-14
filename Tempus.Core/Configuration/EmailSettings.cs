namespace Tempus.Core.Configuration;

public class EmailSettings
{
    /// <summary>
    /// Email provider type (SMTP or SendGrid)
    /// </summary>
    public EmailProvider Provider { get; set; } = EmailProvider.SMTP;

    /// <summary>
    /// SMTP server hostname
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (typically 587 for TLS, 465 for SSL, 25 for non-encrypted)
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username (usually your email address)
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password or app-specific password
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Enable SSL/TLS for SMTP
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// SendGrid API Key (if using SendGrid)
    /// </summary>
    public string SendGridApiKey { get; set; } = string.Empty;

    /// <summary>
    /// From email address (sender)
    /// </summary>
    public string FromEmail { get; set; } = "noreply@tempus-app.com";

    /// <summary>
    /// From display name
    /// </summary>
    public string FromName { get; set; } = "Tempus";

    /// <summary>
    /// Whether email sending is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;
}

public enum EmailProvider
{
    SMTP,
    SendGrid
}

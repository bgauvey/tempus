# Email Configuration Guide

This guide explains how to configure email sending in Tempus for team invitations and notifications.

## Configuration Location

Email settings are configured in `appsettings.json` under the `EmailSettings` section.

## General Settings

```json
{
  "EmailSettings": {
    "Enabled": true,              // Set to true to enable email sending
    "Provider": "SMTP",            // "SMTP" or "SendGrid"
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Tempus"
  }
}
```

## SMTP Configuration

### Gmail

**Important:** You must use an App Password, not your regular Gmail password.

1. Enable 2-Factor Authentication on your Google account
2. Go to https://myaccount.google.com/apppasswords
3. Generate an app password for "Mail"
4. Use that password in the configuration

```json
{
  "EmailSettings": {
    "Enabled": true,
    "Provider": "SMTP",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-16-char-app-password",
    "EnableSsl": true,
    "FromEmail": "your-email@gmail.com",
    "FromName": "Tempus"
  }
}
```

### Microsoft 365 / Outlook.com

```json
{
  "EmailSettings": {
    "Enabled": true,
    "Provider": "SMTP",
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@outlook.com",
    "SmtpPassword": "your-password",
    "EnableSsl": true,
    "FromEmail": "your-email@outlook.com",
    "FromName": "Tempus"
  }
}
```

### SendGrid

1. Sign up for SendGrid at https://sendgrid.com
2. Create an API key in the SendGrid dashboard
3. Use the API key in configuration

```json
{
  "EmailSettings": {
    "Enabled": true,
    "Provider": "SendGrid",
    "SendGridApiKey": "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "Tempus"
  }
}
```

### Other SMTP Providers

#### Amazon SES
```json
{
  "SmtpServer": "email-smtp.us-east-1.amazonaws.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-smtp-username",
  "SmtpPassword": "your-smtp-password",
  "EnableSsl": true
}
```

#### Mailgun
```json
{
  "SmtpServer": "smtp.mailgun.org",
  "SmtpPort": 587,
  "SmtpUsername": "postmaster@your-domain.mailgun.org",
  "SmtpPassword": "your-password",
  "EnableSsl": true
}
```

#### Custom SMTP Server
```json
{
  "SmtpServer": "mail.yourdomain.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-username",
  "SmtpPassword": "your-password",
  "EnableSsl": true
}
```

## Testing Email Configuration

1. Set `Enabled: true` in your configuration
2. Configure your SMTP or SendGrid settings
3. Restart the application
4. Create a team and invite a member
5. Check the application logs to verify email was sent
6. Check your email inbox for the invitation

## Troubleshooting

### Emails not sending
- Verify `Enabled: true`
- Check SMTP credentials are correct
- For Gmail, ensure you're using an App Password
- Check firewall settings allow SMTP connections
- Review application logs for error messages

### Authentication failures
- Double-check username and password
- Ensure SSL is enabled if required by your provider
- Verify the SMTP port is correct (587 for TLS, 465 for SSL)

### Emails going to spam
- Configure SPF and DKIM records for your domain
- Use a verified sender email address
- Consider using a dedicated email service like SendGrid

## Security Best Practices

1. **Never commit passwords to source control**
   - Use environment variables or secrets management
   - Use User Secrets in development: `dotnet user-secrets set "EmailSettings:SmtpPassword" "your-password"`

2. **Use App-Specific Passwords**
   - For Gmail and other providers, use app-specific passwords

3. **Enable SSL/TLS**
   - Always use `EnableSsl: true` for production

4. **Rotate Credentials**
   - Regularly rotate SMTP passwords and API keys

## Development vs Production

### Development
- Set `Enabled: false` to log emails to console instead of sending
- Useful for testing without sending real emails

### Production
- Set `Enabled: true`
- Use environment-specific configuration files
- Store credentials securely (Azure Key Vault, AWS Secrets Manager, etc.)

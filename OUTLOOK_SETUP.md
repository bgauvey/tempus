# Microsoft Outlook/Office 365 Integration Setup Guide

This guide walks you through registering an Azure AD application to enable Outlook calendar integration in Tempus.

## Prerequisites

- Azure account (free tier is sufficient)
- Admin access to Azure Active Directory (for enterprise apps)
- OR a personal Microsoft account (for personal calendar integration)

## Step 1: Register Azure AD Application

### 1.1 Navigate to Azure Portal
1. Go to [https://portal.azure.com](https://portal.azure.com)
2. Sign in with your Microsoft account

### 1.2 Create App Registration
1. In the left sidebar, click **Azure Active Directory** (or search for it)
2. Click **App registrations** in the left menu
3. Click **+ New registration** at the top

### 1.3 Configure Application
Fill in the registration form:

**Name**: `Tempus Calendar Integration` (or any name you prefer)

**Supported account types**: Choose one:
- **Accounts in any organizational directory and personal Microsoft accounts** - Recommended for maximum compatibility
- **Accounts in this organizational directory only** - For single tenant/organization
- **Personal Microsoft accounts only** - For personal calendars only

**Redirect URI**:
- Platform: **Web**
- URI: `https://localhost:7001/outlook-callback` (for development)
- Note: For production, use your actual domain: `https://your-domain.com/outlook-callback`

Click **Register**

### 1.4 Note Your Application (Client) ID
After registration, you'll see the **Overview** page:
- Copy the **Application (client) ID** - You'll need this for `Outlook:ClientId`
- Copy the **Directory (tenant) ID** - You'll need this for `Outlook:TenantId`

## Step 2: Create Client Secret

1. In your app registration, click **Certificates & secrets** in the left menu
2. Click **+ New client secret**
3. Add a description: `Tempus Integration Secret`
4. Choose expiration: **24 months** (recommended)
5. Click **Add**
6. **IMPORTANT**: Copy the **Value** immediately! It will only be shown once.
   - This is your `Outlook:ClientSecret`

## Step 3: Configure API Permissions

1. Click **API permissions** in the left menu
2. Click **+ Add a permission**
3. Select **Microsoft Graph**
4. Select **Delegated permissions**
5. Search for and add these permissions:
   - `Calendars.ReadWrite` - Read and write user calendars
   - `offline_access` - Maintain access to data you have given it access to
6. Click **Add permissions**

### Optional: Grant Admin Consent
If this is for an organization:
1. Click **Grant admin consent for [Your Organization]**
2. Click **Yes** to confirm
3. You should see green checkmarks under "Status"

## Step 4: Update Tempus Configuration

Open `Tempus.Web/appsettings.json` and update the Outlook section:

```json
{
  "Outlook": {
    "ClientId": "YOUR_APPLICATION_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET_VALUE",
    "TenantId": "YOUR_TENANT_ID_OR_COMMON"
  }
}
```

### TenantId Options:
- **"common"** - Allows both personal and work/school accounts (recommended)
- **"organizations"** - Only work/school accounts
- **"consumers"** - Only personal Microsoft accounts
- **"your-tenant-id"** - Specific Azure AD tenant only

### Example Configuration:

```json
{
  "Outlook": {
    "ClientId": "12345678-1234-1234-1234-123456789abc",
    "ClientSecret": "abc123~XyZ.123456789abcdefghijklmnop",
    "TenantId": "common"
  }
}
```

## Step 5: Update Redirect URIs for Production

When deploying to production:

1. Go back to your Azure app registration
2. Click **Authentication** in the left menu
3. Under **Web** → **Redirect URIs**, click **+ Add URI**
4. Add your production URL: `https://your-production-domain.com/outlook-callback`
5. Click **Save**

## Step 6: Test the Integration

1. Start your Tempus application
2. Log in to Tempus
3. Navigate to **Settings** → **Integrations** tab
4. Click **Connect Outlook Calendar**
5. Sign in with your Microsoft account
6. Grant permissions when prompted
7. You should be redirected back to Tempus with a success message

## Troubleshooting

### "Redirect URI mismatch" error
- Ensure the redirect URI in Azure matches exactly what Tempus is using
- Check for http vs https
- Check for trailing slashes

### "Invalid client secret" error
- The client secret may have expired (check expiration in Azure portal)
- Generate a new client secret and update appsettings.json

### "AADSTS70011: Invalid scope" error
- Ensure you've added the required API permissions in Azure
- Try granting admin consent

### "Insufficient privileges" error
- Verify `Calendars.ReadWrite` permission is added
- Check that admin consent was granted (for work/school accounts)

### Configuration not found error
- Ensure appsettings.json has the Outlook section
- Restart the application after updating configuration
- Check that configuration values don't have extra spaces or quotes

## Security Best Practices

1. **Never commit credentials to source control**
   - Use User Secrets for development: `dotnet user-secrets set "Outlook:ClientId" "your-value"`
   - Use Azure Key Vault or environment variables for production

2. **Rotate client secrets regularly**
   - Set expiration to 24 months or less
   - Create a new secret before the old one expires

3. **Use specific tenant ID in production**
   - If possible, use your specific tenant ID instead of "common"
   - This adds an extra layer of security

4. **Monitor application usage**
   - Regularly check Azure AD sign-in logs
   - Review consent grants periodically

## Additional Resources

- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [Microsoft Graph API Calendars](https://docs.microsoft.com/en-us/graph/api/resources/calendar)
- [MSAL.NET Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)

## Support

If you encounter issues not covered in this guide:
1. Check the Tempus application logs
2. Review Azure AD sign-in logs in the Azure portal
3. Verify all configuration values are correct
4. Ensure network/firewall allows connections to Microsoft services

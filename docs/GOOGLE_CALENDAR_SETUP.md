# Google Calendar Integration Setup Guide

This guide will walk you through setting up Google Calendar integration with Tempus.

## Prerequisites

1. A Google account
2. Access to Google Cloud Console
3. Tempus application running locally or deployed

## Step 1: Create Google Cloud Project

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Click on the project dropdown and select "New Project"
3. Name your project (e.g., "Tempus Calendar Integration")
4. Click "Create"

## Step 2: Enable Google Calendar API

1. In the Google Cloud Console, make sure your new project is selected
2. Navigate to "APIs & Services" > "Library"
3. Search for "Google Calendar API"
4. Click on it and press "Enable"

## Step 3: Configure OAuth Consent Screen

1. Go to "APIs & Services" > "OAuth consent screen"
2. Choose "External" user type (unless you have a Google Workspace)
3. Click "Create"
4. Fill in the required information:
   - App name: `Tempus`
   - User support email: Your email
   - Developer contact email: Your email
5. Click "Save and Continue"
6. On the "Scopes" page, click "Add or Remove Scopes"
7. Add the following scope:
   - `https://www.googleapis.com/auth/calendar` (See, edit, share, and permanently delete all calendars)
8. Click "Update" then "Save and Continue"
9. Add test users if needed (your email address)
10. Click "Save and Continue"

## Step 4: Create OAuth 2.0 Credentials

1. Go to "APIs & Services" > "Credentials"
2. Click "Create Credentials" > "OAuth client ID"
3. Select "Web application" as the application type
4. Name it (e.g., "Tempus Web Client")
5. Add Authorized redirect URIs:
   - For local development: `http://localhost:5000/google-callback` (adjust port if needed)
   - For production: `https://yourdomain.com/google-callback`
6. Click "Create"
7. Copy the **Client ID** and **Client Secret** (you'll need these in the next step)

## Step 5: Configure Tempus Application

1. Open `appsettings.json` in the Tempus.Web project
2. Update the Google Calendar configuration with your credentials:

```json
{
  "GoogleCalendar": {
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
  }
}
```

**Important:** For production, use environment variables or Azure Key Vault instead of storing credentials in appsettings.json

### Using Environment Variables (Recommended for Production)

Set the following environment variables:

```bash
export GoogleCalendar__ClientId="YOUR_CLIENT_ID_HERE"
export GoogleCalendar__ClientSecret="YOUR_CLIENT_SECRET_HERE"
```

Or in Windows:

```powershell
$env:GoogleCalendar__ClientId="YOUR_CLIENT_ID_HERE"
$env:GoogleCalendar__ClientSecret="YOUR_CLIENT_SECRET_HERE"
```

## Step 6: Database Migration

The Google Calendar integration requires database changes. Run the following commands:

```bash
cd Tempus.Infrastructure
dotnet ef migrations add AddCalendarIntegrationUserId
dotnet ef database update
```

Or if the application is already running, it will automatically apply migrations on startup.

## Step 7: Test the Integration

1. Start the Tempus application
2. Log in to your account
3. Navigate to "Integrations" page
4. Click "Connect Google Calendar"
5. You'll be redirected to Google's authorization page
6. Grant the requested permissions
7. You'll be redirected back to Tempus
8. Your Google Calendar is now connected!

## Using the Integration

### Sync Events

Once connected, you can:

1. **Manual Sync**: Click "Sync Now" on the Integrations page to sync events between Google Calendar and Tempus
2. **View Sync Status**: See when the last sync occurred
3. **Disconnect**: Remove the integration at any time (this won't delete any events)

### What Gets Synced

- **From Google to Tempus**: All events from your primary Google Calendar for the past month and next 6 months
- **From Tempus to Google**: All events in Tempus for the same date range
- **Two-way Updates**: Changes made in either calendar will sync to the other on the next sync

### Event Matching

Events are matched using a Google ID stored in the event description. This prevents duplicate events when syncing.

## Troubleshooting

### "Google Calendar credentials not configured" Error

Make sure you've added the Client ID and Client Secret to your appsettings.json or environment variables.

### "Invalid redirect URI" Error

Ensure that the redirect URI in your Google Cloud Console matches exactly with your application URL + `/google-callback`.

### Token Expiration

Access tokens expire after 1 hour. The application automatically refreshes them using the refresh token. If you see authentication errors, try disconnecting and reconnecting your Google Calendar.

### Sync Issues

If events aren't syncing:
1. Check that the integration is enabled
2. Look at the last sync time
3. Try manually syncing again
4. Check application logs for detailed error messages

## Security Best Practices

1. **Never commit credentials**: Don't commit `appsettings.json` with real credentials to source control
2. **Use environment variables**: Store credentials in environment variables or a secure vault
3. **Rotate secrets**: Periodically rotate your Client Secret in Google Cloud Console
4. **Monitor access**: Regularly review which applications have access to your Google Calendar
5. **HTTPS only**: Always use HTTPS in production to protect OAuth tokens

## API Rate Limits

Google Calendar API has the following limits:
- 1,000,000 queries per day
- 10 queries per second per user

The Tempus integration is designed to stay well within these limits through:
- Incremental sync (only syncing changes)
- Sync tokens to track changes
- Caching of calendar metadata

## Additional Resources

- [Google Calendar API Documentation](https://developers.google.com/calendar)
- [OAuth 2.0 for Web Server Applications](https://developers.google.com/identity/protocols/oauth2/web-server)
- [Google Calendar API Scopes](https://developers.google.com/calendar/api/auth)

## Support

If you encounter any issues, please:
1. Check the application logs
2. Review this setup guide
3. Open an issue on the GitHub repository

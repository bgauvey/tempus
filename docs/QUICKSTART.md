# Tempus Quick Start Guide

Get up and running with Tempus in 5 minutes!

## Step 1: Install .NET 9 SDK

If you don't have .NET 9 installed:

1. Visit [https://dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Download the SDK for your operating system
3. Run the installer
4. Verify installation by opening a terminal and running:
   ```bash
   dotnet --version
   ```
   You should see version 9.0.x

## Step 2: Extract and Navigate

1. Extract the Tempus folder to your desired location
2. Open a terminal/command prompt
3. Navigate to the folder:
   ```bash
   cd path/to/Tempus
   ```

## Step 3: Run the Application

### Option A: Using Terminal/Command Line

```bash
cd Tempus.Web
dotnet run
```

Wait for the message "Now listening on: https://localhost:7001"

### Option B: Using Visual Studio 2022

1. Double-click `Tempus.sln` to open in Visual Studio
2. Wait for the solution to load and restore packages
3. Press `F5` or click the green "Run" button
4. The application will open in your default browser

### Option C: Using Visual Studio Code

1. Open VS Code
2. File → Open Folder → Select the Tempus folder
3. Open the integrated terminal (Ctrl + `)
4. Run:
   ```bash
   cd Tempus.Web
   dotnet run
   ```

## Step 4: Access the Application

Open your web browser and navigate to:
- **HTTPS**: `https://localhost:7001`
- **HTTP**: `http://localhost:5000`

You'll see the Tempus dashboard!

## First Steps

### Create Your First Event

1. Click "Create Event" button on the dashboard
2. Fill in the details:
   - **Title**: "Team Meeting"
   - **Start Time**: Tomorrow at 10:00 AM
   - **End Time**: Tomorrow at 11:00 AM
   - **Event Type**: Meeting
   - **Priority**: High
3. Click "Save"

### Import Events from Your Calendar

1. Export your calendar as ICS:
   - **Google Calendar**: Settings → Import & Export → Export
   - **Outlook**: File → Save Calendar → iCalendar format
   - **Apple Calendar**: File → Export → Export
2. In Tempus, click "Import ICS" in the sidebar
3. Upload your ICS file
4. Review the events and click "Save All Events"

### Explore the Calendar

1. Click "Calendar" in the sidebar
2. Navigate between months using the arrows
3. Click on any date to see events for that day
4. Use "Jump to date" to quickly navigate to a specific date

## Common Issues

### "Port already in use" Error

If you see this error, another application is using port 5000 or 7001.

**Solution**:
1. Open `Tempus.Web/Properties/launchSettings.json`
2. Change the port numbers:
   ```json
   "applicationUrl": "https://localhost:7002;http://localhost:5001"
   ```
3. Run again

### Database Errors

If you encounter database errors:

**Solution**:
1. Stop the application (Ctrl + C)
2. Delete the `tempus.db` file in the Tempus.Web folder
3. Run the application again (it will recreate the database)

### Package Restore Issues

If packages fail to restore:

**Solution**:
```bash
dotnet nuget locals all --clear
dotnet restore
```

## What's Included

- ✅ **Dashboard**: Overview of your schedule
- ✅ **Events Page**: Create, edit, delete events
- ✅ **Calendar View**: Visual monthly calendar
- ✅ **ICS Import**: Import from other calendar apps
- ✅ **Search**: Find events quickly
- ✅ **Event Types**: Meetings, Tasks, Time Blocks, etc.
- ✅ **Priority Levels**: Low, Medium, High, Urgent

## Next Steps

- Read the full [README.md](README.md) for detailed documentation
- Check out the roadmap for upcoming features
- Customize the application for your needs

## Need Help?

- Check the README.md for troubleshooting
- Review the code documentation
- Open an issue if you find bugs

---

**Welcome to Tempus!** 🎉 Start managing your time more effectively today.

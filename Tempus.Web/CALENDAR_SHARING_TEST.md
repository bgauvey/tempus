# Calendar Sharing API Test Guide

This guide shows how to test the calendar sharing functionality using the test API endpoints.

## Prerequisites

1. Application is running (http://localhost:5000)
2. You're logged in (authentication required)
3. You have at least one calendar created
4. You have another user account to share with

## Test Endpoints

All endpoints require authentication. Use your browser's cookies or add Authorization header.

Base URL: `http://localhost:5000/api/test/calendar-sharing`

### 1. Get Your Calendars

First, get your calendar IDs to use in tests:

```bash
curl -X GET "http://localhost:5000/api/test/calendar-sharing/my-calendars" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE"
```

Response:
```json
{
  "success": true,
  "count": 2,
  "calendars": [
    {
      "id": "guid-here",
      "name": "My Calendar",
      "description": null,
      "color": "#3498db",
      "isDefault": true,
      "isVisible": true
    }
  ]
}
```

### 2. Share a Calendar

Share one of your calendars with another user:

```bash
curl -X POST "http://localhost:5000/api/test/calendar-sharing/share" \
  -H "Content-Type: application/json" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE" \
  -d '{
    "calendarId": "YOUR_CALENDAR_GUID",
    "sharedWithUserId": "OTHER_USER_ID",
    "permission": 1,
    "note": "Sharing my calendar with you!"
  }'
```

Permission levels:
- `0` = FreeBusyOnly (see only if busy)
- `1` = ViewAll (see all details)
- `2` = Edit (make changes)
- `3` = ManageSharing (full control)

Response:
```json
{
  "success": true,
  "message": "Calendar shared successfully",
  "share": {
    "id": "share-guid",
    "calendarId": "calendar-guid",
    "sharedWithUserId": "user-id",
    "permission": 1,
    "permissionName": "See all event details",
    "note": "Sharing my calendar with you!",
    "isAccepted": false,
    "createdAt": "2025-01-13T..."
  }
}
```

### 3. Get Calendars Shared With You

View calendars that others have shared with you:

```bash
curl -X GET "http://localhost:5000/api/test/calendar-sharing/shared-with-me?includeUnaccepted=true" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE"
```

Response:
```json
{
  "success": true,
  "count": 1,
  "shares": [
    {
      "id": "share-guid",
      "calendarName": "Someone's Calendar",
      "calendarId": "calendar-guid",
      "sharedBy": "username",
      "sharedByUserId": "user-id",
      "permission": 1,
      "permissionName": "See all event details",
      "note": "Sharing my calendar with you!",
      "isAccepted": false,
      "color": null,
      "createdAt": "2025-01-13T...",
      "acceptedAt": null
    }
  ]
}
```

### 4. Accept a Calendar Share

Accept a calendar that was shared with you:

```bash
curl -X POST "http://localhost:5000/api/test/calendar-sharing/SHARE_GUID/accept" \
  -H "Content-Type: application/json" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE" \
  -d '{
    "color": "#ff6b6b"
  }'
```

Response:
```json
{
  "success": true,
  "message": "Calendar share accepted"
}
```

### 5. Get Shares for a Calendar

See who a specific calendar is shared with:

```bash
curl -X GET "http://localhost:5000/api/test/calendar-sharing/calendar/CALENDAR_GUID/shares" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE"
```

Response:
```json
{
  "success": true,
  "count": 2,
  "shares": [
    {
      "id": "share-guid",
      "sharedWith": "username1",
      "sharedWithUserId": "user-id-1",
      "sharedBy": "owner-username",
      "sharedByUserId": "owner-id",
      "permission": 1,
      "permissionName": "See all event details",
      "note": null,
      "isAccepted": true,
      "createdAt": "2025-01-13T...",
      "acceptedAt": "2025-01-13T..."
    }
  ]
}
```

### 6. Check Permission

Check if you have a specific permission on a calendar:

```bash
curl -X GET "http://localhost:5000/api/test/calendar-sharing/calendar/CALENDAR_GUID/permission?requiredPermission=1" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE"
```

Response:
```json
{
  "success": true,
  "calendarId": "calendar-guid",
  "userId": "your-user-id",
  "requiredPermission": 1,
  "hasPermission": true,
  "userPermission": 1,
  "userPermissionName": "ViewAll"
}
```

### 7. Subscribe to Public Calendar

Subscribe to a public ICS calendar feed:

```bash
curl -X POST "http://localhost:5000/api/test/calendar-sharing/public/subscribe" \
  -H "Content-Type: application/json" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE" \
  -d '{
    "name": "US Holidays",
    "icsUrl": "https://calendar.google.com/calendar/ical/en.usa%23holiday%40group.v.calendar.google.com/public/basic.ics",
    "category": 0,
    "description": "United States Holidays",
    "color": "#e74c3c"
  }'
```

Categories:
- `0` = Holidays
- `1` = Sports
- `2` = School
- `3` = Religious
- `4` = Weather
- `5` = Entertainment
- `6` = Other

Response:
```json
{
  "success": true,
  "message": "Subscribed to public calendar",
  "calendar": {
    "id": "public-cal-guid",
    "name": "US Holidays",
    "description": "United States Holidays",
    "icsUrl": "https://...",
    "category": 0,
    "categoryName": "Holidays",
    "color": "#e74c3c",
    "isActive": true,
    "eventCount": 15,
    "lastSyncedAt": "2025-01-13T...",
    "createdAt": "2025-01-13T..."
  }
}
```

### 8. Get Public Calendar Subscriptions

View your public calendar subscriptions:

```bash
curl -X GET "http://localhost:5000/api/test/calendar-sharing/public/subscriptions?activeOnly=true" \
  -H "Cookie: .AspNetCore.Identity.Application=YOUR_COOKIE"
```

Response:
```json
{
  "success": true,
  "count": 1,
  "calendars": [
    {
      "id": "public-cal-guid",
      "name": "US Holidays",
      "description": "United States Holidays",
      "icsUrl": "https://...",
      "category": 0,
      "categoryName": "Holidays",
      "color": "#e74c3c",
      "isActive": true,
      "eventCount": 15,
      "lastSyncedAt": "2025-01-13T...",
      "createdAt": "2025-01-13T..."
    }
  ]
}
```

## Testing Scenarios

### Scenario 1: Share Calendar Between Two Users

1. Login as User A
2. Get User A's calendars: `GET /my-calendars`
3. Share a calendar with User B: `POST /share`
4. Login as User B
5. View calendars shared with you: `GET /shared-with-me?includeUnaccepted=true`
6. Accept the share: `POST /{shareId}/accept`
7. Verify acceptance: `GET /shared-with-me`

### Scenario 2: Permission Levels

1. Share calendar with ViewAll permission (1)
2. Check User B has ViewAll: `GET /calendar/{id}/permission?requiredPermission=1`
3. Check User B doesn't have Edit: `GET /calendar/{id}/permission?requiredPermission=2`
4. Should return `hasPermission: false` for Edit

### Scenario 3: Public Calendar Subscription

1. Subscribe to public ICS feed: `POST /public/subscribe`
2. Wait for sync to complete
3. Check subscriptions: `GET /public/subscriptions`
4. Verify eventCount > 0

## Common Public Calendar URLs

### US Holidays
```
https://calendar.google.com/calendar/ical/en.usa%23holiday%40group.v.calendar.google.com/public/basic.ics
```

### Religious Calendars
- Jewish Holidays: `https://calendar.google.com/calendar/ical/en.jewish%23holiday%40group.v.calendar.google.com/public/basic.ics`
- Christian Holidays: `https://calendar.google.com/calendar/ical/en.christian%23holiday%40group.v.calendar.google.com/public/basic.ics`

### Sports (Examples)
Contact specific sports leagues for their official ICS feeds.

## Testing with Browser DevTools

1. Open browser DevTools (F12)
2. Go to Console tab
3. Use `fetch()` API:

```javascript
// Get your calendars
fetch('/api/test/calendar-sharing/my-calendars')
  .then(r => r.json())
  .then(console.log);

// Share a calendar
fetch('/api/test/calendar-sharing/share', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    calendarId: 'YOUR_CALENDAR_GUID',
    sharedWithUserId: 'OTHER_USER_ID',
    permission: 1,
    note: 'Test share'
  })
}).then(r => r.json()).then(console.log);
```

## Notes

- All times are returned in UTC
- GUIDs must be valid format (e.g., `550e8400-e29b-41d4-a716-446655440000`)
- Calendar owners always have ManageSharing permission
- Shared calendars can have custom colors per user
- Public calendar sync happens automatically on subscription

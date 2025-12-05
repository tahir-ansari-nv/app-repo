# API Documentation

## Base URL
- Development: `https://localhost:7113` or `http://localhost:5155`

## Authentication
All API endpoints require JWT Bearer token authentication (except public endpoints if any).

Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

### Required Claims
- `employeeId` or `sub` or `NameIdentifier`: User's GUID
- `role`: "Employee" or "Manager"

## Endpoints

### Timesheet Management

#### 1. Get Timesheet by Week
```http
GET /api/timesheets/week/{employeeId}/{weekStartDate}
```

**Description:** Retrieve timesheet for a specific employee and week.

**Parameters:**
- `employeeId` (GUID): Employee identifier
- `weekStartDate` (DateTime): Monday date of the week (format: yyyy-MM-dd)

**Authorization:** Employee (own) or Manager

**Response (200 OK):**
```json
{
  "timesheetId": "guid",
  "employeeId": "guid",
  "weekStartDate": "2025-12-01T00:00:00",
  "weekEndDate": "2025-12-07T00:00:00",
  "status": 0,
  "submittedAt": null,
  "managerDecisionAt": null,
  "managerId": null,
  "managerDecisionReason": null,
  "createdAt": "2025-12-05T10:00:00",
  "updatedAt": "2025-12-05T10:00:00",
  "entries": [
    {
      "timesheetEntryId": "guid",
      "projectName": "Project Alpha",
      "taskName": "Development",
      "date": "2025-12-01T00:00:00",
      "hours": 8.0
    }
  ]
}
```

**Response (404 Not Found):**
```json
{
  "message": "Timesheet not found for the specified week."
}
```

---

#### 2. Create or Update Timesheet
```http
POST /api/timesheets
```

**Description:** Create a new timesheet or update existing draft/rejected timesheet.

**Authorization:** Employee (own timesheet only)

**Request Body:**
```json
{
  "employeeId": "guid",
  "weekStartDate": "2025-12-01",
  "entries": [
    {
      "projectName": "Project Alpha",
      "taskName": "Development",
      "date": "2025-12-01",
      "hours": 8.0
    },
    {
      "projectName": "Project Beta",
      "taskName": "Testing",
      "date": "2025-12-02",
      "hours": 6.5
    }
  ]
}
```

**Validation Rules:**
- WeekStartDate must be Monday
- ProjectName, TaskName required for all entries
- Hours: 0-24 per entry
- Total hours: ≤100 per week

**Response (200 OK):**
```json
{
  "timesheetId": "guid",
  "employeeId": "guid",
  "weekStartDate": "2025-12-01T00:00:00",
  "weekEndDate": "2025-12-07T00:00:00",
  "status": 0,
  "entries": [...]
}
```

**Response (400 Bad Request):**
```json
{
  "error": "WeekStartDate must be a Monday.; Hours must be between 0 and 24."
}
```

---

#### 3. Submit Timesheet
```http
POST /api/timesheets/submit
```

**Description:** Submit draft timesheet for manager approval.

**Authorization:** Employee (own timesheet only)

**Request Body:**
```json
{
  "timesheetId": "guid"
}
```

**Response (200 OK):**
```json
{
  "message": "Timesheet submitted successfully."
}
```

**Response (400 Bad Request):**
```json
{
  "error": "Only draft timesheets can be submitted."
}
```

---

#### 4. Get Timesheet History
```http
GET /api/timesheets/history/{employeeId}
```

**Description:** Retrieve all timesheets for an employee.

**Parameters:**
- `employeeId` (GUID): Employee identifier

**Authorization:** Employee (own) or Manager

**Response (200 OK):**
```json
[
  {
    "timesheetId": "guid",
    "employeeId": "guid",
    "employeeName": "John Doe",
    "weekStartDate": "2025-12-01T00:00:00",
    "weekEndDate": "2025-12-07T00:00:00",
    "status": 2,
    "submittedAt": "2025-12-05T14:00:00",
    "totalHours": 40.0
  }
]
```

---

### Manager Operations

#### 5. Get Pending Timesheets
```http
GET /api/manager/timesheets/pending
```

**Description:** Retrieve all submitted timesheets for manager's employees.

**Authorization:** Manager only

**Response (200 OK):**
```json
[
  {
    "timesheetId": "guid",
    "employeeId": "guid",
    "employeeName": "Jane Smith",
    "weekStartDate": "2025-12-01T00:00:00",
    "weekEndDate": "2025-12-07T00:00:00",
    "status": 1,
    "submittedAt": "2025-12-05T10:00:00",
    "totalHours": 42.5
  }
]
```

---

#### 6. Approve Timesheet
```http
POST /api/manager/timesheets/{id}/approve
```

**Description:** Approve a submitted timesheet.

**Parameters:**
- `id` (GUID): Timesheet identifier

**Authorization:** Manager (direct reports only)

**Response (200 OK):**
```json
{
  "message": "Timesheet approved successfully."
}
```

**Response (403 Forbidden):**
Returned when manager doesn't have authority over the employee.

---

#### 7. Reject Timesheet
```http
POST /api/manager/timesheets/{id}/reject
```

**Description:** Reject a submitted timesheet with reason.

**Parameters:**
- `id` (GUID): Timesheet identifier

**Authorization:** Manager (direct reports only)

**Request Body:**
```json
{
  "rejectionReason": "Please provide more detail for hours on Project X."
}
```

**Response (200 OK):**
```json
{
  "message": "Timesheet rejected successfully."
}
```

**Side Effects:**
- Timesheet status changed to Rejected
- Employee receives notification
- Audit log created

---

### Notifications

#### 8. Get Notifications
```http
GET /api/notifications/{employeeId}
```

**Description:** Retrieve notifications for an employee.

**Parameters:**
- `employeeId` (GUID): Employee identifier

**Authorization:** Employee (own notifications only)

**Response (200 OK):**
```json
[
  {
    "notificationId": "guid",
    "recipientId": "guid",
    "title": "Timesheet Rejected",
    "message": "Your timesheet for week 2025-12-01 to 2025-12-07 has been rejected. Reason: Please provide more detail.",
    "isRead": false,
    "createdAt": "2025-12-05T15:30:00"
  }
]
```

---

## Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 400 | Bad Request - Validation error |
| 401 | Unauthorized - Invalid/missing token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 500 | Internal Server Error |

## Timesheet Status Values

| Value | Name | Description |
|-------|------|-------------|
| 0 | Draft | Editable by employee |
| 1 | Submitted | Pending manager review |
| 2 | Approved | Approved by manager |
| 3 | Rejected | Rejected by manager, editable by employee |

## Business Rules

1. **Week Definition**: Monday (start) to Sunday (end)
2. **Unique Constraint**: One timesheet per employee per week
3. **Hours Validation**:
   - Per entry: 0-24 hours
   - Per week: ≤100 hours total
4. **Status Transitions**:
   - Draft → Submitted (employee)
   - Submitted → Approved (manager)
   - Submitted → Rejected (manager)
   - Rejected → Draft (employee edits)
5. **Immutability**: Submitted/Approved timesheets are read-only
6. **Notifications**: Sent on rejection only

## Error Response Format

All error responses follow this format:
```json
{
  "error": "Error message describing what went wrong"
}
```

For validation errors, multiple issues are concatenated with semicolons:
```json
{
  "error": "ProjectName is required for all entries.; Hours must be between 0 and 24."
}
```

## Testing with Swagger

1. Start the application: `dotnet run`
2. Navigate to: `https://localhost:7113/swagger`
3. Click "Authorize" and enter JWT token
4. Test endpoints interactively

## Sample JWT Token Generation

**Note:** You need to implement authentication endpoints or integrate with an identity provider (Azure AD, IdentityServer4) to generate valid JWT tokens.

The token should include:
```json
{
  "sub": "employee-guid" or "employeeId": "employee-guid",
  "role": "Employee" or "Manager",
  "exp": 1234567890,
  "iss": "TimesheetManagementAPI",
  "aud": "TimesheetManagementClient"
}
```

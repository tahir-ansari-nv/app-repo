# Timesheet Management System

A comprehensive ASP.NET Core Web API system for managing employee timesheets with approval workflows.

## Overview

This system enables employees to record, edit, and submit weekly timesheets with daily hours allocated per project/task. Submitted timesheets become read-only, pending manager approval or rejection. Managers can review, approve, or reject timesheets, with rejections notifying employees and including a reason.

## Architecture

### Backend
- **Framework**: ASP.NET Core 10.0 Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Authorization**: Role-based (Employee, Manager)

### Key Features
- ✅ Weekly timesheet entry (Monday-Sunday)
- ✅ Multiple projects/tasks per day
- ✅ Validation (0-24 hours per day, max 100 hours per week)
- ✅ Submit/Approve/Reject workflow
- ✅ Comprehensive audit trail
- ✅ Notification system for rejections
- ✅ Timesheet history tracking

## Project Structure

```
TimesheetManagement/
├── Controllers/           # API Controllers
│   ├── TimesheetsController.cs
│   ├── ManagerController.cs
│   └── NotificationsController.cs
├── Models/
│   ├── Entities/         # Database entities
│   │   ├── Employee.cs
│   │   ├── Timesheet.cs
│   │   ├── TimesheetEntry.cs
│   │   ├── AuditLog.cs
│   │   └── Notification.cs
│   ├── DTOs/             # Data Transfer Objects
│   └── Enums/            # Enumerations
├── Data/
│   ├── TimesheetDbContext.cs
│   └── Repositories/     # Data access layer
├── Services/             # Business logic layer
└── Program.cs            # Application entry point
```

## Data Model

### Entities

1. **Employee**: User information
2. **Timesheet**: Weekly timesheet (unique per employee per week)
3. **TimesheetEntry**: Individual time entries (project/task/date/hours)
4. **AuditLog**: Audit trail for all actions
5. **Notification**: In-app notifications

### Status Flow

```
Draft → Submitted → Approved
              ↓
          Rejected → Draft (editable)
```

## API Endpoints

### Timesheet APIs

#### GET /api/timesheets/week/{employeeId}/{weekStartDate}
Get timesheet for a specific week.

#### POST /api/timesheets
Create or update a draft timesheet.

**Request Body:**
```json
{
  "employeeId": "guid",
  "weekStartDate": "2025-12-01",
  "entries": [
    {
      "projectName": "Project A",
      "taskName": "Development",
      "date": "2025-12-01",
      "hours": 8.0
    }
  ]
}
```

#### POST /api/timesheets/submit
Submit a timesheet for approval.

**Request Body:**
```json
{
  "timesheetId": "guid"
}
```

#### GET /api/timesheets/history/{employeeId}
Get timesheet history for an employee.

### Manager APIs

#### GET /api/manager/timesheets/pending
Get all pending timesheets for manager's employees.

#### POST /api/manager/timesheets/{id}/approve
Approve a timesheet.

#### POST /api/manager/timesheets/{id}/reject
Reject a timesheet with a reason.

**Request Body:**
```json
{
  "rejectionReason": "Please provide more detail for Project X hours."
}
```

### Notification APIs

#### GET /api/notifications/{employeeId}
Get notifications for an employee.

## Validation Rules

- **ProjectName**, **TaskName**, **Date**, **Hours** are mandatory for each entry
- Hours must be between 0 and 24 (decimals allowed)
- Total weekly hours cannot exceed 100
- WeekStartDate must be a Monday
- One timesheet per employee per week
- Only draft timesheets can be submitted
- Only submitted timesheets can be approved/rejected

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server (LocalDB or full instance)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd app-repo
   ```

2. **Update connection string** (if needed)
   Edit `src/TimesheetManagement/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Your-Connection-String"
   }
   ```

3. **Create database migrations**
   ```bash
   cd src/TimesheetManagement
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   Navigate to: `https://localhost:<port>/swagger`

## Configuration

### JWT Settings

Update `appsettings.json` with your JWT configuration:

```json
"JwtSettings": {
  "SecretKey": "Your-Secret-Key-At-Least-32-Characters",
  "Issuer": "TimesheetManagementAPI",
  "Audience": "TimesheetManagementClient",
  "ExpiryInMinutes": 60
}
```

### Database Connection

For SQL Server LocalDB (default):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TimesheetManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

## Security Considerations

- ✅ JWT Bearer token authentication
- ✅ Role-based authorization (Employee, Manager)
- ✅ Input validation on all endpoints
- ✅ HTTPS enforcement
- ✅ CORS configuration
- ✅ Comprehensive audit logging
- ⚠️ Store secrets in Azure Key Vault or similar in production
- ⚠️ Use strong JWT secret keys (32+ characters)

## Testing

### Manual Testing with Swagger

1. Start the application
2. Navigate to Swagger UI
3. Use the "Authorize" button to add a JWT token
4. Test the endpoints

### Sample JWT Token Generation

You'll need to implement a separate authentication endpoint or use an external identity provider (e.g., IdentityServer4, Azure AD) to generate JWT tokens with appropriate claims:

- `employeeId` claim for user identification
- `role` claim for Employee/Manager roles

## Future Enhancements

- [ ] Frontend application (Angular/React)
- [ ] Email notifications via SMTP
- [ ] Autosave functionality (client-side)
- [ ] Export timesheets to PDF/Excel
- [ ] Reporting and analytics
- [ ] Integration tests
- [ ] Unit tests for services
- [ ] Rate limiting on submit endpoints
- [ ] Caching for better performance

## License

[Specify License]

## Support

For issues or questions, please contact [support email].
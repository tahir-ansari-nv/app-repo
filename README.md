# Timesheet Management System

A comprehensive .NET Core Web API for managing employee timesheets with an approval workflow. This system allows employees to record and submit weekly timesheets, and managers to review, approve, or reject them.

## Features

- **Timesheet Management**: Create, update, and submit weekly timesheets
- **Entry Tracking**: Record daily hours worked on various projects/tasks
- **Approval Workflow**: Manager review with approve/reject capabilities
- **Validation**: Comprehensive business rule validation
- **Audit Trail**: Full audit logging of all operations
- **JWT Authentication**: Secure token-based authentication
- **Role-Based Authorization**: Employee and Manager roles
- **RESTful API**: Clean, well-documented API endpoints

## Architecture

### Technology Stack
- .NET 8.0 Web API
- Entity Framework Core 8.0
- SQL Server
- JWT Bearer Authentication
- FluentValidation
- Serilog for logging
- xUnit for testing
- Moq for mocking

### Project Structure
```
TimesheetManagement/
├── src/
│   └── TimesheetApi/
│       ├── Controllers/      # API Controllers
│       ├── Services/         # Business Logic
│       ├── Repositories/     # Data Access
│       ├── Models/          # Domain Entities
│       ├── DTOs/            # Data Transfer Objects
│       ├── Validators/      # FluentValidation Validators
│       ├── Middleware/      # Custom Middleware
│       └── Data/            # EF Core DbContext
└── tests/
    └── TimesheetApi.Tests/  # Unit Tests
```

## API Endpoints

### Employee Endpoints

#### Get Timesheet for Week
```http
GET /api/timesheets?weekStartDate=2024-06-03
Authorization: Bearer {token}
```

**Response:**
```json
{
  "timesheetId": "guid",
  "weekStartDate": "2024-06-03",
  "status": "Draft",
  "entries": [
    {
      "entryId": "guid",
      "projectCode": "PRJ001",
      "taskDescription": "Design",
      "date": "2024-06-03",
      "hours": 8.0
    }
  ],
  "createdAt": "2024-06-01T08:00:00Z",
  "updatedAt": "2024-06-02T12:00:00Z"
}
```

#### Create or Update Timesheet
```http
POST /api/timesheets
Authorization: Bearer {token}
Content-Type: application/json

{
  "weekStartDate": "2024-06-03",
  "entries": [
    {
      "projectCode": "PRJ001",
      "taskDescription": "Development",
      "date": "2024-06-03",
      "hours": 7.5
    }
  ]
}
```

#### Submit Timesheet
```http
POST /api/timesheets/{timesheetId}/submit
Authorization: Bearer {token}
```

### Manager Endpoints

#### Get Submitted Timesheets
```http
GET /api/manager/timesheets?status=Submitted
Authorization: Bearer {token}
```

#### Approve Timesheet
```http
POST /api/manager/timesheets/{timesheetId}/approve
Authorization: Bearer {token}
```

#### Reject Timesheet
```http
POST /api/manager/timesheets/{timesheetId}/reject
Authorization: Bearer {token}
Content-Type: application/json

{
  "reason": "Missing task descriptions"
}
```

## Data Model

### Entities

**Employee**
- Id (GUID)
- Name (string, required)
- Email (string, required, unique)
- Role (enum: Employee, Manager)

**Timesheet**
- Id (GUID)
- EmployeeId (GUID, FK)
- WeekStartDate (DateTime, must be Monday)
- Status (enum: Draft, Submitted, Approved, Rejected)
- CreatedAt, UpdatedAt, SubmittedAt, ManagerDecisionAt
- ManagerId (GUID, nullable)
- ManagerDecisionReason (string, nullable)
- RowVersion (byte[], concurrency token)

**TimesheetEntry**
- Id (GUID)
- TimesheetId (GUID, FK)
- ProjectCode (string, required)
- TaskDescription (string, optional)
- Date (DateTime, must be within week)
- Hours (decimal, 0-24 per entry)
- CreatedAt, UpdatedAt

**AuditLog**
- Id (GUID)
- EntityType, EntityId
- Action (enum: Created, Updated, Submitted, Approved, Rejected)
- PerformedBy (GUID)
- Timestamp
- Details (string)

## Business Rules

### Validation
- Week start date must be a Monday
- Entry dates must fall within the selected week
- Hours per entry: 0-24
- Total weekly hours: ≤ 100
- Cannot submit empty timesheets
- Cannot submit timesheets with zero total hours
- Rejection reason: minimum 5 characters

### Status Transitions
- **Draft → Submitted**: Employee submits timesheet
- **Submitted → Approved**: Manager approves
- **Submitted → Rejected**: Manager rejects
- **Rejected → Submitted**: Employee resubmits after corrections

### Authorization
- Employees can only manage their own timesheets
- Managers can review all submitted timesheets
- Only Draft or Rejected timesheets can be edited
- Only Submitted timesheets can be approved/rejected

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB for development)
- Visual Studio 2022 or VS Code (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/tahir-ansari-nv/app-repo.git
   cd app-repo
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database connection**
   
   Update `src/TimesheetApi/appsettings.json` with your SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=TimesheetDb;..."
   }
   ```

4. **Configure JWT settings**
   
   **IMPORTANT**: For production, store the JWT secret in Azure Key Vault or User Secrets, never in appsettings.json!
   
   For development, you can use the default settings, but for production:
   ```bash
   dotnet user-secrets init --project src/TimesheetApi
   dotnet user-secrets set "JwtSettings:SecretKey" "your-secure-secret-key-here" --project src/TimesheetApi
   ```

5. **Create database migrations**
   ```bash
   cd src/TimesheetApi
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run --project src/TimesheetApi
   ```

   The API will be available at: https://localhost:5001

7. **Access Swagger UI**
   
   Navigate to: https://localhost:5001/swagger

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~TimesheetServiceTests"
```

### Building for Production

```bash
# Build in Release mode
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish
```

## Security Considerations

### Authentication
- JWT Bearer tokens required for all endpoints
- Tokens issued by IdentityServer4 or Azure AD
- Token expiration: 60 minutes (configurable)

### Authorization
- Role-based access control (Employee, Manager)
- Claims-based authorization
- User can only access their own data

### Data Protection
- HTTPS enforced for all communications
- SQL Server encryption at rest (TDE recommended)
- Passwords never logged
- Secrets stored in Azure Key Vault or User Secrets

### Input Validation
- FluentValidation for request validation
- SQL injection prevention via EF Core parameterized queries
- XSS prevention through proper content encoding

### Audit Logging
- All key operations logged
- User ID, timestamp, action recorded
- Audit logs immutable

## Development

### Adding New Endpoints
1. Create DTOs in `DTOs/`
2. Add validators in `Validators/`
3. Implement service methods in `Services/`
4. Create controller actions in `Controllers/`
5. Add unit tests in `tests/`

### Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName --project src/TimesheetApi

# Update database
dotnet ef database update --project src/TimesheetApi

# Remove last migration (if not applied)
dotnet ef migrations remove --project src/TimesheetApi
```

## Testing Strategy

### Unit Tests
- Service layer business logic
- Validation rules
- Repository operations (with in-memory DB)

### Integration Tests
- End-to-end API testing
- Database interactions
- Authentication/Authorization

## Monitoring and Logging

### Serilog Configuration
Logs are configured to output to:
- Console (development)
- File system (production)
- Application Insights (recommended for production)

### Health Checks
Consider adding health check endpoints:
- Database connectivity
- External service availability

## CI/CD

Recommended pipeline:
1. Build
2. Run tests
3. Run security scans
4. Publish artifacts
5. Deploy to staging
6. Run integration tests
7. Deploy to production

## Future Enhancements

- [ ] Email/SMS notifications via Azure Service Bus
- [ ] Frontend SPA (Blazor or React)
- [ ] Local auto-save support
- [ ] Timesheet locking after payroll processing
- [ ] Advanced reporting and analytics
- [ ] Multi-tenant support
- [ ] Attachment support for timesheets
- [ ] Rate limiting for API endpoints
- [ ] GraphQL API support
- [ ] Mobile app integration

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

## License

[Your License Here]

## Support

For issues and questions, please open an issue on GitHub.
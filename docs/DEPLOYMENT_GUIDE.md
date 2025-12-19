# Deployment Guide - TimeSheet Portal

This guide provides instructions for deploying the TimeSheet Portal API to various environments.

## Prerequisites

- .NET 8.0 Runtime
- SQL Server (or Azure SQL Database)
- SMTP server for email notifications
- Web server (IIS, Nginx, or Azure App Service)

## Configuration

### 1. Environment Variables

For production, use environment variables or Azure Key Vault for sensitive data:

```bash
ConnectionStrings__DefaultConnection="Server=your-server;Database=TimeSheetPortalDb;User Id=sa;Password=YourPassword;"
JwtSettings__SecretKey="YourSecureSecretKeyHere123456789"
JwtSettings__Issuer="TimeSheetPortal"
JwtSettings__Audience="TimeSheetPortalUsers"
JwtSettings__ExpiryMinutes="60"
EmailSettings__SmtpServer="smtp.sendgrid.net"
EmailSettings__SmtpPort="587"
EmailSettings__SmtpUsername="apikey"
EmailSettings__SmtpPassword="your-sendgrid-api-key"
EmailSettings__FromEmail="noreply@timesheetportal.com"
EmailSettings__ResetPasswordBaseUrl="https://yourdomain.com"
```

### 2. Database Setup

#### Apply Migrations

```bash
cd src/TimeSheetPortal.Infrastructure
dotnet ef database update --startup-project ../TimeSheetPortal.API/TimeSheetPortal.API.csproj --configuration Release
```

#### Production Connection String

Update the connection string in `appsettings.Production.json` or use environment variables.

### 3. Security Configuration

#### Update JWT Secret

Generate a secure random key (at least 32 characters):

```bash
# Linux/Mac
openssl rand -base64 32

# PowerShell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})
```

Store this in Azure Key Vault or secure configuration.

#### Update CORS Origins

In `appsettings.Production.json`:

```json
{
  "AllowedOrigins": [
    "https://yourdomain.com",
    "https://www.yourdomain.com"
  ]
}
```

## Deployment Options

### Option 1: Azure App Service

1. **Create Azure Resources:**
   ```bash
   az group create --name TimeSheetPortal-RG --location eastus
   az sql server create --name timesheetportal-sql --resource-group TimeSheetPortal-RG --location eastus --admin-user sqladmin --admin-password YourPassword123!
   az sql db create --resource-group TimeSheetPortal-RG --server timesheetportal-sql --name TimeSheetPortalDb --service-objective S0
   az appservice plan create --name TimeSheetPortal-Plan --resource-group TimeSheetPortal-RG --sku B1 --is-linux
   az webapp create --resource-group TimeSheetPortal-RG --plan TimeSheetPortal-Plan --name timesheetportal-api --runtime "DOTNET|8.0"
   ```

2. **Configure Application Settings:**
   ```bash
   az webapp config appsettings set --resource-group TimeSheetPortal-RG --name timesheetportal-api --settings \
     ConnectionStrings__DefaultConnection="Server=tcp:timesheetportal-sql.database.windows.net,1433;Database=TimeSheetPortalDb;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;" \
     JwtSettings__SecretKey="YourSecureSecretKeyHere123456789" \
     ASPNETCORE_ENVIRONMENT="Production"
   ```

3. **Deploy Application:**
   ```bash
   cd src/TimeSheetPortal.API
   dotnet publish -c Release -o ./publish
   cd publish
   zip -r ../deploy.zip .
   az webapp deployment source config-zip --resource-group TimeSheetPortal-RG --name timesheetportal-api --src ../deploy.zip
   ```

### Option 2: Docker Container

1. **Create Dockerfile:**

Create `Dockerfile` in the solution root:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TimeSheetPortal.API/TimeSheetPortal.API.csproj", "src/TimeSheetPortal.API/"]
COPY ["src/TimeSheetPortal.Core/TimeSheetPortal.Core.csproj", "src/TimeSheetPortal.Core/"]
COPY ["src/TimeSheetPortal.Infrastructure/TimeSheetPortal.Infrastructure.csproj", "src/TimeSheetPortal.Infrastructure/"]
RUN dotnet restore "src/TimeSheetPortal.API/TimeSheetPortal.API.csproj"
COPY . .
WORKDIR "/src/src/TimeSheetPortal.API"
RUN dotnet build "TimeSheetPortal.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeSheetPortal.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeSheetPortal.API.dll"]
```

2. **Build and Run:**
   ```bash
   docker build -t timesheetportal-api .
   docker run -d -p 8080:80 -e ConnectionStrings__DefaultConnection="..." timesheetportal-api
   ```

### Option 3: Linux Server (Ubuntu/Debian)

1. **Install .NET Runtime:**
   ```bash
   wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   sudo dpkg -i packages-microsoft-prod.deb
   sudo apt-get update
   sudo apt-get install -y aspnetcore-runtime-8.0
   ```

2. **Deploy Application:**
   ```bash
   sudo mkdir -p /var/www/timesheetportal
   sudo chown $USER:$USER /var/www/timesheetportal
   
   cd src/TimeSheetPortal.API
   dotnet publish -c Release -o /var/www/timesheetportal
   ```

3. **Create Systemd Service:**

Create `/etc/systemd/system/timesheetportal.service`:

```ini
[Unit]
Description=TimeSheet Portal API
After=network.target

[Service]
WorkingDirectory=/var/www/timesheetportal
ExecStart=/usr/bin/dotnet /var/www/timesheetportal/TimeSheetPortal.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=timesheetportal
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

4. **Start Service:**
   ```bash
   sudo systemctl enable timesheetportal
   sudo systemctl start timesheetportal
   sudo systemctl status timesheetportal
   ```

5. **Configure Nginx as Reverse Proxy:**

Create `/etc/nginx/sites-available/timesheetportal`:

```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable site and reload Nginx:
```bash
sudo ln -s /etc/nginx/sites-available/timesheetportal /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Option 4: Windows Server with IIS

1. **Install .NET 8.0 Hosting Bundle**
2. **Publish Application:**
   ```powershell
   cd src\TimeSheetPortal.API
   dotnet publish -c Release -o C:\inetpub\timesheetportal
   ```
3. **Create IIS Site:**
   - Open IIS Manager
   - Create new Application Pool (.NET CLR Version: No Managed Code)
   - Create new Website pointing to published folder
   - Configure HTTPS binding with certificate

## Post-Deployment

### 1. Health Check

Test the API is running:
```bash
curl https://yourdomain.com/swagger/index.html
```

### 2. Database Migrations

Ensure migrations are applied:
```bash
curl -X POST https://yourdomain.com/api/health
```

### 3. Monitoring

Set up monitoring for:
- Application logs
- Failed login attempts
- API response times
- Error rates

### 4. Backup Strategy

- Database: Daily backups with 30-day retention
- Configuration: Version controlled
- Secrets: Stored in Key Vault with access logging

## Troubleshooting

### Connection Issues

Check connection string format and network access to SQL Server.

### Authentication Errors

Verify JWT secret key is consistent across all instances.

### Email Not Sending

Check SMTP credentials and firewall rules for outbound SMTP traffic.

## Security Checklist

- [ ] JWT secret key is strong and stored securely
- [ ] HTTPS is enforced
- [ ] Database connection uses encrypted connection
- [ ] CORS is configured for specific origins only
- [ ] Rate limiting is enabled
- [ ] Logs are configured and monitored
- [ ] Sensitive data is not in version control
- [ ] Regular security updates are scheduled

## Rollback Procedure

1. Stop the application
2. Restore previous version from backup
3. Rollback database migrations if needed
4. Start the application
5. Verify functionality

## Support

For deployment issues, contact the DevOps team or create an issue in the repository.

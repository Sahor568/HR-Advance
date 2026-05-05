# HR Management System - Setup Guide

## Overview
This is a complete HR Management System (HRMS) tailored for Nepal labor laws and compliance requirements. The system includes employee management, attendance tracking, leave management, payroll processing, SSF contributions, compliance reporting, and audit logging.

## Prerequisites
1. .NET 10.0 SDK
2. Microsoft SQL Server (LocalDB or full SQL Server)
3. Visual Studio 2022 or VS Code with C# extensions

## Database Setup

### Option 1: Using Existing SQL Server Database "SaHor"
1. Ensure SQL Server is running
2. Update the connection string in `appsettings.json` if needed:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=SaHor;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true"
   }
   ```
3. If using SQL authentication, update connection string to:
   ```json
   "DefaultConnection": "Server=YOUR_SERVER;Database=SaHor;User Id=username;Password=password;MultipleActiveResultSets=true;TrustServerCertificate=true"
   ```

### Option 2: Create New Database
1. Open SQL Server Management Studio
2. Create a new database named "SaHor"
3. Run the following commands to create migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## Application Setup

### 1. Restore NuGet Packages
```bash
dotnet restore
```

### 2. Build the Application
```bash
dotnet build
```

### 3. Apply Database Migrations
```bash
dotnet ef database update
```

### 4. Run the Application
```bash
dotnet run
```

The application will be available at:
- Web Interface: https://localhost:5001 (or http://localhost:5000)
- API Endpoints: https://localhost:5001/api/[controller]

## Default Admin Account
After running the application for the first time, a default admin user is created:
- **Email**: admin@hrms.com
- **Password**: Admin@123
- **Role**: Admin

## API Endpoints

### Employee Management
- `GET /api/employee` - Get all employees (Admin/HRManager)
- `POST /api/employee` - Create new employee
- `GET /api/employee/{id}` - Get employee details
- `PUT /api/employee/{id}` - Update employee
- `DELETE /api/employee/{id}` - Delete employee

### Attendance Management
- `POST /api/attendance/clockin` - Clock in
- `POST /api/attendance/clockout` - Clock out
- `GET /api/attendance/{employeeId}/{date}` - Get attendance for date
- `GET /api/attendance/monthly/{employeeId}/{year}/{month}` - Monthly attendance report

### Leave Management
- `POST /api/leave/request` - Submit leave request
- `GET /api/leave/pending` - Get pending leave requests (Admin/HRManager)
- `PUT /api/leave/approve/{id}` - Approve/reject leave request
- `GET /api/leave/balance/{employeeId}` - Get leave balance

### Payroll Management
- `POST /api/payroll/generate` - Generate payroll for month
- `GET /api/payroll/{employeeId}/{year}/{month}` - Get payroll details
- `POST /api/payroll/approve/{id}` - Approve payroll
- `GET /api/payroll/payslip/{id}` - Download payslip

### Compliance Management
- `POST /api/compliance/accident` - Log accident
- `POST /api/compliance/disciplinary` - Create disciplinary record
- `POST /api/compliance/medicalclaim` - Submit medical insurance claim
- `GET /api/compliance/auditreport` - Generate labor audit report

### SSF Management
- `POST /api/ssf/calculate` - Calculate SSF contributions
- `GET /api/ssf/balance/{employeeId}` - Get SSF balance
- `POST /api/ssf/generatecertificate/{employeeId}` - Generate SSF certificate

### Audit Logs
- `GET /api/auditlog` - Get all audit logs (Admin)
- `GET /api/auditlog/filter` - Filter logs by date/level

### Dashboard
- `GET /api/dashboard/stats` - Get dashboard statistics
- `GET /api/dashboard/probationexpiry` - Get employees with probation expiring soon

## Key Features

### Nepal Labor Law Compliance
1. **Employment Types**: Regular, Time-bound, Task-based, Casual, Part-time
2. **Probation System**: 6-month probation with automatic reminders
3. **Working Hours**: 8 hours/day, 48 hours/week as per Nepal labor law
4. **Leave Types**:
   - Home Leave: 1 day per 20 working days
   - Sick Leave: 12 days per year
   - Festival Leave: 13 days per year
   - Maternity Leave: 98 days
   - Paternity Leave: 15 days
5. **SSF Contributions**: 10% employee, 21% employer
6. **TDS Calculation**: Based on Nepal income tax slabs
7. **Festival Allowance**: 1 month basic salary in Ashwin and Kartik
8. **Medical Insurance**: 1 lakh medical, 7 lakh accident coverage
9. **Disciplinary Actions**: Section 131 compliance with warning letters

### System Architecture
- **Backend**: ASP.NET Core MVC (.NET 10.0)
- **Database**: Entity Framework Core with MSSQL
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Logging**: Serilog with file and database sinks
- **API Design**: RESTful API with proper HTTP status codes
- **Business Logic**: Service layer pattern with dependency injection

## Configuration

### appsettings.json Key Settings
```json
{
  "HRMS": {
    "CompanyName": "Nepal HR Management System",
    "TaxYear": 2081,
    "SSFEmployeeRate": 0.10,
    "SSFEmployerRate": 0.21,
    "OvertimeRate": 1.5,
    "FestivalAllowanceMonths": [ "Ashwin", "Kartik" ],
    "WorkingHoursPerDay": 8,
    "WorkingHoursPerWeek": 48,
    "ProbationMonths": 6
  }
}
```

### Serilog Configuration
Logs are written to:
1. File: `logs/hrms-.log` (rolling daily)
2. Database: `AuditLogs` table in MSSQL

## Troubleshooting

### Database Connection Issues
1. Check SQL Server is running
2. Verify connection string in appsettings.json
3. Ensure database "SaHor" exists and user has permissions
4. Try using `(localdb)\mssqllocaldb` for LocalDB

### Migration Errors
1. Delete the `Migrations` folder
2. Run: `dotnet ef migrations add InitialCreate`
3. Run: `dotnet ef database update`

### Build Errors
1. Restore packages: `dotnet restore`
2. Clean solution: `dotnet clean`
3. Rebuild: `dotnet build`

## Next Steps
1. Create frontend views after all the model controller is fully completed..

## Support
For issues or questions, check the code documentation or contact the development team.
# Getting Started

This guide explains how to run Stock-Lite locally for development purposes.

## Prerequisites

Make sure you have the following installed:
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Node.js](https://nodejs.org/en/download)
- [Docker](https://docs.docker.com/desktop)

You can verify installations using:

```bash
dotnet --version
node --version
npm --version
docker --version
```

Clone the Repository:
```bash
git clone https://github.com/fatihakb01/Stock-Lite.git
cd Stock-Lite
```

## Backend Setup (API)

### 1 Configuration Setup

Before running the API, you must manually create configuration files.

The following files are excluded from Git for security reasons:
- `API/appsettings.json`
- `API/appsettings.Development.json`

Navigate to the API folder and create both files.

**appsettings.json**

Create `API/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**appsettings.Development.json**

Create `API/appsettings.Development.json`:

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_HERE",
    "Issuer": "https://localhost:5001",
    "Audience": "https://localhost:3000",
    "ExpireMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1,1433;Database=StockLite;User Id=SA;Password=Password@1;TrustServerCertificate=True;"
  },
  "Resend": {
    "ApiKey": "YOUR_RESEND_API_KEY",
    "Email": {
      "From": "no-reply@yourdomain.com",
      "To": "your-email@example.com"
    }
  },
  "ClientAppUrl": "https://localhost:3000"
}
```

**🔐 Important Notes**
- Replace `YOUR_SECRET_KEY_HERE` with a secure random string (at least 32 characters).
- Replace `YOUR_RESEND_API_KEY` with your actual API key from Resend.
- Update email addresses as needed.
- You can create a Resend account and generate an API key here: [Resend](https://resend.com/docs)

### 2 Restore & Build

First, make sure that the back-end certificate is trusted by running the following command:
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Next, build the project:
```bash
dotnet build
```
This compiles the solution.

Install EF Core Tools (if not already installed)
```bash
dotnet tool install --global dotnet-ef
```

## Database Setup

First, start the Docker container:
```bash
docker compose up -d
```

Before running the project for the first time, update the database:
```bash
dotnet ef database update --project Infrastructure --startup-project API
```

Navigate to the API project:
```bash
cd API
```

Run the API. From the API folder:
```bash
dotnet watch run
```
The API should now be running locally.

To set up an environment, run the following SQL code in the database:
```sql
BEGIN TRAN

		DECLARE @name NVARCHAR(100) = 'Test Company',
				@email NVARCHAR(255) = 'test@test.com',
				@userName NVARCHAR(50) = 'Vatos01',
				@isActive BIT = 1,
				@phoneNumber NVARCHAR(20) = '+31612345678',
				@address NVARCHAR(250) = 'Test street 123',
				@companyId UNIQUEIDENTIFIER

		-- Set-up the environment
		EXEC [dbo].[CreateCompany]
			@name,
			@email,
			@isActive,
			@phoneNumber,
			@address
		
		-- If you want seed data in your system
		SELECT @companyId = Id FROM Companies WHERE Name = 'Test Company'

		EXEC [dbo].[SeedData]
			@companyId,
			1,
			1000,
			75,
			10,
			50

COMMIT TRAN
```

## Frontend Setup (Client)

First, create a `client/.env.development` file:

```bash
VITE_API_URL=https://localhost:5001/api
```

Navigate to the client folder:
```bash
cd client
```

Install Dependencies
```bash
npm install
```

Run the Development Server
```bash
npm run dev
```

The frontend should now be running locally and connected to the API.
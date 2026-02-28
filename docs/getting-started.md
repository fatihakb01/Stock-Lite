# Getting Started

This guide explains how to run Stock-Lite locally for development purposes.

## Prerequisites

Make sure you have the following installed:
- .NET 10 SDK
- Node.js (version v24.11.0)
- npm (version 11.8.0)
- Optional: Docker (version 28.5.1)

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

Restore & Build
```bash
dotnet build
```
This compiles the solution.

Install EF Core Tools (if not already installed)
```bash
dotnet tool install --global dotnet-ef
```

## Database Setup

Before running the project for the first time, update the database:
```bash
dotnet ef database update --project Infrastructure --startup-project API
```

Start the Docker container:
```bash
docker compose up -d
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

## Frontend Setup (Client)

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
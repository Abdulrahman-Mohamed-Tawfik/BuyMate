# BuyMate
BuyMate is e-commerce store system for managing products, users and carts

# Deployment & Execution

This section describes how to install, configure, and run the ASP.NET Core MVC E-Commerce Application, both locally and in a deployed environment.

## 1. System Requirements

Before running the application, ensure your environment meets the following requirements:

- **CPU:** 1.8 GHz or faster (Quad-core recommended).
- **RAM:** 8 GB Minimum (16 GB Recommended for Visual Studio performance)
- **Disk Space:** 2 GB free (application, logs, and database)
- **OS:**
  - Windows 10/11, Windows Server 2019+
  - Linux (Ubuntu, Debian, etc.) or macOS with .NET support
- **Runtime:**
  - .NET SDK 8.0 (or newer)
- **Database:**
  - Microsoft SQL Server (2019 or newer) or LocalDB.
- **Tools (optional but recommended):**
  - Visual Studio 2022 / VS Code / Jetbrains Rider
  - Git

## 2. Installation Steps

### 1. Clone the repository

```bash
git clone https://github.com/Abdulrahman-Mohamed-Tawfik/BuyMate.git
cd BuyMate
```

### 2. Restore NuGet packages

```bash
dotnet restore
```

### 3. Set up the database

- Update the connection string in appsettings.json (see [Configuration Instructions](#3-configuration-instructions) below).

- Apply EF Core migrations:

```bash
dotnet ef database update
```

### 4. Build the project

```bash
dotnet build
```

## 3. Configuration Instructions

The application uses Entity Framework Core to manage the database. You must configure your connection string before running.

1. Open appsettings.json in the root of the "BuyMate" project.

2. Locate the ConnectionStrings section.

3. Update DefaultConnection to match your local SQL Server instance.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Update:

- YOUR_SERVER → your SQL Server instance name

- YOUR_DB → your database name

## 4. Execution Guide

### 4.1 Running Locally (Development)

#### Option A – Using Visual Studio

1. Open the solution file: YourProject.sln.

2. Set the startup project to the web project (e.g., YourProject.Web).

3. Ensure appsettings.Development.json is configured for your local database.

4. Press F5 (Debug) or Ctrl+F5 (Run without debugging).

5. By default, the app will listen on the URLs configured in launchSettings.json, often:
   - http://localhost:5000
   - https://localhost:5001
   - (or the port shown in your terminal)

#### Option B – Using the .NET CLI

From the web project directory (where .csproj is):

```bash
dotnet run
```

### 4.2 Running in Production

1. Publish the application

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

2. Output Location: `bin\Release\net8.0\win-x64\publish\`

3. File: `BuyMate.exe` (This is the portable executable).

## 5. API Documentation (Internal)

While this is an MVC application, it utilizes an internal API endpoint for AJAX operations in the Cart Module.

The following endpoint accept JSON payload and return JSON response.

| Method | Endpoint        | Description          | Payload                            |
| ------ | --------------- | -------------------- | ---------------------------------- |
| POST   | /Cart/AddToCart | Adds an item to cart | { productId: Guid, quantity: int } |

## 6. Executable Files & Deployment Link

### 6.1 Compiled / Packaged Application

After publishing, the compiled app is located in the publish folder (see 4.2 Running in Production):

- Windows (framework-dependent):

  - `BuyMate.dll` (run with `dotnet BuyMate.dll`)

- Windows (self-contained):

  - `BuyMate.exe` (double-click or run in terminal)

- Linux/macOS (self-contained):

  - Executable file: `BuyMate.Web`

You can generate a self-contained build with:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
```

(Replace `win-x64` with `linux-x64`, `osx-x64`, etc. as needed.)

### 6.2 Deployed Web Application

1. Deploy the published files (see [Running in production](#42-running-in-production) section) to your server (IIS, Linux with Nginx/Apache reverse proxy, Docker container, or cloud platform such as Azure, AWS, etc.).

2. Set environment variables on the server (connection strings, API keys, etc.).

3. Configure web server

   - IIS: Configure a site pointing to the publish folder and install the ASP.NET Core Hosting Bundle.

   - Linux: Configure a systemd service for the app and a reverse proxy (Nginx/Apache).

Access the application via the configured domain or IP, for example:
https://your-domain.com

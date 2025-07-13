# Penger API

A financial management API built with .NET 7 and PostgreSQL.

## Project Overview

This API handles user authentication, account management, and currency operations for the Penger financial management system. It's built using .NET 7 and PostgreSQL.

## Prerequisites

- .NET 7 SDK
- PostgreSQL 12+
- An IDE (Visual Studio, VS Code, etc.)

## Getting Started

### Database Setup

1. Install PostgreSQL if you haven't already
2. Create a new database named `penger_api`
3. Update the connection string in `appsettings.json` if needed

### Running the Application

1. Clone the repository
2. Navigate to the project directory
3. Run the migrations: `dotnet ef migrations add InitialCreate`
4. Apply the migrations: `dotnet ef database update`
5. Start the application: `dotnet run`

The application will automatically seed the database with initial data for currencies and account types.

## API Documentation

Once the application is running, you can access the Swagger documentation at:
https://localhost:7001/swagger

## Project Structure

- **Controllers/**: API endpoints
- **Models/**: Entity models
- **Data/**: Database context and repositories
- **DTOs/**: Data transfer objects
- **Services/**: Business logic
- **Middleware/**: Custom middleware components
- **Extensions/**: Extension methods
- **Configurations/**: Configuration classes
- **Helpers/**: Utility classes

## Features

- User authentication with JWT
- OTP verification
- Account management
- Currency operations

## Entity Models

- **User**: Represents a user in the system
- **OTP**: One-time passwords for verification
- **Currency**: Available currencies
- **AccountType**: Types of financial accounts
- **Account**: User's financial accounts
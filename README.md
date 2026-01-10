# FruitHub API

## Overview
**FruitHub** is a RESTful e-commerce backend API for managing **products, categories, carts, orders, and users**.

This API is designed to be consumed by **mobile applications** and implements the business rules and functional requirements defined in the **Fruits Hub mobile app:**
[Link](https://github.com/1-abdelrahman-khalil-1/Fruits-Hub-App)

The project is built using **Clean Architecture**, with **JWT + Refresh Token authentication** and **role-based authorization**.

## Live API
The API is deployed and publicly accessible.

Base URL:
https://your-deployed-api-url

Swagger documentation:
https://your-deployed-api-url/swagger
Swagger provides full API documentation, request/response examples, and authentication support.

## Features
- User registration & authentication (JWT + refresh tokens)
- Role-based authorization (Admin / User)
- Product and category management
- Filtering, pagination, and sorting for products and orders
- Favorite List 
- Shopping cart & order workflow
- Centralized exception handling
- Input validation and meaningful error responses
- Unit testing for core business logic

## Tech Stack
- Backend: ASP.NET Core Web API
- Language: C#
- Authentication: ASP.NET Identity + JWT + Refresh Token
- Database: SQL Server
- ORM: Entity Framework Core
- Testing: xUnit, Moq

## Architecture
This project follows Clean Architecture, as explained in the Microsoft article:
[Common web application architectures](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

The main goal of this architecture is to keep the **business logic independent from infrastructure**, making the application easier to test and maintain, and allowing infrastructure changes without affecting the core logic.

### Layers
- API: Controllers, Middlewares
- ApplicationCore: Business logic, services, interfaces, domain models
- Infrastructure: ORM, Data access (Repositories), Identity, third-party services

### Key patterns:
- Repository pattern
- Unit of Work
- Dependency Injection
- Middleware-based error handling

## Setup & Run
You have **two options** to run the project.

### Option 1: Run Locally
1. Clone the repository:
```shell
git clone https://github.com/omargoher/FruitHub.git
cd FruitHub
```
2. Copy `.env.example` to `.env` and configure environment variables.
3. Load environment variables:
```shell
source .env
```
4. Apply Migrations:
```shell
dotnet ef database update  -p ./FruitHub.Infrastructure/ -s ./FruitHub.API/ -c AppIdentityDbContext
dotnet ef database update  -p ./FruitHub.Infrastructure/ -s ./FruitHub.API/ -c ApplicationDbContext
```
6. Run the application
```csharp
dotnet run
```

### Option 2: Use Docker
>(Coming soon)
> Docker support will simplify setup and deployment using containers.

## Future Improvements
- Redis for OTP and password reset token caching
- Rate limiting for API endpoints- Centralized logging
- Order cancellation (Improve)
- Real-time chat
- Notification System
- Payment integration
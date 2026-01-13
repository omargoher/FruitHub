# FruitHub API

## Overview
**FruitHub** is a RESTful e-commerce backend API for managing **users, products, categories, carts, favorites, and orders**.

The API is designed primarily for **mobile applications** and implements the business rules defined in the **FruitHub Mobile App**:  
[https://github.com/1-abdelrahman-khalil-1/Fruits-Hub-App](https://github.com/1-abdelrahman-khalil-1/Fruits-Hub-App)

The project follows **Clean Architecture principles**, uses **JWT + Refresh Tokens** for authentication, and enforces **role-based authorization**.

---

## Core Features
- User registration and authentication
- JWT access tokens with refresh token rotation
- Role-based authorization (Admin / User)
- Product and category management
- Favorites (wishlist) support
- Shopping cart management
- Order lifecycle (checkout, status updates)
- Filtering, sorting, and pagination for products and orders
- Centralized exception handling
- Consistent and meaningful API error responses
- Unit testing for core business logic

---

## Tech Stack
- **Backend Framework:** ASP.NET Core Web API
- **Language:** C#
- **Authentication:** ASP.NET Identity + JWT + Refresh Tokens
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Testing:** xUnit, Moq
- **Containerization:** Docker & Docker Compose

---

## Architecture
FruitHub follows **Clean Architecture**, as described in the official Microsoft guide:  
[https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)

### Architectural Goals
- Keep business logic independent from infrastructure
- Making the application easier to test and maintain
- Allow infrastructure changes without affecting the core logic

### Layers
- API: Controllers, Middlewares
- ApplicationCore: Business logic, services, interfaces, domain models
- Infrastructure: ORM, Data access (Repositories), Identity, third-party services

### Key patterns:
- Repository pattern
- Unit of Work
- Dependency Injection
- Middleware-based error handling

---

## Setup & Run (Docker)
### Prerequisites
- Docker
- Docker Compose

### Steps

1. Clone the repository:
    ```bash
    git clone <repository-url>
    ```

2. Create an `.env` file based on [`.env.example`](./.env.example) and update values as needed.

3. Build and run the application: 
    ```bash
    docker compose up -d --build
    ```

### After Startup
- Swagger UI:
  [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)
- Database migrations are applied automatically on startup
- Default roles and admin user are seeded

#### Admin Account
Admin credentials can be **customized using the `.env` file**.
The following values are the **default configuration** defined in `.env.example`:
- **Full Name:** Omar Goher
- **Username:** OmarGoher
- **Email:** admin@fruithub.com
- **Password:** Admin@123

---

## Current Limitations
- No payment gateway integration yet
- No real-time notifications
- No rate limiting in v1

---

## Planned Improvements
- Redis for OTP and password reset token caching
- Rate limiting for API endpoints
- Centralized logging
- Real-time chat
- Notification System
- Payment gateway integration
# Identity & Access Management

This document describes how **identity-related operations** are handled in the FruitHub API, including **user registration**, **email verification**, **authentication**, **token lifecycle management**, and **password recovery**.

---

## Overview

FruitHub authentication follows a multi-step flow to ensure that only verified users can access the system.

**Key characteristics:**

* JWT-based authentication
* Refresh token support
* Email verification & forget password using OTP
* Role-based authorization

---

## Authentication Flow Summary

1. **User Registration**
2. **Email Confirmation (OTP)**
3. **User Login**
4. **Authenticated Requests**

---

## 1. User Registration

### Endpoint

```
POST /api/Users
```

### Description

Creates a new user account with basic profile information.
After registration, the user **must confirm their email** before being allowed to log in.

### Request Body

```json
{
  "fullName": "Omar Ahmed Goher",
  "userName": "OmarGoher",
  "email": "omar@example.com",
  "password": "StrongPassword@123"
}
```

### Successful Response (201 Created)

```json
{
  "id": 1,
  "fullName": "John Doe",
  "email": "john@example.com"
}
```

### Business Rules

* Email must be unique
* Password must meet security requirements
* Email is **not confirmed** after registration

---

## 2. Email Confirmation (OTP)

Email confirmation is handled using a **One-Time Password (OTP)** sent to the user's email address.

### 2.1 Send Email Confirmation Code

#### Endpoint

```
POST /api/email-confirmations
```

#### Description

Generates an OTP and sends it to the user's email address. 
This operation is asynchronous, so the API returns **202 Accepted**.

#### Request Body

```json
{
  "email": "omar@example.com"
}
```

#### Response

```
202 Accepted
```

#### Behavior

* Generates a 6-digit OTP
* Sends the OTP via email
* OTP is stored temporarily for verification

---

### 2.2 Confirm Email Using OTP

#### Endpoint

```
PUT /api/email-confirmations
```

#### Description

Validates the OTP and marks the user's email as confirmed.

#### Request Body

```json
{
  "email": "omar@example.com",
  "otp": "123456"
}
```

#### Response

```
204 No Content
```

#### Business Rules

* OTP expires after **10 minutes**
* Limited number of verification attempts
* Invalid or expired OTP results in **400 Bad Request**

---

## 3. User Login

### Endpoint

```
POST /api/sessions
```

### Description

Authenticates the user and creates a new session by issuing an **access token (JWT)** and a **refresh token**.

### Login Flow

1. Validate email and password
2. Ensure email is confirmed
3. Generate JWT access token
4. Generate refresh token

### Request Body

```json
{
  "email": "omar@example.com",
  "password": "StrongPassword@123"
}
```

### Successful Response (200 OK)

```json
{
  "email": "omar@example.com",
  "accessToken": "<jwt-token>",
  "refreshToken": "<refresh-token>",
  "accessTokenExpiresAt": "2026-01-12T14:30:00Z",
  "refreshTokenExpiresAt": "2026-02-12T14:30:00Z"
}
```

### Error Responses

* **400 Bad Request** – Invalid payload
* **401 Unauthorized** – Invalid credentials
* **403 Forbidden** – Email not confirmed

---

## 4. Refresh Token Rotation

FruitHub uses **refresh token rotation** to maintain secure user sessions without requiring frequent re-login.

### Endpoint

```
POST /api/sessions/refresh
```

### Description

Refreshes an expired (or soon-to-expire) access token using a valid refresh token. If the refresh token is valid, a **new access token and a new refresh token** are issued.

> **Important:** The old refresh token is revoked immediately and can no longer be used.

---

### Request Body

```json
{
  "refreshToken": "<refresh-token>"
}
```

---

### Successful Response (200 OK)

```json
{
  "email": "omar@example.com",
  "accessToken": "<new-jwt-token>",
  "refreshToken": "<new-refresh-token>",
  "accessTokenExpiresAt": "2026-01-12T15:00:00Z",
  "refreshTokenExpiresAt": "2026-02-12T15:00:00Z"
}
```

---

### Refresh Flow

1. Client sends refresh token
2. Server validates refresh token
3. Old refresh token is revoked
4. New access token is generated
5. New refresh token is generated and stored
6. New token returned to the client

---

### Error Responses

* **400 Bad Request** – Invalid payload
* **401 Unauthorized** – Invalid, expired, or revoked refresh token

---
## 4. Logout

### Endpoint

```
DELETE /api/sessions
```

### Description

Logs out the currently authenticated user by revoking all active refresh tokens associated with the user.

### Logout Flow

1. The user **must be authenticated** using a valid access token.
2. The system **revokes all active refresh tokens** for the user.

### Authorization
- **Required**: Bearer JWT access token

### Response

#### Success
```
204 No Content
```
Logout completed successfully. All refresh tokens have been revoked.

#### Error Responses

* **401 Unauthorized** – The user is not authenticated or the access token is missing/invalid.

---

## 5. Password Reset Flow

FruitHub provides a **secure, multi-step password recovery flow** using OTP verification and short-lived reset tokens.

This design ensures that passwords can only be reset by users who control the registered email address.

---

### Password Reset Summary

1. Request password reset (send OTP)
2. Verify OTP and issue reset token
3. Reset password using reset token

---

## 5.1 Request Password Reset

### Endpoint

```
POST /api/password-resets
```

### Description

Initiates the password reset process by sending a **one-time password (OTP)** to the user's email address.

This operation is asynchronous, so the API returns **202 Accepted**.

### Request Body

```json
{
  "email": "omar@example.com"
}
```

### Response

```
202 Accepted
```

### Behavior

* Generates a 6-digit OTP
* Sends the OTP via email
* OTP is stored temporarily for verification

### Error Responses

* **400 Bad Request** – Invalid payload
* **404 Not Found** – User does not exist

---

## 5.2 Verify Password Reset OTP

### Endpoint

```
PUT /api/password-resets/verify
```

### Description

Validates the OTP sent to the user's email address and returns a **short-lived reset token**.

### Request Body

```json
{
  "email": "omar@example.com",
  "otp": "123456"
}
```

### Successful Response (200 OK)

```json
{
  "resetToken": "<password-reset-token>"
}
```

### Business Rules

* OTP expires after **10 minutes**
* Limited number of verification attempts

### Error Responses

* **400 Bad Request** – Invalid or expired OTP
* **404 Not Found** – User not found

---

## 5.3 Reset Password

### Endpoint

```
PUT /api/password-resets
```

### Description

Resets the user's password using a valid reset token. The reset token is invalidated immediately after use.

### Request Body

```json
{
  "resetToken": "<password-reset-token>",
  "newPassword": "StrongPassword@123"
}
```

### Response

```
204 No Content
```

### Error Responses

* **400 Bad Request** – Invalid or expired reset token
* **404 Not Found** – User not found

---

## 6. Token Usage

### Access Token (JWT)

* Sent in the `Authorization` header
* Used to access protected endpoints

```
Authorization: Bearer <access-token>
```

### Refresh Token

* Used only with the refresh endpoint
* Has a longer lifetime than the access token
* Is **rotated on every refresh**
* Is revoked on logout or reuse detection

---

## Security Notes

* Passwords are never stored in plain text
* Tokens are generated using secure cryptographic algorithms
* Email verification prevents fake or unreachable accounts
* Refresh tokens allow session continuity without re-login

---

## Notes for Clients

* Registration **does not** automatically log the user in
* Email confirmation is **required** before login
* Handle `403 Forbidden` during login by prompting email verification

---



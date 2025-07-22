# Telegram Bot for Car Insurance Sales

This bot assists users in submitting vehicle documents and automatically issues insurance policies via Telegram. Admins can monitor, approve, and simulate document flows.

## Setup Instructions

### 1. Clone the Repository

Repository: https://github.com/MariaVS28/CarInsuranceBot.git

### 2. Configure environment variables
Provide environment variables for:

- "TELEGRAM_BOT_TOKEN": "your telegram bot token"

- "GEMINI_TOKEN": "your gemini token"

- "MINDEE_KEY": "your mindee key"

- "AZURE_DB_PASSWORD": "your azure bd password"

- "MINDEE_VRC_KEY": "your key that you provided for implementation Mindee vehicle registration certificate feature"

- "MINDEE_VRC_ACCOUNT": "your account that you provided for implementation Mindee vehicle registration certificate feature"

Replace the your_*_ placeholders with your actual keys and secrets.

### 3. Restore dependencies and run

```bash
dotnet restore
dotnet run
```

## Features

- Guided document collection
- Real-time insurance policy generation
- PDF delivery inside Telegram
- Admin approval support
- Admin tools for testing and debugging

---

## User Commands

| Command         | Description                                     |
|-----------------|-------------------------------------------------|
| `/start`        | Start interaction with the bot                  |
| `/help`         | Show available commands and provide guid        |
| `/ready`        | Start process                                   |
| `/status`       | Check current status of your process            |
| `/cancel`       | Stop process                                    |
| `/yes`          | Confirm that the shown document is correct      |
| `/no`           | Reject the prociding information                |
| `/resendpolicy` | Resend the generated policy (PDF)               |

---

## Admin Commands

>  **Usage:** Some commands require a Telegram numeric user ID as a parameter (not `@username`).  
> Example: `/giveadmin 51684765`

| Command                    | Description                                                   |
|----------------------------|---------------------------------------------------------------|
| `/giveadmin [userId]`      | Grant admin privileges to a user                              |
| `/revokeadmin [userId]`    | Revoke admin rights from a user                               |
| `/policiessummary`         | Show summary of issued policies                               |
| `/failedpolicieslogs`      | View logs of failed policy generations                        |
| `/mockdocumentdata`        | Simulate document data for QA/testing                         |
| `/unmockdocumentdata`      | Stop using mock data and resume real document parsing         |
| `/getpendingpolicies`      | List all pending policies awaiting approval                   |
| `/approvepolicy [userId]`  | Approve a specific user's pending policy                      |

---

## Unknown Commands

The bot handles unrecognized commands gracefully by informing the user that the command is unknown and suggesting to use `/help` to see the list of valid commands.


---

### Notes

- **UserId** must be a numeric Telegram ID (not `@username`).
- Only users with `IsAdmin = true` can run admin-level commands.
- Use `/help` inside the bot to view this command list interactively.

---

# 🧱 General Architecture

The project follows a layered architecture to maintain clean separation of concerns and improve maintainability.

---

### 📡 API Layer
- Acts as the **entry point** for all external requests.
- Handles HTTP endpoints or Telegram webhook triggers.
- Responsible for routing and request delegation to the business layer.

---

### ⚙️ BLL (Business Logic Layer)
- Contains all **core application logic**.
- Main components:
  - `FlowService` – routes Telegram commands to the appropriate handlers.
  - **Supporting Services**:
    - `AIChatService` – handles AI features (e.g. Gemini).
    - `TelegramService` – manages Telegram Bot interactions.
    - `TelegramFileLoaderService` – downloads and processes user-uploaded files from Telegram.
    - `MindeeService` – integrates with the Mindee document API.
    - `DuplicateRequestDetectorService` – prevents duplicate or rapid requests.
    - `PolicyGenerationService` – generates PDF versions of policies and documents.

---

### 🗄️ DAL (Data Access Layer)
- Contains:
  - **Entity Framework Core models** – representing entities such as `User`, `Policy`, `Document`, etc.
  - `DbContext` – manages database schema and entity tracking.
  - **Repositories** – encapsulate query and persistence logic for accessing the database.

---

# Why Hierarchical CQRS + MediatR is a Poor Fit for a Telegram Bot for Car Insurance Sales

## CQRS + MediatR is Overkill for This Telegram Bot

- **CQRS** is an architectural pattern that separates read and write models.
- It often requires:
  - A second read database
  - Microservices for independent scaling
- This adds significant complexity and usually involves extra libraries like **MediatR**.

---

## Why This Is Not a Good Fit for Our Bot

- The Telegram bot handles simple command processing and user interactions.
- CQRS overhead makes:
  - Development more complicated
  - Maintenance harder
  - Debugging slower
- No real benefit given the bot’s straightforward requirements.

---

### Summary

**CQRS + MediatR is overkill and introduces unnecessary complexity, infrastructure, and operational costs for a simple Telegram bot for car insurance sales.**
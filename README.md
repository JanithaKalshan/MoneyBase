# Chat Service API

This repository hosts a backend service for managing chat sessions, built with a focus on **Clean Architecture** principles, utilizing **MediatR** for a **CQRS (Command Query Responsibility Segregation)** design pattern, and thoroughly tested with **XUnit**.

## Table of Contents

1.  [About The Project](#about-the-project)
    * [Built With](#built-with)
2.  [Project Structure](#project-structure)
3.  [Getting Started](#getting-started)
    * [Prerequisites](#prerequisites)
    * [Installation](#installation)
    * [Running the API](#running-the-api)
    * [Running Tests](#running-tests)
4.  [Core Concepts & Components](#core-concepts--components)
    * [CQRS & MediatR](#cqrs--mediatr)
    * [Chat Queue Service (`IChatQueueService`)](#chat-queue-service-ichatqueueservice)
    * [Teams Creator (`TeamsCreator`)](#teams-creator-teamscreator)
    * [API Endpoints](#api-endpoints)


## About The Project

This project provides a robust and scalable backend API for handling chat session management. It implements a clear separation of concerns, making it maintainable, testable, and adaptable to future changes. The service allows users to initiate chat sessions, poll their status, and retrieve lists of active chats.

### Built With

* **.NET 9.0:** The latest version of .NET for high performance and modern features.
* **ASP.NET Core:** For building robust web APIs.
* **MediatR:** An in-process messaging library used to implement the CQRS pattern, separating commands (actions) from queries (data retrieval).
* **XUnit:** A powerful, open-source testing framework for .NET.
* **Moq:** A mocking library used alongside XUnit for effective unit testing.
* **Microsoft.AspNetCore.Mvc.Testing:** For comprehensive integration testing of the API endpoints.
* **Swagger/OpenAPI:** For interactive API documentation and testing.
* **Dependency Injection (Built-in ASP.NET Core):** For managing service lifetimes and dependencies.

## Project Structure

The solution adheres to **Clean Architecture** principles, dividing the application into distinct layers:

* **`MoneyBase.API`**:
    * **Presentation Layer:** The entry point of the application. Contains ASP.NET Core controllers and API definitions. Handles HTTP requests and mediates them to the Application layer.
* **`MoneyBase.Application`**:
    * **Application Layer:** Contains the application's business logic. This is where MediatR `IRequestHandlers` (Commands and Queries) reside. It orchestrates domain entities and interacts with the Infrastructure layer via interfaces.
* **`MoneyBase.Domain`**:
    * **Domain Layer:** The core of the business logic. Contains domain entities (`ChatSession`), value objects, and domain services. It is independent of all other layers.
* **`MoneyBase.Infrastructure`**:
    * **Infrastructure Layer:** Implements interfaces defined in the Domain and Application layers. Handles external concerns like data persistence, external service integrations, and queue implementations (`ChatQueueService`, `TeamsCreator`).
* **`MoneyBase.Test`**:
    * **Test Project:** Contains all unit and integration tests for the solution, ensuring code quality and correctness.

## Getting Started

To get a local copy up and running, follow these simple steps.

### Prerequisites

* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed on your machine.
* A code editor like [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/).

### Installation

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/JanithaKalshan/MoneyBase.git])
    cd MoneyBase
    ```
    (Replace `JanithaKalshan/MoneyBase` with the actual repository URL).

2.  **Restore NuGet packages:**
    ```bash
    dotnet restore
    ```

### Running the API

To run the web API locally:

1.  **Set `MoneyBase.API` as the startup project.**
    * **In Visual Studio:** Right-click on the `MoneyBase.API` project in Solution Explorer and select "Set as Startup Project."
    * **From command line:** Navigate into the `MoneyBase.API` directory.

2.  **Run the application:**
    ```bash
    dotnet run
    ```
    The API will typically run on `https://localhost:7076` (port varies). The console output will show the exact URLs.

3.  **Access Swagger UI:** Once the application is running, you can access the interactive API documentation and test the endpoints via Swagger UI at:
    `https://localhost:7076/swagger` 
### Running Tests

The project includes both Unit Tests and Integration Tests to ensure high code quality.

1.  **Navigate to the solution root directory:**
    ```bash
    cd C:\MyProjects\MoneyBase\ # Or wherever your solution file (.sln) is located
    ```

2.  **Run all tests:**
    ```bash
    dotnet test
    ```
    This command will discover and execute all unit and integration tests defined in the `MoneyBase.Test` project.

    * **Unit Tests:** Focus on individual components (e.g., MediatR handlers) in isolation, using `Moq` to mock dependencies.
    * **Integration Tests:** Utilize `Microsoft.AspNetCore.Mvc.Testing` to spin up an in-memory instance of the entire `MoneyBase.API` application, testing the API endpoints end-to-end.

## Core Concepts & Components

### CQRS & MediatR

The application leverages **CQRS** (Command Query Responsibility Segregation) to separate read operations (Queries) from write operations (Commands). **MediatR** acts as the dispatcher, sending requests (Commands or Queries) to their respective handlers, promoting a clean and decoupled design.

* **Commands:** (e.g., `CreateChatCommand`, `ChatPollCommand`) represent actions that change the system's state. They are handled by `IRequestHandler<TCommand, TResult>`.
* **Queries:** (e.g., `GetAllChatsQuery`) represent requests for data that do not change the system's state. They are handled by `IRequestHandler<TQuery, TResult>`.

### Chat Queue Service (`IChatQueueService`)

* Located in the `MoneyBase.Infrastructure` layer.
* This service manages the lifecycle of chat sessions, including enqueuing new chats, retrieving active ones, and updating their poll status.
* For this project, `ChatQueueService` is implemented as an **in-memory queue**, suitable for demonstration and testing purposes. 

### Teams Creator (`TeamsCreator`)

* Located in the `MoneyBase.Infrastructure` layer.
* This class is responsible for managing data related to teams. While its specific functionality (e.g., creating teams, retrieving team details) isn't fully detailed in the provided snippets, its presence in the Infrastructure layer indicates it handles external concerns related to team data.

### API Endpoints

The `MoneyBase.API` project exposes the following endpoints:

* **`POST /api/Chat/start-chat`**
    * **Summary:** Start a new chat session with an agent.
    * **Description:** Creates and enqueues a new chat session.
    * **Request Body:** `CreateChatCommand` (e.g., `{ "userId": "string" }`).
    * **Responses:** `200 OK` with the `ChatId` (GUID) of the newly created chat, or `500 Internal Server Error` if the queue is full (in development mode, will include "Queue is full" message).

* **`POST /api/Chat/poll-chat`**
    * **Summary:** Poll a specific chat session.
    * **Description:** Updates the `LastPolledAt` timestamp for a given chat session, indicating it's still active.
    * **Request Body:** `ChatPollCommand` (e.g., `{ "chatId": "guid" }`).
    * **Responses:** `200 OK` (no content).

* **`GET /api/Chat/get-all-chats`**
    * **Summary:** Retrieve a list of all chat sessions.
    * **Description:** Returns details for all chat sessions.
    * **Responses:** `200 OK` with a `List<ChatSession>`.

---


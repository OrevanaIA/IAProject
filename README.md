# AIProject - Task Management Application

## Overview
AIProject is a robust task management application built with C# and .NET 6.0 that implements clean architecture and design patterns. The application provides a comprehensive task management system with features like task status tracking, priority management, and categorization.

## Table of Contents
- [Features](#features)
- [Architecture & Design Patterns](#architecture--design-patterns)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Installation](#installation)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [Deployment](#deployment)
- [Usage Examples](#usage-examples)
- [Validation Rules](#validation-rules)
- [Error Handling](#error-handling)
- [Performance Optimizations](#performance-optimizations)
- [Security Features](#security-features)
- [Best Practices](#best-practices)
- [Contributing](#contributing)
- [License](#license)

## Features
- Create new tasks with descriptions
- Set and update task status (Pending, In Progress, Completed)
- Assign priority levels (Alta, Media, Baja)
- Add and manage task categories
- Filter tasks by status
- Search tasks by description
- Task validation (minimum description length)
- Due date management
- Persistent storage using JSON
- Transaction management
- Thread-safe operations
- Caching for improved performance
- Security logging and input sanitization

## Architecture & Design Patterns

### Repository Pattern
- `ITaskRepository` interface defines the contract for task operations
- Implementation in `TaskRepository` class provides data access logic
- Separates business logic from data access concerns
- Enables easy switching between different data storage implementations

### Unit of Work Pattern
- `IUnitOfWork` interface manages transactions and data persistence
- Ensures data consistency across operations
- Provides transaction management with Begin/Commit/Rollback capabilities
- Centralizes data access through repository management

### DTO Pattern
- `TaskDTO` handles data transfer between layers
- Separates domain models from data transfer objects
- Provides clean data contracts for external communication
- Reduces coupling between layers

### Dependency Injection
- Services are registered and resolved through Microsoft's DI container
- Promotes loose coupling and testability
- Simplifies service lifetime management

## Project Structure
```
AIProject/
├── DTOs/
│   └── TaskDTO.cs                # Data Transfer Objects
├── Infrastructure/
│   ├── TaskRepository.cs         # Repository implementation
│   └── UnitOfWork.cs             # Unit of Work implementation
├── Interfaces/
│   ├── ICacheService.cs          # Cache service interface
│   ├── ISecurityLogger.cs        # Security logging interface
│   ├── ITaskRepository.cs        # Repository interface
│   ├── ITaskService.cs           # Task service interface
│   ├── ITaskValidator.cs         # Task validation interface
│   └── IUnitOfWork.cs            # Unit of Work interface
├── Models/
│   └── PaginationParams.cs       # Pagination parameters
├── Security/
│   └── InputSanitizer.cs         # Input sanitization utilities
├── Services/
│   ├── ConsoleSecurityLogger.cs  # Security logger implementation
│   ├── InMemoryCacheService.cs   # Cache service implementation
│   ├── TaskService.cs            # Task service implementation
│   └── TaskValidator.cs          # Task validation implementation
├── Program.cs                    # Application entry point
├── TaskItem.cs                   # Domain model
├── tasks.json                    # Data storage file
└── Tests/                        # Unit tests directory
    ├── TaskItemTests.cs          # Tests for TaskItem
    └── TaskRepositoryTests.cs    # Tests for TaskRepository
```

## Requirements
- .NET 6.0 SDK or higher
- Visual Studio 2022, Visual Studio Code, or any compatible IDE
- Git (for cloning the repository)

## Installation

### Option 1: Clone the Repository
1. Open a terminal or command prompt
2. Clone the repository:
   ```bash
   git clone https://your-repository-url/AIProject.git
   cd AIProject
   ```
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Build the solution:
   ```bash
   dotnet build
   ```

### Option 2: Download the Source Code
1. Download the source code as a ZIP file from the repository
2. Extract the ZIP file to your preferred location
3. Open a terminal or command prompt and navigate to the extracted directory
4. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
5. Build the solution:
   ```bash
   dotnet build
   ```

### Option 3: Using Visual Studio
1. Clone or download the repository as described above
2. Open the solution file (`AIModulo03.sln`) in Visual Studio
3. Right-click on the solution in Solution Explorer and select "Restore NuGet Packages"
4. Build the solution by pressing Ctrl+Shift+B or selecting Build > Build Solution from the menu

## Running the Application

### From Command Line
Navigate to the project directory and run:
```bash
dotnet run
```

### From Visual Studio
1. Open the solution in Visual Studio
2. Set the main project as the startup project (right-click on the project and select "Set as Startup Project")
3. Press F5 or click the "Start" button to run the application

## Testing

### Running Tests from Command Line
1. Navigate to the project directory
2. Run all tests:
   ```bash
   dotnet test
   ```
3. Run specific test project:
   ```bash
   dotnet test ./Tests/Tests.csproj
   ```
4. Run tests with coverage:
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov.info
   ```

### Running Tests from Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Click "Run All Tests" or select specific tests to run
4. View test results and coverage in the Test Explorer window

### Test Categories
- **Unit Tests**: Tests individual components in isolation
- **Integration Tests**: Tests interactions between components
- **Validation Tests**: Ensures data validation rules are enforced correctly

## Deployment

### Publishing for Production
1. Navigate to the project directory
2. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
3. The published files will be in the `./publish` directory

### Deployment Options

#### Self-contained Deployment
To create a self-contained deployment that includes the .NET runtime:
```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```
Replace `win-x64` with the appropriate runtime identifier for your target platform:
- Windows: `win-x64`, `win-x86`
- macOS: `osx-x64`, `osx-arm64`
- Linux: `linux-x64`, `linux-arm64`

#### Docker Deployment
1. Create a Dockerfile in the project root:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
   WORKDIR /app
   
   COPY *.csproj ./
   RUN dotnet restore
   
   COPY . ./
   RUN dotnet publish -c Release -o out
   
   FROM mcr.microsoft.com/dotnet/runtime:6.0
   WORKDIR /app
   COPY --from=build /app/out .
   ENTRYPOINT ["dotnet", "AIModulo03.dll"]
   ```
2. Build the Docker image:
   ```bash
   docker build -t aiproject .
   ```
3. Run the container:
   ```bash
   docker run -it aiproject
   ```

## Usage Examples

### Basic Task Management
```csharp
// Using Unit of Work pattern
using (IUnitOfWork unitOfWork = new UnitOfWork())
{
    try
    {
        // Begin transaction
        unitOfWork.BeginTransaction();

        // Create a new task using DTO
        var taskDto = new TaskDTO
        {
            Description = "Complete project documentation",
            Status = TaskStatus.Pending,
            Priority = Priority.Alta,
            DueDate = DateTime.Now.AddDays(7)
        };

        // Add task through repository
        unitOfWork.TaskRepository.Add(taskDto);

        // Commit transaction and save changes
        unitOfWork.CommitTransaction();
        unitOfWork.SaveChanges();
    }
    catch (Exception)
    {
        // Rollback on error
        unitOfWork.RollbackTransaction();
        throw;
    }
}
```

### Using Cache and Pagination
```csharp
// Obtain tasks with pagination
var paginationParams = new PaginationParams(1, 10);
var tasks = await repository.GetAllPagedAsync(paginationParams);

// Use cache
var cachedTask = await cacheService.GetAsync<TaskDTO>(taskId);
```

### Security Example
```csharp
// Sanitize input
var description = InputSanitizer.SanitizeTaskDescription(input);

// Security logging
await securityLogger.LogOperationAsync(
    "UpdateTask",
    $"Task {taskId} updated",
    userId
);
```

## Validation Rules
- Task description must be between 10 and 100 characters
- Task status must be a valid TaskStatus enum value
- Priority must be a valid Priority enum value
- Task ID must be unique
- Creation date and last modified date are automatically managed

## Error Handling
The application includes robust error handling for:
- Invalid task parameters
- File I/O operations
- Data validation
- Concurrent access to task data
- Transaction management

## Performance Optimizations

### Caching
- Implementation of ICacheService for frequently accessed tasks
- Automatic cache invalidation
- Efficient memory management

### Pagination
- PaginationParams for large data sets
- Configurable page limits
- Optimized sorting

### Optimized Queries
- Asynchronous methods in ITaskRepository
- Indexed searches
- Efficient data loading

## Security Features

### Data Validation
- InputSanitizer for injection prevention
- Robust input validation
- Output sanitization

### Logging and Auditing
- ISecurityLogger for critical operations
- Modification tracking
- Security monitoring

## Best Practices
- SOLID principles implementation
- Clean Code architecture
- Design patterns usage
- Comprehensive unit testing
- Exception handling
- Data validation
- Thread-safe operations
- Transaction management

## Contributing
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License
This project is licensed under the MIT License - see the LICENSE file for details.

---

**Note**: This project contains code with namespace "Sprint02Tasks" but the project file is named "AIModulo03.csproj". This discrepancy is maintained for backward compatibility.

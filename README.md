# AIProject - Task Management Application

## Overview
AIProject is a robust task management application built with C# and .NET 6.0 that implements clean architecture and design patterns. The application provides a comprehensive task management system with features like task status tracking, priority management, categorization, and secure user authentication.

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
- JWT-based authentication and authorization
- User management with secure password handling

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
├── Controllers/
│   ├── AuthController.cs         # Authentication endpoints
│   └── TasksController.cs        # Task management endpoints
├── DTOs/
│   ├── AuthDTOs.cs               # Authentication data transfer objects
│   └── TaskDTO.cs                # Task data transfer objects
├── Infrastructure/
│   ├── TaskRepository.cs         # Task repository implementation
│   ├── UnitOfWork.cs             # Unit of Work implementation
│   └── UserRepository.cs         # User repository implementation
├── Interfaces/
│   ├── IAuthService.cs           # Authentication service interface
│   ├── ICacheService.cs          # Cache service interface
│   ├── ISecurityLogger.cs        # Security logging interface
│   ├── ITaskRepository.cs        # Task repository interface
│   ├── ITaskService.cs           # Task service interface
│   ├── ITaskValidator.cs         # Task validation interface
│   ├── IUnitOfWork.cs            # Unit of Work interface
│   └── IUserRepository.cs        # User repository interface
├── Models/
│   ├── PaginationParams.cs       # Pagination parameters
│   └── User.cs                   # User domain model
├── Security/
│   └── InputSanitizer.cs         # Input sanitization utilities
├── Services/
│   ├── AuthService.cs            # Authentication service implementation
│   ├── ConsoleSecurityLogger.cs  # Security logger implementation
│   ├── InMemoryCacheService.cs   # Cache service implementation
│   ├── TaskService.cs            # Task service implementation
│   └── TaskValidator.cs          # Task validation implementation
├── appsettings.json              # Application configuration
├── appsettings.Development.json  # Development configuration
├── Program.cs                    # Application entry point
├── TaskItem.cs                   # Task domain model
├── tasks.json                    # Task data storage file
└── Tests/                        # Unit tests directory
    ├── CacheServiceTests.cs      # Tests for cache service
    ├── SecurityTests.cs          # Tests for security features
    ├── TaskItemTests.cs          # Tests for task domain model
    ├── TaskRepositoryTests.cs    # Tests for task repository
    ├── TaskServiceTests.cs       # Tests for task service
    └── TaskValidatorTests.cs     # Tests for task validation
```

## Requirements
- .NET 6.0 SDK or higher
- Visual Studio 2022, Visual Studio Code, or any compatible IDE
- Git (for cloning the repository)
- Internet connection (for NuGet package restoration)
- Minimum 4GB RAM and 1GB disk space

## Installation

### Prerequisites
Before installing, ensure you have:
- .NET 6.0 SDK installed (verify with `dotnet --version`)
- Git installed (verify with `git --version`)
- Sufficient disk space (at least 1GB)
- Required permissions to install packages and create files

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

### Configuration
1. Review and update the `appsettings.json` file with your environment-specific settings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "YourConnectionStringHere"
     },
     "JwtSettings": {
       "SecretKey": "YourSecretKeyHere",
       "Issuer": "YourIssuer",
       "Audience": "YourAudience",
       "ExpirationInMinutes": 60
     }
   }
   ```
2. For development, you can modify `appsettings.Development.json` with development-specific settings

### Troubleshooting Installation Issues
- **Package Restore Fails**: Ensure you have internet connectivity and try running `dotnet restore --force`
- **Build Errors**: Check for compatibility issues with .NET version using `dotnet --info`
- **Permission Issues**: Run your terminal/command prompt as administrator (Windows) or use sudo (Linux/macOS)
- **Missing Dependencies**: Ensure all required .NET workloads are installed with `dotnet workload install`

## Running the Application

### From Command Line
Navigate to the project directory and run:
```bash
# Run with default configuration
dotnet run

# Run with specific environment
dotnet run --environment Development

# Run with specific configuration file
dotnet run --configuration Release
```

### From Visual Studio
1. Open the solution in Visual Studio
2. Set the main project as the startup project (right-click on the project and select "Set as Startup Project")
3. Select the desired configuration (Debug/Release) from the dropdown menu
4. Press F5 or click the "Start" button to run the application

### Using the API
Once the application is running:
1. The API will be available at `https://localhost:5001` or `http://localhost:5000` (check console output for exact URL)
2. You can use tools like Postman, curl, or Swagger UI to interact with the API
3. For authentication, obtain a JWT token by sending a POST request to `/api/auth/login` with valid credentials
4. Use the token in the Authorization header for subsequent requests: `Authorization: Bearer {your_token}`

## Testing

### Test Categories
- **Unit Tests**: Tests individual components in isolation
  - TaskItemTests: Validates task domain model behavior
  - TaskValidatorTests: Ensures validation rules work correctly
  - CacheServiceTests: Verifies caching functionality
- **Integration Tests**: Tests interactions between components
  - TaskRepositoryTests: Tests repository operations with data storage
  - TaskServiceTests: Verifies service layer integration
- **Security Tests**: Validates security features
  - SecurityTests: Tests input sanitization and security logging
  - AuthenticationTests: Verifies JWT token generation and validation

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
4. Run tests with filters:
   ```bash
   # Run only unit tests
   dotnet test --filter Category=Unit
   
   # Run tests for specific component
   dotnet test --filter FullyQualifiedName~TaskRepository
   ```
5. Run tests with coverage:
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov.info
   ```
6. Generate HTML coverage report (requires ReportGenerator tool):
   ```bash
   # Install ReportGenerator if not already installed
   dotnet tool install -g dotnet-reportgenerator-globaltool
   
   # Generate coverage report
   reportgenerator -reports:./lcov.info -targetdir:./coverage -reporttypes:Html
   ```

### Running Tests from Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Click "Run All Tests" or select specific tests to run
4. View test results and coverage in the Test Explorer window
5. For code coverage visualization, install the "Fine Code Coverage" extension

### Troubleshooting Test Issues
- **Tests Failing**: Check test output for specific error messages
- **Coverage Not Generating**: Ensure Coverlet packages are properly installed
- **Slow Tests**: Use `--blame` flag to identify slow running tests: `dotnet test --blame`
- **Parallel Test Execution**: Speed up tests with `dotnet test --parallel`

## Deployment

### Pre-Deployment Checklist
- Ensure all tests pass: `dotnet test`
- Update configuration for production environment
- Remove any sensitive information from code and configuration
- Set appropriate logging levels
- Verify database migrations (if applicable)

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

#### Trimmed Deployment (Reduce Size)
For smaller deployment packages:
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishTrimmed=true -o ./publish
```

#### Docker Deployment
1. Create a Dockerfile in the project root:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
   WORKDIR /app
   EXPOSE 80
   EXPOSE 443
   
   FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
   WORKDIR /src
   COPY ["AIModulo03.csproj", "./"]
   RUN dotnet restore "AIModulo03.csproj"
   COPY . .
   RUN dotnet build "AIModulo03.csproj" -c Release -o /app/build
   
   FROM build AS publish
   RUN dotnet publish "AIModulo03.csproj" -c Release -o /app/publish
   
   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "AIModulo03.dll"]
   ```
2. Build the Docker image:
   ```bash
   docker build -t aiproject .
   ```
3. Run the container:
   ```bash
   docker run -p 8080:80 -p 8443:443 aiproject
   ```

#### Cloud Deployment

##### Azure App Service
1. Create an Azure App Service:
   ```bash
   az group create --name myResourceGroup --location westeurope
   az appservice plan create --name myAppServicePlan --resource-group myResourceGroup --sku B1
   az webapp create --name myAIProject --resource-group myResourceGroup --plan myAppServicePlan --runtime "DOTNET|6.0"
   ```
2. Deploy the application:
   ```bash
   dotnet publish -c Release
   cd bin/Release/net6.0/publish
   zip -r site.zip *
   az webapp deployment source config-zip --resource-group myResourceGroup --name myAIProject --src site.zip
   ```

##### AWS Elastic Beanstalk
1. Install the AWS EB CLI
2. Initialize the EB environment:
   ```bash
   eb init -p dotnet AIProject
   ```
3. Create the environment:
   ```bash
   eb create AIProject-env
   ```
4. Deploy the application:
   ```bash
   dotnet publish -c Release
   eb deploy
   ```

### Post-Deployment Verification
1. Verify the application is running correctly:
   ```bash
   curl https://your-deployment-url/api/health
   ```
2. Check logs for any errors:
   ```bash
   # For Docker
   docker logs aiproject
   
   # For Azure
   az webapp log tail --name myAIProject --resource-group myResourceGroup
   
   # For AWS
   eb logs
   ```
3. Run smoke tests to verify critical functionality

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

### Authentication & Authorization
- JWT-based authentication system
- Role-based access control
- Token expiration and refresh mechanisms
- Secure password storage with hashing and salting

### Data Validation
- InputSanitizer for injection prevention
- Robust input validation
- Output sanitization
- Request throttling and rate limiting

### Logging and Auditing
- ISecurityLogger for critical operations
- Modification tracking
- Security monitoring
- Audit trails for sensitive operations

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

## API Documentation

### Authentication Endpoints
- `POST /api/auth/register`: Register a new user
- `POST /api/auth/login`: Authenticate and receive JWT token
- `POST /api/auth/refresh`: Refresh an expired token

### Task Management Endpoints
- `GET /api/tasks`: Get all tasks (supports pagination and filtering)
- `GET /api/tasks/{id}`: Get a specific task by ID
- `POST /api/tasks`: Create a new task
- `PUT /api/tasks/{id}`: Update an existing task
- `DELETE /api/tasks/{id}`: Delete a task

For detailed API documentation, refer to the Swagger UI available at `/swagger` when running the application in development mode.

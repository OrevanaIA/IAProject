using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Models;

namespace AIProject.Services
{
    public class TaskService : ITaskService
    {
        // Synchronous methods implementation
        
        public TaskDTO CreateTask(TaskDTO taskDto)
        {
            return CreateTaskAsync(taskDto).GetAwaiter().GetResult();
        }
        
        public IEnumerable<TaskDTO> GetAllTasks()
        {
            return GetAllTasksAsync().GetAwaiter().GetResult();
        }
        
        public void UpdateTaskStatus(int taskId, AIProject.Models.TaskStatus status)
        {
            UpdateTaskStatusAsync(taskId, status).GetAwaiter().GetResult();
        }
        
        public void DeleteTask(int id)
        {
            DeleteTaskAsync(id).GetAwaiter().GetResult();
        }
        
        public void AddCategoryToTask(int taskId, string category)
        {
            AddCategoryToTaskAsync(taskId, category).GetAwaiter().GetResult();
        }
        
        public void UpdateTaskPriority(int taskId, AIProject.Models.Priority priority)
        {
            UpdateTaskPriorityAsync(taskId, priority).GetAwaiter().GetResult();
        }
        
        public IEnumerable<TaskDTO> SearchTasks(string searchTerm)
        {
            return SearchTasksAsync(searchTerm).GetAwaiter().GetResult();
        }
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskValidator _taskValidator;
        private readonly ICacheService _cacheService;
        private readonly ISecurityLogger _securityLogger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public TaskService(
            IUnitOfWork unitOfWork,
            ITaskValidator taskValidator,
            ICacheService cacheService,
            ISecurityLogger securityLogger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _taskValidator = taskValidator ?? throw new ArgumentNullException(nameof(taskValidator));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _securityLogger = securityLogger ?? throw new ArgumentNullException(nameof(securityLogger));
        }

        private string GetCacheKey(int taskId) => $"task_{taskId}";
        private string GetListCacheKey(string type, string value) => $"tasks_{type}_{value}";

        private async Task ExecuteInTransactionAsync(Func<Task> action, string operationName)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                await action();
                _unitOfWork.SaveChanges();
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                await _securityLogger.LogOperationAsync(
                    operationName,
                    $"Error: {ex.Message}",
                    "system"
                );
                throw;
            }
        }

        public async Task<TaskDTO> CreateTaskAsync(TaskDTO taskDto)
        {
            if (taskDto == null)
                throw new ArgumentNullException(nameof(taskDto));

            await _taskValidator.ValidateTaskAsync(taskDto);

            var startTime = DateTime.UtcNow;
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.Add(taskDto);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskDto.Id.ToString(),
                    "Created new task",
                    "system"
                );
            }, "CreateTask");

            var duration = DateTime.UtcNow - startTime;
            await _securityLogger.LogPerformanceMetricAsync(
                "CreateTask",
                duration,
                $"Task ID: {taskDto.Id}"
            );

            await _cacheService.SetAsync(GetCacheKey(taskDto.Id), taskDto, _cacheExpiration);
            return taskDto;
        }

        public async Task<TaskDTO> GetTaskAsync(int id)
        {
            var cacheKey = GetCacheKey(id);
            var cachedTask = await _cacheService.GetAsync<TaskDTO>(cacheKey);
            
            if (cachedTask != null)
                return cachedTask;

            var task = _unitOfWork.TaskRepository.GetById(id);
            if (task != null)
            {
                await _cacheService.SetAsync(cacheKey, task, _cacheExpiration);
            }
            return task;
        }

        public async Task<IEnumerable<TaskDTO>> GetAllTasksAsync()
        {
            const string cacheKey = "all_tasks";
            var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = _unitOfWork.TaskRepository.GetAll();
            await _cacheService.SetAsync(cacheKey, tasks, _cacheExpiration);
            return tasks;
        }

        public async Task<IEnumerable<TaskDTO>> GetTasksByStatusAsync(AIProject.Models.TaskStatus status)
        {
            var cacheKey = GetListCacheKey("status", status.ToString());
            var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = _unitOfWork.TaskRepository.GetByStatus(status);
            await _cacheService.SetAsync(cacheKey, tasks, _cacheExpiration);
            return tasks;
        }

        public async Task<IEnumerable<TaskDTO>> GetTasksByPriorityAsync(AIProject.Models.Priority priority)
        {
            var cacheKey = GetListCacheKey("priority", priority.ToString());
            var cachedTasks = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedTasks != null)
                return cachedTasks;

            var tasks = _unitOfWork.TaskRepository.GetByPriority(priority);
            await _cacheService.SetAsync(cacheKey, tasks, _cacheExpiration);
            return tasks;
        }

        public async Task<IEnumerable<TaskDTO>> SearchTasksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));

            var cacheKey = GetListCacheKey("search", searchTerm);
            var cachedResults = await _cacheService.GetAsync<IEnumerable<TaskDTO>>(cacheKey);
            
            if (cachedResults != null)
                return cachedResults;

            var results = _unitOfWork.TaskRepository.Search(searchTerm);
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5)); // Shorter cache for search results
            return results;
        }

        public async Task UpdateTaskAsync(TaskDTO taskDto)
        {
            if (taskDto == null)
                throw new ArgumentNullException(nameof(taskDto));

            await _taskValidator.ValidateTaskAsync(taskDto);

            var startTime = DateTime.UtcNow;
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.Update(taskDto);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskDto.Id.ToString(),
                    "Updated task",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskDto.Id));
            }, "UpdateTask");

            var duration = DateTime.UtcNow - startTime;
            await _securityLogger.LogPerformanceMetricAsync(
                "UpdateTask",
                duration,
                $"Task ID: {taskDto.Id}"
            );
        }

        public async Task DeleteTaskAsync(int id)
        {
            var startTime = DateTime.UtcNow;
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.Delete(id);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    id.ToString(),
                    "Deleted task",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(id));
            }, "DeleteTask");

            var duration = DateTime.UtcNow - startTime;
            await _securityLogger.LogPerformanceMetricAsync(
                "DeleteTask",
                duration,
                $"Task ID: {id}"
            );
        }

        public async Task AddCategoryToTaskAsync(int taskId, string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty", nameof(category));

            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.AddCategory(taskId, category);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskId.ToString(),
                    $"Added category: {category}",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskId));
            }, "AddCategory");
        }

        public async Task UpdateTaskStatusAsync(int taskId, AIProject.Models.TaskStatus status)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.UpdateStatus(taskId, status);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskId.ToString(),
                    $"Updated status to: {status}",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskId));
            }, "UpdateStatus");
        }

        public async Task UpdateTaskPriorityAsync(int taskId, AIProject.Models.Priority priority)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                _unitOfWork.TaskRepository.UpdatePriority(taskId, priority);
                await _securityLogger.LogDataChangeAsync(
                    "Task",
                    taskId.ToString(),
                    $"Updated priority to: {priority}",
                    "system"
                );
                await _cacheService.RemoveAsync(GetCacheKey(taskId));
            }, "UpdatePriority");
        }
    }
}

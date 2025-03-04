using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using AIProject.Models;

namespace AIProject.Infrastructure
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _filePath;
        private List<TaskItem> _tasks;
        private readonly object _lock = new object();

        public TaskRepository(string filePath = "tasks.json")
        {
            _filePath = filePath;
            LoadTasks();
        }

        private void LoadTasks()
        {
            lock (_lock)
            {
                if (System.IO.File.Exists(_filePath))
                {
                    string json = System.IO.File.ReadAllText(_filePath);
                    _tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
                }
                else
                {
                    _tasks = new List<TaskItem>();
                }
            }
        }

        public void SaveChanges()
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(_filePath, json);
            }
        }

        public async Task SaveChangesAsync()
        {
            string json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(_filePath, json);
        }

        public TaskDTO GetById(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            return task != null ? TaskDTO.FromEntity(task) : null;
        }

        public IEnumerable<TaskDTO> GetAll()
        {
            return _tasks.Select(TaskDTO.FromEntity);
        }

        public IEnumerable<TaskDTO> GetByStatus(AIProject.Models.TaskStatus status)
        {
            return _tasks.Where(t => t.Status == status).Select(TaskDTO.FromEntity);
        }

        public IEnumerable<TaskDTO> Search(string searchTerm)
        {
            return _tasks
                .Where(t => t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(TaskDTO.FromEntity);
        }

        public void Add(TaskDTO taskDto)
        {
            if (taskDto == null) throw new ArgumentNullException(nameof(taskDto));
            
            var task = taskDto.ToEntity();
            if (_tasks.Any(t => t.Id == task.Id))
            {
                throw new InvalidOperationException($"Task with ID {task.Id} already exists");
            }
            
            _tasks.Add(task);
        }

        public void Update(TaskDTO taskDto)
        {
            if (taskDto == null) throw new ArgumentNullException(nameof(taskDto));

            var existingIndex = _tasks.FindIndex(t => t.Id == taskDto.Id);
            if (existingIndex == -1)
            {
                throw new InvalidOperationException($"Task with ID {taskDto.Id} not found");
            }

            _tasks[existingIndex] = taskDto.ToEntity();
        }

        public void Delete(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                _tasks.Remove(task);
            }
        }

        public IEnumerable<TaskDTO> GetByPriority(Priority priority)
        {
            return _tasks.Where(t => t.Priority == priority).Select(TaskDTO.FromEntity);
        }

        public void AddCategory(int taskId, string category)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                throw new InvalidOperationException($"Task with ID {taskId} not found");
            }

            if (!task.Categories.Contains(category))
            {
                task.Categories.Add(category);
            }
        }

        public void UpdateStatus(int taskId, AIProject.Models.TaskStatus status)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                throw new InvalidOperationException($"Task with ID {taskId} not found");
            }

            task.Status = status;
            task.LastModifiedDate = DateTime.Now;
        }

        public void UpdatePriority(int taskId, Priority priority)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
            {
                throw new InvalidOperationException($"Task with ID {taskId} not found");
            }

            task.Priority = priority;
            task.LastModifiedDate = DateTime.Now;
        }

        // Async interface implementations
        public async Task<TaskDTO> GetByIdAsync(int id)
        {
            return await Task.FromResult(GetById(id));
        }

        public async Task<IEnumerable<TaskDTO>> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var query = GetAll();
            
            // Apply pagination
            var pagedItems = query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize);
                
            return await Task.FromResult(pagedItems);
        }

        public async Task<IEnumerable<TaskDTO>> GetByStatusPagedAsync(AIProject.Models.TaskStatus status, PaginationParams paginationParams)
        {
            var query = GetByStatus(status);
            
            // Apply pagination
            var pagedItems = query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize);
                
            return await Task.FromResult(pagedItems);
        }

        public async Task<IEnumerable<TaskDTO>> SearchOptimizedAsync(string searchTerm, PaginationParams paginationParams)
        {
            var query = Search(searchTerm);
            
            // Apply pagination
            var pagedItems = query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize);
                
            return await Task.FromResult(pagedItems);
        }

        public async Task<int> AddAsync(TaskDTO task)
        {
            Add(task);
            await SaveChangesAsync();
            return task.Id;
        }

        public async Task UpdateAsync(TaskDTO task)
        {
            Update(task);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Delete(id);
            await SaveChangesAsync();
        }

        public async Task<IEnumerable<TaskDTO>> GetByPriorityPagedAsync(Priority priority, PaginationParams paginationParams)
        {
            var query = GetByPriority(priority);
            
            // Apply pagination
            var pagedItems = query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize);
                
            return await Task.FromResult(pagedItems);
        }

        public async Task AddCategoryAsync(int taskId, string category)
        {
            AddCategory(taskId, category);
            await SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int taskId, AIProject.Models.TaskStatus status)
        {
            UpdateStatus(taskId, status);
            await SaveChangesAsync();
        }

        public async Task UpdatePriorityAsync(int taskId, Priority priority)
        {
            UpdatePriority(taskId, priority);
            await SaveChangesAsync();
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await Task.FromResult(_tasks.Count);
            }
            
            return await Task.FromResult(_tasks.Count(t => 
                t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task InvalidateCacheAsync(int taskId)
        {
            // In a real implementation, this would invalidate cache entries
            // Since we don't have a cache implementation, this is a no-op
            await Task.CompletedTask;
        }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using AIProject.Models;

// Task Repository Class (Handles data access)
public class TaskRepository
{
    private List<TaskItem> tasks;
    private int nextId;
    private readonly string jsonFilePath = "tasks.json";

    public TaskRepository()
    {
        LoadFromJson();
        nextId = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;
    }

    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    private void SaveToJson()
    {
        string jsonString = JsonSerializer.Serialize(tasks, GetJsonOptions());
        File.WriteAllText(jsonFilePath, jsonString);
        
    }

    private void LoadFromJson()
    {
        if (File.Exists(jsonFilePath))
        {
            string jsonString = File.ReadAllText(jsonFilePath);
            tasks = JsonSerializer.Deserialize<List<TaskItem>>(jsonString, GetJsonOptions()) ?? new List<TaskItem>();
        }
        else
        {
            tasks = new List<TaskItem>();
        }
    }

    public void AddTask(string description, AIProject.Models.TaskStatus status, Priority priority = Priority.Media, DateTime? dueDate = null, List<string> categories = null)
    {
        // Validate description length
        if (string.IsNullOrEmpty(description) || description.Length < 10 || description.Length > 100)
        {
            throw new ArgumentException("Description must be between 10 and 100 characters.");
        }

        // Validate status is a valid enum value
        if (!Enum.IsDefined(typeof(AIProject.Models.TaskStatus), status))
        {
            throw new ArgumentException("Invalid task status.");
        }

        // Validate priority is a valid enum value
        if (!Enum.IsDefined(typeof(Priority), priority))
        {
            throw new ArgumentException("Invalid priority level.");
        }

        var task = new TaskItem(nextId++, description, status)
        {
            Priority = priority,
            DueDate = dueDate,
            Categories = categories ?? new List<string>()
        };

        tasks.Add(task);
        SaveToJson();
    }

    public void UpdateTaskDueDate(int id, DateTime? dueDate)
    {
        var task = FindTask(id);
        if (task != null)
        {
            task.DueDate = dueDate;
            task.LastModifiedDate = DateTime.Now;
            SaveToJson();
        }
        else
        {
            throw new ArgumentException("Task not found.");
        }
    }

    public void UpdateTaskPriority(int id, Priority priority)
    {
        if (!Enum.IsDefined(typeof(Priority), priority))
        {
            throw new ArgumentException("Invalid priority level.");
        }

        var task = FindTask(id);
        if (task != null)
        {
            task.Priority = priority;
            task.LastModifiedDate = DateTime.Now;
            SaveToJson();
        }
        else
        {
            throw new ArgumentException("Task not found.");
        }
    }

    public void UpdateTaskStatus(int id, AIProject.Models.TaskStatus status)
    {
        if (!Enum.IsDefined(typeof(AIProject.Models.TaskStatus), status))
        {
            throw new ArgumentException("Invalid task status.");
        }

        var task = FindTask(id);
        if (task != null)
        {
            task.Status = status;
            task.LastModifiedDate = DateTime.Now;
            SaveToJson();
        }
        else
        {
            throw new ArgumentException("Task not found.");
        }
    }

    public void AddTaskCategory(int id, string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category cannot be empty.");
        }

        var task = FindTask(id);
        if (task != null)
        {
            if (!task.Categories.Contains(category))
            {
                task.Categories.Add(category);
                task.LastModifiedDate = DateTime.Now;
                SaveToJson();
            }
        }
        else
        {
            throw new ArgumentException("Task not found.");
        }
    }

    public void RemoveTaskCategory(int id, string category)
    {
        var task = FindTask(id);
        if (task != null)
        {
            if (task.Categories.Remove(category))
            {
                task.LastModifiedDate = DateTime.Now;
                SaveToJson();
            }
        }
        else
        {
            throw new ArgumentException("Task not found.");
        }
    }

    public List<TaskItem> ListTasks(
        AIProject.Models.TaskStatus? statusFilter = null,
        string searchTerm = null,
        string sortBy = null,
        bool ascending = true,
        List<string> categories = null,
        int? pageSize = null,
        int pageNumber = 1)
    {
        LoadFromJson(); // Refresh from file in case of external changes
        
        // Apply filters
        var filteredTasks = tasks.AsEnumerable();
        
        if (statusFilter.HasValue)
        {
            filteredTasks = filteredTasks.Where(t => t.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredTasks = filteredTasks.Where(t => 
                t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (categories != null && categories.Any())
        {
            filteredTasks = filteredTasks.Where(t => 
                t.Categories.Any(c => categories.Contains(c, StringComparer.OrdinalIgnoreCase)));
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            filteredTasks = sortBy.ToLower() switch
            {
                "id" => ascending ? filteredTasks.OrderBy(t => t.Id) : filteredTasks.OrderByDescending(t => t.Id),
                "description" => ascending ? filteredTasks.OrderBy(t => t.Description) : filteredTasks.OrderByDescending(t => t.Description),
                "status" => ascending ? filteredTasks.OrderBy(t => t.Status) : filteredTasks.OrderByDescending(t => t.Status),
                "priority" => ascending ? filteredTasks.OrderBy(t => t.Priority) : filteredTasks.OrderByDescending(t => t.Priority),
                "duedate" => ascending ? filteredTasks.OrderBy(t => t.DueDate) : filteredTasks.OrderByDescending(t => t.DueDate),
                "creationdate" => ascending ? filteredTasks.OrderBy(t => t.CreationDate) : filteredTasks.OrderByDescending(t => t.CreationDate),
                "lastmodified" => ascending ? filteredTasks.OrderBy(t => t.LastModifiedDate) : filteredTasks.OrderByDescending(t => t.LastModifiedDate),
                _ => filteredTasks
            };
        }

        var result = filteredTasks.ToList();

        // Apply pagination
        if (pageSize.HasValue && pageSize.Value > 0)
        {
            result = result
                .Skip((pageNumber - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .ToList();
        }

        return result;
    }

    public TaskItem FindTask(int id)
    {
        LoadFromJson(); // Refresh from file in case of external changes
        return tasks.FirstOrDefault(t => t.Id == id);
    }

    public bool DeleteTask(int id, bool confirmDelete = true)
    {
        var taskToRemove = tasks.FirstOrDefault(t => t.Id == id);
        if (taskToRemove != null)
        {
            if (!confirmDelete || ConfirmAction($"Are you sure you want to delete task {id}? (y/n): "))
            {
                tasks.Remove(taskToRemove);
                SaveToJson();
                return true;
            }
        }
        return false;
    }

    private bool ConfirmAction(string message)
    {
        Console.Write(message);
        var response = Console.ReadLine()?.Trim().ToLower() ?? "n";
        return response == "y" || response == "yes";
    }
}

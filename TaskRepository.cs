using System.Text.Json;
using System.Text.Json.Serialization;
using AIProject.Models;

/// <summary>
/// Repositorio que gestiona el acceso a datos de tareas, proporcionando operaciones CRUD
/// y funcionalidades adicionales para manipular tareas.
/// </summary>
/// <remarks>
/// Esta clase implementa el patrón Repository, encapsulando la lógica de acceso a datos
/// y proporcionando una API para manipular tareas. Características principales:
/// - Persistencia de datos en formato JSON
/// - Operaciones CRUD completas
/// - Filtrado, ordenación y paginación
/// - Validación de datos
/// - Gestión automática de IDs
/// </remarks>
/// <example>
/// Ejemplo de uso básico:
/// <code>
/// var repository = new TaskRepository();
/// repository.AddTask("Completar informe", TaskStatus.Pending, Priority.Alta);
/// var tasks = repository.ListTasks(statusFilter: TaskStatus.Pending);
/// </code>
/// </example>
public class TaskRepository
{
    /// <summary>
    /// Colección en memoria de las tareas gestionadas por el repositorio.
    /// </summary>
    private List<TaskItem> tasks;

    /// <summary>
    /// ID que se asignará a la próxima tarea creada.
    /// </summary>
    private int nextId;

    /// <summary>
    /// Ruta del archivo JSON donde se almacenan las tareas.
    /// </summary>
    private readonly string jsonFilePath = "tasks.json";

    /// <summary>
    /// Constructor que inicializa una nueva instancia del repositorio de tareas.
    /// </summary>
    /// <remarks>
    /// Al crear una instancia, se cargan las tareas existentes desde el archivo JSON
    /// y se calcula el próximo ID disponible.
    /// </remarks>
    public TaskRepository()
    {
        LoadFromJson();
        nextId = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;
    }

    /// <summary>
    /// Obtiene las opciones de serialización JSON para tareas.
    /// </summary>
    /// <returns>Opciones configuradas para la serialización de tareas</returns>
    /// <remarks>
    /// Configura la serialización para:
    /// - Formatear el JSON con indentación para mejor legibilidad
    /// - Convertir enumeraciones a strings para mejor legibilidad y compatibilidad
    /// </remarks>
    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <summary>
    /// Guarda la colección de tareas en el archivo JSON.
    /// </summary>
    /// <remarks>
    /// Este método serializa la lista completa de tareas y la escribe en el archivo,
    /// sobrescribiendo cualquier contenido anterior.
    /// </remarks>
    private void SaveToJson()
    {
        string jsonString = JsonSerializer.Serialize(tasks, GetJsonOptions());
        File.WriteAllText(jsonFilePath, jsonString);
        
    }

    /// <summary>
    /// Carga la colección de tareas desde el archivo JSON.
    /// </summary>
    /// <remarks>
    /// Si el archivo existe, deserializa su contenido en la lista de tareas.
    /// Si no existe, inicializa una lista vacía.
    /// </remarks>
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

    /// <summary>
    /// Añade una nueva tarea al repositorio.
    /// </summary>
    /// <param name="description">Descripción detallada de la tarea</param>
    /// <param name="status">Estado inicial de la tarea</param>
    /// <param name="priority">Prioridad de la tarea (por defecto: Media)</param>
    /// <param name="dueDate">Fecha límite opcional para completar la tarea</param>
    /// <param name="categories">Lista opcional de categorías para la tarea</param>
    /// <exception cref="ArgumentException">
    /// Si la descripción es null, vacía, o no cumple con los requisitos de longitud (10-100 caracteres),
    /// o si el estado o prioridad no son valores válidos de sus respectivas enumeraciones.
    /// </exception>
    /// <remarks>
    /// Este método:
    /// - Valida los datos de entrada
    /// - Crea una nueva instancia de TaskItem con un ID único
    /// - Añade la tarea a la colección
    /// - Persiste los cambios en el archivo JSON
    /// </remarks>
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

    /// <summary>
    /// Actualiza la fecha límite de una tarea existente.
    /// </summary>
    /// <param name="id">ID de la tarea a actualizar</param>
    /// <param name="dueDate">Nueva fecha límite (puede ser null para eliminar la fecha)</param>
    /// <exception cref="ArgumentException">Si no se encuentra la tarea con el ID especificado</exception>
    /// <remarks>
    /// Este método:
    /// - Busca la tarea por su ID
    /// - Actualiza su fecha límite
    /// - Actualiza la fecha de última modificación
    /// - Persiste los cambios en el archivo JSON
    /// </remarks>
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

    /// <summary>
    /// Actualiza la prioridad de una tarea existente.
    /// </summary>
    /// <param name="id">ID de la tarea a actualizar</param>
    /// <param name="priority">Nueva prioridad para la tarea</param>
    /// <exception cref="ArgumentException">
    /// Si la prioridad no es un valor válido de la enumeración Priority,
    /// o si no se encuentra la tarea con el ID especificado
    /// </exception>
    /// <remarks>
    /// Este método:
    /// - Valida que la prioridad sea un valor válido
    /// - Busca la tarea por su ID
    /// - Actualiza su prioridad
    /// - Actualiza la fecha de última modificación
    /// - Persiste los cambios en el archivo JSON
    /// </remarks>
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

    /// <summary>
    /// Actualiza el estado de una tarea existente.
    /// </summary>
    /// <param name="id">ID de la tarea a actualizar</param>
    /// <param name="status">Nuevo estado para la tarea</param>
    /// <exception cref="ArgumentException">
    /// Si el estado no es un valor válido de la enumeración TaskStatus,
    /// o si no se encuentra la tarea con el ID especificado
    /// </exception>
    /// <remarks>
    /// Este método:
    /// - Valida que el estado sea un valor válido
    /// - Busca la tarea por su ID
    /// - Actualiza su estado
    /// - Actualiza la fecha de última modificación
    /// - Persiste los cambios en el archivo JSON
    /// </remarks>
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

    /// <summary>
    /// Añade una categoría a una tarea existente.
    /// </summary>
    /// <param name="id">ID de la tarea</param>
    /// <param name="category">Categoría a añadir</param>
    /// <exception cref="ArgumentException">
    /// Si la categoría es null o vacía,
    /// o si no se encuentra la tarea con el ID especificado
    /// </exception>
    /// <remarks>
    /// Este método:
    /// - Valida que la categoría no esté vacía
    /// - Busca la tarea por su ID
    /// - Añade la categoría si no existe ya
    /// - Actualiza la fecha de última modificación
    /// - Persiste los cambios en el archivo JSON
    /// </remarks>
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

    /// <summary>
    /// Elimina una categoría de una tarea existente.
    /// </summary>
    /// <param name="id">ID de la tarea</param>
    /// <param name="category">Categoría a eliminar</param>
    /// <exception cref="ArgumentException">Si no se encuentra la tarea con el ID especificado</exception>
    /// <remarks>
    /// Este método:
    /// - Busca la tarea por su ID
    /// - Elimina la categoría si existe
    /// - Actualiza la fecha de última modificación
    /// - Persiste los cambios en el archivo JSON
    /// </remarks>
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

    /// <summary>
    /// Obtiene una lista filtrada, ordenada y paginada de tareas.
    /// </summary>
    /// <param name="statusFilter">Filtro opcional por estado</param>
    /// <param name="searchTerm">Término de búsqueda opcional para filtrar por descripción</param>
    /// <param name="sortBy">Campo por el que ordenar (id, description, status, priority, duedate, creationdate, lastmodified)</param>
    /// <param name="ascending">True para ordenar ascendentemente, False para descendentemente</param>
    /// <param name="categories">Lista opcional de categorías para filtrar</param>
    /// <param name="pageSize">Tamaño de página para paginación</param>
    /// <param name="pageNumber">Número de página (comenzando en 1)</param>
    /// <returns>Lista de tareas que cumplen con los criterios especificados</returns>
    /// <remarks>
    /// Este método:
    /// - Recarga las tareas desde el archivo JSON para asegurar datos actualizados
    /// - Aplica filtros por estado, término de búsqueda y categorías
    /// - Aplica ordenación según el campo y dirección especificados
    /// - Aplica paginación si se especifica un tamaño de página
    /// 
    /// Ejemplos de uso:
    /// - Listar todas las tareas: ListTasks()
    /// - Filtrar por estado: ListTasks(statusFilter: TaskStatus.Pending)
    /// - Buscar por término: ListTasks(searchTerm: "urgente")
    /// - Ordenar por fecha límite: ListTasks(sortBy: "duedate", ascending: false)
    /// - Paginar resultados: ListTasks(pageSize: 10, pageNumber: 2)
    /// </remarks>
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

    /// <summary>
    /// Busca una tarea por su ID.
    /// </summary>
    /// <param name="id">ID de la tarea a buscar</param>
    /// <returns>La tarea encontrada, o null si no existe</returns>
    /// <remarks>
    /// Este método recarga las tareas desde el archivo JSON para asegurar
    /// que se trabaja con los datos más actualizados.
    /// </remarks>
    public TaskItem FindTask(int id)
    {
        LoadFromJson(); // Refresh from file in case of external changes
        return tasks.FirstOrDefault(t => t.Id == id);
    }

    /// <summary>
    /// Elimina una tarea por su ID.
    /// </summary>
    /// <param name="id">ID de la tarea a eliminar</param>
    /// <param name="confirmDelete">Si es true, solicita confirmación al usuario antes de eliminar</param>
    /// <returns>True si la tarea fue eliminada, False en caso contrario</returns>
    /// <remarks>
    /// Este método:
    /// - Busca la tarea por su ID
    /// - Si confirmDelete es true, solicita confirmación al usuario
    /// - Elimina la tarea si se encuentra y se confirma la eliminación
    /// - Persiste los cambios en el archivo JSON
    /// </remarks>
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

    /// <summary>
    /// Solicita confirmación al usuario para una acción.
    /// </summary>
    /// <param name="message">Mensaje a mostrar al usuario</param>
    /// <returns>True si el usuario confirma, False en caso contrario</returns>
    /// <remarks>
    /// Este método muestra el mensaje en la consola y espera una respuesta del usuario.
    /// Considera como confirmación las respuestas "y" o "yes" (sin distinguir mayúsculas/minúsculas).
    /// </remarks>
    private bool ConfirmAction(string message)
    {
        Console.Write(message);
        var response = Console.ReadLine()?.Trim().ToLower() ?? "n";
        return response == "y" || response == "yes";
    }
}

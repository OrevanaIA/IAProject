using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIProject.Controllers
{
    /// <summary>
    /// Controlador que gestiona las operaciones CRUD para tareas.
    /// </summary>
    /// <remarks>
    /// Este controlador proporciona endpoints para:
    /// - Listar todas las tareas
    /// - Obtener una tarea específica
    /// - Crear nuevas tareas
    /// - Actualizar tareas existentes
    /// - Eliminar tareas
    /// 
    /// Todos los endpoints requieren autenticación mediante JWT.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para todos los endpoints
    public class TasksController : ControllerBase
    {
        /// <summary>
        /// Servicio de tareas utilizado por el controlador.
        /// </summary>
        private readonly ITaskService _taskService;

        /// <summary>
        /// Constructor que inicializa una nueva instancia del controlador de tareas.
        /// </summary>
        /// <param name="taskService">Servicio de tareas a utilizar</param>
        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Obtiene todas las tareas.
        /// </summary>
        /// <returns>
        /// 200 OK con la lista de todas las tareas.
        /// </returns>
        /// <remarks>
        /// Este endpoint devuelve todas las tareas disponibles en el sistema.
        /// Requiere autenticación mediante JWT.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        /// <summary>
        /// Obtiene una tarea específica por su ID.
        /// </summary>
        /// <param name="id">ID de la tarea a obtener</param>
        /// <returns>
        /// 200 OK con la tarea solicitada.
        /// 404 Not Found si la tarea no existe.
        /// </returns>
        /// <remarks>
        /// Este endpoint busca y devuelve una tarea específica por su identificador único.
        /// Requiere autenticación mediante JWT.
        /// </remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _taskService.GetTaskAsync(id);
            if (task == null)
                return NotFound();

            return Ok(task);
        }

        /// <summary>
        /// Crea una nueva tarea.
        /// </summary>
        /// <param name="taskDto">DTO con los datos de la nueva tarea</param>
        /// <returns>
        /// 201 Created con la tarea creada y la URL para acceder a ella.
        /// 400 Bad Request si los datos son inválidos.
        /// </returns>
        /// <remarks>
        /// Este endpoint crea una nueva tarea con los datos proporcionados.
        /// Requiere autenticación mediante JWT.
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskDTO taskDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTask = await _taskService.CreateTaskAsync(taskDto);
            return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        }

        /// <summary>
        /// Actualiza una tarea existente.
        /// </summary>
        /// <param name="id">ID de la tarea a actualizar</param>
        /// <param name="taskDto">DTO con los datos actualizados de la tarea</param>
        /// <returns>
        /// 204 No Content si la actualización es exitosa.
        /// 400 Bad Request si los datos son inválidos o hay un desajuste de ID.
        /// 404 Not Found si la tarea no existe.
        /// </returns>
        /// <remarks>
        /// Este endpoint actualiza una tarea existente con los datos proporcionados.
        /// El ID en la URL debe coincidir con el ID en el cuerpo de la solicitud.
        /// Requiere autenticación mediante JWT.
        /// </remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDTO taskDto)
        {
            if (id != taskDto.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _taskService.UpdateTaskAsync(taskDto);
            var updated = true;
            if (!updated)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Elimina una tarea existente.
        /// </summary>
        /// <param name="id">ID de la tarea a eliminar</param>
        /// <returns>
        /// 204 No Content si la eliminación es exitosa.
        /// 404 Not Found si la tarea no existe.
        /// </returns>
        /// <remarks>
        /// Este endpoint elimina permanentemente una tarea del sistema.
        /// Requiere autenticación mediante JWT y el rol de administrador.
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdminRole")] // Solo administradores pueden eliminar
        public async Task<IActionResult> DeleteTask(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            var deleted = true;
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}

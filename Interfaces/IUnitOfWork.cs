using System;
using System.Threading.Tasks;

namespace AIProject.Interfaces
{
    /// <summary>
    /// Interfaz que implementa el patrón Unit of Work para gestionar transacciones y
    /// mantener la consistencia en las operaciones de base de datos.
    /// </summary>
    /// <remarks>
    /// Esta interfaz proporciona:
    /// - Gestión de transacciones para operaciones atómicas
    /// - Acceso centralizado a los repositorios
    /// - Persistencia de cambios sincrónica y asincrónica
    /// - Implementación de IDisposable para liberar recursos
    /// </remarks>
    /// <example>
    /// Ejemplo de uso en una operación transaccional:
    /// <code>
    /// using (var unitOfWork = new UnitOfWork())
    /// {
    ///     try
    ///     {
    ///         unitOfWork.BeginTransaction();
    ///         var task = unitOfWork.TaskRepository.GetById(1);
    ///         task.Status = TaskStatus.Completed;
    ///         unitOfWork.TaskRepository.Update(task);
    ///         unitOfWork.SaveChanges();
    ///         unitOfWork.CommitTransaction();
    ///     }
    ///     catch (Exception)
    ///     {
    ///         unitOfWork.RollbackTransaction();
    ///         throw;
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Obtiene el repositorio de tareas.
        /// </summary>
        /// <remarks>
        /// Proporciona acceso al repositorio de tareas manteniendo el contexto
        /// de la unidad de trabajo actual.
        /// </remarks>
        ITaskRepository TaskRepository { get; }

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos de forma sincrónica.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si hay errores al guardar los cambios</exception>
        /// <remarks>
        /// Este método:
        /// - Persiste todos los cambios realizados a través de los repositorios
        /// - Mantiene la consistencia de los datos
        /// - Valida las entidades antes de guardarlas
        /// </remarks>
        void SaveChanges();

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos de forma asincrónica.
        /// </summary>
        /// <returns>Una tarea que representa la operación asincrónica</returns>
        /// <exception cref="InvalidOperationException">Si hay errores al guardar los cambios</exception>
        /// <remarks>
        /// Versión asincrónica de SaveChanges, útil para operaciones que pueden
        /// tomar tiempo sin bloquear el hilo principal.
        /// </remarks>
        Task SaveChangesAsync();

        /// <summary>
        /// Inicia una nueva transacción.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si ya hay una transacción activa o si hay problemas al iniciar la transacción
        /// </exception>
        /// <remarks>
        /// - Crea un nuevo ámbito transaccional
        /// - Debe ser emparejado con CommitTransaction o RollbackTransaction
        /// - Permite operaciones atómicas que involucran múltiples cambios
        /// </remarks>
        void BeginTransaction();

        /// <summary>
        /// Confirma la transacción actual.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si no hay una transacción activa o si hay problemas al confirmar
        /// </exception>
        /// <remarks>
        /// - Hace permanentes todos los cambios realizados durante la transacción
        /// - Solo debe llamarse si todos los cambios fueron exitosos
        /// - Libera el ámbito transaccional actual
        /// </remarks>
        void CommitTransaction();

        /// <summary>
        /// Revierte la transacción actual.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Si no hay una transacción activa o si hay problemas al revertir
        /// </exception>
        /// <remarks>
        /// - Descarta todos los cambios realizados durante la transacción
        /// - Debe llamarse en caso de error para mantener la consistencia
        /// - Libera el ámbito transaccional actual
        /// </remarks>
        void RollbackTransaction();
    }
}

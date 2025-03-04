using System;
using System.Threading.Tasks;
using System.Text.Json;
using AIProject.Interfaces;

namespace AIProject.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _filePath;
        private bool _isTransactionActive;
        private string _transactionBackup;
        private ITaskRepository _taskRepository;

        public UnitOfWork(string filePath = "tasks.json")
        {
            _filePath = filePath;
            _isTransactionActive = false;
        }

        public ITaskRepository TaskRepository
        {
            get
            {
                if (_taskRepository == null)
                {
                    _taskRepository = new TaskRepository(_filePath);
                }
                return _taskRepository;
            }
        }

        public void BeginTransaction()
        {
            if (_isTransactionActive)
            {
                throw new InvalidOperationException("A transaction is already active");
            }

            try
            {
                if (System.IO.File.Exists(_filePath))
                {
                    _transactionBackup = System.IO.File.ReadAllText(_filePath);
                }
                _isTransactionActive = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to begin transaction", ex);
            }
        }

        public void CommitTransaction()
        {
            if (!_isTransactionActive)
            {
                throw new InvalidOperationException("No active transaction to commit");
            }

            _isTransactionActive = false;
            _transactionBackup = null;
        }

        public void RollbackTransaction()
        {
            if (!_isTransactionActive)
            {
                throw new InvalidOperationException("No active transaction to rollback");
            }

            try
            {
                if (_transactionBackup != null)
                {
                    System.IO.File.WriteAllText(_filePath, _transactionBackup);
                }
                else if (System.IO.File.Exists(_filePath))
                {
                    System.IO.File.Delete(_filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to rollback transaction", ex);
            }
            finally
            {
                _isTransactionActive = false;
                _transactionBackup = null;
            }
        }

        public void SaveChanges()
        {
            if (_taskRepository != null)
            {
                ((TaskRepository)_taskRepository).SaveChanges();
            }
        }

        public async Task SaveChangesAsync()
        {
            if (_taskRepository != null)
            {
                await ((TaskRepository)_taskRepository).SaveChangesAsync();
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_isTransactionActive)
                    {
                        RollbackTransaction();
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

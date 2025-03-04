using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIProject.Models;

namespace Tests
{
    [TestClass]
    public class TaskRepositoryTests
    {
        private TaskRepository _repository;
        private string _testFilePath;

        [TestInitialize]
        public void Initialize()
        {
            // Create a new instance of TaskRepository
            _repository = new TaskRepository();
            _testFilePath = "test_tasks.json";
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Delete the temporary file after tests
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [TestMethod]
        public void AddTask_ValidTask_AddsToRepository()
        {
            // Arrange
            string description = "Test task description";
            AIProject.Models.TaskStatus status = AIProject.Models.TaskStatus.Pending;
            Priority priority = Priority.Alta;

            // Act
            _repository.AddTask(description, status, priority);
            var tasks = _repository.ListTasks();

            // Assert
            Assert.AreEqual(1, tasks.Count);
            Assert.AreEqual(description, tasks[0].Description);
            Assert.AreEqual(status, tasks[0].Status);
            Assert.AreEqual(priority, tasks[0].Priority);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddTask_ShortDescription_ThrowsException()
        {
            // Arrange
            string description = "Short"; // Less than 10 characters

            // Act
            _repository.AddTask(description, AIProject.Models.TaskStatus.Pending);
        }

        [TestMethod]
        public void UpdateTaskStatus_ExistingTask_UpdatesStatus()
        {
            // Arrange
            _repository.AddTask("Test task description", AIProject.Models.TaskStatus.Pending);
            var tasks = _repository.ListTasks();
            int taskId = tasks[0].Id;

            // Act
            _repository.UpdateTaskStatus(taskId, AIProject.Models.TaskStatus.InProgress);
            tasks = _repository.ListTasks();

            // Assert
            Assert.AreEqual(AIProject.Models.TaskStatus.InProgress, tasks[0].Status);
        }

        [TestMethod]
        public void UpdateTaskPriority_ExistingTask_UpdatesPriority()
        {
            // Arrange
            _repository.AddTask("Test task description", AIProject.Models.TaskStatus.Pending, Priority.Media);
            var tasks = _repository.ListTasks();
            int taskId = tasks[0].Id;

            // Act
            _repository.UpdateTaskPriority(taskId, Priority.Alta);
            tasks = _repository.ListTasks();

            // Assert
            Assert.AreEqual(Priority.Alta, tasks[0].Priority);
        }

        [TestMethod]
        public void AddTaskCategory_ExistingTask_AddsCategory()
        {
            // Arrange
            _repository.AddTask("Test task description", AIProject.Models.TaskStatus.Pending);
            var tasks = _repository.ListTasks();
            int taskId = tasks[0].Id;
            string category = "Important";

            // Act
            _repository.AddTaskCategory(taskId, category);
            tasks = _repository.ListTasks();

            // Assert
            Assert.IsTrue(tasks[0].Categories.Contains(category));
        }

        [TestMethod]
        public void ListTasks_WithStatusFilter_ReturnsFilteredTasks()
        {
            // Arrange
            _repository.AddTask("Pending task", AIProject.Models.TaskStatus.Pending);
            _repository.AddTask("In progress task", AIProject.Models.TaskStatus.InProgress);
            _repository.AddTask("Completed task", AIProject.Models.TaskStatus.Completed);

            // Act
            var pendingTasks = _repository.ListTasks(statusFilter: AIProject.Models.TaskStatus.Pending);
            var inProgressTasks = _repository.ListTasks(statusFilter: AIProject.Models.TaskStatus.InProgress);
            var completedTasks = _repository.ListTasks(statusFilter: AIProject.Models.TaskStatus.Completed);

            // Assert
            Assert.AreEqual(1, pendingTasks.Count);
            Assert.AreEqual(1, inProgressTasks.Count);
            Assert.AreEqual(1, completedTasks.Count);
            Assert.AreEqual("Pending task", pendingTasks[0].Description);
            Assert.AreEqual("In progress task", inProgressTasks[0].Description);
            Assert.AreEqual("Completed task", completedTasks[0].Description);
        }

        [TestMethod]
        public void ListTasks_WithSearchTerm_ReturnsMatchingTasks()
        {
            // Arrange
            _repository.AddTask("First task", AIProject.Models.TaskStatus.Pending);
            _repository.AddTask("Second task", AIProject.Models.TaskStatus.Pending);
            _repository.AddTask("Another task", AIProject.Models.TaskStatus.Pending);

            // Act
            var firstTasks = _repository.ListTasks(searchTerm: "First");
            var taskTasks = _repository.ListTasks(searchTerm: "task");

            // Assert
            Assert.AreEqual(1, firstTasks.Count);
            Assert.AreEqual(3, taskTasks.Count);
            Assert.AreEqual("First task", firstTasks[0].Description);
        }

        [TestMethod]
        public void DeleteTask_ExistingTask_RemovesTask()
        {
            // Arrange
            _repository.AddTask("Task to delete", AIProject.Models.TaskStatus.Pending);
            var tasks = _repository.ListTasks();
            int taskId = tasks[0].Id;

            // Act
            bool result = _repository.DeleteTask(taskId, false);
            tasks = _repository.ListTasks();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, tasks.Count);
        }

        [TestMethod]
        public void FindTask_ExistingId_ReturnsTask()
        {
            // Arrange
            _repository.AddTask("Task to find", AIProject.Models.TaskStatus.Pending);
            var tasks = _repository.ListTasks();
            int taskId = tasks[0].Id;

            // Act
            var foundTask = _repository.FindTask(taskId);

            // Assert
            Assert.IsNotNull(foundTask);
            Assert.AreEqual("Task to find", foundTask.Description);
        }

        [TestMethod]
        public void FindTask_NonExistingId_ReturnsNull()
        {
            // Act
            var foundTask = _repository.FindTask(999);

            // Assert
            Assert.IsNull(foundTask);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel.DataAnnotations;
using AIProject.Models;

namespace Sprint02Tasks.Tests
{
    [TestClass]
    public class TaskItemTests
    {
        [TestMethod]
        public void Constructor_ValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            int id = 1;
            string description = "Valid task description";
            AIProject.Models.TaskStatus status = AIProject.Models.TaskStatus.Pending;

            // Act
            var task = new TaskItem(id, description, status);

            // Assert
            Assert.AreEqual(id, task.Id);
            Assert.AreEqual(description, task.Description);
            Assert.AreEqual(status, task.Status);
            Assert.AreEqual(Priority.Media, task.Priority); // Default priority
            Assert.IsNotNull(task.Categories);
            Assert.AreEqual(0, task.Categories.Count);
            Assert.AreEqual(DateTime.Now.Date, task.CreationDate.Date);
            Assert.AreEqual(DateTime.Now.Date, task.LastModifiedDate.Date);
            Assert.IsNull(task.DueDate);
        }

        [TestMethod]
        public void Description_ValidationAttributes_AreCorrect()
        {
            // Arrange
            var propertyInfo = typeof(TaskItem).GetProperty("Description");
            var requiredAttribute = (RequiredAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(RequiredAttribute));
            var stringLengthAttribute = (StringLengthAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(StringLengthAttribute));

            // Assert
            Assert.IsNotNull(requiredAttribute, "Required attribute is missing");
            Assert.IsNotNull(stringLengthAttribute, "StringLength attribute is missing");
            Assert.AreEqual(10, stringLengthAttribute.MinimumLength, "Minimum length should be 10");
            Assert.AreEqual(100, stringLengthAttribute.MaximumLength, "Maximum length should be 100");
        }

        [TestMethod]
        public void ToString_WithAllProperties_ReturnsFormattedString()
        {
            // Arrange
            var task = new TaskItem(1, "Test description", AIProject.Models.TaskStatus.Pending)
            {
                Priority = Priority.Alta,
                DueDate = new DateTime(2024, 12, 31)
            };
            task.Categories.Add("Test Category");

            // Act
            string result = task.ToString();

            // Assert
            StringAssert.Contains(result, "ID: 1");
            StringAssert.Contains(result, "Test description");
            StringAssert.Contains(result, "Status: Pending");
            StringAssert.Contains(result, "Priority: Alta");
            StringAssert.Contains(result, "Due: 12/31/2024");
            StringAssert.Contains(result, "Categories: Test Category");
        }

        [TestMethod]
        public void ToString_WithoutOptionalProperties_ReturnsFormattedString()
        {
            // Arrange
            var task = new TaskItem(1, "Test description", AIProject.Models.TaskStatus.Pending);

            // Act
            string result = task.ToString();

            // Assert
            StringAssert.Contains(result, "ID: 1");
            StringAssert.Contains(result, "Test description");
            StringAssert.Contains(result, "Status: Pending");
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex("Due:"));
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex("Categories:"));
        }

        [TestMethod]
        public void Categories_WhenInitialized_IsEmptyList()
        {
            // Arrange & Act
            var task = new TaskItem(1, "Test description", AIProject.Models.TaskStatus.Pending);

            // Assert
            Assert.IsNotNull(task.Categories);
            Assert.AreEqual(0, task.Categories.Count);
        }

        [TestMethod]
        public void LastModifiedDate_WhenCreated_EqualsCreationDate()
        {
            // Arrange & Act
            var task = new TaskItem(1, "Test description", AIProject.Models.TaskStatus.Pending);

            // Assert
            Assert.AreEqual(task.CreationDate, task.LastModifiedDate);
        }
    }
}

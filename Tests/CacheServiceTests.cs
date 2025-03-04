using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using AIProject.DTOs;
using AIProject.Services;
using AIProject.Models;

namespace AIProject.Tests
{
    [TestClass]
    public class CacheServiceTests
    {
        private InMemoryCacheService _cacheService;

        [TestInitialize]
        public void Initialize()
        {
            _cacheService = new InMemoryCacheService();
        }

        #region Caché

        [TestMethod]
        public async Task SetAsync_ValidItem_StoresInCache()
        {
            // Arrange
            string key = "test_key";
            string value = "test_value";
            TimeSpan expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task SetAsync_ComplexObject_StoresInCache()
        {
            // Arrange
            string key = "task_key";
            var taskDto = new TaskDTO
            {
                Id = 1,
                Description = "Tarea en caché",
                Status = AIProject.Models.TaskStatus.Pending,
                Priority = Priority.Alta
            };
            TimeSpan expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, taskDto, expiration);
            var result = await _cacheService.GetAsync<TaskDTO>(key);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(taskDto.Id, result.Id);
            Assert.AreEqual(taskDto.Description, result.Description);
            Assert.AreEqual(taskDto.Status, result.Status);
            Assert.AreEqual(taskDto.Priority, result.Priority);
        }

        [TestMethod]
        public async Task GetAsync_NonExistingKey_ReturnsNull()
        {
            // Act
            var result = await _cacheService.GetAsync<string>("non_existing_key");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAsync_ExpiredItem_ReturnsNull()
        {
            // Arrange
            string key = "expired_key";
            string value = "expired_value";
            TimeSpan expiration = TimeSpan.FromMilliseconds(1); // Very short expiration

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            await Task.Delay(10); // Wait for expiration
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RemoveAsync_ExistingKey_RemovesFromCache()
        {
            // Arrange
            string key = "remove_key";
            string value = "remove_value";
            TimeSpan expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            await _cacheService.RemoveAsync(key);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RemoveAsync_NonExistingKey_DoesNotThrowException()
        {
            // Act & Assert
            // No exception should be thrown
            await _cacheService.RemoveAsync("non_existing_key");
        }

        [TestMethod]
        public async Task GetAsync_WrongType_ReturnsNull()
        {
            // Arrange
            string key = "type_key";
            string value = "string_value";
            TimeSpan expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<object>(key); // Try to get as int

            // Assert
            Assert.AreEqual(default(int), result);
        }

        [TestMethod]
        public async Task SetAsync_NullKey_DoesNotThrowException()
        {
            // Arrange
            string value = "null_key_value";
            TimeSpan expiration = TimeSpan.FromMinutes(5);

            // Act & Assert
            // No exception should be thrown
            await _cacheService.SetAsync(null, value, expiration);
        }

        [TestMethod]
        public async Task SetAsync_NullValue_StoresNull()
        {
            // Arrange
            string key = "null_value_key";
            TimeSpan expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync<string>(key, null, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SetAsync_ZeroExpiration_DoesNotStore()
        {
            // Arrange
            string key = "zero_expiration_key";
            string value = "zero_expiration_value";
            TimeSpan expiration = TimeSpan.Zero;

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SetAsync_NegativeExpiration_DoesNotStore()
        {
            // Arrange
            string key = "negative_expiration_key";
            string value = "negative_expiration_value";
            TimeSpan expiration = TimeSpan.FromMinutes(-5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SetAsync_SameKeyTwice_UpdatesValue()
        {
            // Arrange
            string key = "update_key";
            string value1 = "original_value";
            string value2 = "updated_value";
            TimeSpan expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value1, expiration);
            await _cacheService.SetAsync(key, value2, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(value2, result);
        }

        #endregion
    }
}

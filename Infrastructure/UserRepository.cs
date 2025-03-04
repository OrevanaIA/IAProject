using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIProject.Interfaces;
using AIProject.Models;

namespace AIProject.Infrastructure
{
    public class UserRepository : IUserRepository
    {
        private readonly string _filePath;
        private List<User> _users;
        private readonly object _lock = new object();

        public UserRepository(string filePath = "users.json")
        {
            _filePath = filePath;
            LoadUsers();
        }

        private void LoadUsers()
        {
            lock (_lock)
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
                else
                {
                    _users = new List<User>();
                    SaveChanges(); // Create the file
                }
            }
        }

        private void SaveChanges()
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await Task.FromResult(_users.Any(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await Task.FromResult(_users.Any(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<int> AddAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            
            // Generate ID if not set
            if (user.Id <= 0)
            {
                user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            }
            
            _users.Add(user);
            SaveChanges();
            
            return await Task.FromResult(user.Id);
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var existingIndex = _users.FindIndex(u => u.Id == user.Id);
            if (existingIndex == -1)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found");
            }

            _users[existingIndex] = user;
            SaveChanges();
            
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _users.Remove(user);
                SaveChanges();
            }
            
            await Task.CompletedTask;
        }
    }
}

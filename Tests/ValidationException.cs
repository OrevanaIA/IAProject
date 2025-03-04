using System;

namespace AIProject.Tests
{
    // Helper class for validation exceptions
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}

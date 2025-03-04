using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Security.Claims;

namespace AIProject.Security
{
    /// <summary>
    /// Clase que proporciona métodos de sanitización para datos de entrada y salida
    /// para prevenir ataques de inyección y XSS.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa:
    /// - Sanitización de entrada de texto
    /// - Validación de datos sensibles
    /// - Prevención de XSS
    /// - Limpieza de datos para almacenamiento seguro
    /// </remarks>
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitiza texto general para prevenir XSS y ataques de inyección.
        /// </summary>
        /// <param name="input">Texto a sanitizar</param>
        /// <returns>Texto sanitizado</returns>
        /// <remarks>
        /// Realiza las siguientes operaciones:
        /// - Elimina HTML y scripts
        /// - Escapa caracteres especiales
        /// - Normaliza espacios en blanco
        /// </remarks>
        public static string SanitizeText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Eliminar HTML
            input = HttpUtility.HtmlEncode(input);
            
            // Normalizar espacios en blanco
            input = Regex.Replace(input, @"\s+", " ");
            
            // Eliminar caracteres no imprimibles
            input = Regex.Replace(input, @"[^\u0020-\u007E]", string.Empty);
            
            return input.Trim();
        }

        /// <summary>
        /// Sanitiza una descripción de tarea.
        /// </summary>
        /// <param name="description">Descripción a sanitizar</param>
        /// <returns>Descripción sanitizada</returns>
        /// <remarks>
        /// Aplica reglas específicas para descripciones:
        /// - Longitud máxima
        /// - Caracteres permitidos
        /// - Formato específico
        /// </remarks>
        public static string SanitizeTaskDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return string.Empty;

            // Sanitización básica
            description = SanitizeText(description);
            
            // Limitar longitud
            if (description.Length > 500)
                description = description.Substring(0, 500);
            
            return description;
        }

        /// <summary>
        /// Sanitiza una categoría de tarea.
        /// </summary>
        /// <param name="category">Categoría a sanitizar</param>
        /// <returns>Categoría sanitizada</returns>
        /// <remarks>
        /// Aplica reglas específicas para categorías:
        /// - Solo letras, números y guiones
        /// - Sin espacios al inicio/final
        /// - Longitud máxima
        /// </remarks>
        public static string SanitizeCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return string.Empty;

            // Sanitización básica
            category = SanitizeText(category);
            
            // Solo permitir letras, números y guiones
            category = Regex.Replace(category, @"[^a-zA-Z0-9\-\s]", string.Empty);
            
            // Limitar longitud
            if (category.Length > 50)
                category = category.Substring(0, 50);
            
            return category;
        }

        /// <summary>
        /// Valida y sanitiza un ID.
        /// </summary>
        /// <param name="id">ID a validar</param>
        /// <returns>True si el ID es válido, False en caso contrario</returns>
        /// <remarks>
        /// Verifica que el ID:
        /// - Sea un número positivo
        /// - No exceda límites razonables
        /// - No contenga caracteres especiales
        /// </remarks>
        public static bool ValidateId(int id)
        {
            return id > 0 && id < int.MaxValue;
        }

        /// <summary>
        /// Sanitiza datos para mostrar en la interfaz de usuario.
        /// </summary>
        /// <param name="input">Datos a sanitizar</param>
        /// <returns>Datos seguros para mostrar</returns>
        /// <remarks>
        /// Prepara datos para presentación:
        /// - Escapa HTML
        /// - Previene XSS
        /// - Formatea correctamente
        /// </remarks>
        public static string SanitizeForDisplay(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Escapar HTML y convertir saltos de línea
            input = HttpUtility.HtmlEncode(input)
                .Replace(Environment.NewLine, "<br />")
                .Replace("\n", "<br />");
            
            return input;
        }
    }
}

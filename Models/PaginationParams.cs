using System;

namespace AIProject.Models
{
    /// <summary>
    /// Clase que encapsula los parámetros de paginación para optimizar
    /// las consultas de grandes conjuntos de datos.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa:
    /// - Validación de parámetros de paginación
    /// - Valores por defecto seguros
    /// - Límites máximos para prevenir sobrecarga
    /// </remarks>
    public class PaginationParams
    {
        private int _pageSize;
        private int _pageNumber;
        
        /// <summary>
        /// Número máximo de elementos permitidos por página.
        /// </summary>
        /// <remarks>
        /// Limita el tamaño de la página para prevenir consultas que puedan
        /// afectar el rendimiento del sistema.
        /// </remarks>
        public const int MaxPageSize = 50;

        /// <summary>
        /// Tamaño de página por defecto si no se especifica otro valor.
        /// </summary>
        public const int DefaultPageSize = 10;

        /// <summary>
        /// Obtiene o establece el número de elementos por página.
        /// </summary>
        /// <remarks>
        /// Si se establece un valor mayor que MaxPageSize, se utilizará MaxPageSize.
        /// Si se establece un valor menor que 1, se utilizará DefaultPageSize.
        /// </remarks>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? DefaultPageSize : value;
        }

        /// <summary>
        /// Obtiene o establece el número de página actual.
        /// </summary>
        /// <remarks>
        /// Si se establece un valor menor que 1, se utilizará 1.
        /// </remarks>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Constructor que inicializa una nueva instancia con valores por defecto.
        /// </summary>
        public PaginationParams()
        {
            PageNumber = 1;
            PageSize = DefaultPageSize;
        }

        /// <summary>
        /// Constructor que inicializa una nueva instancia con valores específicos.
        /// </summary>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <exception cref="ArgumentException">Si pageNumber es menor que 1</exception>
        public PaginationParams(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        /// <summary>
        /// Calcula el número de elementos a saltar para la página actual.
        /// </summary>
        /// <returns>Número de elementos a saltar</returns>
        public int GetSkip()
        {
            return (PageNumber - 1) * PageSize;
        }
    }
}

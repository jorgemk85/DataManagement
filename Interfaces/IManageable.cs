﻿using DataManagement.Standard.Attributes;
using System;

namespace DataManagement.Standard.Interfaces
{
    public interface IManageable<TKey> where TKey : struct
    {
        #region Properties
        TKey? Id { get; set; }
        #endregion

        #region Unmanaged Properties
        [UnmanagedProperty]
        DateTime? FechaCreacion { get; set; }
        [UnmanagedProperty]
        DateTime? FechaModificacion { get; set; }
        #endregion

        #region Unlinked Properties
        /// <summary>
        /// Almacena el nombre de la tabla en la base de datos.
        /// </summary>
        [UnlinkedProperty]
        string DataBaseTableName { get; }
        /// <summary>
        /// Almacena el nombre del schema de la tabla en la base de datos.
        /// </summary>
        [UnlinkedProperty]
        string Schema { get; }
        /// <summary>
        /// Especifica si se desea utilizar las funciones de cache en la clase actual.
        /// </summary>
        [UnlinkedProperty]
        bool IsCacheEnabled { get; }
        /// <summary>
        /// Si se estan utilizando las funciones de cache, se puede especificar la expiracion o vigencia del mismo en segundos.
        /// </summary>
        [UnlinkedProperty]
        int CacheExpiration { get; }

        /// <summary>
        /// Se utiliza para facilitar el acceso al tipo de la llave primaria del objeto.
        /// </summary>
        [UnlinkedProperty]
        Type KeyType { get; } 
        #endregion
    }
}

﻿namespace DataManagement.Standard.Events
{
    /// <summary>
    /// Provocado despues de ejecutar un comando de tipo INSERT en la base de datos.
    /// </summary>
    public delegate void InsertExecutedEventHandler(InsertExecutedEventArgs e);

    public class InsertExecutedEventArgs : ExecutedEventArgs
    {
        public InsertExecutedEventArgs(string tableName, Models.Result result) : base(tableName, result) { }
    }
}

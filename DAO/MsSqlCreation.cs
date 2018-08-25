﻿using DataManagement.Standard.Attributes;
using DataManagement.Standard.Enums;
using DataManagement.Standard.Interfaces;
using DataManagement.Standard.Models;
using DataManagement.Standard.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataManagement.Standard.DAO
{
    internal class MsSqlCreation : ICreatable
    {
        public void SetStoredProceduresParameters<T, TKey>(PropertiesData<T> properties, T obj, StringBuilder queryBuilder, bool setDefaultNull, bool considerPrimary) where T : IManageable<TKey>, new() where TKey : struct
        {
            // Aqui se colocan los parametros segun las propiedades del objeto 
            foreach (KeyValuePair<string, PropertyInfo> property in properties.ManagedProperties)
            {
                // Si la propiedad actual es la primaria y esta es del tipo int? y no se debe considerar para estos parametros, entonces se salta a la sig propiedad.
                // Esto significa que la propiedad primaria es Identity o Auto Increment y no se debe de mandar como parametro en un Insert.
                if (property.Value.Equals(properties.PrimaryProperty) && property.Value.PropertyType.Equals(typeof(int?)) && !considerPrimary)
                {
                    continue;
                }
                // Si la propiedad es DateCreated o DateModified o AutoProperty, no se debe mandar como parametro
                // Esto es por que estos valores se alimentan desde el procedimiento almacenado.
                if (properties.AutoProperties.ContainsKey(property.Value.Name))
                {
                    continue;
                }
                if (setDefaultNull)
                {
                    queryBuilder.AppendFormat("    @_{0} {1} = null,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                }
                else
                {
                    queryBuilder.AppendFormat("    @_{0} {1},\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                }
            }
        }

        public string CreateInsertStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            StringBuilder insertsBuilder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();
            PropertiesData<T> properties = new PropertiesData<T>();
            T obj = new T();

            if (properties.ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.InsertSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.InsertSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T, TKey>(properties, obj, queryBuilder, false, false);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.Append("DECLARE @actualTime datetime;\n");
            queryBuilder.Append("SET @actualTime = getdate();\n");
            queryBuilder.AppendFormat("INSERT INTO {0}.{1}{2} (\n", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);

            // Seccion para especificar a que columnas se va a insertar y sus valores.
            foreach (KeyValuePair<string, PropertyInfo> property in properties.ManagedProperties)
            {
                if (property.Value.Equals(properties.PrimaryProperty) && property.Value.PropertyType.Equals(typeof(int?)))
                {
                    continue;
                }
                else
                {
                    insertsBuilder.AppendFormat("    {0},\n", property.Value.Name);
                    if (properties.AutoProperties.TryGetValue(property.Value.Name, out PropertyInfo autoProperty))
                    {
                        valuesBuilder.AppendFormat("    {0},\n", GetAutoPropertyValue(properties.AutoPropertyTypes[property.Value.Name]));
                    }
                    else
                    {
                        valuesBuilder.AppendFormat("    @_{0},\n", property.Value.Name);
                    }
                }
            }
            insertsBuilder.Remove(insertsBuilder.Length - 2, 2);
            queryBuilder.Append(insertsBuilder);
            queryBuilder.Append(")\nVALUES (\n");
            valuesBuilder.Remove(valuesBuilder.Length - 2, 2);
            queryBuilder.Append(valuesBuilder);
            queryBuilder.Append(");\nEND");

            Logger.Info("Created a new query for Insert Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateUpdateStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertiesData<T> properties = new PropertiesData<T>();
            T obj = new T();

            if (properties.ManagedProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.UpdateSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.UpdateSuffix);
            }


            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T, TKey>(properties, obj, queryBuilder, false, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.Append("DECLARE @actualTime datetime;\n");
            queryBuilder.Append("SET @actualTime = getdate();\n");
            queryBuilder.AppendFormat("UPDATE {0}.{1}{2}\n", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("SET\n");

            // Se especifica el parametro que va en x columna.
            foreach (KeyValuePair<string, PropertyInfo> property in properties.ManagedProperties)
            {
                if (property.Equals(properties.PrimaryProperty) || property.Value.Name.Equals(properties.DateCreatedProperty.Name))
                {
                    continue;
                }
                if (properties.AutoProperties.TryGetValue(property.Value.Name, out PropertyInfo autoProperty))
                {
                    queryBuilder.AppendFormat("    {0} = {1},\n", property.Value.Name, GetAutoPropertyValue(properties.AutoPropertyTypes[property.Value.Name]));
                }
                else
                {
                    queryBuilder.AppendFormat("    {0} = ISNULL(@_{0}, {0}),\n", property.Value.Name);
                }
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.AppendFormat("WHERE Id = @_Id;\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Update Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateDeleteStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.DeleteSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.DeleteSuffix);
            }

            queryBuilder.Append(string.Format("@_Id {0}\n", GetSqlDataType(typeof(TKey))));
            queryBuilder.Append("AS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("DELETE FROM {0}.{1}{2}\n", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);
            queryBuilder.AppendFormat("WHERE Id = @_Id;\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Delete Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateSelectAllStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            T obj = new T();

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.SelectAllSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.SelectAllSuffix);
            }

            queryBuilder.Append("AS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2}\n", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("ORDER BY FechaCreacion DESC\n");
            queryBuilder.Append("END");

            Logger.Info("Created a new query for SelectAll Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateSelectStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertiesData<T> properties = new PropertiesData<T>();
            T obj = new T();

            if (properties.FilteredProperties.Count == 0) return string.Empty;

            if (doAlter)
            {
                queryBuilder.AppendFormat("ALTER PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.SelectSuffix);
            }
            else
            {
                queryBuilder.AppendFormat("CREATE PROCEDURE {0}.{1}{2}{3}\n", obj.Schema, Manager.StoredProcedurePrefix, obj.DataBaseTableName, Manager.SelectSuffix);
            }

            // Aqui se colocan los parametros segun las propiedades del objeto
            SetStoredProceduresParameters<T, TKey>(properties, obj, queryBuilder, true, true);

            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append("\nAS\n");
            queryBuilder.Append("BEGIN\n");
            queryBuilder.AppendFormat("SELECT * FROM {0}.{1}{2}\n", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);
            queryBuilder.Append("WHERE\n");

            // Se especifica el parametro que va en x columna.
            foreach (KeyValuePair<string, PropertyInfo> property in properties.FilteredProperties)
            {
                queryBuilder.AppendFormat("    {0} LIKE ISNULL(CONCAT('%', @_{0}, '%'), {0}) AND\n", property.Value.Name);
            }

            queryBuilder.Remove(queryBuilder.Length - 4, 4);
            queryBuilder.AppendFormat("\nORDER BY {0} desc;\n", properties.DateCreatedProperty.Name);
            queryBuilder.Append("END");

            Logger.Info("Created a new query for Select Stored Procedure:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableCreation<T, TKey>() where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertiesData<T> properties = new PropertiesData<T>();
            T obj = new T();

            if (properties.ManagedProperties.Count == 0) return string.Empty;

            queryBuilder.AppendFormat("CREATE TABLE {0}.{1}{2}\n", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);

            queryBuilder.Append("(");
            // Aqui se colocan las propiedades del objeto. Una por columna por su puesto.
            foreach (KeyValuePair<string, PropertyInfo> property in properties.ManagedProperties)
            {
                string isNullable = Nullable.GetUnderlyingType(property.Value.PropertyType) == null || property.Equals(properties.PrimaryProperty) ? "NOT NULL" : string.Empty;
                if (property.Equals(properties.PrimaryProperty))
                {
                    if (property.Value.PropertyType.Equals(typeof(int?)))
                    {
                        queryBuilder.AppendFormat("{0} {1} IDENTITY(1,1) NOT NULL PRIMARY KEY,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                    }
                    else
                    {
                        queryBuilder.AppendFormat("{0} {1} NOT NULL PRIMARY KEY,\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType));
                    }
                }
                else
                {
                    queryBuilder.AppendFormat("{0} {1} {2},\n", property.Value.Name, GetSqlDataType(property.Value.PropertyType), isNullable);
                }
            }
            queryBuilder.Remove(queryBuilder.Length - 2, 2);
            queryBuilder.Append(");");

            Logger.Info("Created a new query for Create Table:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string CreateQueryForTableAlteration<T, TKey>(Dictionary<string, ColumnDefinition> columnDetails, Dictionary<string, KeyDefinition> keyDetails) where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            List<string> columnsFound = new List<string>();
            bool foundDiference = false;
            PropertiesData<T> properties = new PropertiesData<T>();
            T obj = new T();

            if (properties.ManagedProperties.Count == 0) return string.Empty;

            string fullyQualifiedTableName = string.Format("{0}.{1}{2}", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);

            foreach (KeyValuePair<string, PropertyInfo> property in properties.ManagedProperties)
            {
                columnDetails.TryGetValue(property.Value.Name, out ColumnDefinition columnDefinition);
                string sqlDataType = GetSqlDataType(property.Value.PropertyType);
                bool isNullable = Nullable.GetUnderlyingType(property.Value.PropertyType) == null ? false : true;
                string nullWithDefault = isNullable == true ? string.Empty : string.Format("NOT NULL DEFAULT {0}", GetDefault(property.Value.PropertyType));

                if (columnDefinition == null)
                {
                    // Agregar propiedad a tabla ya que no existe.
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ADD {0} {1} {2};\n", property.Value.Name, sqlDataType, nullWithDefault);
                    foundDiference = true;
                    continue;
                }
                columnDefinition.Column_Type = columnDefinition.Character_Maximum_Length != null ? string.Format("{0}({1})", columnDefinition.Data_Type, columnDefinition.Character_Maximum_Length) : columnDefinition.Data_Type;
                if (!sqlDataType.Equals(columnDefinition.Column_Type))
                {
                    // Si el data type cambio, entonces lo modifica.
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ALTER COLUMN {0} {1};\n", property.Value.Name, sqlDataType);
                    foundDiference = true;
                }
                if (columnDefinition.Is_Nullable.Equals("YES") && !isNullable && !property.Equals(properties.PrimaryProperty))
                {
                    // Si la propiedad ya no es nullable, entonces la cambia en la base de datos
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ALTER COLUMN {0} {1} NOT NULL;\n", property.Value.Name, sqlDataType);
                    foundDiference = true;
                }
                if (columnDefinition.Is_Nullable.Equals("NO") && isNullable && !property.Equals(properties.PrimaryProperty))
                {
                    // Si la propiedad ES nullable, entonces la cambia en la base de datos
                    queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                    queryBuilder.AppendFormat("ALTER COLUMN {0} {1};\n", property.Value.Name, sqlDataType);
                    foundDiference = true;
                }
                if (keyDetails.TryGetValue(property.Value.Name, out KeyDefinition keyDefinition))
                {
                    // Si existe una llave en la base de datos relacionada a esta propiedad entonces...
                    ForeignModel foreignAttribute = property.Value.GetCustomAttribute<ForeignModel>();
                    if (foreignAttribute == null)
                    {
                        // En el caso de que no tenga ya el atributo, significa que dejo de ser una propiedad relacionada con algun modelo foraneo y por ende, debemos de eliminar la llave foranea
                        queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                        queryBuilder.AppendFormat("DROP FOREIGN KEY {0};\n", keyDefinition.Constraint_Name);
                        keyDetails.Remove(property.Value.Name);
                        foundDiference = true;
                    }
                }
                columnsFound.Add(property.Value.Name);
            }

            // Extraemos las columnas en la tabla que ya no estan en las propiedades del modelo para quitarlas.
            foreach (KeyValuePair<string, ColumnDefinition> detail in columnDetails.Where(q => !columnsFound.Contains(q.Key)))
            {
                queryBuilder.AppendFormat("ALTER TABLE {0} \n", fullyQualifiedTableName);
                queryBuilder.AppendFormat("DROP COLUMN {0};\n", detail.Value.Column_Name);
                foundDiference = true;
                continue;
            }

            if (!foundDiference)
            {
                queryBuilder.Clear();
            }
            else
            {
                Logger.Info("Created a new query for Alter Table:");
                Logger.Info(queryBuilder.ToString());
            }

            queryBuilder.Append(GetCreateForeignKeysQuery<T, TKey>(keyDetails));

            return queryBuilder.ToString();
        }

        public object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                object value = Activator.CreateInstance(type);
                if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    value = "''";
                }
                return value;
            }
            return null;
        }

        public string GetCreateForeignKeysQuery<T, TKey>(Dictionary<string, KeyDefinition> keyDetails = null) where T : IManageable<TKey>, new() where TKey : struct
        {
            StringBuilder queryBuilder = new StringBuilder();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(q => q.GetCustomAttribute<UnmanagedProperty>() == null && q.GetCustomAttribute<ForeignModel>() != null && !keyDetails.ContainsKey(q.Name)).ToArray();
            T obj = new T();

            if (properties.Length == 0) return string.Empty;

            queryBuilder.AppendFormat("ALTER TABLE {0}.{1}{2}\n", obj.Schema, Manager.TablePrefix, obj.DataBaseTableName);

            foreach (PropertyInfo property in properties)
            {
                ForeignModel foreignAttribute = property.GetCustomAttribute<ForeignModel>();
                IManageable<TKey> foreignModel = (IManageable<TKey>)Activator.CreateInstance(foreignAttribute.Model);
                queryBuilder.AppendFormat("ADD CONSTRAINT FK_{0}_{1}\n", obj.DataBaseTableName, foreignModel.DataBaseTableName);
                queryBuilder.AppendFormat("FOREIGN KEY({0}) REFERENCES {1}.{2}{3}(Id) ON DELETE {4} ON UPDATE NO ACTION;\n", property.Name, obj.Schema, Manager.TablePrefix, foreignModel.DataBaseTableName, foreignAttribute.Action.ToString().Replace("_", " "));
            }

            Logger.Info("Created a new query for Create Foreign Keys:");
            Logger.Info(queryBuilder.ToString());
            return queryBuilder.ToString();
        }

        public string GetSqlDataType(Type codeType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(codeType);

            if (underlyingType == null)
            {
                underlyingType = codeType;
            }

            switch (underlyingType.Name.ToLower())
            {
                case "boolean":
                    return "bit";
                case "bool":
                    return "bit";
                case "guid":
                    return "uniqueidentifier";
                case "char":
                    return "char(1)";
                case "string":
                    return "varchar(255)";
                case "datetime":
                    return "datetime";
                case "decimal":
                    return "decimal(18,2)";
                case "single":
                    return "real";
                case "float":
                    return "float";
                case "double":
                    return "float";
                case "byte":
                    return "tinyint";
                case "sbyte":
                    return "smallint";
                case "byte[]":
                    return "varbinary(1024)";
                case "short":
                    return "smallint";
                case "ushort":
                    return "numeric(5)";
                case "int":
                    return "int";
                case "uint":
                    return "numeric(10)";
                case "int16":
                    return "smallint";
                case "int32":
                    return "int";
                case "uint32":
                    return "numeric(10)";
                case "int64":
                    return "bigint";
                case "uint64":
                    return "numeric(20)";
                case "long":
                    return "bigint";
                case "ulong":
                    return "numeric(20)";
                default:
                    return "varbinary(1024)";
            }
        }

        private string GetAutoPropertyValue(AutoPropertyTypes type)
        {
            switch (type)
            {
                case AutoPropertyTypes.Date:
                    return "CONVERT(DATE, GETDATE()) ";
                case AutoPropertyTypes.DateTime:
                    return "GETDATE()";
                default:
                    return "GETDATE()";
            }
        }

        public string CreateInsertListStoredProcedure<T, TKey>(bool doAlter) where T : IManageable<TKey>, new() where TKey : struct
        {
            throw new NotImplementedException();
        }
    }
}

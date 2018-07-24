﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace DataAccess.BO
{
    public class Tools
    {
        public static T ConvertDataTableToObjectOfType<T>(DataTable dataTable) where T : new()
        {
            T newObject = new T();
            if (dataTable.Rows.Count > 0)
            {
                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, ConvertStringToType(dataTable.Rows[0][propertyInfo.Name].ToString(), propertyInfo.PropertyType));
                    }
                }
            }
            else
            {
                return default(T);
            }

            return newObject;
        }

        public static Dictionary<TKey, TValue> ConvertDataTableToDictionary<TKey, TValue>(DataTable dataTable, string keyName, string valueName)
        {
            if (dataTable.Columns.Count != 2)
            {
                throw new Exception("Esta funcion requiere 2 columnas en la DataTable de forma forzosa.");
            }

            Dictionary<TKey, TValue> newDictionary = new Dictionary<TKey, TValue>();
            if (dataTable.Rows.Count > 0)
            {
                TKey key;
                TValue value;

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    key = (TKey)ConvertStringToType(dataTable.Rows[i][keyName].ToString(), typeof(TKey));
                    value = (TValue)ConvertStringToType(dataTable.Rows[i][valueName].ToString(), typeof(TValue));

                    newDictionary.Add(key, value);
                }
            }

            return newDictionary;
        }

        public static List<T> ConvertDataTableToListOfType<T>(DataTable dataTable) where T : new()
        {
            List<T> newList = new List<T>();

            foreach (DataRow row in dataTable.Rows)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                T newObject = new T();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, ConvertStringToType(row[propertyInfo.Name].ToString(), propertyInfo.PropertyType));
                    }
                }
                newList.Add(newObject);
            }

            return newList;
        }

        public static Dictionary<Guid, T> ConvertDataTableToDictionaryOfType<T>(DataTable dataTable) where T : new()
        {
            Dictionary<Guid, T> newDictionary = new Dictionary<Guid, T>();
            foreach (DataRow row in dataTable.Rows)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                T newObject = new T();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (dataTable.Columns.Contains(propertyInfo.Name))
                    {
                        propertyInfo.SetValue(newObject, ConvertStringToType(row[propertyInfo.Name].ToString(), propertyInfo.PropertyType));
                    }
                }
                newDictionary.Add((newObject as Main).Id.GetValueOrDefault(), newObject);
            }

            return newDictionary;
        }

        public static DataTable ConvertListToDataTableOfType<T>(List<T> list)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                Type type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in list)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["id"] };
            return dataTable;
        }

        public static object ConvertStringToType(string value, Type targetType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(targetType);

            if (underlyingType != null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
            }
            else
            {
                underlyingType = targetType;
            }
            switch (underlyingType.Name)
            {
                case "Guid":
                    return Guid.Parse(value);
                case "String":
                    return value;
                default:
                    return Convert.ChangeType(value, underlyingType);
            }
        }

        public static T SetParametersInObject<T>(Parameter[] parameters) where T : new()
        {
            T newObj = new T();

            (newObj as Main).Id = null;

            foreach (Parameter data in parameters)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(data.Name);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(newObj, data.Value);
                }
            }

            return newObj;
        }

        public static int? StringToInteger(string value, bool nullable)
        {
            if (!int.TryParse(value, out int newValue))
            {
                if (!nullable)
                {
                    newValue = 0;
                }
                else
                {
                    return null;
                }
            }

            return newValue;
        }

        public static int StringToInteger(string value)
        {
            if (!int.TryParse(value, out int newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static Int64? StringToInt64(string value, bool nullable)
        {
            if (!Int64.TryParse(value, out long newValue))
            {
                if (!nullable)
                {
                    newValue = 0;
                }
                else
                {
                    return null;
                }
            }

            return newValue;
        }

        public static Int64 StringToInt64(string value)
        {
            if (!Int64.TryParse(value, out long newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static decimal? StringToDecimal(string value, bool nullable)
        {
            if (!Decimal.TryParse(value, out decimal newValue))
            {
                if (!nullable)
                {
                    newValue = 0;
                }
                else
                {
                    return null;
                }
            }

            return newValue;
        }

        public static decimal StringToDecimal(string value)
        {
            if (!Decimal.TryParse(value, out decimal newValue))
            {
                newValue = 0;
            }

            return newValue;
        }

        public static Guid StringToGuid(string value)
        {
            if (!Guid.TryParse(value, out Guid newValue))
            {
                newValue = new Guid();
            }

            return newValue;
        }

        public static Object MySqlParameterToObject(MySqlParameter parameter)
        {
            return parameter.Value;
        }

        public static Object MSSqlParameterToObject(SqlParameter parameter)
        {
            return parameter.Value;
        }

        public static List<Object> MySqlParameterCollectionToList(MySqlParameterCollection parameters)
        {
            List<Object> objects = new List<object>();

            foreach (MySqlParameter parameter in parameters)
            {
                objects.Add(MySqlParameterToObject(parameter));
            }
            return objects;
        }

        public static List<Object> MSSqlParameterCollectionToList(SqlParameterCollection parameters)
        {
            List<Object> objects = new List<object>();

            foreach (SqlParameter parameter in parameters)
            {
                objects.Add(MSSqlParameterToObject(parameter));
            }
            return objects;
        }
    }
}

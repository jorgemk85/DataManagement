﻿using DataManagement.Standard.Enums;
using DataManagement.Standard.Exceptions;
using DataManagement.Standard.Extensions;
using DataManagement.Standard.Interfaces;
using DataManagement.Standard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace DataManagement.Standard.DAO
{
    internal class QueryEvaluation
    {
        IOperable operation;
        ConnectionTypes connectionType;

        public QueryEvaluation(ConnectionTypes connectionType)
        {
            this.connectionType = connectionType;
            operation = Operation.GetOperationBasedOnConnectionType(connectionType);
        }

        public Result Evaluate<T, TKey>(T obj, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Result resultado = null;
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelect<T, TKey>(obj, out resultado, isCached, cache, isPartialCache, forceQueryDataBase, connectionToUse);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll<T, TKey>(obj, out resultado, isCached, cache, forceQueryDataBase, connectionToUse);
                    break;
                case TransactionTypes.Delete:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) DeleteInCache<T, TKey>(obj, cache);
                    break;
                case TransactionTypes.Insert:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) InsertInCache<T, TKey>(obj, cache);
                    break;
                case TransactionTypes.Update:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) UpdateInCache<T, TKey>(obj, cache);
                    break;
                case TransactionTypes.StoredProcedure:
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, transactionType);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        public Result Evaluate<T, TKey>(List<T> list, TransactionTypes transactionType, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            Result resultado = null;
            bool isCached = cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.InsertList:
                    resultado = operation.ExecuteProcedure<T, TKey>(list, connectionToUse, transactionType);
                    if (isCached && resultado.IsSuccessful) InsertListInCache<T, TKey>(list, cache);
                    break;
                default:
                    break;
            }

            return resultado;
        }

        private void EvaluateSelect<T, TKey>(T obj, out Result resultado, bool isCached, Result cache, bool isPartialCache, bool forceQueryDataBase, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            if (forceQueryDataBase)
            {
                resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);
            }
            else
            {
                resultado = isCached == true ? SelectInCache<T, TKey>(obj, cache) : operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);

                resultado.IsFromCache = isCached == true ? true : false;
                if (isCached && isPartialCache && resultado.Data.Rows.Count == 0)
                {
                    resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.Select);
                }
            }
        }

        private void EvaluateSelectAll<T, TKey>(T obj, out Result resultado, bool isCached, Result cache, bool forceQueryDataBase, string connectionToUse) where T : Cope<T, TKey>, new() where TKey : struct
        {
            if (forceQueryDataBase)
            {
                resultado = operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.SelectAll);
            }
            else
            {
                resultado = isCached == true ? cache : operation.ExecuteProcedure<T, TKey>(obj, connectionToUse, TransactionTypes.SelectAll);
                resultado.IsFromCache = isCached == true ? true : false;
            }
        }

        private Result SelectInCache<T, TKey>(T obj, Result cache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            int valueIndex = 0;
            List<object> values = new List<object>();
            string predicate = string.Empty;
            StringBuilder builder = new StringBuilder();

            foreach (KeyValuePair<string, PropertyInfo> property in Manager<T, TKey>.ModelComposition.FilteredProperties)
            {
                if (property.Value.GetValue(obj) != null)
                {
                    builder.AppendFormat("{0} == @{1} and ", property.Value.Name, valueIndex);
                    values.Add(property.Value.GetValue(obj));
                    valueIndex++;
                }
            }

            predicate = builder.ToString();
            if (string.IsNullOrEmpty(predicate))
            {
                throw new InvalidNumberOfParametersException();
            }
            else
            {
                predicate = predicate.Substring(0, predicate.Length - 5);
                var queryableList = cache.Data.ToList<T>().AsQueryable();
                var resultList = queryableList.Where(predicate, values.ToArray()).ToList();
                return new Result(resultList.ToDataTable<T, TKey>(), true, true);
            }
        }

        private DataRow SetRowData<T, TKey>(DataRow row, T obj) where T : Cope<T, TKey>, new() where TKey : struct
        {
            foreach (PropertyInfo property in Manager<T, TKey>.ModelComposition.Properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    row[property.Name] = property.GetValue(obj);
                }
            }
            return row;
        }

        private void UpdateInCache<T, TKey>(T obj, Result cache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            SetRowData<T, TKey>(cache.Data.Rows.Find(obj.Id.GetValueOrDefault()), obj).AcceptChanges();
        }

        private void InsertInCache<T, TKey>(T obj, Result cache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            cache.Data.Rows.Add(SetRowData<T, TKey>(cache.Data.NewRow(), obj));
            cache.Data.AcceptChanges();
        }

        private void InsertListInCache<T, TKey>(List<T> list, Result cache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            foreach (T obj in list)
            {
                cache.Data.Rows.Add(SetRowData<T, TKey>(cache.Data.NewRow(), obj));
            }
            cache.Data.AcceptChanges();
        }

        public void AlterCache(DataRow row, Result cache)
        {
            DataRow cacheRow = cache.Data.Rows.Find(row[row.Table.PrimaryKey[0]]);
            string columnName = string.Empty;

            if (cacheRow == null)
            {
                // NO existe la fila: la agrega.
                cache.Data.Rows.Add(row.ItemArray);
                cache.Data.AcceptChanges();
            }
            else
            {
                // SI existe la fila: la actualiza.
                for (int i = 0; i < cacheRow.ItemArray.Length; i++)
                {
                    columnName = cacheRow.Table.Columns[i].ColumnName;
                    cacheRow[columnName] = row[columnName];
                }
            }
            cache.Data.AcceptChanges();
        }

        private void DeleteInCache<T, TKey>(T obj, Result cache) where T : Cope<T, TKey>, new() where TKey : struct
        {
            for (int i = 0; i < cache.Data.Rows.Count; i++)
            {
                DataRow row = cache.Data.Rows[i];
                if (row[row.Table.PrimaryKey[0]].Equals(obj.Id.GetValueOrDefault()))
                {
                    row.Delete();
                    cache.Data.AcceptChanges();
                    break;
                }
            }
        }
    }
}

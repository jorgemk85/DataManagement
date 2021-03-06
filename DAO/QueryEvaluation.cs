﻿using OneData.Enums;
using OneData.Extensions;
using OneData.Interfaces;
using OneData.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace OneData.DAO
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

        public Result<T> Evaluate<T>(object obj, Expression<Func<T, bool>> expression, TransactionTypes transactionType, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            Result<T> resultado = null;
            bool hasCache = dataCache.Cache == null ? false : true;

            switch (transactionType)
            {
                case TransactionTypes.Select:
                    EvaluateSelectQuery(expression, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.SelectAll:
                    EvaluateSelectAll((T)obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.Delete:
                    EvaluateDelete((T)obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.DeleteMassive:
                    EvaluateDeleteMassive((IEnumerable<T>)obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.Insert:
                    EvaluateInsert((T)obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.InsertMassive:
                    EvaluateInsertMassive((IEnumerable<T>)obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.Update:
                    EvaluateUpdate((T)obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                case TransactionTypes.UpdateMassive:
                    EvaluateUpdateMassive((IEnumerable<T>)obj, out resultado, hasCache, ref dataCache, queryOptions);
                    break;
                default:
                    throw new NotSupportedException($"El tipo de transaccion {transactionType.ToString()} no puede ser utilizado con esta funcion.");
            }

            return resultado;
        }

        private void EvaluateSelectQuery<T>(Expression<Func<T, bool>> expression, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure(queryOptions, TransactionTypes.Select, true, null, expression);
            }
            else
            {
                resultado = hasCache == true ? SelectInCache(expression, dataCache, queryOptions) : operation.ExecuteProcedure(queryOptions, TransactionTypes.Select, true, null, expression);

                if (hasCache && dataCache.IsPartialCache && resultado.Data.Count == 0)
                {
                    resultado = operation.ExecuteProcedure(queryOptions, TransactionTypes.Select, true, null, expression);
                    AlterCache(resultado, ref dataCache);
                }

                if (!hasCache && resultado.IsSuccessful)
                {
                    dataCache.Cache = resultado;
                    dataCache.LastCacheUpdate = DateTime.Now.Ticks;
                }
            }
        }

        private void EvaluateInsert<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.Insert, true, obj, null);
            if (hasCache && resultado.IsSuccessful)
            {
                InsertInCache(obj, ref dataCache);
            }
        }

        private void EvaluateInsertMassive<T>(IEnumerable<T> list, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.InsertMassive, true, list, null);
            if (hasCache && resultado.IsSuccessful)
            {
                InsertMassiveInCache(list, ref dataCache);
            }
        }

        private void EvaluateUpdateMassive<T>(IEnumerable<T> list, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.UpdateMassive, true, list, null);
            if (hasCache && resultado.IsSuccessful)
            {
                UpdateMassiveInCache(list, ref dataCache);
            }
        }

        private void EvaluateDeleteMassive<T>(IEnumerable<T> list, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.DeleteMassive, true, list, null);
            if (hasCache && resultado.IsSuccessful)
            {
                DeleteMassiveInCache(list, ref dataCache);
            }
        }

        private void EvaluateUpdate<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.Update, true, obj, null);
            if (hasCache && resultado.IsSuccessful)
            {
                UpdateInCache(obj, ref dataCache);
            }
        }

        private void EvaluateDelete<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.Delete, true, obj, null);
            if (hasCache && resultado.IsSuccessful)
            {
                DeleteInCache(obj, ref dataCache);
            }
        }

        private void EvaluateSelectAll<T>(T obj, out Result<T> resultado, bool hasCache, ref DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            if (!dataCache.IsEnabled)
            {
                resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.SelectAll, true, obj, null);
            }
            else
            {
                if (hasCache && !dataCache.IsPartialCache)
                {
                    resultado = SelectInCache(null, dataCache, queryOptions);
                }
                else
                {
                    resultado = operation.ExecuteProcedure<T>(queryOptions, TransactionTypes.SelectAll, true, obj, null);
                    dataCache.Cache = resultado;
                }
            }

            if (dataCache.IsEnabled && resultado.IsSuccessful && resultado.Data.Count > 0 && queryOptions.MaximumResults == -1 & queryOptions.Offset == 0)
            {
                dataCache.IsPartialCache = false;
            }
        }

        private Result<T> SelectInCache<T>(Expression<Func<T, bool>> expression, DataCache<T> dataCache, QueryOptions queryOptions) where T : IManageable, new()
        {
            IQueryable<T> queryableList = dataCache.Cache.Data.Values.AsQueryable();

            if (expression != null)
            {
                queryableList = queryableList.Where(expression);
            }

            Dictionary<dynamic, T> resultList = null;

            if (queryOptions.Offset > 0 && queryOptions.MaximumResults > 0)
            {
                resultList = queryableList.OrderByDescending(obj => Manager<T>.Composition.DateModifiedProperty.GetValue(obj)).Skip(queryOptions.Offset).Take(queryOptions.MaximumResults).ToDictionary(Manager<T>.Composition.PrimaryKeyProperty.Name, Manager<T>.Composition.PrimaryKeyProperty.PropertyType);
            }
            else if (queryOptions.Offset > 0)
            {
                resultList = queryableList.OrderByDescending(obj => Manager<T>.Composition.DateModifiedProperty.GetValue(obj)).Skip(queryOptions.Offset).ToDictionary(Manager<T>.Composition.PrimaryKeyProperty.Name, Manager<T>.Composition.PrimaryKeyProperty.PropertyType);
            }
            else if (queryOptions.MaximumResults > 0)
            {
                resultList = queryableList.OrderByDescending(obj => Manager<T>.Composition.DateModifiedProperty.GetValue(obj)).Take(queryOptions.MaximumResults).ToDictionary(Manager<T>.Composition.PrimaryKeyProperty.Name, Manager<T>.Composition.PrimaryKeyProperty.PropertyType);
            }

            if (resultList == null)
            {
                resultList = dataCache.Cache.Data;
            }

            return new Result<T>(resultList, true, true);
        }

        private void UpdateInCache<T>(T obj, ref DataCache<T> dataCache) where T : IManageable, new()
        {
            Manager<T>.Composition.DateModifiedProperty.SetValue(obj, DateTime.Now);
            dataCache.Cache.Data[Manager<T>.Composition.PrimaryKeyProperty.GetValue(obj)] = obj;
        }

        private void InsertInCache<T>(T obj, ref DataCache<T> dataCache) where T : IManageable, new()
        {
            Manager<T>.Composition.DateCreatedProperty.SetValue(obj, DateTime.Now);
            Manager<T>.Composition.DateModifiedProperty.SetValue(obj, DateTime.Now);
            dataCache.Cache.Data.Add(Manager<T>.Composition.PrimaryKeyProperty.GetValue(obj), obj);
        }

        private void InsertMassiveInCache<T>(IEnumerable<T> list, ref DataCache<T> dataCache) where T : IManageable, new()
        {
            foreach (T obj in list)
            {
                dataCache.Cache.Data.Add(Manager<T>.Composition.PrimaryKeyProperty.GetValue(obj), obj);
            }
        }

        private void UpdateMassiveInCache<T>(IEnumerable<T> list, ref DataCache<T> dataCache) where T : IManageable, new()
        {
            foreach (T obj in list)
            {
                dataCache.Cache.Data[Manager<T>.Composition.PrimaryKeyProperty.GetValue(obj)] = obj;
            }
        }

        private void DeleteMassiveInCache<T>(IEnumerable<T> list, ref DataCache<T> dataCache) where T : IManageable, new()
        {
            foreach (T obj in list)
            {
                dataCache.Cache.Data.Remove(Manager<T>.Composition.PrimaryKeyProperty.GetValue(obj));
            }
        }

        private void AlterCache<T>(Result<T> resultado, ref DataCache<T> dataCache)
        {
            foreach (KeyValuePair<dynamic, T> item in resultado.Data)
            {
                dataCache.Cache.Data[item.Key] = item.Value;
            }
            dataCache.LastCacheUpdate = DateTime.Now.Ticks;
        }

        private void DeleteInCache<T>(T obj, ref DataCache<T> dataCache) where T : IManageable, new()
        {
            dataCache.Cache.Data.Remove(obj);
        }
    }
}

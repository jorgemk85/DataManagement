﻿using DataManagement.Interfaces;
using DataManagement.Models;
using DataManagement.Tools;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DataManagement.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void ToExcel<T>(this IEnumerable<T> list, string fullyQualifiedFileName)
        {
            ExcelSerializer.SerializeIEnumerableOfTypeToExcel(list, fullyQualifiedFileName);
        }

        public static void FromExcel<T>(this IEnumerable<T> list, ref List<T> myList, string worksheetName, string fullyQualifiedFileName) where T : new()
        {
            myList = ExcelSerializer.DeserializeExcelToListOfType<T>(worksheetName, fullyQualifiedFileName);
        }

        public static void ToFile<T>(this IEnumerable<T> list, string fullyQualifiedFileName, char separator)
        {
            FileSerializer.SerializeIEnumerableOfTypeToFile(list, fullyQualifiedFileName, separator);
        }

        public static void FromFile<T>(this IEnumerable<T> list, ref List<T> myList, string fullyQualifiedFileName, char separator, Encoding encoding) where T : new()
        {
            myList = FileSerializer.DeserializeFileToListOfType<T>(fullyQualifiedFileName, separator, encoding);
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : new()
        {
            return DataSerializer.ConvertIEnumerableToDataTableOfGenericType(list);
        }

        public static DataTable ToDataTable<T, TKey>(this IEnumerable<T> list) where T : Cope<T, TKey>, new() where TKey : struct
        {
            return DataSerializer.ConvertIEnumerableToDataTableOfType<T, TKey>(list);
        }
    }
}

﻿using OneData.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OneData.Tools
{
    public class ExcelSerializer
    {
        ///// <summary>
        ///// Serializa un objeto de tipo IEnumerable <typeparamref name="T"/> y lo guarda en un archivo Excel.
        ///// </summary>
        ///// <typeparam name="T">Tipo de objeto.</typeparam>
        ///// <param name="list">Lista de objetos de tipo <typeparamref name="T"/> que se convertira.</param>
        ///// <param name="fullyQualifiedFileName">Directorio completo, incluyendo nombre de archivo y extension. Se utiliza para guardar el producto final.</param>
        //public static void SerializeIEnumerableOfTypeToExcel<T>(IEnumerable<T> list, string fullyQualifiedFileName)
        //{
        //    SerializeIEnumerable(list, fullyQualifiedFileName);
        //}

        ///// <summary>
        ///// Serializa un objeto de tipo IEnumerable <typeparamref name="T"/> y lo guarda en un archivo Excel utilizando Async.
        ///// </summary>
        ///// <typeparam name="T">Tipo de objeto.</typeparam>
        ///// <param name="list">Lista de objetos de tipo <typeparamref name="T"/> que se convertira.</param>
        ///// <param name="fullyQualifiedFileName">Directorio completo, incluyendo nombre de archivo y extension. Se utiliza para guardar el producto final.</param>
        //public static async void SerializeIEnumerableOfTypeToExcelAsync<T>(IEnumerable<T> list, string fullyQualifiedFileName)
        //{
        //    await Task.Run(() => SerializeIEnumerable(list, fullyQualifiedFileName));
        //}

        //private static void SerializeIEnumerable<T>(IEnumerable<T> list, string fullyQualifiedFileName)
        //{
        //    PropertyInfo[] properties = typeof(T).GetProperties();

        //    using (ExcelPackage excelPackage = new ExcelPackage())
        //    {
        //        ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("Pagina 1");

        //        SetExcelHeadersAndTypes(excelWorksheet, ref properties);
        //        SetExcelContent(excelWorksheet, list, ref properties);
        //        excelWorksheet.Cells[excelWorksheet.Dimension.Address].AutoFitColumns();
        //        excelPackage.SaveAs(new FileInfo(fullyQualifiedFileName));
        //    }
        //}

        ///// <summary>
        ///// Deserializa un archivo de Excel y lo convierte en un objeto List de tipo <typeparamref name="T"/>.
        ///// </summary>
        ///// <typeparam name="T">Tipo de objeto.</typeparam>
        ///// <param name="worksheetName">Nombre de la pagina donde se encuentra la informacion a deserializar.</param>
        ///// <param name="fullyQualifiedFileName">Direccion completa del archivo de Excel. Incluir directorio, nombre y extension.</param>
        ///// <returns></returns>
        //public static List<T> DeserializeExcelToListOfType<T>(string worksheetName, string fullyQualifiedFileName) where T : new()
        //{
        //    return DeserializeExcel<T>(worksheetName, fullyQualifiedFileName);
        //}

        ///// <summary>
        ///// Deserializa un archivo de Excel y lo convierte en un objeto List de tipo <typeparamref name="T"/> utilizando Async.
        ///// </summary>
        ///// <typeparam name="T">Tipo de objeto.</typeparam>
        ///// <param name="worksheetName">Nombre de la pagina donde se encuentra la informacion a deserializar.</param>
        ///// <param name="fullyQualifiedFileName">Direccion completa del archivo de Excel. Incluir directorio, nombre y extension.</param>
        ///// <returns></returns>
        //public static async Task<List<T>> DeserializeExcelToListOfTypeAsync<T>(string worksheetName, string fullyQualifiedFileName) where T : new()
        //{
        //    return await Task.Run(() => DeserializeExcel<T>(worksheetName, fullyQualifiedFileName));
        //}

        //private static List<T> DeserializeExcel<T>(string worksheetName, string fullyQualifiedFileName) where T : new()
        //{
        //    PropertyInfo[] properties = typeof(T).GetProperties();

        //    try
        //    {
        //        if (!File.Exists(fullyQualifiedFileName))
        //        {
        //            throw new FileNotFoundException();
        //        }
        //        using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(fullyQualifiedFileName)))
        //        {
        //            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets[worksheetName];

        //            return GetExcelContent<T>(excelWorksheet, GetHeadersFromFirstRow(excelWorksheet), ref properties);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        //private static List<T> GetExcelContent<T>(ExcelWorksheet excelWorksheet, List<string> headers, ref PropertyInfo[] properties) where T : new()
        //{
        //    ExcelAddressBase dimension = excelWorksheet.Dimension;
        //    List<T> newList = new List<T>();
        //    Dictionary<string, string> columnsNames = new Dictionary<string, string>();

        //    for (int y = 0; y < dimension.End.Row - 1; y++)
        //    {
        //        T newObj = new T();
        //        columnsNames.Clear();
        //        for (int i = 0; i < dimension.End.Column; i++)
        //        {
        //            columnsNames.Add(headers[i], excelWorksheet.Cells[y + 2, i + 1].Value.ToString());
        //        }
        //        foreach (string header in headers)
        //        {
        //            for (int x = 0; x < properties.Length; x++)
        //            {
        //                HeaderName headerNameAttribute;
        //                string propertyName = string.Empty;

        //                propertyName = properties[x].Name;
        //                headerNameAttribute = properties[x].GetCustomAttribute<HeaderName>();
        //                if (headerNameAttribute != null)
        //                {
        //                    propertyName = headerNameAttribute.Name;
        //                }

        //                if (propertyName.Equals(header))
        //                {
        //                    SetValueInProperty(columnsNames, excelWorksheet.Cells[y + 2, x + 1].Value.ToString(), properties[x], headerNameAttribute, header, newObj);
        //                    break;
        //                }
        //            }
        //        }
        //        newList.Add(newObj);
        //    }

        //    return newList;
        //}

        //private static List<string> GetHeadersFromFirstRow(ExcelWorksheet excelWorksheet)
        //{
        //    List<string> headers = new List<string>();

        //    ExcelCellAddress end = excelWorksheet.Dimension.End;
        //    for (int i = 1; i < end.Column + 1; i++)
        //    {
        //        headers.Add(excelWorksheet.Cells[1, i].Value.ToString());
        //    }

        //    return headers;
        //}

        //private static void SetExcelHeadersAndTypes(ExcelWorksheet excelWorksheet, ref PropertyInfo[] properties)
        //{
        //    string propertyName = string.Empty;
        //    HeaderName headerName;

        //    // Headers
        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        propertyName = properties[i].Name;
        //        headerName = properties[i].GetCustomAttribute<HeaderName>();
        //        if (headerName != null)
        //        {
        //            propertyName = headerName.Name;
        //        }
        //        excelWorksheet.Column(i + 1).Style.Numberformat.Format = GetExcelFormatBasedOnType(properties[i].PropertyType);
        //        excelWorksheet.Cells[1, i + 1].Value = propertyName;
        //    }
        //}

        //private static void SetExcelContent<T>(ExcelWorksheet excelWorksheet, IEnumerable<T> list, ref PropertyInfo[] properties)
        //{
        //    // Content
        //    int y = 0;
        //    foreach (T item in list)
        //    {
        //        for (int x = 0; x < properties.Length; x++)
        //        {
        //            excelWorksheet.Cells[y + 2, x + 1].Value = properties[x].GetValue(item);
        //        }
        //        y++;
        //    }
        //}

        //private static string GetExcelFormatBasedOnType(Type type)
        //{
        //    Type underlyingType = Nullable.GetUnderlyingType(type);

        //    if (underlyingType == null)
        //    {
        //        underlyingType = type;
        //    }

        //    string typeClass = underlyingType.Name.ToLower();
        //    switch (typeClass)
        //    {
        //        case "datetime":
        //            return DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
        //        case "decimal":
        //            return "$###,###,##0.00";
        //        case "float":
        //            return "0.00";
        //        case "int":
        //            return "#";
        //        case "int32":
        //            return "#";
        //        case "int64":
        //            return "#";
        //        default:
        //            return "General";
        //    }
        //}

        //private static void SetValueInProperty<T>(Dictionary<string, string> columns, string cellValue, PropertyInfo property, HeaderName headerNameAttribute, string header, T newObj)
        //{
        //    if (!property.CanWrite)
        //    {
        //        return;
        //    }

        //    if (columns.ContainsKey(header))
        //    {
        //        property.SetValue(newObj, SimpleConverter.ConvertStringToType(cellValue, property.PropertyType));
        //    }
        //    else
        //    {
        //        if (headerNameAttribute == null)
        //        {
        //            throw new Exception(string.Format("No se encontro algun dato para el encabezado {0} en el archivo proporcionado.", header));
        //        }
        //        else
        //        {
        //            if (headerNameAttribute.Important)
        //            {
        //                throw new Exception(string.Format("No se encontro algun dato para el encabezado {0} en el archivo proporcionado.", header));
        //            }
        //        }
        //    }
        //}
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Common;

namespace GenericPopulator
{
   public class ListHelper<T>
   {
      public List<T> CreateList(DbDataReader reader)
      {
         Func<DbDataReader, T> readRow = this.GetReader(reader);

         var results = new List<T>();

         while (reader.Read())
            results.Add(readRow(reader));

         return results;
      }
      public List<T> SortList(List<T> listToSort, string propertyName, bool ascending)
      {
         // verify that the propertyName is valid
         var propertyNames = typeof(T).GetProperties().ToList().Select(p => p.Name).ToList();
         if (!propertyNames.Contains(propertyName))
            throw new ArgumentOutOfRangeException("There is no property named: " + propertyName);

         var paramExpression = Expression.Parameter(typeof(T), "item");
         var propertyExpression = Expression.Convert(Expression.Property(paramExpression, propertyName), typeof(object));
         var lambdaExpression = Expression.Lambda<Func<T, object>>(propertyExpression, paramExpression);

         if (ascending)
            return listToSort.AsQueryable().OrderBy(lambdaExpression).ToList();
         else
            return listToSort.AsQueryable().OrderByDescending(lambdaExpression).ToList();
      }

      private Func<DbDataReader, T> GetReader(DbDataReader reader)
      {
         Delegate resDelegate;

         List<string> readerColumns = new List<string>();
         for (int index = 0; index < reader.FieldCount; index++)
            readerColumns.Add(reader.GetName(index));

         // a list of LINQ expressions that will be used for each data row
         var statements = new List<Expression>();

         // get the indexer property of DbDataReader
         var indexerProperty = typeof(DbDataReader).GetProperty("Item", new[] { typeof(string) });

         // Parameter expression to create instance of object
         ParameterExpression instanceParam = Expression.Variable(typeof(T));
         ParameterExpression readerParam = Expression.Parameter(typeof(DbDataReader));

         // create and assign new instance of variable
         BinaryExpression createInstance = Expression.Assign(instanceParam, Expression.New(typeof(T)));
         statements.Add(createInstance);

         // loop through each of the properties in T to determine how to populate the new instance properties
         var properties = typeof(T).GetProperties();
         var columnNames = this.GetColumnNames(properties);

         foreach (var property in properties)
         {
            string columnName = columnNames[property.Name];
            //string columnName = property.Name;
            if (readerColumns.Contains(columnName.ToLower() ))
            {
               // get the instance.Property
               MemberExpression setProperty = Expression.Property(instanceParam, property);

               // the database column name will be what is in the columnNames list -- defaults to the property name
               IndexExpression readValue = Expression.MakeIndex(readerParam, indexerProperty, new[] { Expression.Constant(columnName) });
               ConstantExpression nullValue = Expression.Constant(DBNull.Value, typeof(System.DBNull));
               BinaryExpression valueNotNull = Expression.NotEqual(readValue, nullValue);
               if (property.PropertyType.Name.ToLower().Equals("string"))
               {
                  ConditionalExpression assignProperty = Expression.IfThenElse(valueNotNull, Expression.Assign(setProperty, Expression.Convert(readValue, property.PropertyType)), Expression.Assign(setProperty, Expression.Constant("", typeof(System.String))));
                  statements.Add(assignProperty);
               }
               else
               {
                  ConditionalExpression assignProperty = Expression.IfThen(valueNotNull, Expression.Assign(setProperty, Expression.Convert(readValue, property.PropertyType)));
                  statements.Add(assignProperty);
               }
            }
         }
         var returnStatement = instanceParam;
         statements.Add(returnStatement);

         var body = Expression.Block(instanceParam.Type, new[] { instanceParam }, statements.ToArray());
         var lambda = Expression.Lambda<Func<DbDataReader, T>>(body, readerParam);
         resDelegate = lambda.Compile();

         return (Func<DbDataReader, T>)resDelegate;
      }
      private Dictionary<string, string> GetColumnNames(PropertyInfo[] properties)
      {
         var columnNames = new Dictionary<string, string>();
         foreach (var property in properties)
         {
            string columnName = property.Name;

            var attributes = property.GetCustomAttributes(typeof(Attributes.DatabaseProperty), true);
            if (attributes.Length > 0)
               columnName = ((Attributes.DatabaseProperty)attributes[0]).ColumnName;

            columnNames.Add(property.Name, columnName);
         }
         return columnNames;
      }
   }
}

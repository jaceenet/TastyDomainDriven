using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using TastyDomainDriven.MsSql;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public sealed class SqlAppendOnlyStoreAsyncTests
    {
        [Fact]
        public void StringConstructorTest()
        {
            SqlAppendOnlyStoreAsync store = new SqlAppendOnlyStoreAsync(string.Empty);
            Assert.Equal("[events]", GetCurrentValue(store), StringComparer.Ordinal);
            store = new SqlAppendOnlyStoreAsync(string.Empty, "custom");
            Assert.Equal("[custom]", GetCurrentValue(store), StringComparer.Ordinal);
            store = new SqlAppendOnlyStoreAsync(string.Empty, "[custom]");
            Assert.Equal("[custom]", GetCurrentValue(store), StringComparer.Ordinal);
        }

        [Fact]
        public void SqlConnectionConstructorTest()
        {
            SqlConnection connection = new SqlConnection(string.Empty);
            SqlAppendOnlyStoreAsync store = new SqlAppendOnlyStoreAsync(connection);
            Assert.Equal("[events]", GetCurrentValue(store), StringComparer.Ordinal);
            store = new SqlAppendOnlyStoreAsync(connection, "custom");
            Assert.Equal("[custom]", GetCurrentValue(store), StringComparer.Ordinal);
            store = new SqlAppendOnlyStoreAsync(connection, "[custom]");
            Assert.Equal("[custom]", GetCurrentValue(store), StringComparer.Ordinal);
        }

        private string GetCurrentValue(SqlAppendOnlyStoreAsync store)
        {
            Type t = store.GetType();
            FieldInfo field = t.GetField("tableName", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
            if (field == null)
            {
                return null;
            }
            object value = field.GetValue(store);
            return value.ToString();
        }

    }
}

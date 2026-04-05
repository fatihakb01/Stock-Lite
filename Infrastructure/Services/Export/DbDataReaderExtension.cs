using System.Data.Common;

namespace Infrastructure.Services.Export
{
    public static class DbDataReaderExtension
    {
        public static T? GetNullableField<T>(this DbDataReader reader, string column) where T : struct
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T>(ordinal);
        }

        public static T GetField<T>(this DbDataReader reader, string column) where T : struct
        {
            var ordinal = reader.GetOrdinal(column);

            if (reader.IsDBNull(ordinal))
                throw new InvalidOperationException($"Unexpected NULL value in non-nullable column '{column}'.");

            return reader.GetFieldValue<T>(ordinal);
        }

        public static T GetRefField<T>(this DbDataReader reader, string column) where T : class
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.GetFieldValue<T>(ordinal);
        }
    }
}

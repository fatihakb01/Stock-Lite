using Infrastructure.Services.Export;
using Moq;
using System.Data.Common;

namespace Test.Unit.Services.Export
{
    public class DbDataReaderExtensionTest
    {
        // --- GetNullableField ---
        [Fact]
        public void GetNullableField_ColumnIsNull_ReturnsNull()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("RecommendedOrderDate")).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var result = mock.Object.GetNullableField<DateTime>("RecommendedOrderDate");

            Assert.Null(result);
        }

        [Fact]
        public void GetNullableField_ColumnIsNotNull_ReturnsValue()
        {
            var expected = new DateTime(2026, 1, 1);

            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("RecommendedOrderDate")).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetFieldValue<DateTime>(0)).Returns(expected);

            var result = mock.Object.GetNullableField<DateTime>("RecommendedOrderDate");

            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetNullableField_IntColumn_NotNull_ReturnsCorrectValue(int expected)
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("WeekIndex")).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetFieldValue<int>(0)).Returns(expected);

            var result = mock.Object.GetNullableField<int>("WeekIndex");

            Assert.Equal(expected, result);
        }

        // --- GetField ---
        [Fact]
        public void GetField_ColumnIsNull_ThrowsInvalidOperationException()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("QuantityOnHand")).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                mock.Object.GetField<int>("QuantityOnHand"));

            Assert.Contains("QuantityOnHand", ex.Message);
        }

        [Fact]
        public void GetField_ColumnIsNotNull_ReturnsValue()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("QuantityOnHand")).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetFieldValue<int>(0)).Returns(99);

            var result = mock.Object.GetField<int>("QuantityOnHand");

            Assert.Equal(99, result);
        }

        [Fact]
        public void GetField_DecimalColumn_NotNull_ReturnsValue()
        {
            var expected = 3.14m;

            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("NetStockPosition")).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetFieldValue<decimal>(0)).Returns(expected);

            var result = mock.Object.GetField<decimal>("NetStockPosition");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetField_BoolColumn_NotNull_ReturnsValue()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("NeedsReorder")).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(false);
            mock.Setup(r => r.GetFieldValue<bool>(0)).Returns(true);

            var result = mock.Object.GetField<bool>("NeedsReorder");

            Assert.True(result);
        }

        [Fact]
        public void GetField_ExceptionMessage_ContainsColumnName()
        {
            const string column = "QuantityOnHand";

            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal(column)).Returns(0);
            mock.Setup(r => r.IsDBNull(0)).Returns(true);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                mock.Object.GetField<int>(column));

            Assert.Contains(column, ex.Message);
        }

        // --- GetRefField ---
        [Fact]
        public void GetRefField_ColumnHasValue_ReturnsString()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("SKU")).Returns(0);
            mock.Setup(r => r.GetFieldValue<string>(0)).Returns("PROD-001");

            var result = mock.Object.GetRefField<string>("SKU");

            Assert.Equal("PROD-001", result);
        }

        [Fact]
        public void GetRefField_ColumnHasEmptyString_ReturnsEmptyString()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("SKU")).Returns(0);
            mock.Setup(r => r.GetFieldValue<string>(0)).Returns(string.Empty);

            var result = mock.Object.GetRefField<string>("SKU");

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetRefField_ColumnIsDbNull_ReturnsNull()
        {
            // GetRefField calls GetFieldValue directly without a null check
            // so when the DB column is NULL, GetFieldValue returns null for reference types
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("SKU")).Returns(0);
            mock.Setup(r => r.GetFieldValue<string>(0)).Returns((string)null!);

            var result = mock.Object.GetRefField<string>("SKU");

            Assert.Null(result);
        }

        // --- GetOrdinal behaviour ---
        [Fact]
        public void GetField_UnknownColumn_ThrowsFromGetOrdinal()
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(r => r.GetOrdinal("NonExistentColumn"))
                .Throws(new IndexOutOfRangeException("NonExistentColumn"));

            Assert.Throws<IndexOutOfRangeException>(() =>
                mock.Object.GetField<int>("NonExistentColumn"));
        }
    }
}
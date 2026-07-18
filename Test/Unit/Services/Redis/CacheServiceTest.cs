using Infrastructure.Services.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;
using Test.Helper;

namespace Test.Unit.Services.Redis
{
    public class CacheServiceTest
    {
        private readonly Mock<IDistributedCache> _distributedCacheMock;
        private readonly CacheService _cacheService;
        private readonly CancellationToken _cancellationToken;

        public CacheServiceTest()
        {
            _distributedCacheMock = new Mock<IDistributedCache>();
            _cacheService = new CacheService(_distributedCacheMock.Object);
            _cancellationToken = CancellationToken.None;
        }

        [Fact]
        public async Task GetAsync_KeyExists_ReturnsDeserializedValue()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = new TestClass { Id = 1, Name = "Test" };
            var jsonValue = JsonSerializer.Serialize(expectedValue);
            var bytes = Encoding.UTF8.GetBytes(jsonValue);

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, _cancellationToken))
                .ReturnsAsync(bytes);

            // Act
            var result = await _cacheService.GetAsync<TestClass>(key, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedValue.Id, result.Id);
            Assert.Equal(expectedValue.Name, result.Name);
            _distributedCacheMock.Verify(x => x.GetAsync(key, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_KeyDoesNotExist_ReturnsDefault()
        {
            // Arrange
            var key = "non-existent-key";

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, _cancellationToken))
                .ReturnsAsync((byte[]?)null);

            // Act
            var result = await _cacheService.GetAsync<TestClass>(key, _cancellationToken);

            // Assert
            Assert.Null(result);
            _distributedCacheMock.Verify(x => x.GetAsync(key, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_KeyExistsWithNullValue_ReturnsDefault()
        {
            // Arrange
            var key = "null-value-key";

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, _cancellationToken))
                .ReturnsAsync((byte[]?)null);

            // Act
            var result = await _cacheService.GetAsync<string>(key, _cancellationToken);

            // Assert
            Assert.Null(result);
            _distributedCacheMock.Verify(x => x.GetAsync(key, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ValueType_ReturnsDeserializedValue()
        {
            // Arrange
            var key = "int-key";
            var expectedValue = 42;
            var jsonValue = JsonSerializer.Serialize(expectedValue);
            var bytes = Encoding.UTF8.GetBytes(jsonValue);

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, _cancellationToken))
                .ReturnsAsync(bytes);

            // Act
            var result = await _cacheService.GetAsync<int>(key, _cancellationToken);

            // Assert
            Assert.Equal(expectedValue, result);
            _distributedCacheMock.Verify(x => x.GetAsync(key, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task GetAsync_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var key = "invalid-json-key";
            var invalidJson = "invalid json";
            var bytes = Encoding.UTF8.GetBytes(invalidJson);

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, _cancellationToken))
                .ReturnsAsync(bytes);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() =>
                _cacheService.GetAsync<TestClass>(key, _cancellationToken));

            _distributedCacheMock.Verify(x => x.GetAsync(key, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithValue_UsesDefaultExpiry()
        {
            // Arrange
            var key = "test-key";
            var value = new TestClass { Id = 1, Name = "Test" };
            var jsonValue = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(jsonValue);

            // Act
            await _cacheService.SetAsync(key, value, ct: _cancellationToken);

            // Assert
            _distributedCacheMock.Verify(
                x => x.SetAsync(
                    key,
                    bytes,
                    It.Is<DistributedCacheEntryOptions>(o =>
                        o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5)),
                    _cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithCustomExpiry_UsesCustomExpiry()
        {
            // Arrange
            var key = "test-key";
            var value = new TestClass { Id = 1, Name = "Test" };
            var customExpiry = TimeSpan.FromHours(2);
            var jsonValue = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(jsonValue);

            // Act
            await _cacheService.SetAsync(key, value, customExpiry, _cancellationToken);

            // Assert
            _distributedCacheMock.Verify(
                x => x.SetAsync(
                    key,
                    bytes,
                    It.Is<DistributedCacheEntryOptions>(o =>
                        o.AbsoluteExpirationRelativeToNow == customExpiry),
                    _cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_WithNullValue_SetsNullValue()
        {
            // Arrange
            var key = "null-key";
            string? value = null;

            // Act
            await _cacheService.SetAsync<string?>(key, value, ct: _cancellationToken);

            // Assert
            _distributedCacheMock.Verify(
                x => x.SetAsync(
                    key,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    _cancellationToken),
                Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SetAsync_WithZeroOrNegativeExpiry_UsesNegativeExpiry(int expiry)
        {
            // Arrange
            var key = "test-key";
            var value = new TestClass { Id = 1, Name = "Test" };
            var invalidExpiry = TimeSpan.FromMinutes(expiry);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _cacheService.SetAsync(key, value, invalidExpiry, _cancellationToken));
        }

        [Fact]
        public async Task RemoveAsync_ExistingKey_RemovesKey()
        {
            // Arrange
            var key = "test-key";

            // Act
            await _cacheService.RemoveAsync(key, _cancellationToken);

            // Assert
            _distributedCacheMock.Verify(x => x.RemoveAsync(key, _cancellationToken), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("non-existent-key")]
        public async Task RemoveAsync_NonExistentKey_StillCallsRemove(string? key)
        {
            // Act
            await _cacheService.RemoveAsync(key!, _cancellationToken);

            // Assert
            _distributedCacheMock.Verify(x => x.RemoveAsync(key!, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task SetAndGetAsync_RoundTrip_ReturnsSameValue()
        {
            // Arrange
            var key = "roundtrip-key";
            var expectedValue = new TestClass { Id = 100, Name = "RoundTrip" };
            var jsonValue = JsonSerializer.Serialize(expectedValue);
            var bytes = Encoding.UTF8.GetBytes(jsonValue);

            _distributedCacheMock
                .Setup(x => x.SetAsync(
                    key,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    _cancellationToken))
                .Returns(Task.CompletedTask);

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, _cancellationToken))
                .ReturnsAsync(bytes);

            // Act
            await _cacheService.SetAsync(key, expectedValue, ct: _cancellationToken);
            var result = await _cacheService.GetAsync<TestClass>(key, _cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedValue.Id, result.Id);
            Assert.Equal(expectedValue.Name, result.Name);
        }

        [Fact]
        public async Task SetAndRemoveAsync_AfterRemove_GetReturnsDefault()
        {
            // Arrange
            var key = "remove-test-key";
            var value = new TestClass { Id = 200, Name = "RemoveTest" };

            _distributedCacheMock
                .Setup(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), _cancellationToken))
                .Returns(Task.CompletedTask);

            _distributedCacheMock
                .Setup(x => x.RemoveAsync(key, _cancellationToken))
                .Returns(Task.CompletedTask);

            _distributedCacheMock
                .Setup(x => x.GetAsync(key, _cancellationToken))
                .ReturnsAsync((byte[]?)null);

            // Act
            await _cacheService.SetAsync(key, value, ct: _cancellationToken);
            await _cacheService.RemoveAsync(key, _cancellationToken);
            var result = await _cacheService.GetAsync<TestClass>(key, _cancellationToken);

            // Assert
            Assert.Null(result);
            _distributedCacheMock.Verify(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), _cancellationToken), Times.Once);
            _distributedCacheMock.Verify(x => x.RemoveAsync(key, _cancellationToken), Times.Once);
            _distributedCacheMock.Verify(x => x.GetAsync(key, _cancellationToken), Times.Once);
        }
    }
}
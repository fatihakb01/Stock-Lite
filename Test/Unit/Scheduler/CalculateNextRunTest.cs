using Domain.Enums;
using Infrastructure.Services.Scheduler;

namespace Test.Unit.Scheduler
{

    public class CalculateNextRunTest
    {
        [Theory]
        [InlineData(Period.Daily)]
        [InlineData(Period.Weekly)]
        [InlineData(Period.Monthly)]
        public void CalculateNextRun_WithNullCurrent_ReturnsNull(Period period)
        {
            var result = JobTaskDispatcher.CalculateNextRun(null, period);

            Assert.Null(result);
        }

        [Fact]
        public void CalculateNextRun_WithNullPeriod_ReturnsNull()
        {
            var current = new DateTime(2026, 1, 1);

            var result = JobTaskDispatcher.CalculateNextRun(current, null);

            Assert.Null(result);
        }

        [Fact]
        public void CalculateNextRun_WithNullCurrentAndPeriod_ReturnsNull()
        {
            var result = JobTaskDispatcher.CalculateNextRun(null, null);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(Period.Daily, 1)]
        [InlineData(Period.Weekly, 7)]
        public void CalculateNextRun_AddsCorrectDays(Period period, int expectedDays)
        {
            var current = new DateTime(2026, 1, 1);

            var actual = JobTaskDispatcher.CalculateNextRun(current, period);
            var expected = current.AddDays(expectedDays);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Period.Monthly, 1)]
        public void CalculationNextRun_AddsCorrectMonths(Period period, int expectedMonths)
        {
            var current = new DateTime(2026, 1, 1);

            var actual = JobTaskDispatcher.CalculateNextRun(current, period);
            var expected = current.AddMonths(expectedMonths);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CalculateNextRun_WithUnsupportedPeriod_ThrowsInvalidOperationException()
        {
            var current = new DateTime(2026, 1, 1);
            var unsupportedPeriod = (Period) 99;

            Assert.Throws<InvalidOperationException>(() =>
                JobTaskDispatcher.CalculateNextRun(current, unsupportedPeriod));
        }
    }
}
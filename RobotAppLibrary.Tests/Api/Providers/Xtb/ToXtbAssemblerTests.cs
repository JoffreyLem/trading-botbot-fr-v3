namespace RobotAppLibrary.Tests.Api.Providers.Xtb;

using System;
using RobotAppLibrary.Api.Providers.Xtb.Assembler;
using Modeles;
using RobotAppLibrary.Api.Providers.Xtb.Code;
using Xunit;
using FluentAssertions;

public class ToXtbAssemblerTests
{
    [Theory]
    [InlineData(Timeframe.OneMinute, 1)]
    [InlineData(Timeframe.FiveMinutes, 5)]
    [InlineData(Timeframe.FifteenMinutes, 15)]
    [InlineData(Timeframe.ThirtyMinutes, 30)]
    [InlineData(Timeframe.OneHour, 60)]
    [InlineData(Timeframe.FourHour, 240)]
    [InlineData(Timeframe.Daily, 1440)]
    [InlineData(Timeframe.Weekly, 10080)]
    [InlineData(Timeframe.Monthly, 43200)]
    public void ToPeriodCode_ShouldReturnExpectedCode(Timeframe timeframe, long expectedCode)
    {
        var result = ToXtbAssembler.ToPeriodCode(timeframe);
        result.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData(Timeframe.OneMinute, 1L, -1)]
    [InlineData(Timeframe.FiveMinutes, 5L, -1)]
    [InlineData(Timeframe.FifteenMinutes, 15L, -7)]
    [InlineData(Timeframe.ThirtyMinutes, 30L, -7)]
    [InlineData(Timeframe.OneHour, 60L, -7)]
    [InlineData(Timeframe.FourHour, 240L, -7)]
    [InlineData(Timeframe.Daily, 1440L, -7)]
    [InlineData(Timeframe.Weekly, 10080L, -7)]
    [InlineData(Timeframe.Monthly, 43200L, -7)]
    public void SetDateTime_ShouldReturnExpectedPeriodCodeAndDate(Timeframe timeframe, long expectedPeriodCode, int monthsToAdd)
    {
        var (periodCode, dateTime) = ToXtbAssembler.SetDateTime(timeframe);
        dateTime.Should().BeCloseTo(DateTime.Now.AddMonths(monthsToAdd), TimeSpan.FromDays(1));
        periodCode.Should().Be(new PERIOD_CODE(expectedPeriodCode));
    }

    [Theory]
    [InlineData(Timeframe.OneMinute, -1)]
    [InlineData(Timeframe.FiveMinutes, -1)]
    [InlineData(Timeframe.FifteenMinutes, -7)]
    [InlineData(Timeframe.ThirtyMinutes, -7)]
    [InlineData(Timeframe.OneHour, -7)]
    [InlineData(Timeframe.FourHour, -7)]
    [InlineData(Timeframe.Daily, -7)]
    [InlineData(Timeframe.Weekly, -7)]
    [InlineData(Timeframe.Monthly, -7)]
    public void SetDateTimeForChart_ShouldReturnExpectedDate(Timeframe timeframe, int monthsToAdd)
    {
        var result = ToXtbAssembler.SetDateTimeForChart(timeframe);
        result.Should().BeCloseTo(DateTime.Now.AddMonths(monthsToAdd), TimeSpan.FromDays(1));
    }

    [Theory]
    [InlineData(TypeOperation.Buy, 0)]
    [InlineData(TypeOperation.Sell, 1)]
    public void ToTradeOperationCode_ShouldReturnExpectedCode(TypeOperation typeOperation, long expectedCode)
    {
        var result = ToXtbAssembler.ToTradeOperationCode(typeOperation);
        result.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData(TypeOperation.None)]
    public void ToTradeOperationCode_ShouldThrowArgumentOutOfRangeException(TypeOperation typeOperation)
    {
        Action act = () => ToXtbAssembler.ToTradeOperationCode(typeOperation);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(TransactionType.Open, 0)]
    [InlineData(TransactionType.Close, 2)]
    [InlineData(TransactionType.Modify, 3)]
    [InlineData(TransactionType.Delete, 4)]
    public void ToTradeTransactionType_ShouldReturnExpectedCode(TransactionType transactionType, long expectedCode)
    {
        var result = ToXtbAssembler.ToTradeTransactionType(transactionType);
        result.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData((TransactionType)99)]
    public void ToTradeTransactionType_ShouldThrowArgumentOutOfRangeException(TransactionType transactionType)
    {
        Action act = () => ToXtbAssembler.ToTradeTransactionType(transactionType);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

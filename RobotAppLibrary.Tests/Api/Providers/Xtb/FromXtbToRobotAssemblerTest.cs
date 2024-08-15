using FluentAssertions;
using RobotAppLibrary.Api.Providers.Xtb.Assembler;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Tests.Api.Providers.Xtb;

public class FromXtbToRobotAssemblerTests
{
    [Theory]
    [InlineData(null, Category.Unknow)]
    [InlineData("", Category.Unknow)]
    [InlineData("FX", Category.Forex)]
    [InlineData("IND", Category.Indices)]
    [InlineData("STC", Category.Stock)]
    [InlineData("CMD", Category.Commodities)]
    [InlineData("CRT", Category.Crypto)]
    [InlineData("ETF", Category.ExchangeTradedFund)]
    [InlineData("OTHER", Category.Unknow)]
    public void GetCategory_ShouldReturnExpectedCategory(string symbol, Category expectedCategory)
    {
        var result = FromXtbToRobotAssembler.GetCategory(symbol);
        result.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData(0, TypeOperation.Buy)]
    [InlineData(1, TypeOperation.Sell)]
    public void GetTypeOperation_ShouldReturnExpectedTypeOperation(long code, TypeOperation expectedTypeOperation)
    {
        var result = FromXtbToRobotAssembler.GetTypeOperation(code);
        result.Should().Be(expectedTypeOperation);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void GetTypeOperation_ShouldThrowArgumentOutOfRangeException(long code)
    {
        Action act = () => FromXtbToRobotAssembler.GetTypeOperation(code);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, StatusPosition.Close)]
    [InlineData(1, StatusPosition.Pending)]
    [InlineData(3, StatusPosition.Accepted)]
    [InlineData(4, StatusPosition.Rejected)]
    public void ToTradeStatusFromTradeStatusStreaming_ShouldReturnExpectedStatusPosition(long statusCode,
        StatusPosition expectedStatusPosition)
    {
        var result = FromXtbToRobotAssembler.ToTradeStatusFromTradeStatusStreaming(statusCode);
        result.Should().Be(expectedStatusPosition);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public void ToTradeStatusFromTradeStatusStreaming_ShouldThrowArgumentOutOfRangeException(long statusCode)
    {
        Action act = () => FromXtbToRobotAssembler.ToTradeStatusFromTradeStatusStreaming(statusCode);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, StatusPosition.Open)]
    [InlineData(1, StatusPosition.Pending)]
    [InlineData(2, StatusPosition.Close)]
    [InlineData(3, StatusPosition.Updated)]
    [InlineData(4, StatusPosition.Close)]
    public void ToTradeStatusFromTradeStreaming_ShouldReturnExpectedStatusPosition(long statusCode,
        StatusPosition expectedStatusPosition)
    {
        var result = FromXtbToRobotAssembler.ToTradeStatusFromTradeStreaming(statusCode);
        result.Should().Be(expectedStatusPosition);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    public void ToTradeStatusFromTradeStreaming_ShouldThrowArgumentOutOfRangeException(long statusCode)
    {
        Action act = () => FromXtbToRobotAssembler.ToTradeStatusFromTradeStreaming(statusCode);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("[S/L]", ReasonClosed.Sl)]
    [InlineData("[T/P]", ReasonClosed.Tp)]
    [InlineData("S/O", ReasonClosed.Margin)]
    [InlineData("Other comment", ReasonClosed.Closed)]
    [InlineData(null, ReasonClosed.Closed)]
    [InlineData("", ReasonClosed.Closed)]
    public void ComputeCommentReasonClosed_ShouldReturnExpectedReasonClosed(string comment,
        ReasonClosed? expectedReasonClosed)
    {
        var result = FromXtbToRobotAssembler.ComputeCommentReasonClosed(comment);
        result.Should().Be(expectedReasonClosed);
    }
}
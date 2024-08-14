using System.Collections.Concurrent;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Providers.Xtb.Assembler;

public static class FromXtbToRobotAssembler
{
    private static readonly ConcurrentDictionary<string, Category> CategoryCache = new();

    public static Category GetCategory(string? symbol)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            return Category.Unknow;
        }

        if (CategoryCache.TryGetValue(symbol, out var cachedCategory))
        {
            return cachedCategory;
        }

        var category = symbol switch
        {
            "FX" => Category.Forex,
            "IND" => Category.Indices,
            "STC" => Category.Stock,
            "CMD" => Category.Commodities,
            "CRT" => Category.Crypto,
            "ETF" => Category.ExchangeTradedFund,
            _ => Category.Unknow
        };

        CategoryCache[symbol] = category;
        return category;
    }


    public static TypeOperation GetTypeOperation(long code)
    {
        switch (code)
        {
            case 0:
                return TypeOperation.Buy;
            case 1:
                return TypeOperation.Sell;
            // case 2:
            //     return TypeOperation.BuyLimit;
            // case 3:
            //     return TypeOperation.SellLimit;
            // case 4:
            //     return TypeOperation.BuyStop;
            // case 5:
            //     return TypeOperation.SellStop;
            // case 6:
            //     return TypeOperation.Balance;
            default:
                throw new ArgumentOutOfRangeException(nameof(code), code, "Invalid operation code.");
        }
    }

    public static StatusPosition ToTradeStatusFromTradeStatusStreaming(long statusCode)
    {
        switch (statusCode)
        {
            case 0:
                return StatusPosition.Close;
            case 1:
                return StatusPosition.Pending;
            case 3:
                return StatusPosition.Accepted;
            case 4:
                return StatusPosition.Rejected;
            default:
                throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Invalid status code");
        }
    }

    public static StatusPosition ToTradeStatusFromTradeStreaming(long statusCode)
    {
        switch (statusCode)
        {
            case 0:
                return StatusPosition.Open;
            case 1:
                return StatusPosition.Pending;
            case 2:
                return StatusPosition.Close;
            case 3:
                return StatusPosition.Updated;
            case 4:
                return StatusPosition.Close;
            default:
                throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Invalid status code");
        }
    }

    public static ReasonClosed? ComputeCommentReasonClosed(string? comment)
    {
        switch (comment)
        {
            case "[S/L]":
                return ReasonClosed.Sl;
            case "[T/P]":
                return ReasonClosed.Tp;
            case not null when comment.Contains("S/O"):
                return ReasonClosed.Margin;
            default:
                return ReasonClosed.Closed;
        }
    }
}
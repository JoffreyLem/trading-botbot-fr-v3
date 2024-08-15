using RobotAppLibrary.Api.Providers.Xtb.Code;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Providers.Xtb.Assembler;

public static class ToXtbAssembler
{
    public static long ToPeriodCode(Timeframe timeframe)
    {
        return timeframe switch
        {
            Timeframe.OneMinute => 1,
            Timeframe.FiveMinutes => 5,
            Timeframe.FifteenMinutes => 15,
            Timeframe.ThirtyMinutes => 30,
            Timeframe.OneHour => 60,
            Timeframe.FourHour => 240,
            Timeframe.Daily => 1440,
            Timeframe.Weekly => 10080,
            Timeframe.Monthly => 43200,
            _ => throw new ArgumentOutOfRangeException(nameof(timeframe), timeframe, null)
        };
    }

    public static long ToTradeOperationCode(TypeOperation typePosition)
    {
        switch (typePosition)
        {
            case TypeOperation.Buy:
                return 0;
            case TypeOperation.Sell:
                return 1;
            // case TypeOperation.BuyLimit:
            //     return 2;
            // case TypeOperation.SellLimit:
            //     return 3;
            // case TypeOperation.BuyStop:
            //     return 4;
            // case TypeOperation.SellStop:
            //     return 5;
            // case TypeOperation.Balance:
            //     return 6;
            // case TypeOperation.Credit:
            // case TypeOperation.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(typePosition), typePosition, null);
        }
    }

    public static long ToTradeTransactionType(TransactionType transactionType)
    {
        switch (transactionType)
        {
            case TransactionType.Open:
                return 0;
            case TransactionType.Close:
                return 2;
            case TransactionType.Modify:
                return 3;
            case TransactionType.Delete:
                return 4;
            default:
                throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null);
        }
    }


    public static (PERIOD_CODE periodCode, DateTime dateTime) SetDateTime(Timeframe tf)
    {
        DateTime dateTime;
        PERIOD_CODE periodCodeData;
        switch (tf)
        {
            case Timeframe.OneMinute:
                dateTime = DateTime.Now.AddMonths(-1);
                periodCodeData = PERIOD_CODE.PERIOD_M1;
                return (periodCodeData, dateTime);

            case Timeframe.FiveMinutes:
                dateTime = DateTime.Now.AddMonths(-1);
                periodCodeData = PERIOD_CODE.PERIOD_M5;
                return (periodCodeData, dateTime);

            case Timeframe.FifteenMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_M15;
                return (periodCodeData, dateTime);
            case Timeframe.ThirtyMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_M30;
                return (periodCodeData, dateTime);
            case Timeframe.OneHour:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_H1;
                return (periodCodeData, dateTime);
            case Timeframe.FourHour:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_H4;
                return (periodCodeData, dateTime);
            case Timeframe.Daily:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_D1;
                return (periodCodeData, dateTime);
            case Timeframe.Weekly:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_W1;
                return (periodCodeData, dateTime);
            case Timeframe.Monthly:
                dateTime = DateTime.Now.AddMonths(-7);
                periodCodeData = PERIOD_CODE.PERIOD_MN1;
                return (periodCodeData, dateTime);


            default:
                throw new ArgumentException("Periode code n'existe pas");
        }
    }


    public static DateTime SetDateTimeForChart(Timeframe tf)
    {
        DateTime dateTime;

        switch (tf)
        {
            case Timeframe.OneMinute:
                dateTime = DateTime.Now.AddMonths(-1);
                return dateTime;
            case Timeframe.FiveMinutes:
                dateTime = DateTime.Now.AddMonths(-1);
                return dateTime;
            case Timeframe.FifteenMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.ThirtyMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.OneHour:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.FourHour:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.Daily:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.Weekly:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.Monthly:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;


            default:
                throw new ArgumentException("Periode code n'existe pas");
        }
    }
}
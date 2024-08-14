namespace RobotAppLibrary.Api.Providers.Xtb.Code;

public class ERR_CODE(string code)
{
    public static readonly ERR_CODE INVALID_PRICE = new("BE001");
    public static readonly ERR_CODE INVALID_SL_TP = new("BE002");
    public static readonly ERR_CODE INVALID_VOLUME = new("BE003");
    public static readonly ERR_CODE LOGIN_DISABLED = new("BE004");
    public static readonly ERR_CODE LOGIN_NOT_FOUND = new("BE005");
    public static readonly ERR_CODE MARKET_IS_CLOSED = new("BE006");
    public static readonly ERR_CODE MISMATCHED_PARAMETERS = new("BE007");
    public static readonly ERR_CODE MODIFICATION_DENIED = new("BE008");
    public static readonly ERR_CODE NOT_ENOUGH_MONEY = new("BE009");
    public static readonly ERR_CODE QUOTES_ARE_OFF = new("BE010");
    public static readonly ERR_CODE OPPOSITE_POSITIONS_PROHIBITED = new("BE011");
    public static readonly ERR_CODE SHORT_POSITIONS_PROHIBITED = new("BE012");
    public static readonly ERR_CODE PRICE_HAS_CHANGED = new("BE013");
    public static readonly ERR_CODE REQUESTS_TOO_FREQUENT = new("BE014");
    public static readonly ERR_CODE REQUOTE = new("BE015");
    public static readonly ERR_CODE TOO_MANY_TRADE_REQUESTS = new("BE016");
    public static readonly ERR_CODE TRADE_IS_DISABLED = new("BE018");
    public static readonly ERR_CODE TRADE_TIMEOUT = new("BE019");
    public static readonly ERR_CODE SYMBOL_NOT_EXIST_FOR_ACCOUNT = new("BE094");
    public static readonly ERR_CODE CANNOT_TRADE_ON_SYMBOL = new("BE095");
    public static readonly ERR_CODE CANNOT_CLOSE_PENDING = new("BE096");
    public static readonly ERR_CODE CANNOT_CLOSE_ALREADY_CLOSED_ORDER = new("BE097");
    public static readonly ERR_CODE NO_SUCH_TRANSACTION = new("BE098");
    public static readonly ERR_CODE UNKNOWN_SYMBOL = new("BE101");
    public static readonly ERR_CODE UNKNOWN_TRANSACTION_TYPE = new("BE102");
    public static readonly ERR_CODE USER_NOT_LOGGED = new("BE103");
    public static readonly ERR_CODE COMMAND_NOT_EXIST = new("BE104");
    public static readonly ERR_CODE INTERNAL_ERROR = new("EX001");
    public static readonly ERR_CODE OTHER_ERROR = new("BE099");

    public virtual string StringValue
    {
        get
        {
            if (code == null) return "";

            return code;
        }
    }

    public static string getErrorDescription(string errorCode)
    {
        return new ERR_CODE(errorCode).getDescription();
    }

    public string getDescription()
    {
        if (code.Equals("")) return "";

        if (code.Equals(INVALID_PRICE.StringValue)) return "Invalid price.";

        if (code.Equals(INVALID_SL_TP.StringValue)) return "Invalid SL/TP.";

        if (code.Equals(INVALID_VOLUME.StringValue)) return "Invalid volume.";

        if (code.Equals(LOGIN_DISABLED.StringValue)) return "Login disabled.";

        if (code.Equals(LOGIN_NOT_FOUND.StringValue)) return "Login not found.";

        if (code.Equals(MARKET_IS_CLOSED.StringValue)) return "Market is closed!";

        if (code.Equals(MISMATCHED_PARAMETERS.StringValue)) return "Mismatched parameters.";

        if (code.Equals(MODIFICATION_DENIED.StringValue)) return "Modification denied.";

        if (code.Equals(NOT_ENOUGH_MONEY.StringValue)) return "Not enough money!";

        if (code.Equals(QUOTES_ARE_OFF.StringValue)) return "Quotes are off!";

        if (code.Equals(OPPOSITE_POSITIONS_PROHIBITED.StringValue)) return "Opposite positions prohibited!";

        if (code.Equals(SHORT_POSITIONS_PROHIBITED.StringValue)) return "Short positions prohibited!";

        if (code.Equals(PRICE_HAS_CHANGED.StringValue)) return "Price has changed..";

        if (code.Equals(REQUESTS_TOO_FREQUENT.StringValue)) return "Requests too frequent!";

        if (code.Equals(REQUOTE.StringValue)) return "Requote..";

        if (code.Equals(TOO_MANY_TRADE_REQUESTS.StringValue)) return "Too many trade requests!";

        if (code.Equals(TRADE_IS_DISABLED.StringValue)) return "Trade is disabled.";

        if (code.Equals(TRADE_TIMEOUT.StringValue)) return "Trade timeout..";

        if (code.Equals(SYMBOL_NOT_EXIST_FOR_ACCOUNT.StringValue)) return "Symbol not existent for account.";

        if (code.Equals(CANNOT_TRADE_ON_SYMBOL.StringValue)) return "Cannot trade on symbol.";

        if (code.Equals(CANNOT_CLOSE_PENDING.StringValue)) return "Cannot close pending.";

        if (code.Equals(CANNOT_CLOSE_ALREADY_CLOSED_ORDER.StringValue))
            return "Cannot close - order already closed.";

        if (code.Equals(NO_SUCH_TRANSACTION.StringValue)) return "No such transaction.";

        if (code.Equals(UNKNOWN_SYMBOL.StringValue)) return "Unknown symbol.";

        if (code.Equals(UNKNOWN_TRANSACTION_TYPE.StringValue)) return "Unknown transaction type.";

        if (code.Equals(USER_NOT_LOGGED.StringValue)) return "User not logged.";

        if (code.Equals(COMMAND_NOT_EXIST.StringValue)) return "Command does not exist.";

        if (code.Equals(INTERNAL_ERROR.StringValue)) return "Internal error.";

        if (code.Equals(OTHER_ERROR.StringValue)) return "Internal error (2).";

        return "Unknown error";
    }
}
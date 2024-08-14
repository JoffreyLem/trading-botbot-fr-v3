namespace RobotAppLibrary.Modeles;

public class TradeHourRecord
{
    public List<HoursRecordData> HoursRecords { get; set; } = new();

    public class HoursRecordData
    {
        public DayOfWeek Day { get; set; }

        public TimeSpan From { get; set; }

        public TimeSpan To { get; set; }
    }
}
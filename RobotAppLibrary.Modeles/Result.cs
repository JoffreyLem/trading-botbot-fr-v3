namespace RobotAppLibrary.Modeles;

public class GlobalResults
{
    public Result? Result { get; set; }
    public List<Position> Positions { get; set; } = new();
    public List<MonthlyResult> MonthlyResults { get; set; } = new();
}

public class MonthlyResult
{
    public List<Position> Positions = new();
    public DateTime Date { get; set; }
    public Result? Result { get; set; }
}

public class Result
{
    public decimal DrawndownMax { get; set; }
    public decimal Drawndown { get; set; }
    public decimal GainMax { get; set; }
    public decimal MoyenneNegative { get; set; }
    public decimal MoyennePositive { get; set; }
    public decimal MoyenneProfit { get; set; }
    public decimal PerteMax { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal ProfitNegatif { get; set; }
    public decimal ProfitPositif { get; set; }
    public decimal RatioMoyennePositifNegatif { get; set; }
    public double TauxReussite { get; set; }
    public double TotalPositionNegative { get; set; }
    public double TotalPositionPositive { get; set; }
    public double TotalPositions { get; set; }

    public Result DeepCopy()
    {
        return new Result
        {
            DrawndownMax = DrawndownMax,
            Drawndown = Drawndown,
            GainMax = GainMax,
            MoyenneNegative = MoyenneNegative,
            MoyennePositive = MoyennePositive,
            MoyenneProfit = MoyenneProfit,
            PerteMax = PerteMax,
            Profit = Profit,
            ProfitFactor = ProfitFactor,
            ProfitNegatif = ProfitNegatif,
            ProfitPositif = ProfitPositif,
            RatioMoyennePositifNegatif = RatioMoyennePositifNegatif,
            TauxReussite = TauxReussite,
            TotalPositionNegative = TotalPositionNegative,
            TotalPositionPositive = TotalPositionPositive,
            TotalPositions = TotalPositions
        };
    }
}
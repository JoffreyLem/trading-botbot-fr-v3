using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.TradingManager;

public interface IStrategyResult 
{
    GlobalResults GlobalResults { get; set; }
  
    int LooseStreak { get; set; }
    double ToleratedDrawnDown { get; set; }
    bool SecureControlPosition { get; set; }
    public event EventHandler<EventTreshold>? ResultTresholdEvent;

    public EventTreshold? Treshold { get; set; }
}

public class StrategyResult : IStrategyResult
{
    private readonly IApiProviderBase _apiProviderBase;
    
    private readonly string _positionReference;

    private AccountBalance _accountBalance = null!;

    private readonly ResultCalculator _resultCalculator = new ResultCalculator();
    public GlobalResults GlobalResults { get; set; } = new GlobalResults();

    public int LooseStreak { get; set; } = 10;
    public double ToleratedDrawnDown { get; set; } = 10;
    public bool SecureControlPosition { get; set; }
    
    public event EventHandler<EventTreshold>? ResultTresholdEvent;
    
    public EventTreshold? Treshold { get; set; }

    public StrategyResult(IApiProviderBase apiProviderBase, string positionReference)
    {
        _apiProviderBase = apiProviderBase;
        _positionReference = positionReference;
        Init();
    }

    private void Init()
    {
        _apiProviderBase.NewBalanceEvent += (_, balance) => _accountBalance = balance;
        _apiProviderBase.PositionClosedEvent += ApiProviderBaseOnPositionClosedEvent;
        _accountBalance = _apiProviderBase.GetBalanceAsync().Result;
        var listPositions = _apiProviderBase.GetAllPositionsByCommentAsync(_positionReference).Result;
        if (listPositions is { Count: > 0 })
        {
            GlobalResults.Positions.AddRange(listPositions);

            GlobalResults.Result = _resultCalculator.CalculateResults(GlobalResults.Positions);
            
            var groupedPositions = GlobalResults.Positions
                .GroupBy(p =>
                {
                    var date = p.DateClose.GetValueOrDefault();
                    return new DateTime(date.Year, date.Month, 1);
                })
                .Select(g => new 
                {
                    Date = g.Key, 
                    Positions = g.ToList()
                }).ToList();
            
            foreach (var groupedResult in groupedPositions.Select(positionGrouped => new MonthlyResult()
                     {
                         Date = positionGrouped.Date,
                         Result = _resultCalculator.CalculateResults(positionGrouped.Positions),
                         Positions = positionGrouped.Positions
                     }))
            {
                GlobalResults.MonthlyResults.Add(groupedResult);
            }

        }
    }

    private void ApiProviderBaseOnPositionClosedEvent(object? sender, Position e)
    {
        if (e.StrategyId == _positionReference)
        {
            UpdateResult(e);
            if (SecureControlPosition) TresholdCheck();
        }
    }

    private void UpdateResult(Position position)
    {
        
        GlobalResults.Positions.Add(position);
     
        GlobalResults.Result = _resultCalculator.CalculateResults(GlobalResults.Positions);
        
        var posDate = position.DateClose.GetValueOrDefault();
        var selectedGroupedResult = GlobalResults.MonthlyResults.FirstOrDefault(x => x.Date.Year == posDate.Year && x.Date.Month == posDate.Month);

        if (selectedGroupedResult is null)
        {
            var groupedResult = new MonthlyResult()
            {
                Date = new DateTime(posDate.Year, posDate.Month, 1),
                Result = _resultCalculator.CalculateResults([position])

            };
            
            groupedResult.Positions.Add(position);
            
            GlobalResults.MonthlyResults.Add(groupedResult);
        }
        else
        {
            selectedGroupedResult.Positions.Add(position);
            selectedGroupedResult.Result = _resultCalculator.CalculateResults(selectedGroupedResult.Positions);
        }
    }
    
    private void TresholdCheck()
    {
        CheckDrawnDownTreshold();
        CheckLooseStreakTreshold();
        CheckProfitFactorTreshold();
    }
    

    private void CheckDrawnDownTreshold()
    {
        var drawndown = GlobalResults.Result?.Drawndown;
        var drawDownTheorique = _accountBalance.Balance * (ToleratedDrawnDown / 100);

        if (drawndown > 0 && drawndown >= (decimal)drawDownTheorique)
        {
         
            ResultTresholdEvent?.Invoke(this, EventTreshold.Drowdown);
        }
            
    }

    private void CheckLooseStreakTreshold()
    {
        var selected = GlobalResults.Positions.TakeLast(LooseStreak).ToList();

        if (selected.Count == LooseStreak && selected.TrueForAll(x => x.Profit < 0))
        {
            Treshold = EventTreshold.LooseStreak;
            ResultTresholdEvent?.Invoke(this, EventTreshold.LooseStreak);
        }
            
    }

    private void CheckProfitFactorTreshold()
    {
        var profitfactor = GlobalResults.Result?.ProfitFactor;
        if (profitfactor is > 0 and <= 1)
        {
            Treshold = EventTreshold.Profitfactor;
            ResultTresholdEvent?.Invoke(this, EventTreshold.Profitfactor);
        }
    }


}
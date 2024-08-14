using System.ComponentModel;
using System.Runtime.CompilerServices;
using robot_project_v3.Server.Dto.Enum;

namespace robot_project_v3.Server.Dto.Response;

public class PositionDto : INotifyPropertyChanged
{
    private decimal? _profit;
    private decimal? _stopLoss;
    private decimal? _takeProfit;

    public string? Id { get; set; }
    public string? Symbol { get; set; }
    public string? TypePosition { get; set; }
    public double? Spread { get; set; }

    public decimal? Profit
    {
        get => _profit;
        set => SetField(ref _profit, value);
    }

    public decimal? OpenPrice { get; set; }
    public DateTime DateOpen { get; set; }
    public decimal? ClosePrice { get; set; }
    public DateTime? DateClose { get; set; }
    public string? ReasonClosed { get; set; }

    public decimal? StopLoss
    {
        get => _stopLoss;
        set => SetField(ref _stopLoss, value);
    }

    public decimal? TakeProfit
    {
        get => _takeProfit;
        set => SetField(ref _takeProfit, value);
    }

    public double? Volume { get; set; }
    public decimal? Pips { get; set; }
    public string? StatusPosition { get; set; }
    public string Comment { get; set; }

    public PositionStateEnum PositionState { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
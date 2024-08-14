using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using robot_project_v3.Server.Dto.Response;

namespace robot_project_v3.Server.Hubs;

public interface IHubInfoClient
{
    Task ReceiveCandle(CandleDto candle);
    Task ReceiveTick(TickDto tick);
    Task ReceivePosition(PositionDto positionDto);
    Task ReceiveBalance(AccountBalanceDto accountBalanceDto);
}

[Authorize]
public class HubInfoClient : Hub<IHubInfoClient>
{
    public async Task SendCandle(CandleDto candle)
    {
        await Clients.All.ReceiveCandle(candle);
    }

    public async Task SendTick(TickDto tick)
    {
        await Clients.All.ReceiveTick(tick);
    }

    public async Task SendPosition(PositionDto positionDto)
    {
        await Clients.All.ReceivePosition(positionDto);
    }

    public async Task SendBalance(AccountBalanceDto accountBalanceDto)
    {
        await Clients.All.ReceiveBalance(accountBalanceDto);
    }
}
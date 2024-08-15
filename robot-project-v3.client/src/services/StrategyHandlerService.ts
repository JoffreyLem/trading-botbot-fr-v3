import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";
import {
  BackTestRequestDto,
  GlobalResultsDto,
  PositionDto,
  StrategyInfoDto,
  StrategyInitDto,
} from "../modeles/dto.ts";

const basePath = "/api/Strategy";

export class StrategyService {
  static async initStrategy(strategyInitDto: StrategyInitDto): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse(`${basePath}/init`, {
      method: "POST",
      body: JSON.stringify(strategyInitDto),
    });
  }

  static async getAllStrategy(): Promise<StrategyInitDto[]> {
    return await ApiMiddlewareService.callApi<StrategyInitDto[]>(
      `${basePath}/all`,
      {
        method: "GET",
      },
    );
  }

  static async closeStrategy(id: string): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse(
      `${basePath}/close/${id}`,
      {
        method: "POST",
      },
    );
  }

  static async getStrategyInfo(id: string): Promise<StrategyInfoDto> {
    return await ApiMiddlewareService.callApi<StrategyInfoDto>(
      `${basePath}/${id}/info`,
      {
        method: "GET",
      },
    );
  }

  static async getResult(id: string): Promise<GlobalResultsDto> {
    return await ApiMiddlewareService.callApi<GlobalResultsDto>(
      `${basePath}/${id}/result`,
      {
        method: "GET",
      },
    );
  }

  static async setCanRun(id: string, value: boolean): Promise<void> {
    const url = new URL(`${basePath}/${id}/canRun`, window.location.origin);
    url.searchParams.append("value", value.toString());

    await ApiMiddlewareService.callApiWithoutResponse(url.toString(), {
      method: "POST",
    });
  }

  static async getOpenedPositions(id: string): Promise<PositionDto[]> {
    return await ApiMiddlewareService.callApi<PositionDto[]>(
      `${basePath}/${id}/positions/opened`,
      {
        method: "GET",
      },
    );
  }

  //TODO : Revoir les backtests
  static async runBackTest(
    id: string,
    backTestDto: BackTestRequestDto,
  ): Promise<BackTestRequestDto> {
    return await ApiMiddlewareService.callApi<BackTestRequestDto>(
      `${basePath}/runBacktest/${id}`,
      {
        method: "POST",
        body: JSON.stringify(backTestDto),
      },
    );
  }

  static async getBacktestResult(id: string): Promise<BackTestRequestDto> {
    return await ApiMiddlewareService.callApi<BackTestRequestDto>(
      `${basePath}/${id}/resultBacktest`,
      {
        method: "GET",
      },
    );
  }
}

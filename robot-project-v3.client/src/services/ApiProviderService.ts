import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";
import { ConnectDto, SymbolInfoDto } from "../modeles/dto.ts";
import { ApiResponse } from "../modeles/ApiResponse.ts";

const basePath = "/api/ApiProvider";

export class ApiProviderService {
  static async connect(connectDto: ConnectDto): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse(`${basePath}/connect`, {
      method: "POST",
      body: JSON.stringify({
        user: connectDto.user,
        pwd: connectDto.pwd,
        handlerEnum: connectDto.handlerEnum,
      }),
    });
  }

  static async disconnect(): Promise<void> {
    await ApiMiddlewareService.callApiWithoutResponse(
      `${basePath}/disconnect`,
      {
        method: "POST",
      },
    );
  }

  static async isConnected(): Promise<boolean> {
    const response = await ApiMiddlewareService.callApi<ApiResponse<boolean>>(
      `${basePath}/isConnected`,
      { method: "GET" },
    );
    return response.data;
  }

  static async getTypeHandler(): Promise<string> {
    const response = await ApiMiddlewareService.callApi<ApiResponse<string>>(
      `${basePath}/typeHandler`,
      { method: "GET" },
    );

    return response.data;
  }

  static async getListHandler(): Promise<string[]> {
    return await ApiMiddlewareService.callApi<string[]>(
      `${basePath}/listHandlers`,
      { method: "GET" },
    );
  }

  static async getAllSymbol(): Promise<SymbolInfoDto[]> {
    return await ApiMiddlewareService.callApi<SymbolInfoDto[]>(
      `${basePath}/allSymbols`,
      { method: "GET" },
    );
  }
}

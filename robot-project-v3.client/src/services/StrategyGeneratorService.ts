import { ApiMiddlewareService } from "./ApiMiddlewareService.ts";
import {
  StrategyCompilationResponseDto,
  StrategyFileDto,
} from "../modeles/dto.ts";

const basePath = "/api/StrategyBuilder";

export class StrategyGeneratorService {
  static async createNewStrategy(
    file: File,
  ): Promise<StrategyCompilationResponseDto> {
    const formData = new FormData();
    formData.append("file", file);

    return await ApiMiddlewareService.callApi<StrategyCompilationResponseDto>(
      `${basePath}`,
      {
        method: "POST",
        body: formData,
      },
    );
  }

  static async getAllStrategyFiles(): Promise<StrategyFileDto[]> {
    return await ApiMiddlewareService.callApi<StrategyFileDto[]>(
      `${basePath}/GetAll`,
      {
        method: "GET",
      },
    );
  }

  static async getStrategyFile(id: number): Promise<StrategyFileDto> {
    return await ApiMiddlewareService.callApi<StrategyFileDto>(
      `${basePath}/${id}`,
      {
        method: "GET",
      },
    );
  }

  static async deleteStrategyFile(id: number): Promise<void> {
    return await ApiMiddlewareService.callApiWithoutResponse(
      `${basePath}/${id}`,
      {
        method: "DELETE",
      },
    );
  }

  static async updateStrategyFile(
    id: number,
    file: File,
  ): Promise<StrategyCompilationResponseDto> {
    const formData = new FormData();
    formData.append("file", file);
    return await ApiMiddlewareService.callApi<StrategyCompilationResponseDto>(
      `${basePath}/${id}`,
      {
        method: "PUT",
        body: formData,
      },
    );
  }
}

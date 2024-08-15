import { MsalAuthService } from "./MsalAuthService.ts";
import { ApiResponseError } from "../modeles/ApiResponseError.ts";
import { ApiException } from "../exceptions/ApiException.ts";

export class ApiMiddlewareService {
  static async callApi<T>(url: string, options: RequestInit): Promise<T> {
    const response = await this.performRequest(url, options);
    return (await response.json()) as T;
  }

  static async callApiWithoutResponse(
    url: string,
    options: RequestInit,
  ): Promise<void> {
    await this.performRequest(url, options);
  }

  private static async performRequest(
    url: string,
    options: RequestInit,
  ): Promise<Response> {
    try {
      const headers = await this.prepareHeaders(options);
      const fetchOptions = { ...options, headers };
      const response = await fetch(url, fetchOptions);

      await this.handleResponseErrors(response);

      return response;
    } catch (error) {
      this.handleError(error);
    }
  }

  private static async prepareHeaders(options: RequestInit): Promise<Headers> {
    const headers = new Headers(options.headers || {});
    const accessToken = await MsalAuthService.getAuthToken();

    headers.set("Authorization", `Bearer ${accessToken}`);

    if (
      options.method === "POST" &&
      !(options.body instanceof FormData) &&
      !headers.has("Content-Type")
    ) {
      headers.set("Content-Type", "application/json");
    }

    return headers;
  }

  private static async handleResponseErrors(response: Response): Promise<void> {
    if (!response.ok) {
      if (response.headers.get("Content-Type")?.includes("application/json")) {
        const errorBody: ApiResponseError = await response.json();
        throw new ApiException(errorBody.Error);
      } else {
        throw new Error(`HTTP error: ${response.status}`);
      }
    }
  }

  private static handleError(error: unknown): never {
    if (error instanceof ApiException) {
      throw error;
    } else {
      console.error("Erreur inattendue :", error);
      throw new Error("Une erreur inattendue est survenue.");
    }
  }
}

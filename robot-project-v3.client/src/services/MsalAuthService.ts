// MsalAuthService.ts
import {
  InteractionRequiredAuthError,
  IPublicClientApplication,
} from "@azure/msal-browser";

export class MsalAuthService {
  static msalInstance: IPublicClientApplication;

  static initialize(msalInstance: IPublicClientApplication) {
    this.msalInstance = msalInstance;
  }

  public static async getAuthToken(): Promise<string> {
    const request = {
      scopes: ["api://21543424-93d7-4cf1-a776-383de1100a79/access_as_user"],
      account: this.msalInstance.getAllAccounts()[0],
    };
    try {
      const response = await this.msalInstance.acquireTokenSilent(request);
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        // Fallback to interactive method if silent fails
        const interactiveResponse =
          await this.msalInstance.acquireTokenPopup(request);
        return interactiveResponse.accessToken;
      } else {
        // Handle other errors
        throw error;
      }
    }
  }
}

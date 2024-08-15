import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./index.css";
import { BrowserRouter } from "react-router-dom";
import { Configuration, PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { MsalAuthService } from "./services/MsalAuthService.ts";

const configuration: Configuration = {
  auth: {
    clientId: "4345fd32-2545-442c-8c0b-99e073b79940",
    authority:
      "https://login.microsoftonline.com/2f5c37b4-fddf-49dd-93a4-8069a154b896",
    redirectUri: window.location.origin + "/signin-oidc",
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: true,
  },
};

const pca =
  await PublicClientApplication.createPublicClientApplication(configuration);
MsalAuthService.initialize(pca);
const root = ReactDOM.createRoot(document.getElementById("root")!);
root.render(
  <React.StrictMode>
    <MsalProvider instance={pca}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </MsalProvider>
  </React.StrictMode>,
);

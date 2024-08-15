import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
  useMsal,
} from "@azure/msal-react";
import React, { useEffect } from "react";
import { Route, Routes } from "react-router-dom";
import Layout from "./Layout.tsx";

import NotFound from "./pages/NotFound.tsx";
import { AppProviders } from "./contexts/AppProviders.tsx";
import Home from "./pages/Home/Home.tsx";
import StrategyDetails from "./pages/StrategyDetails/StrategyDetails.tsx";
import StrategyCreator from "./pages/StrategyCreator/StrategyCreator.tsx";

function App() {
  const { instance, accounts } = useMsal();

  useEffect(() => {
    if (accounts.length === 0) {
      instance.loginPopup().catch((e) => {
        console.error(e);
      });
    }
  }, [instance, accounts]);

  return (
    <React.Fragment>
      <AuthenticatedTemplate>
        <AppProviders>
          <Routes>
            <Route path="/" element={<Layout />}>
              <Route index element={<Home />} />
              <Route path="/strategyCreator" element={<StrategyCreator />} />
              <Route
                path="/strategy/:strategyId"
                element={<StrategyDetails />}
              />
            </Route>
            <Route path="*" element={<NotFound />} />
          </Routes>
        </AppProviders>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <p>Loading...</p>
      </UnauthenticatedTemplate>
    </React.Fragment>
  );
}

export default App;

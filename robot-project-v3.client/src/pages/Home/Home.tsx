import React, { useContext, useEffect, useState } from "react";
import { AuthProviderContext } from "../../contexts/AuthApiProviderContext.tsx";
import Spinner from "../../components/Spinner.tsx";
import { StrategyProvider } from "../../contexts/StrategyProvider.tsx";
import StrategyForm from "./StrategyForm.tsx";
import StrategyList from "./StrategyList.tsx";

const Home: React.FC = () => {
  const [isLoading, setIsLoading] = useState(false);
  const authContext = useContext(AuthProviderContext);

  useEffect(() => {
    setIsLoading(true);
    if (!authContext) {
      return;
    }
    setIsLoading(false);
  }, [authContext]);

  if (isLoading) {
    return <Spinner />;
  }

  if (!authContext?.connected) {
    return (
      <div
        className="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-700 p-4"
        role="alert"
      >
        <p className="font-bold">Attention</p>
        <p>Connexion à une API nécessaire</p>
      </div>
    );
  }

  return (
    <StrategyProvider>
      <div>
        <StrategyForm />
      </div>
      <div>
        <StrategyList />
      </div>
    </StrategyProvider>
  );
};

export default Home;

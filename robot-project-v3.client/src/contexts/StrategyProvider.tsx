import React, { createContext, ReactNode, useState } from "react";

interface StrategyContextProps {
  refreshList: boolean;
  handleRefresh: () => void;
}

export const StrategyContext = createContext<StrategyContextProps>(
  {} as StrategyContextProps,
);

interface StrategyProviderProps {
  children: ReactNode;
}

export const StrategyProvider: React.FC<StrategyProviderProps> = ({
  children,
}) => {
  const [refreshList, setRefreshList] = useState(false);

  const handleRefresh = () => {
    setRefreshList((prev) => !prev);
  };

  return (
    <StrategyContext.Provider value={{ refreshList, handleRefresh }}>
      {children}
    </StrategyContext.Provider>
  );
};

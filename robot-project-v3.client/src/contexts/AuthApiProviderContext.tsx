import React, { createContext, ReactNode, useEffect, useState } from "react";
import { ApiProviderService } from "../services/ApiProviderService.ts";
import { ConnectDto } from "../modeles/dto.ts";
import { useErrorHandler } from "../hooks/UseErrorHandler.tsx";

interface AuthContextType {
  connected: boolean;
  login: (connectDto: ConnectDto) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthProviderContext = createContext<AuthContextType | undefined>(
  undefined,
);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [connected, setConnected] = useState<boolean>(false);
  const handleError = useErrorHandler();

  useEffect(() => {
    ApiProviderService.isConnected()
      .then((response) => setConnected(response))
      .catch(handleError);
  }, []);
  const login = async (connectDto: ConnectDto) => {
    return ApiProviderService.connect(connectDto)
      .then(() => {
        setConnected(true);
      })
      .catch(handleError);
  };

  const logout = async () => {
    return ApiProviderService.disconnect()
      .then(() => setConnected(false))
      .catch(handleError);
  };
  return (
    <AuthProviderContext.Provider value={{ connected, login, logout }}>
      {children}
    </AuthProviderContext.Provider>
  );
};

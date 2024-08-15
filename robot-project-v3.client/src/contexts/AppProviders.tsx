import React, { ReactNode } from "react";
import { NotificationProvider } from "./NotificationContext.tsx";
import { AuthProvider } from "./AuthApiProviderContext.tsx";

export const AppProviders: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  return (
    <NotificationProvider>
      <AuthProvider>{children}</AuthProvider>
    </NotificationProvider>
  );
};

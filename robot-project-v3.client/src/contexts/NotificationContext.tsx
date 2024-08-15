import { createContext, ReactNode, useState } from "react";
import NotificationPopup from "../components/NotificationPopup.tsx";
import { Notification } from "../modeles/Notification.ts";

interface NotificationContextType {
  notifications: Notification[];
  addNotification: (
    message: string,
    type: "error" | "info",
    info?: string,
  ) => void;
  removeNotification: (id: string) => void;
}

export const NotificationContext = createContext<
  NotificationContextType | undefined
>(undefined);

export const NotificationProvider = ({ children }: { children: ReactNode }) => {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const addNotification = (
    message: string,
    type: "error" | "info",
    info?: string,
  ) => {
    const id = new Date().getTime().toString();
    setNotifications((prevNotifications) => [
      ...prevNotifications,
      { id, message, type, info },
    ]);
  };

  const removeNotification = (id: string) => {
    setNotifications((prevNotifications) =>
      prevNotifications.filter((notification) => notification.id !== id),
    );
  };

  return (
    <NotificationContext.Provider
      value={{ notifications, addNotification, removeNotification }}
    >
      <NotificationPopup />
      {children}
    </NotificationContext.Provider>
  );
};

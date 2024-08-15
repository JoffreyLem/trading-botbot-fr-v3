import React, { useEffect } from "react";
import { useNotification } from "../hooks/UseNotification.tsx";
import { Notification } from "../modeles/Notification.ts";

const NotificationPopup: React.FC = () => {
  const { notifications, removeNotification } = useNotification();

  useEffect(() => {
    const timers = notifications.map((notification: Notification) =>
      setTimeout(() => {
        removeNotification(notification.id);
      }, 10000),
    );

    return () => timers.forEach((timer) => clearTimeout(timer));
  }, [notifications, removeNotification]);

  if (notifications.length === 0) return null;

  return (
    <div className="fixed top-5 right-5 z-[1000] w-72 max-h-[calc(100%-40px)] overflow-y-auto">
      {notifications.map((notification: Notification) => (
        <div
          key={notification.id}
          className={`relative mb-4 p-4 rounded shadow-lg ${
            notification.type === "error"
              ? "bg-red-100 text-red-700"
              : "bg-blue-100 text-blue-700"
          }`}
          role="alert"
        >
          <strong className="font-bold">
            {notification.type.toUpperCase()}
          </strong>
          : {notification.message}
          {notification.info && <div>Info: {notification.info}</div>}
          <button
            onClick={() => removeNotification(notification.id)}
            className="absolute top-2 right-2 text-black text-lg leading-none focus:outline-none"
            aria-label="Close"
          >
            &times;
          </button>
        </div>
      ))}
    </div>
  );
};

export default NotificationPopup;

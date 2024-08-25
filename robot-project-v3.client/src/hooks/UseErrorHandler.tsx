// ErrorHandler.tsx

import { useNotification } from "./UseNotification.tsx";
import { ApiException } from "../exceptions/ApiException.ts";

export const useErrorHandler = () => {
  const { addNotification } = useNotification();

  return (error: Error | ApiException) => {
    addNotification(error.message, "error");
  };
};

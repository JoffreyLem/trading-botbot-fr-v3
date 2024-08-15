import React, { useContext, useEffect, useState } from "react";
import { ConnectDto } from "../modeles/dto.ts";
import { useErrorHandler } from "../hooks/UseErrorHandler.tsx";
import { ApiProviderService } from "../services/ApiProviderService.ts";
import Spinner from "./Spinner.tsx";
import { AuthProviderContext } from "../contexts/AuthApiProviderContext.tsx";
import { FormProps } from "./Modal.tsx";

const LoginForm: React.FC<FormProps> = ({ onClose }) => {
  const authContext = useContext(AuthProviderContext);

  const [apiHandlerList, setApihandlerList] = useState<string[]>([]);

  const [defaultApiHandlerSelected, setDefaultApiHandlerselected] =
    useState<string>();
  const [connectDto, setConnectDto] = useState<ConnectDto>({});
  const handleError = useErrorHandler();
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (!authContext) {
      return;
    }
    setIsLoading(true);
    const fetchApiHandlerList = ApiProviderService.getListHandler()
      .then((r) => setApihandlerList(r))
      .catch(handleError);

    const fetchTypeHandler = authContext.connected
      ? ApiProviderService.getTypeHandler()
          .then((response) => setDefaultApiHandlerselected(response))
          .catch(handleError)
      : Promise.resolve();

    Promise.all([fetchApiHandlerList, fetchTypeHandler]).finally(() =>
      setIsLoading(false),
    );
  }, [authContext]);

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsLoading(true);
    authContext
      ?.login(connectDto)
      .then(() => onClose())
      .finally(() => setIsLoading(false));
  };

  const handleSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setConnectDto((prevConnectDto) => ({
      ...prevConnectDto,
      handlerEnum: e.target.value,
    }));
    setDefaultApiHandlerselected(e.target.value);
  };

  const handleDisconnect = () => {
    setIsLoading(true);
    authContext
      ?.logout()
      .then(() => onClose())
      .finally(() => setIsLoading(false));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setConnectDto((prevConnectDto) => ({
      ...prevConnectDto,
      [e.target.name]: e.target.value,
    }));
  };

  if (isLoading) {
    return <Spinner />;
  }

  return (
    <div className="flex flex-col">
      <div className="w-full">
        <label
          htmlFor="Provider"
          className="block text-sm font-medium text-gray-700"
        >
          Provider
        </label>
        <select
          value={defaultApiHandlerSelected}
          onChange={handleSelect}
          disabled={authContext?.connected}
          className="block w-full p-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Sélectionnez une option</option>
          {apiHandlerList.map((option, index) => (
            <option key={index} value={option}>
              {option}
            </option>
          ))}
        </select>
      </div>
      {authContext?.connected ? (
        <div className="mt-3">
          <button
            className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500"
            onClick={handleDisconnect}
          >
            Disconnect
          </button>
        </div>
      ) : (
        <div className="w-full md:w-1/3">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <label
                htmlFor="User"
                className="block text-sm font-medium text-gray-700"
              >
                User
              </label>
              <input
                type="text"
                className="block p-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                id="user"
                name="user"
                value={connectDto?.user}
                onChange={handleInputChange}
              />
            </div>
            <div className="space-y-2">
              <label
                htmlFor="password"
                className="block text-sm font-medium text-gray-700"
              >
                Password
              </label>
              <input
                type="password"
                className="block p-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                id="last-name"
                name="pwd"
                value={connectDto?.pwd}
                onChange={handleInputChange}
              />
            </div>
            <div className="mt-3">
              <button
                type="submit"
                className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                Submit
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};

export default LoginForm;

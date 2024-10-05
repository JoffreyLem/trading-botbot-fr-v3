import React, { useContext, useEffect, useState } from "react";

import { StrategyGeneratorService } from "../../services/StrategyGeneratorService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";

import { StrategyService } from "../../services/StrategyHandlerService.ts";
import {
  StrategyFileDto,
  StrategyInitDto, SymbolInfoDto,
} from "../../modeles/dto.ts";
import { ApiProviderService } from "../../services/ApiProviderService.ts";
import Spinner from "../../components/Spinner.tsx";
import AutocompleteInput from "../../components/AutoCompleteInput.tsx";
import { StrategyContext } from "../../contexts/StrategyProvider.tsx";

const StrategyForm: React.FC = () => {
  const [strategyInitDto, setStrategyInitDto] = useState<StrategyInitDto>({
    strategyFileId: "",
    symbol: "",
  });

  const [allStrategy, setAllStrategy] = useState<StrategyFileDto[]>([]);
  const [allSymbol, setAllSymbol] = useState<SymbolInfoDto[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const { handleRefresh } = useContext(StrategyContext);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);

    Promise.all([
      StrategyGeneratorService.getAllStrategyFiles()
        .then((response) => setAllStrategy(response))
        .catch(handleError),
      ApiProviderService.getAllSymbol()
        .then((response) => setAllSymbol(response))
        .catch(handleError),
    ]).finally(() => {
      setIsLoading(false);
    });
  }, []);

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    StrategyService.initStrategy(strategyInitDto)
      .catch(handleError)
      .finally(() => {
        setIsLoading(false);
        handleRefresh();
      });
  };

  const handleChange = (
    event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const { name, value } = event.target;

    setStrategyInitDto((prevState) => ({
      ...prevState,
      [name]: value,
    }));
  };

  const handleValueChange = (value: string) => {
    setStrategyInitDto((prevState) => ({
      ...prevState,
      symbol: value,
    }));
  };

  const symbols: string[] = allSymbol
    .map((symbolInfo) => symbolInfo.symbol)
    .filter((symbol) => symbol !== undefined) as string[];

  if (isLoading) {
    return <Spinner />;
  }

  return (
    <div className="flex justify-start">
      <div className="w-1/4 pr-2.5">
        <h2 className="text-2xl font-semibold mb-6">Ajouter une strategy</h2>
        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label
              htmlFor="strategyName"
              className="block mb-2 font-medium text-gray-700"
            >
              Strategy
            </label>
            <select
              id="strategyFileId"
              name="strategyFileId"
              onChange={handleChange}
              value={strategyInitDto.strategyFileId}
              className="w-full p-2 border border-gray-300 rounded min-w-[200px]"
              aria-label="Select Strategy"
            >
              <option value="">Sélectionnez une option</option>
              {allStrategy.map((option, index) => (
                <option key={index} value={option.id}>
                  {option.name}
                </option>
              ))}
            </select>
          </div>

          <div className="mb-4">
            <label
              htmlFor="symbol"
              className="block mb-2 font-medium text-gray-700"
            >
              Symbol:
            </label>
            <AutocompleteInput
              suggestions={symbols}
              onValueChange={handleValueChange}
            />
          </div>
          <div className="mb-4">
            <button
              type="submit"
              className="w-full py-2 px-4 bg-blue-500 text-white rounded hover:bg-blue-600 min-w-[200px]"
            >
              Submit
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default StrategyForm;

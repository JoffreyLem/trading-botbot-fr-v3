import React, { useEffect, useState } from "react";

import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { StrategyInfoDto } from "../../modeles/dto.ts";
import { StrategyFormProps } from "./StrategyFormProps.tsx";
import Spinner from "../../components/Spinner.tsx";

const StrategyDataInfo: React.FC<StrategyFormProps> = ({ strategyInfo }) => {
  const [formData, setFormData] = useState<StrategyInfoDto | undefined>(
    strategyInfo,
  );
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    setFormData(strategyInfo);
  }, [strategyInfo]);

  const handleCanRunChange = () => {
    setIsLoading(true);
    // @ts-ignore
    StrategyService.setCanRun(strategyInfo.id, !formData?.canRun)
      .then(() => {
        setFormData({
          ...formData,
          canRun: !formData?.canRun,
        } as StrategyInfoDto);
      })
      .catch((err) => setError(err.message))
      .finally(() => {
        setIsLoading(false);
      });
  };

  if (error) {
    return <div className="text-red-500">Erreur: {error}</div>;
  }

  return (
    <form className="space-y-4">
      <div className="flex flex-col">
        <label htmlFor="symbol" className="mb-1 font-medium text-gray-700">
          Symbol
        </label>
        <input
          type="text"
          className="border border-gray-300 p-2 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          id="symbol"
          value={formData?.symbol}
          readOnly
        />
      </div>

      <div className="flex flex-col">
        <label
          htmlFor="strategyName"
          className="mb-1 font-medium text-gray-700"
        >
          Strategy Name
        </label>
        <input
          type="text"
          className="border border-gray-300 p-2 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          id="strategyName"
          value={formData?.strategyName}
          readOnly
        />
      </div>

      <div className="flex items-center space-x-2">
        <label htmlFor="canRun" className="font-medium text-gray-700">
          Can Run
        </label>
        {isLoading && <Spinner />}
        <input
          type="checkbox"
          className="form-checkbox h-5 w-5 text-blue-600"
          id="canRun"
          checked={formData?.canRun}
          onChange={handleCanRunChange}
        />
      </div>
    </form>
  );
};

export default StrategyDataInfo;

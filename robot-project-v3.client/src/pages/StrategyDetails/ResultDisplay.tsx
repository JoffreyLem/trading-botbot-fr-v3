import React, { useEffect, useState } from "react";

import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { ResultDto } from "../../modeles/dto.ts";
import Result from "../../components/Result.tsx";
import Spinner from "../../components/Spinner.tsx";

const ResultDisplay: React.FC<{ strategyId: string }> = ({ strategyId }) => {
  const [result, setResult] = useState<ResultDto>();

  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);

    StrategyService.getResult(strategyId)
      .then((data) => setResult(data.result))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [strategyId]);

  if (isLoading) {
    return <Spinner />;
  }

  return <Result result={result} />;
};

export default ResultDisplay;

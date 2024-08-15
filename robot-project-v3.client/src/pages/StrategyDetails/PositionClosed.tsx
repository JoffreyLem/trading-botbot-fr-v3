import React, { useEffect, useState } from "react";

import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { PositionDto } from "../../modeles/dto.ts";
import Spinner from "../../components/Spinner.tsx";
import PositionComponent from "../../components/PositionComponent.tsx";

const PositionClosed: React.FC<{
  strategyId: string;
}> = ({ strategyId }) => {
  const [positions, setPositions] = useState<PositionDto[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();
  useEffect(() => {
    setIsLoading(true);
    StrategyService.getResult(strategyId)
      .then((response) => setPositions(response.positions!))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [strategyId]);

  if (isLoading) {
    return <Spinner />;
  }

  return <PositionComponent positions={positions} positionClosed={true} />;
};

export default PositionClosed;

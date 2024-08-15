import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { StrategyInfoDto } from "../../modeles/dto.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import Spinner from "../../components/Spinner.tsx";
import DynamicTabs from "../../components/DynamicTabs.tsx";
import StrategyDataInfo from "./StrategyDataInfo.tsx";
import GraphComponent from "./GraphComponent.tsx";
import PositionOpened from "./PositionOpened.tsx";
import PositionClosed from "./PositionClosed.tsx";
import ResultDisplay from "./ResultDisplay.tsx";
import Backtest from "./Backtest.tsx";

const StrategyDetails: React.FC = () => {
  const { strategyId } = useParams<string>();
  const [strategyInfo, setStrategyInfo] = useState<StrategyInfoDto>();
  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();
  const navigate = useNavigate();

  useEffect(() => {
    setIsLoading(true);
    if (strategyId != null) {
      StrategyService.getStrategyInfo(strategyId)
        .then((rsp) => setStrategyInfo(rsp))
        .catch((e) => {
          handleError(e);
          navigate("/");
        })
        .finally(() => {
          setIsLoading(false);
        });
    }
  }, [strategyId]);

  const deleteStrategy = () => {
    setIsLoading(true);
    if (strategyId != null) {
      StrategyService.closeStrategy(strategyId)
        .then(() => navigate("/"))
        .catch(handleError)
        .finally(() => {
          setIsLoading(false);
        });
    }
  };

  if (isLoading) {
    return <Spinner />;
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <button
          className="bg-gray-500 hover:bg-gray-700 text-white font-bold py-2 px-4 rounded"
          onClick={() => navigate("/")}
        >
          Retour
        </button>
        <button
          className="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded"
          onClick={deleteStrategy}
        >
          Supprimer
        </button>
      </div>
      <div>
        <StrategyDataInfo strategyInfo={strategyInfo} />
      </div>
      <div>
        <GraphComponent strategyInfo={strategyInfo} />
      </div>
      <div>
        <DynamicTabs>
          <DynamicTabs.TabPanel title="Positions ouvertes">
            <PositionOpened strategyId={strategyId!} />
          </DynamicTabs.TabPanel>
          <DynamicTabs.TabPanel title="Positions closes">
            <PositionClosed strategyId={strategyId!} />
          </DynamicTabs.TabPanel>
          <DynamicTabs.TabPanel title="Result">
            <ResultDisplay strategyId={strategyId!} />
          </DynamicTabs.TabPanel>
          <DynamicTabs.TabPanel title="Backtest">
            <Backtest strategyId={strategyId!} />
          </DynamicTabs.TabPanel>
        </DynamicTabs>
      </div>
    </div>
  );
};

export default StrategyDetails;

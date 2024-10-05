import React, { useContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { StrategyContext } from "../../contexts/StrategyProvider.tsx";
import Spinner from "../../components/Spinner.tsx";
import {StrategyInfoDto} from "../../modeles/dto.ts";

const StrategyList: React.FC = () => {
  const [allStrategy, setAllStrategy] = useState<StrategyInfoDto[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { refreshList } = useContext(StrategyContext);
  const handleError = useErrorHandler();

  const handleRowClick = (strategyId: string) => {
    navigate(`/strategy/${strategyId}`);
  };

  useEffect(() => {
    setIsLoading(true);

    const getAllStrategy = StrategyService.getAllStrategy()
      .then((response) => setAllStrategy(response))
      .catch(handleError);

    Promise.all([getAllStrategy]).finally(() => {
      setIsLoading(false);
    });
  }, [refreshList]);

  const handleDelete = (
    event: React.MouseEvent<HTMLButtonElement>,
    strategyId: string,
  ) => {
    setIsLoading(true);
    event.stopPropagation();
    StrategyService.closeStrategy(strategyId)
      .then(() => {
        setAllStrategy((prevStrategies) =>
          prevStrategies.filter((strategy) => strategy.id !== strategyId),
        );
      })
      .catch(handleError)
      .finally(() => {
        setIsLoading(false);
      });
  };

  if (isLoading) {
    return <Spinner />;
  }

  return (
    <div className="overflow-x-auto">
      <h2 className="text-2xl font-semibold mb-6">Liste des strategies</h2>
      <table className="min-w-full bg-white border border-gray-300">
        <thead className="bg-gray-200">
          <tr>
            <th className="py-2 px-4 border-b border-gray-300 text-left">
              Strategy Name
            </th>
            <th className="py-2 px-4 border-b border-gray-300 text-left">
              Symbol
            </th>
            <th className="py-2 px-4 border-b border-gray-300 text-left">
              Supprimer
            </th>
          </tr>
        </thead>
        <tbody>
          {allStrategy.map((strategy) => (
            <tr
              key={strategy.id}
              className="hover:bg-gray-100 cursor-pointer"
              onClick={() => handleRowClick(strategy.id!)}
            >
              <td className="py-2 px-4 border-b border-gray-300">
                {strategy.strategyName}
              </td>
              <td className="py-2 px-4 border-b border-gray-300">
                {strategy.symbol}
              </td>
              <td className="py-2 px-4 border-b border-gray-300">
                <button
                  onClick={(event) => handleDelete(event, strategy.id!)}
                  className="text-red-500 hover:text-red-700 focus:outline-none"
                >
                  Supprimer
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default StrategyList;

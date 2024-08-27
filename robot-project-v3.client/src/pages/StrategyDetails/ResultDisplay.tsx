import React, { useEffect, useState } from "react";
import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { GlobalResultsDto, MonthlyResultDto } from "../../modeles/dto.ts";
import Result from "../../components/Result.tsx";
import Spinner from "../../components/Spinner.tsx";

const ResultDisplay: React.FC<{ strategyId: string }> = ({ strategyId }) => {
  const [result, setResult] = useState<GlobalResultsDto>();
  const [isLoading, setIsLoading] = useState(false);
  const [showDetailedResults, setShowDetailedResults] = useState(false); // Etat pour gérer la rétractabilité
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);

    StrategyService.getResult(strategyId)
      .then((data) => setResult(data))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [strategyId]);

  if (isLoading) {
    return <Spinner />;
  }

  return (
    <div className="space-y-6">
      {" "}
      {/* Utilisation de l'espace pour séparer les sections */}
      {/* Section Résultat Global */}
      <div className="border p-4 rounded-lg shadow-sm bg-white">
        <h2 className="text-xl font-bold text-blue-600">Global Result</h2>
        {result?.result ? (
          <Result result={result.result} />
        ) : (
          <p className="text-gray-500">Aucun résultat global disponible.</p>
        )}
      </div>
      {/* Section Résultats Détailés */}
      <div className="border p-4 rounded-lg shadow-sm bg-white">
        <h2 className="text-xl font-bold text-blue-600">
          Detailed Results
          <button
            onClick={() => setShowDetailedResults(!showDetailedResults)}
            className="ml-4 text-sm text-blue-500 underline"
          >
            {showDetailedResults ? "Hide" : "Show"}
          </button>
        </h2>

        {showDetailedResults && ( // Rendre les résultats détaillés rétractables
          <div className="space-y-4 mt-4">
            {result?.monthlyResults && result.monthlyResults.length > 0 ? (
              result.monthlyResults.map(
                (monthlyResult: MonthlyResultDto, index: number) => (
                  <div
                    key={index}
                    className="border p-4 rounded-lg shadow-sm bg-gray-50"
                  >
                    <h3 className="text-lg font-bold">
                      {monthlyResult.date
                        ? new Date(monthlyResult.date).toLocaleDateString()
                        : "Date inconnue"}
                    </h3>
                    {monthlyResult.result ? (
                      <Result result={monthlyResult.result} />
                    ) : (
                      <p className="text-gray-500">
                        Aucun résultat disponible pour ce mois.
                      </p>
                    )}
                  </div>
                ),
              )
            ) : (
              <p className="text-gray-500">
                Aucun résultat détaillé disponible.
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default ResultDisplay;

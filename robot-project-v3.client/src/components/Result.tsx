import React from "react";
import { ResultDto } from "../modeles/dto.ts";

const resultDisplayComponent: React.FC<{
  result: ResultDto | undefined;
}> = ({ result }) => {
  if (!result) {
    return (
      <div className="bg-yellow-100 text-yellow-800 p-4 rounded-md">
        Aucune donnée disponible
      </div>
    );
  }

  return (
    <div className="mt-4 p-4 bg-white rounded-md shadow-md">
      <form className="space-y-4">
        <div>
          <label
            htmlFor="drawndownMax"
            className="block text-sm font-medium text-gray-700"
          >
            Drawndown Max
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="drawndownMax"
            value={result.drawndownMax}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="gainMax"
            className="block text-sm font-medium text-gray-700"
          >
            Gain Max
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="gainMax"
            value={result.gainMax}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="moyenneNegative"
            className="block text-sm font-medium text-gray-700"
          >
            Moyenne Negative
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="moyenneNegative"
            value={result.moyenneNegative}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="moyennePositive"
            className="block text-sm font-medium text-gray-700"
          >
            Moyenne Positive
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="moyennePositive"
            value={result.moyennePositive}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="moyenneProfit"
            className="block text-sm font-medium text-gray-700"
          >
            Moyenne Profit
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="moyenneProfit"
            value={result.moyenneProfit}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="perteMax"
            className="block text-sm font-medium text-gray-700"
          >
            Perte Max
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="perteMax"
            value={result.perteMax}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="profit"
            className="block text-sm font-medium text-gray-700"
          >
            Profit
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="profit"
            value={result.profit}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="profitFactor"
            className="block text-sm font-medium text-gray-700"
          >
            Profit Factor
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="profitFactor"
            value={result.profitFactor}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="profitNegatif"
            className="block text-sm font-medium text-gray-700"
          >
            Profit Négatif
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="profitNegatif"
            value={result.profitNegatif}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="profitPositif"
            className="block text-sm font-medium text-gray-700"
          >
            Profit Positif
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="profitPositif"
            value={result.profitPositif}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="ratioMoyennePositifNegatif"
            className="block text-sm font-medium text-gray-700"
          >
            Ratio Moyenne Positif/Négatif
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="ratioMoyennePositifNegatif"
            value={result.ratioMoyennePositifNegatif}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="tauxReussite"
            className="block text-sm font-medium text-gray-700"
          >
            Taux de Réussite
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="tauxReussite"
            value={result.tauxReussite}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="totalPositionNegative"
            className="block text-sm font-medium text-gray-700"
          >
            Total Position Négative
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="totalPositionNegative"
            value={result.totalPositionNegative}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="totalPositionPositive"
            className="block text-sm font-medium text-gray-700"
          >
            Total Position Positive
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="totalPositionPositive"
            value={result.totalPositionPositive}
            readOnly
          />
        </div>

        <div>
          <label
            htmlFor="totalPositions"
            className="block text-sm font-medium text-gray-700"
          >
            Total Positions
          </label>
          <input
            type="text"
            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            id="totalPositions"
            value={result.totalPositions}
            readOnly
          />
        </div>
      </form>
    </div>
  );
};

export default resultDisplayComponent;

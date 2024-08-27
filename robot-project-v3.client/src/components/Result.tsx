import React from "react";
import { ResultDto } from "../modeles/dto.ts";

const resultDisplayComponent: React.FC<{ result: ResultDto | undefined }> = ({
  result,
}) => {
  if (!result) {
    return (
      <div className="bg-yellow-100 text-yellow-800 p-4 rounded-md">
        Aucune donnée disponible
      </div>
    );
  }

  // Tableau des champs à afficher
  const fields = [
    { id: "drawndownMax", label: "Drawndown Max", value: result.drawndownMax },
    { id: "gainMax", label: "Gain Max", value: result.gainMax },
    {
      id: "moyenneNegative",
      label: "Moyenne Négative",
      value: result.moyenneNegative,
    },
    {
      id: "moyennePositive",
      label: "Moyenne Positive",
      value: result.moyennePositive,
    },
    {
      id: "moyenneProfit",
      label: "Moyenne Profit",
      value: result.moyenneProfit,
    },
    { id: "perteMax", label: "Perte Max", value: result.perteMax },
    { id: "profit", label: "Profit", value: result.profit },
    { id: "profitFactor", label: "Profit Factor", value: result.profitFactor },
    {
      id: "profitNegatif",
      label: "Profit Négatif",
      value: result.profitNegatif,
    },
    {
      id: "profitPositif",
      label: "Profit Positif",
      value: result.profitPositif,
    },
    {
      id: "ratioMoyennePositifNegatif",
      label: "Ratio Moyenne Positif/Négatif",
      value: result.ratioMoyennePositifNegatif,
    },
    {
      id: "tauxReussite",
      label: "Taux de Réussite",
      value: result.tauxReussite,
    },
    {
      id: "totalPositionNegative",
      label: "Total Position Négative",
      value: result.totalPositionNegative,
    },
    {
      id: "totalPositionPositive",
      label: "Total Position Positive",
      value: result.totalPositionPositive,
    },
    {
      id: "totalPositions",
      label: "Total Positions",
      value: result.totalPositions,
    },
  ];

  return (
    <div className="mt-4 p-4 bg-white rounded-md shadow-md">
      <h2 className="text-lg font-bold text-gray-800 mb-4">Résultats</h2>
      <form className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4">
        {fields.map((field) => (
          <div key={field.id}>
            <label
              htmlFor={field.id}
              className="block text-sm font-medium text-gray-700"
            >
              {field.label}
            </label>
            <input
              type="text"
              className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm p-2 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
              id={field.id}
              value={field.value}
              readOnly
            />
          </div>
        ))}
      </form>
    </div>
  );
};

export default resultDisplayComponent;

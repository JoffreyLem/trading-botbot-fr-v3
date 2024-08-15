import React, { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";

import { StrategyGeneratorService } from "../../services/StrategyGeneratorService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { StrategyFileDto } from "../../modeles/dto.ts";
import Spinner from "../../components/Spinner.tsx";
import CompilationErrorComponent from "./CompilationErrorComponent.tsx";
import CreateUpdateModalStrategy from "./CreateUpdateModalStrategy.tsx";
import { ApiException } from "../../exceptions/ApiException.ts";

const StrategyCreator: React.FC = () => {
  const [strategyFiles, setStrategyFiles] = useState<StrategyFileDto[]>([]);
  const { instance } = useMsal();
  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();
  const [modalShow, setModalShow] = useState(false);
  const [compilError, setCompilError] = useState<string[] | null>(null);
  const [strategyIdUpdate, setStrategyIdupdate] = useState<number | null>(null);

  useEffect(() => {
    setIsLoading(true);
    StrategyGeneratorService.getAllStrategyFiles()
      .then((r) => setStrategyFiles(r))
      .catch(handleError)
      .finally(() => setIsLoading(false));
  }, [instance]);

  const handleUpdate = (strategyId: number) => {
    setStrategyIdupdate(strategyId);
    setModalShow(true);
  };

  const handleDelete = (id: number) => {
    StrategyGeneratorService.deleteStrategyFile(id)
      .then(() =>
        setStrategyFiles(strategyFiles.filter((file) => file.id !== id)),
      )
      .catch(handleError);
  };

  const handleCreate = () => {
    setModalShow(true);
  };

  const handleSubmitModal = (file: File) => {
    setIsLoading(true);
    setCompilError(null);
    if (strategyIdUpdate === null) {
      StrategyGeneratorService.createNewStrategy(file)
        .then((r) => {
          if (r.compiled) {
            addnewStrategy(r.strategyFileDto as StrategyFileDto);
          } else {
            setCompilError(r.errors as string[]);
          }
        })
        .catch(handleError);
    } else {
      StrategyGeneratorService.updateStrategyFile(strategyIdUpdate, file)
        .then((r) => {
          if (r.compiled) {
            updateStrategyFile(r.strategyFileDto as StrategyFileDto);
          } else {
            setCompilError(r.errors as string[]);
          }
        })
        .catch(handleError)
        .finally(() => {
          setStrategyIdupdate(null);
        });
    }
    setIsLoading(false);
  };

  const addnewStrategy = (strategy: StrategyFileDto) => {
    setStrategyFiles((currentStrategyFiles) => [
      ...currentStrategyFiles,
      strategy,
    ]);
  };

  const updateStrategyFile = (strategy: StrategyFileDto) => {
    setStrategyFiles((currentFiles) =>
      currentFiles.map((file) => {
        if (file.id === strategy.id) {
          return { ...strategy };
        }

        return file;
      }),
    );
  };

  const downloadFile = async (file: StrategyFileDto) => {
    try {
      const dataFile: StrategyFileDto =
        await StrategyGeneratorService.getStrategyFile(file.id!);
      const blob = new Blob([dataFile.data!], { type: "text/plain" });

      const fileUrl = URL.createObjectURL(blob);

      const link = document.createElement("a");
      link.href = fileUrl;

      link.download = `${file.name!.replace(/\.[^/.]+$/, "")}.cs`;

      document.body.appendChild(link);
      link.click();

      document.body.removeChild(link);
      URL.revokeObjectURL(fileUrl);
    } catch (e: any | Error | ApiException) {
      handleError(e);
    }
  };

  if (isLoading) {
    return <Spinner />;
  }

  return (
    <div>
      <button
        className="bg-green-500 text-white px-4 py-2 rounded mb-3 hover:bg-green-600"
        onClick={handleCreate}
      >
        Créer
      </button>
      <CreateUpdateModalStrategy
        show={modalShow}
        onClose={() => setModalShow(false)}
        handleSubmit={handleSubmitModal}
      />
      {compilError && (
        <CompilationErrorComponent
          title="Erreur de compilation"
          errors={compilError}
        />
      )}
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white border border-gray-300">
          <thead className="bg-gray-200">
            <tr>
              <th className="py-2 px-4 border-b border-gray-300 text-left">
                Name
              </th>
              <th className="py-2 px-4 border-b border-gray-300 text-left">
                Version
              </th>
              <th className="py-2 px-4 border-b border-gray-300 text-left">
                Last Update
              </th>
              <th className="py-2 px-4 border-b border-gray-300 text-left">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {strategyFiles.map((file) => (
              <tr key={file.id} className="hover:bg-gray-100">
                <td className="py-2 px-4 border-b border-gray-300">
                  {file.name}
                </td>
                <td className="py-2 px-4 border-b border-gray-300">
                  {file.version}
                </td>
                <td className="py-2 px-4 border-b border-gray-300">
                  {file.lastDateUpdate?.toString()}
                </td>
                <td className="py-2 px-4 border-b border-gray-300 space-x-2">
                  <button
                    className="bg-blue-500 text-white px-3 py-1 rounded hover:bg-blue-600"
                    onClick={() => handleUpdate(file.id!)}
                  >
                    Update
                  </button>
                  <button
                    className="bg-red-500 text-white px-3 py-1 rounded hover:bg-red-600"
                    onClick={() => handleDelete(file.id!)}
                  >
                    Delete
                  </button>
                  <button
                    className="bg-gray-500 text-white px-3 py-1 rounded hover:bg-gray-600"
                    onClick={() => downloadFile(file)}
                  >
                    Télécharger
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default StrategyCreator;

import React, { useEffect, useState } from "react";

import * as signalR from "@microsoft/signalr";

import { StrategyService } from "../../services/StrategyHandlerService.ts";
import { useErrorHandler } from "../../hooks/UseErrorHandler.tsx";
import { MsalAuthService } from "../../services/MsalAuthService.ts";
import { PositionDto } from "../../modeles/dto.ts";
import Spinner from "../../components/Spinner.tsx";
import PositionComponent from "../../components/PositionComponent.tsx";

const PositionOpened: React.FC<{
  strategyId: string;
}> = ({ strategyId }) => {
  const [positions, setPositions] = useState<PositionDto[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const handleError = useErrorHandler();

  useEffect(() => {
    setIsLoading(true);
    StrategyService.getOpenedPositions(strategyId)
      .then((r) => setPositions(r))
      .catch(handleError)
      .finally(() => {
        setIsLoading(false);
      });
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/infoClient", {
        accessTokenFactory: () => MsalAuthService.getAuthToken(),
      })
      .withAutomaticReconnect()
      .build();

    connection
      .start()
      .then(() => {
        console.log("Connected to the hub");

        connection.on("ReceivePosition", (position: PositionDto) => {
          setPositions((prevPositions) => {
            const existingPositionIndex = prevPositions.findIndex(
              (p) => p.id === position.id,
            );
            let newPositions = [...prevPositions];

            switch (position.statusPosition) {
              case "Opened":
                if (existingPositionIndex === -1) {
                  newPositions.push(position);
                }
                break;
              case "Updated":
                if (existingPositionIndex >= 0) {
                  newPositions[existingPositionIndex] = {
                    ...newPositions[existingPositionIndex],
                    ...position,
                  };
                } else {
                  newPositions.push(position);
                }
                break;
              case "Close":
              case "Rejected":
                newPositions = newPositions.filter((p) => p.id !== position.id);
                break;
              default:
                break;
            }
            return newPositions;
          });
        });
      })
      .catch((err) => console.error("Connection error: ", err));
  }, [strategyId]);

  if (isLoading) {
    return <Spinner />;
  }

  return <PositionComponent positions={positions} positionClosed={false} />;
};

export default PositionOpened;

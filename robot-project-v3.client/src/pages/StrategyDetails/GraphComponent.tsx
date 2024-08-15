import React, { useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";

import { useMsal } from "@azure/msal-react";
import { MsalAuthService } from "../../services/MsalAuthService.ts";
import { CandleDto, TickDto } from "../../modeles/dto.ts";
import { StrategyFormProps } from "./StrategyFormProps.tsx";

const TradingData: React.FC<StrategyFormProps> = ({ strategyInfo }) => {
  const [currentCandle, setCurrentCandle] = useState<CandleDto | null>(
    strategyInfo?.lastCandle ?? null,
  );
  const [currentTick, setCurrentTick] = useState<TickDto | null>(null);
  const { instance } = useMsal();

  useEffect(() => {
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

        connection.on("ReceiveCandle", (candle: CandleDto) => {
          setCurrentCandle(candle);
        });

        connection.on("ReceiveTick", (tick: TickDto) => {
          setCurrentTick(tick);
        });
      })
      .catch((err) => console.error("Connection error: ", err));

    return () => {
      connection.stop();
    };
  }, [instance]);

  return (
    <div>
      <h2>Current Candle</h2>
      {currentCandle && (
        <div>{`Date: ${currentCandle.date}, Open: ${currentCandle.open}, High: ${currentCandle.high}, Low: ${currentCandle.low}, Close: ${currentCandle.close}`}</div>
      )}
      <h2>Current Tick</h2>
      {currentTick && (
        <div>{`Date: ${currentTick.date}, Ask: ${currentTick.ask}, Bid: ${currentTick.bid}`}</div>
      )}
    </div>
  );
};

export default TradingData;

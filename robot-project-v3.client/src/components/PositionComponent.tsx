import React, { CSSProperties } from "react";
import { FixedSizeList as List } from "react-window";
import { PositionDto } from "../modeles/dto.ts";

const ROW_HEIGHT = 50;

const PositionComponent: React.FC<{
  positions: PositionDto[];
  positionClosed: boolean;
}> = ({ positions, positionClosed }) => {
  interface RowProps {
    index: number;
    style: CSSProperties;
  }

  const Row: React.FC<RowProps> = ({ index, style }) => {
    const position = positions[index];
    return (
      <div style={style} className="flex items-center border-b border-gray-300">
        <div className="flex-1 p-2 text-left">{position?.symbol}</div>
        <div className="flex-1 p-2 text-left">{position?.typePosition}</div>
        <div className="flex-1 p-2 text-left">{position?.volume}</div>
        <div className="flex-1 p-2 text-left">{position?.spread}</div>
        <div className="flex-1 p-2 text-left">
          {position?.dateOpen?.toString()}
        </div>
        <div className="flex-1 p-2 text-left">{position?.openPrice}</div>
        <div className="flex-1 p-2 text-left">{position?.profit}</div>
        <div className="flex-1 p-2 text-left">{position?.stopLoss}</div>
        <div className="flex-1 p-2 text-left">{position?.takeProfit}</div>
        {positionClosed && (
          <>
            <div className="flex-1 p-2 text-left">
              {position?.dateClose?.toString()}
            </div>
            <div className="flex-1 p-2 text-left">{position?.closePrice}</div>
            <div className="flex-1 p-2 text-left">{position?.reasonClosed}</div>
          </>
        )}
      </div>
    );
  };

  return (
    <div className="border border-gray-300">
      <div className="bg-gray-100">
        <div className="flex items-center border-b border-gray-300">
          <div className="flex-1 p-2 text-left">Symbol</div>
          <div className="flex-1 p-2 text-left">Type</div>
          <div className="flex-1 p-2 text-left">Volume</div>
          <div className="flex-1 p-2 text-left">Spread</div>
          <div className="flex-1 p-2 text-left">Date open</div>
          <div className="flex-1 p-2 text-left">Open price</div>
          <div className="flex-1 p-2 text-left">Profit</div>
          <div className="flex-1 p-2 text-left">Stop loss</div>
          <div className="flex-1 p-2 text-left">Take profit</div>
          {positionClosed && (
            <>
              <div className="flex-1 p-2 text-left">Date close</div>
              <div className="flex-1 p-2 text-left">Close price</div>
              <div className="flex-1 p-2 text-left">Reason closed</div>
            </>
          )}
        </div>
      </div>
      <List
        height={400}
        itemCount={positions.length}
        itemSize={ROW_HEIGHT}
        width="100%"
      >
        {Row}
      </List>
    </div>
  );
};

export default PositionComponent;

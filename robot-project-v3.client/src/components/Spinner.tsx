import React from "react";

interface SpinnerProps {
  size?: string;
  color?: string;
}

const Spinner: React.FC<SpinnerProps> = ({
  size = "w-8 h-8",
  color = "border-blue-500",
}) => {
  return (
    <div
      className={`inline-block ${size} border-4 border-t-transparent border-solid rounded-full animate-spin ${color}`}
    ></div>
  );
};

export default Spinner;

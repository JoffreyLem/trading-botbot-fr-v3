import React from "react";

interface SpinnerProps {
  size?: string;
  color?: string;
  fixed?: boolean;
}

const Spinner: React.FC<SpinnerProps> = ({
  size = "w-8 h-8",
  color = "border-blue-500",
  fixed = true,
}) => {
  return (
    <div
      className={`inline-block ${size} border-4 border-t-transparent border-solid rounded-full animate-spin ${color} ${
        fixed ? "fixed inset-0 m-auto z-50" : ""
      }`}
    ></div>
  );
};

export default Spinner;

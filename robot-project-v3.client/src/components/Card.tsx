import React, { useState } from "react";
import { FaChevronDown, FaChevronUp } from "react-icons/fa";

interface CardProps {
  title: string;
  children: React.ReactNode;
}

const Card: React.FC<CardProps> = ({ title, children }) => {
  const [isExpanded, setIsExpanded] = useState(true);

  const toggleExpand = () => {
    setIsExpanded(!isExpanded);
  };

  return (
    <div className="bg-white shadow-md rounded-lg p-6 inline-block">
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-xl font-semibold">{title}</h2>
        <button
          onClick={toggleExpand}
          className="text-blue-500 hover:text-blue-700 focus:outline-none ml-4"
          aria-label={isExpanded ? "Réduire" : "Agrandir"}
        >
          {isExpanded ? <FaChevronUp /> : <FaChevronDown />}
        </button>
      </div>
      {isExpanded && <div>{children}</div>}
    </div>
  );
};

export default Card;

import React, { useState } from "react";

interface AutocompleteInputProps {
  suggestions: string[];
  onValueChange?: (value: string) => void;
}

const AutocompleteInput: React.FC<AutocompleteInputProps> = ({
  suggestions,
  onValueChange,
}) => {
  const [inputValue, setInputValue] = useState<string>("");
  const [filteredSuggestions, setFilteredSuggestions] = useState<string[]>([]);
  const [showSuggestions, setShowSuggestions] = useState<boolean>(false);

  const updateSuggestions = (value: string) => {
    setInputValue(value);
    onValueChange && onValueChange(value);
    if (!value) {
      setFilteredSuggestions([]);
      setShowSuggestions(false);
      return;
    }
    const filtered = suggestions.filter((suggestion) =>
      suggestion.toLowerCase().includes(value.toLowerCase()),
    );
    setFilteredSuggestions(filtered);
    setShowSuggestions(true);
  };

  const selectSuggestion = (suggestion: string) => {
    setInputValue(suggestion);
    onValueChange && onValueChange(suggestion);
    setFilteredSuggestions([]);
    setShowSuggestions(false);
  };

  return (
    <div className="relative min-w-[200px]">
      <input
        className="w-full border border-gray-300 p-2 rounded"
        type="text"
        value={inputValue}
        onChange={(e) => updateSuggestions(e.target.value)}
      />
      {showSuggestions && (
        <ul className="absolute w-full border border-gray-300 bg-white z-10 mt-1 p-0 list-none">
          {filteredSuggestions.map((suggestion, index) => (
            <li
              key={index}
              className="px-4 py-2 cursor-pointer hover:bg-gray-200"
              onClick={() => selectSuggestion(suggestion)}
            >
              {suggestion}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default AutocompleteInput;

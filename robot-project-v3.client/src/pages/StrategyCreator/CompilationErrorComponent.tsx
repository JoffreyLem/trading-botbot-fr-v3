import React from "react";

interface ErrorComponentProps {
  errors: string | string[];
  title?: string;
}

const CompilationErrorComponent: React.FC<ErrorComponentProps> = ({
  errors,
  title = "Erreur",
}) => {
  const errorMessages = Array.isArray(errors) ? errors : [errors];

  return (
    <div className="bg-red-100 border-l-4 border-red-500 text-red-700 p-4">
      <strong className="block font-bold mb-2">{title}</strong>
      {errorMessages.map((error, index) => (
        <p key={index} className="mt-1">
          {error}
        </p>
      ))}
    </div>
  );
};

export default CompilationErrorComponent;

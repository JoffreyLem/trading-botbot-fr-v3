import React from "react";

export interface FormProps {
  onClose: () => void;
}

interface ModalProps extends FormProps {
  isOpen: boolean;
  children: React.ReactNode;
  title?: string;
}

const Modal: React.FC<ModalProps> = ({ isOpen, onClose, children, title }) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-gray-800 bg-opacity-75 flex items-center justify-center z-50">
      <div className="bg-white p-4 rounded shadow-lg relative w-auto max-w-full max-h-full overflow-auto">
        <div className="flex justify-between items-center mb-4">
          {title && <h2 className="text-xl font-semibold">{title}</h2>}
          <button
            onClick={onClose}
            className="text-gray-600 hover:text-gray-900 focus:outline-none"
          >
            X
          </button>
        </div>
        <div className={`max-w-full max-h-full ${title ? "pt-4" : "pt-8"}`}>
          {children}
        </div>
      </div>
    </div>
  );
};

export default Modal;

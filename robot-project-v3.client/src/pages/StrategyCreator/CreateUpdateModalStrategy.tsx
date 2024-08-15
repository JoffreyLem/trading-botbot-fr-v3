import React, { useState } from "react";

const CreateUpdateModalStrategy: React.FC<{
  show: boolean;
  onClose: () => void;
  handleSubmit: (file: File) => void;
}> = ({ show, onClose, handleSubmit }) => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.files && event.target.files[0]) {
      setSelectedFile(event.target.files[0]);
    } else {
      setSelectedFile(null);
    }
  };

  const handleFileDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    if (event.dataTransfer.files && event.dataTransfer.files[0]) {
      setSelectedFile(event.dataTransfer.files[0]);
    } else {
      setSelectedFile(null);
    }
  };

  const handleSubmitInternal = (event: React.FormEvent) => {
    event.preventDefault();

    if (selectedFile) {
      handleSubmit(selectedFile);
      setSelectedFile(null);
    }

    onClose();
  };

  if (!show) {
    return null;
  }

  return (
    <div className="fixed inset-0 flex items-center justify-center z-50 bg-gray-800 bg-opacity-75">
      <div className="bg-white rounded-lg shadow-lg w-full max-w-md mx-auto">
        <div className="flex justify-between items-center p-4 border-b">
          <h5 className="text-lg font-semibold">Upload File</h5>
          <button
            type="button"
            className="text-gray-500 hover:text-gray-800 focus:outline-none"
            onClick={onClose}
          >
            &times;
          </button>
        </div>
        <form onSubmit={handleSubmitInternal}>
          <div className="p-4">
            <div
              onDragOver={(e) => e.preventDefault()}
              onDrop={handleFileDrop}
              className="text-center p-6 my-2 border-dashed border-2 border-gray-300 rounded cursor-pointer hover:bg-gray-100"
            >
              Drag and drop a file here, or click to select a file.
              <input
                type="file"
                className="hidden"
                onChange={handleFileChange}
                id="fileUpload"
              />
              <label
                htmlFor="fileUpload"
                className="block mt-4 text-blue-600 hover:text-blue-800 cursor-pointer"
              >
                Select File
              </label>
            </div>
            {selectedFile && (
              <p className="text-sm text-gray-700 mt-2">
                Selected file: {selectedFile.name}
              </p>
            )}
          </div>
          <div className="flex justify-end p-4 border-t">
            <button
              type="button"
              className="bg-gray-500 text-white px-4 py-2 rounded mr-2 hover:bg-gray-600"
              onClick={onClose}
            >
              Close
            </button>
            <button
              type="submit"
              className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
            >
              Upload
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateUpdateModalStrategy;

import React, { useState } from "react";
import { useMsal } from "@azure/msal-react";

export const ProfilDropdown: React.FC = () => {
  const { instance } = useMsal();
  const [isOpen, setIsOpen] = useState(false);

  const toggleDropdown = () => {
    setIsOpen(!isOpen);
  };

  const handleLogout = () => {
    instance.logoutRedirect().then();
  };

  return (
    <div className="relative ml-auto">
      <button
        onClick={toggleDropdown}
        className="bg-gray-800 hover:bg-gray-700 text-white py-2 px-4 rounded"
      >
        Profil
      </button>
      {isOpen && (
        <ul className="absolute right-0 mt-2 w-48 bg-white shadow-lg rounded">
          <li>
            <button
              onClick={handleLogout}
              className="block w-full text-left px-4 py-2 text-gray-700 hover:bg-gray-100"
            >
              Déconnexion
            </button>
          </li>
        </ul>
      )}
    </div>
  );
};

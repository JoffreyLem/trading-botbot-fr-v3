import React, { useState } from "react";
import { Link, Outlet } from "react-router-dom";
import Modal from "./components/Modal.tsx";
import LoginForm from "./components/LoginApiProvider.tsx";
import { ProfilDropdown } from "./components/ProfilDropdown.tsx";

const Layout: React.FC = () => {
  const [isModalApiOpen, setIsModalApiOpen] = useState(false);

  const openModal = () => setIsModalApiOpen(true);
  const closeModal = () => setIsModalApiOpen(false);

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-black text-white p-4 sticky top-0 z-50">
        <nav className="grid grid-cols-3 items-center w-full">
          <div className="flex items-center space-x-4">
            <div className="text-lg font-bold">
              <Link to="/" className="hover:text-gray-300">
                BOT Bot
              </Link>
            </div>
            <ul className="flex space-x-4 border-l border-gray-600 pl-4">
              <li>
                <Link to="/" className="hover:text-gray-300">
                  Accueil
                </Link>
              </li>
              <li>
                <Link to="/strategyCreator" className="hover:text-gray-300">
                  Strategy Creator
                </Link>
              </li>
            </ul>
          </div>

          <div className="flex justify-center">
            <button
              onClick={openModal}
              className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
            >
              API
            </button>
          </div>

          <div className="flex justify-end">
            <ProfilDropdown />
          </div>
        </nav>
      </header>

      <main className="flex-grow w-full p-4 bg-gray-100">
        <Outlet />
      </main>

      <Modal
        isOpen={isModalApiOpen}
        onClose={closeModal}
        title="Connexion au provider"
      >
        <LoginForm onClose={closeModal} />
      </Modal>
    </div>
  );
};

export default Layout;

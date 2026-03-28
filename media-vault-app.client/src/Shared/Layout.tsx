import { useState } from "react";
import { Outlet, Link } from "react-router-dom";
import RegisterUserCard from "../Components/UserAccount/RegisterUserCard";
import Login from "../Components/UserAccount/LoginCard";
import { useUser } from "./UserContext";

export default function Layout() {
  const [showRegisterPopup, setShowRegisterPopup] = useState(false);
  const [showLoginPopup, setShowLoginPopup] = useState(false);
  const { currentUser, isAuthenticated, isLoading, logout } = useUser();

  const openRegisterWindow = () => {
    setShowRegisterPopup(true);
    setShowLoginPopup(false);
  };

  const openLoginWindow = () => {
    setShowLoginPopup(true);
    setShowRegisterPopup(false);
  };

  const closeRegisterWindow = (toLogin: boolean) => {
    setShowRegisterPopup(false);
    if (toLogin) {
      setShowLoginPopup(true);
    }
  };

  const closeLoginWindow = () => {
    setShowLoginPopup(false);
  };

  const handleLogout = async () => {
    await logout();
  };

  return (
    <div className="bg-background-light dark:bg-background-dark font-display text-slate-900 dark:text-slate-100 antialiased">
      {showRegisterPopup && <RegisterUserCard onCancel={closeRegisterWindow} />}
      {showLoginPopup && <Login onCancel={closeLoginWindow} />}
      <div className="relative flex min-h-screen w-full flex-col overflow-x-hidden">
        <div className="layout-container flex h-full grow flex-col">
          <header className="flex items-center justify-between whitespace-nowrap border-b border-solid border-slate-200 dark:border-slate-800 px-6 md:px-20 py-4 bg-background-light/80 dark:bg-background-dark/80 backdrop-blur-md sticky top-0 z-50">
            <div className="flex items-center gap-3">
              <div className="flex items-center justify-center size-10 bg-primary rounded-lg text-white">
                <span className="material-symbols-outlined">analytics</span>
              </div>
              <h2 className="text-slate-900 dark:text-white text-xl font-black leading-tight tracking-tight">
                MediaVault
              </h2>
            </div>
            <div className="flex flex-1 justify-end gap-4 md:gap-10">
              <nav className="hidden md:flex items-center gap-8">
                <Link to="/dashboard">Dashboard</Link>
                <a
                  className="text-slate-600 dark:text-slate-300 hover:text-primary dark:hover:text-primary text-sm font-medium transition-colors"
                  href="#features"
                >
                  Features
                </a>
                <a
                  className="text-slate-600 dark:text-slate-300 hover:text-primary dark:hover:text-primary text-sm font-medium transition-colors"
                  href="#"
                >
                  Pricing
                </a>
                <a
                  className="text-slate-600 dark:text-slate-300 hover:text-primary dark:hover:text-primary text-sm font-medium transition-colors"
                  href="#"
                >
                  Community
                </a>
              </nav>
              <div className="flex gap-3">
                {isLoading ? (
                  <div className="flex min-w-21 items-center justify-center rounded-lg h-10 px-4 bg-slate-200 dark:bg-slate-800 text-slate-900 dark:text-white text-sm font-bold opacity-70">
                    Loading...
                  </div>
                ) : isAuthenticated ? (
                  <>
                    <div className="flex items-center rounded-lg h-10 px-4 bg-slate-200 dark:bg-slate-800 text-slate-900 dark:text-white text-sm font-semibold">
                      {currentUser?.username}
                    </div>
                    <button
                      type="button"
                      className="flex min-w-21 cursor-pointer items-center justify-center rounded-lg h-10 px-4 bg-slate-200 dark:bg-slate-800 text-slate-900 dark:text-white text-sm font-bold transition-all hover:opacity-90"
                      onClick={() => void handleLogout()}
                    >
                      Logout
                    </button>
                  </>
                ) : (
                  <>
                    <button
                      type="button"
                      className="flex min-w-21 cursor-pointer items-center justify-center rounded-lg h-10 px-4 bg-slate-200 dark:bg-slate-800 text-slate-900 dark:text-white text-sm font-bold transition-all hover:opacity-90"
                      onClick={openLoginWindow}
                    >
                      Login
                    </button>
                    <button
                      type="button"
                      className="flex min-w-21 cursor-pointer items-center justify-center rounded-lg h-10 px-4 bg-primary text-white text-sm font-bold transition-all hover:shadow-lg hover:shadow-primary/20"
                      onClick={openRegisterWindow}
                    >
                      Sign Up
                    </button>
                  </>
                )}
              </div>
            </div>
          </header>

          {/* <main className="flex flex-1 flex-row justify-center"> */}
          <main className="flex-1">
            <Outlet /> {/* like @Body */}
          </main>

          {/* <!-- Footer --> */}
          <footer className="border-t border-slate-200 dark:border-slate-800 bg-background-light dark:bg-background-dark px-6 md:px-20 py-12">
            <div className="mx-auto max-w-7xl flex flex-col gap-12">
              <div className="flex flex-col md:flex-row justify-between items-center gap-8">
                <div className="flex items-center gap-3">
                  <div className="flex items-center justify-center size-8 bg-primary rounded text-white">
                    <span className="material-symbols-outlined text-xl">
                      analytics
                    </span>
                  </div>
                  <h2 className="text-slate-900 dark:text-white text-lg font-black leading-tight">
                    MediaVault
                  </h2>
                </div>
                <nav className="flex flex-wrap items-center justify-center gap-8 md:gap-12">
                  <a
                    className="text-slate-500 dark:text-slate-400 hover:text-primary transition-colors text-sm font-medium"
                    href="#"
                  >
                    Privacy Policy
                  </a>
                  <a
                    className="text-slate-500 dark:text-slate-400 hover:text-primary transition-colors text-sm font-medium"
                    href="#"
                  >
                    Terms of Service
                  </a>
                  <a
                    className="text-slate-500 dark:text-slate-400 hover:text-primary transition-colors text-sm font-medium"
                    href="#"
                  >
                    Cookie Policy
                  </a>
                  <a
                    className="text-slate-500 dark:text-slate-400 hover:text-primary transition-colors text-sm font-medium"
                    href="#"
                  >
                    Contact Us
                  </a>
                </nav>
              </div>
              <div className="flex flex-col md:flex-row justify-between items-center gap-6 border-t border-slate-200 dark:border-slate-800 pt-8">
                <p className="text-slate-500 dark:text-slate-400 text-sm">
                  © 2024 MediaVault Inc. All rights reserved.
                </p>
                <div className="flex gap-6">
                  <a
                    className="text-slate-400 hover:text-primary transition-colors"
                    href="#"
                  >
                    <span className="material-symbols-outlined">public</span>
                  </a>
                  <a
                    className="text-slate-400 hover:text-primary transition-colors"
                    href="#"
                  >
                    <span className="material-symbols-outlined">groups</span>
                  </a>
                  <a
                    className="text-slate-400 hover:text-primary transition-colors"
                    href="#"
                  >
                    <span className="material-symbols-outlined">share</span>
                  </a>
                </div>
              </div>
            </div>
          </footer>
        </div>
      </div>
    </div>
  );
}

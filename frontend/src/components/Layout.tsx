import { useState } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { logout } from "../api/auth";

const Layout = ({ children }: { children: React.ReactNode }) => {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);

  const handleLogout = async () => {
    try {
      await logout();
    } finally {
      signOut();
      navigate("/login");
    }
  };

  const navLink = (to: string, label: string, onClick?: () => void) => {
    const active =
      to === "/" ? location.pathname === "/" : location.pathname.startsWith(to);
    return (
      <Link
        to={to}
        onClick={onClick}
        className={`text-sm font-medium px-4 py-2 rounded-lg transition-colors ${
          active
            ? "bg-blue-50 text-blue-700 font-semibold"
            : "text-gray-500 hover:text-gray-800 hover:bg-gray-100"
        }`}
      >
        {label}
      </Link>
    );
  };

  return (
    <div className="min-h-screen bg-slate-50">
      <nav className="bg-white border-b border-gray-200 px-4 sm:px-8 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-8">
            <div className="flex items-center gap-2">
              <div className="w-6 h-6 bg-blue-600 rounded-md" />
              <span className="text-lg font-bold text-gray-900">DocuFlow</span>
            </div>
            <div className="hidden sm:flex items-center gap-1">
              {navLink("/", "Dashboard")}
              {navLink("/documents", "Documents")}
              {navLink("/about", "About")}
            </div>
          </div>
          <div className="hidden sm:flex items-center gap-4">
            <span className="text-sm text-gray-600 font-medium">
              {user?.fullName}
            </span>
            <div className="w-px h-4 bg-gray-600" />
            <button
              onClick={handleLogout}
              className="text-sm text-gray-600 hover:text-red-600 font-medium transition-colors cursor-pointer"
            >
              Sign out
            </button>
          </div>
          <button
            onClick={() => setMenuOpen((open) => !open)}
            aria-label="Toggle navigation menu"
            className="sm:hidden p-2 -mr-2 text-gray-600 hover:text-gray-900 cursor-pointer"
          >
            <svg
              className="w-6 h-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              {menuOpen ? (
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M6 18L18 6M6 6l12 12"
                />
              ) : (
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M4 6h16M4 12h16M4 18h16"
                />
              )}
            </svg>
          </button>
        </div>

        {menuOpen && (
          <div className="sm:hidden mt-4 flex flex-col gap-1 border-t border-gray-100 pt-4">
            {navLink("/", "Dashboard", () => setMenuOpen(false))}
            {navLink("/documents", "Documents", () => setMenuOpen(false))}
            {navLink("/about", "About", () => setMenuOpen(false))}
            <div className="border-t border-gray-100 mt-2 pt-2 flex items-center justify-between px-4">
              <span className="text-sm text-gray-600 font-medium">
                {user?.fullName}
              </span>
              <button
                onClick={handleLogout}
                className="text-sm text-gray-600 hover:text-red-600 font-medium transition-colors cursor-pointer"
              >
                Sign out
              </button>
            </div>
          </div>
        )}
      </nav>
      <main className="max-w-6xl mx-auto px-4 sm:px-8 py-6 sm:py-10">{children}</main>
    </div>
  );
};

export default Layout;

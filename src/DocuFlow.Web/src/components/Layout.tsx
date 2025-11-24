import { Link, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import { logout } from "../api/auth";

const Layout = ({ children }: { children: React.ReactNode }) => {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = async () => {
    try {
      await logout();
    } finally {
      signOut();
      navigate("/login");
    }
  };

  const navLink = (to: string, label: string) => {
    const active = location.pathname === to;
    return (
      <Link
        to={to}
        className={`text-sm font-medium px-3 py-2 rounded-lg transition-colors ${
          active
            ? "bg-blue-50 text-blue-600"
            : "text-gray-600 hover:text-gray-900 hover:bg-gray-100"
        }`}
      >
        {label}
      </Link>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white border-b border-gray-200 px-6 py-3 flex items-center justify-between">
        <div className="flex items-center gap-6">
          <span className="text-lg font-bold text-blue-600">DocuFlow</span>
          <div className="flex items-center gap-1">
            {navLink("/", "Dashboard")}
            {navLink("/documents", "Documents")}
          </div>
        </div>
        <div className="flex items-center gap-4">
          <span className="text-sm text-gray-500">{user?.fullName}</span>
          <button
            onClick={handleLogout}
            className="text-sm text-gray-600 hover:text-gray-900 font-medium"
          >
            Sign out
          </button>
        </div>
      </nav>
      <main className="max-w-6xl mx-auto px-6 py-8">{children}</main>
    </div>
  );
};

export default Layout;

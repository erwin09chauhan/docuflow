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
    const active =
      to === "/" ? location.pathname === "/" : location.pathname.startsWith(to);
    return (
      <Link
        to={to}
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
      <nav className="bg-white border-b border-gray-200 px-8 py-4 flex items-center justify-between">
        <div className="flex items-center gap-8">
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 bg-blue-600 rounded-md" />
            <span className="text-lg font-bold text-gray-900">DocuFlow</span>
          </div>
          <div className="flex items-center gap-1">
            {navLink("/", "Dashboard")}
            {navLink("/documents", "Documents")}
            {navLink("/about", "About")}
          </div>
        </div>
        <div className="flex items-center gap-4">
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
      </nav>
      <main className="max-w-6xl mx-auto px-8 py-10">{children}</main>
    </div>
  );
};

export default Layout;

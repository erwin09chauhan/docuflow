import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { login } from "../../api/auth";

const schema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});

type FormData = z.infer<typeof schema>;

const LoginPage = () => {
  const { setUser } = useAuth();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    try {
      const response = await login(data.email, data.password);
      setUser(response.user);
      navigate("/");
    } catch {
      setError("root", { message: "Invalid email or password" });
    }
  };

  return (
    <div
      className="min-h-screen flex items-center justify-center px-8"
      style={{
        backgroundColor: "#f8fafc",
        backgroundImage: `radial-gradient(circle, #cbd5e1 1px, transparent 1px)`,
        backgroundSize: "24px 24px",
      }}
    >
      <div className="w-full max-w-4xl flex items-center gap-16">
        {/* Left — branding */}
        <div className="hidden lg:flex flex-col gap-8 flex-1">
          <div className="flex items-center gap-2">
            <div className="w-7 h-7 bg-blue-600 rounded-md" />
            <span className="text-xl font-bold text-gray-900">DocuFlow</span>
          </div>

          <div>
            <h2 className="text-3xl font-semibold text-gray-900 leading-snug mb-2">
              Document processing,
              <br />
              automated.
            </h2>
            <p className="text-gray-500 text-sm">
              Upload documents, extract structured data with AI, all in one
              place.
            </p>
          </div>

          <ul className="space-y-4">
            {[
              "Upload PDFs, invoices, and spreadsheets",
              "AI extracts structured data instantly",
              "Multi-tenant, secure, and production-ready",
            ].map((feature) => (
              <li
                key={feature}
                className="flex items-center gap-3 text-gray-600 text-sm"
              >
                <div className="w-5 h-5 rounded-full bg-blue-100 flex items-center justify-center flex-shrink-0">
                  <svg
                    className="w-3 h-3 text-blue-600"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    strokeWidth={3}
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M5 13l4 4L19 7"
                    />
                  </svg>
                </div>
                {feature}
              </li>
            ))}
          </ul>

          <p className="text-gray-400 text-xs">© 2026 DocuFlow</p>
        </div>

        {/* Right — form card */}
        <div className="w-full lg:w-auto lg:min-w-[400px] bg-white rounded-2xl border border-gray-200 shadow-sm p-8">
          <div className="lg:hidden flex items-center gap-2 mb-8">
            <div className="w-6 h-6 bg-blue-600 rounded-md" />
            <span className="text-lg font-bold text-gray-900">DocuFlow</span>
          </div>

          <h1 className="text-2xl font-semibold text-gray-900 mb-1">
            Welcome back
          </h1>
          <p className="text-gray-500 text-sm mb-8">
            Sign in to your DocuFlow account
          </p>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Email
              </label>
              <input
                {...register("email")}
                type="email"
                placeholder="you@example.com"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
              />
              {errors.email && (
                <p className="text-red-500 text-xs mt-1">
                  {errors.email.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Password
              </label>
              <input
                {...register("password")}
                type="password"
                placeholder="••••••••"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
              />
              {errors.password && (
                <p className="text-red-500 text-xs mt-1">
                  {errors.password.message}
                </p>
              )}
            </div>

            {errors.root && (
              <p className="text-red-500 text-sm">{errors.root.message}</p>
            )}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors cursor-pointer mt-2"
            >
              {isSubmitting ? "Signing in..." : "Sign in"}
            </button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Don't have an account?{" "}
            <Link
              to="/register"
              className="text-blue-600 hover:underline font-medium"
            >
              Sign up
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;

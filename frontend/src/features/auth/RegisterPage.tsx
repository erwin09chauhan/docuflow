import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { register as registerUser } from "../../api/auth";

const schema = z.object({
  firstName: z.string().min(1, "First name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
  tenantName: z.string().min(2, "Organisation name is required"),
});

type FormData = z.infer<typeof schema>;

const RegisterPage = () => {
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
      const response = await registerUser(
        data.email,
        data.password,
        data.firstName,
        data.lastName,
        data.tenantName,
      );
      setUser(response.user);
      navigate("/");
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      const message =
        error?.response?.data?.error ??
        "Unable to connect to the server. Make sure the API is running.";
      setError("root", { message });
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
              Start processing
              <br />
              documents today.
            </h2>
            <p className="text-gray-500 text-sm">
              Set up your organisation in seconds and let AI do the heavy
              lifting.
            </p>
          </div>

          <ul className="space-y-4">
            {[
              "No manual data entry — AI extracts everything",
              "Invite your team and collaborate securely",
              "Supports PDFs, invoices, Excel and more",
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
            Create an account
          </h1>
          <p className="text-gray-500 text-sm mb-8">
            Get started with DocuFlow today
          </p>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="flex gap-3">
              <div className="flex-1">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  First name
                </label>
                <input
                  {...register("firstName")}
                  type="text"
                  placeholder="Arvind"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
                {errors.firstName && (
                  <p className="text-red-500 text-xs mt-1">
                    {errors.firstName.message}
                  </p>
                )}
              </div>
              <div className="flex-1">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Last name
                </label>
                <input
                  {...register("lastName")}
                  type="text"
                  placeholder="Chauhan"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                />
                {errors.lastName && (
                  <p className="text-red-500 text-xs mt-1">
                    {errors.lastName.message}
                  </p>
                )}
              </div>
            </div>

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

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Organisation name
              </label>
              <input
                {...register("tenantName")}
                type="text"
                placeholder="Acme Corp"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
              />
              {errors.tenantName && (
                <p className="text-red-500 text-xs mt-1">
                  {errors.tenantName.message}
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
              {isSubmitting ? "Creating account..." : "Create account"}
            </button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Already have an account?{" "}
            <Link
              to="/login"
              className="text-blue-600 hover:underline font-medium"
            >
              Sign in
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;

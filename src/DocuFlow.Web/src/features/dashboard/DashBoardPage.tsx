import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { getDocuments } from "../../api/documents";

const statusColor = (status: string) => {
  switch (status) {
    case "Completed":
      return "bg-green-100 text-green-700";
    case "Processing":
      return "bg-blue-100 text-blue-700";
    case "Failed":
      return "bg-red-100 text-red-700";
    default:
      return "bg-gray-100 text-gray-600";
  }
};

const DashboardPage = () => {
  const { user } = useAuth();
  const { data, isLoading } = useQuery({
    queryKey: ["documents", 1, 5],
    queryFn: () => getDocuments(1, 5),
  });

  const docs = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const completed = docs.filter((d) => d.status === "Completed").length;
  const failed = docs.filter((d) => d.status === "Failed").length;

  return (
    <div className="space-y-8">
      {/* Welcome */}
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">
          Welcome back, {user?.fullName?.split(" ")[0]}
        </h1>
        <p className="text-gray-600 text-md mt-1">
          Here's what's happening with your documents
        </p>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white rounded-xl border border-gray-200 p-5 border-l-4 border-l-blue-500">
          <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">
            Total Documents
          </p>
          <p className="text-3xl font-bold text-gray-900 mt-2">{total}</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-5 border-l-4 border-l-green-500">
          <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">
            Completed
          </p>
          <p className="text-3xl font-bold text-green-600 mt-2">{completed}</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-5 border-l-4 border-l-red-400">
          <p className="text-xs font-medium text-gray-600 uppercase tracking-wide">
            Failed
          </p>
          <p className="text-3xl font-bold text-red-500 mt-2">{failed}</p>
        </div>
      </div>

      {/* Recent documents */}
      <div className="bg-white rounded-xl border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-gray-900">
            Recent Documents
          </h2>
          <Link
            to="/documents"
            className="text-sm text-blue-600 hover:underline"
          >
            View all
          </Link>
        </div>

        {isLoading ? (
          <div className="px-6 py-10 text-sm text-gray-400 text-center">
            Loading...
          </div>
        ) : docs.length === 0 ? (
          <div className="px-6 py-10 text-sm text-gray-400 text-center">
            No documents yet.{" "}
            <Link to="/documents" className="text-blue-600 hover:underline">
              Upload your first document
            </Link>
          </div>
        ) : (
          <div className="divide-y divide-gray-100">
            {docs.map((doc) => (
              <Link
                key={doc.id}
                to={`/documents/${doc.id}`}
                className="flex items-center justify-between px-6 py-4 hover:bg-slate-50 transition-colors"
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-lg bg-blue-50 flex items-center justify-center flex-shrink-0">
                    <svg
                      className="w-4 h-4 text-blue-500"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                      strokeWidth={2}
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                      />
                    </svg>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-gray-900">
                      {doc.fileName}
                    </p>
                    <p className="text-xs text-gray-600 mt-0.5">
                      {new Date(doc.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
                <span
                  className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColor(doc.status)}`}
                >
                  {doc.status}
                </span>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default DashboardPage;

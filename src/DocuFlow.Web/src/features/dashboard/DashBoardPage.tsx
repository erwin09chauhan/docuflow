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
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">
          Welcome back, {user?.fullName?.split(" ")[0]}
        </h1>
        <p className="text-gray-500 text-sm mt-1">
          Here's what's happening with your documents
        </p>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <p className="text-sm text-gray-500">Total Documents</p>
          <p className="text-3xl font-bold text-gray-900 mt-1">{total}</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <p className="text-sm text-gray-500">Completed</p>
          <p className="text-3xl font-bold text-green-600 mt-1">{completed}</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <p className="text-sm text-gray-500">Failed</p>
          <p className="text-3xl font-bold text-red-500 mt-1">{failed}</p>
        </div>
      </div>

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
          <div className="px-6 py-8 text-sm text-gray-400 text-center">
            Loading...
          </div>
        ) : docs.length === 0 ? (
          <div className="px-6 py-8 text-sm text-gray-400 text-center">
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
                className="flex items-center justify-between px-6 py-4 hover:bg-gray-50 transition-colors"
              >
                <div>
                  <p className="text-sm font-medium text-gray-900">
                    {doc.fileName}
                  </p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    {new Date(doc.createdAt).toLocaleDateString()}
                  </p>
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

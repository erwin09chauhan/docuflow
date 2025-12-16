import { useParams, useNavigate } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { getDocumentById, getExtractedFields } from "../../api/documents";

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

const confidenceColor = (score: number) => {
  if (score >= 0.8) return "bg-green-500";
  if (score >= 0.5) return "bg-amber-400";
  return "bg-red-400";
};

const DocumentDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const queryClient = useQueryClient();

  const {
    data: doc,
    isLoading: docLoading,
    isError: docError,
  } = useQuery({
    queryKey: ["document", id],
    queryFn: () => getDocumentById(id!),
    enabled: !!id,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      if (status === "Completed" || status === "Failed") {
        queryClient.invalidateQueries({ queryKey: ["documents"] });
        return false;
      }
      return 3000;
    },
  });

  const { data: fields, isLoading: fieldsLoading } = useQuery({
    queryKey: ["extractedFields", id],
    queryFn: () => getExtractedFields(id!),
    enabled: !!id && doc?.status === "Completed",
  });

  if (docLoading) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="w-6 h-6 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (docError) {
    return (
      <div className="text-center py-16">
        <p className="text-sm font-medium text-red-600">
          Unable to connect to the server.
        </p>
        <p className="text-xs text-gray-500 mt-1">
          Make sure the API is running and try again.
        </p>
      </div>
    );
  }

  if (!doc) {
    return (
      <div className="text-sm text-gray-600 text-center py-16">
        Document not found.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <button
        onClick={() => navigate(-1)}
        className="flex items-center gap-1.5 text-sm font-medium text-gray-700 bg-white hover:text-gray-900 border border-gray-200 hover:border-gray-300 px-3 py-1.5 rounded-lg cursor-pointer transition-colors"
      >
        <svg
          className="w-4 h-4"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor"
          strokeWidth={2}
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M15 19l-7-7 7-7"
          />
        </svg>
        Back
      </button>
      {/* Document Info */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center flex-shrink-0">
              <svg
                className="w-5 h-5 text-blue-500"
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
              <h1 className="text-xl font-semibold text-gray-900">
                {doc.fileName}
              </h1>
              <p className="text-sm text-gray-600 mt-0.5">
                Uploaded {new Date(doc.createdAt).toLocaleDateString()}
              </p>
            </div>
          </div>
          <span
            className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColor(doc.status)}`}
          >
            {doc.status}
          </span>
        </div>

        <div className="grid grid-cols-3 gap-4 mt-6 pt-6 border-t border-gray-100">
          <div>
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">
              Schema
            </p>
            <p className="text-sm font-semibold text-gray-900 mt-1">
              {doc.schema}
            </p>
          </div>
          <div>
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">
              File Size
            </p>
            <p className="text-sm font-semibold text-gray-900 mt-1">
              {doc.sizeBytes < 1024
                ? `${doc.sizeBytes} B`
                : doc.sizeBytes < 1024 * 1024
                  ? `${(doc.sizeBytes / 1024).toFixed(1)} KB`
                  : `${(doc.sizeBytes / (1024 * 1024)).toFixed(1)} MB`}
            </p>
          </div>
          <div>
            <p className="text-xs text-gray-500 uppercase tracking-wide font-medium">
              Processed
            </p>
            <p className="text-sm font-semibold text-gray-900 mt-1">
              {doc.processedAt
                ? new Date(doc.processedAt).toLocaleDateString()
                : "—"}
            </p>
          </div>
        </div>
      </div>

      {/* Extracted Fields */}
      <div className="bg-white rounded-xl border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-100">
          <h2 className="text-sm font-semibold text-gray-900">
            Extracted Fields
          </h2>
        </div>

        {doc.status !== "Completed" ? (
          <div className="px-6 py-10 text-center">
            {doc.status === "Failed" ? (
              <p className="text-sm text-red-500">
                Extraction failed for this document.
              </p>
            ) : (
              <div className="flex flex-col items-center gap-3">
                <div className="w-6 h-6 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
                <p className="text-sm text-gray-600">
                  Extraction in progress. This page will update automatically.
                </p>
              </div>
            )}
          </div>
        ) : fieldsLoading ? (
          <div className="flex items-center justify-center py-10">
            <div className="w-6 h-6 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
          </div>
        ) : !fields || fields.length === 0 ? (
          <div className="px-6 py-10 text-sm text-gray-600 text-center">
            No fields extracted.
          </div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="text-xs text-gray-500 uppercase tracking-wide border-b border-gray-100">
                <th className="text-left px-6 py-3 font-medium">Field</th>
                <th className="text-left px-6 py-3 font-medium">Value</th>
                <th className="text-left px-6 py-3 font-medium">Confidence</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {fields.map((field) => (
                <tr
                  key={field.id}
                  className="hover:bg-slate-50 transition-colors"
                >
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    {field.fieldName}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-700">
                    {field.fieldValue}
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <div className="flex-1 bg-gray-100 rounded-full h-2 max-w-24">
                        <div
                          className={`h-2 rounded-full ${confidenceColor(field.confidenceScore)}`}
                          style={{ width: `${field.confidenceScore * 100}%` }}
                        />
                      </div>
                      <span className="text-xs font-medium text-gray-700">
                        {(field.confidenceScore * 100).toFixed(0)}%
                      </span>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default DocumentDetailPage;

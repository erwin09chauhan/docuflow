import { useParams, useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
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

const DocumentDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: doc, isLoading: docLoading } = useQuery({
    queryKey: ["document", id],
    queryFn: () => getDocumentById(id!),
    enabled: !!id,
  });

  const { data: fields, isLoading: fieldsLoading } = useQuery({
    queryKey: ["extractedFields", id],
    queryFn: () => getExtractedFields(id!),
    enabled: !!id && doc?.status === "Completed",
  });

  if (docLoading) {
    return (
      <div className="text-sm text-gray-400 text-center py-16">Loading...</div>
    );
  }

  if (!doc) {
    return (
      <div className="text-sm text-gray-400 text-center py-16">
        Document not found.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <button
          onClick={() => navigate(-1)}
          className="text-sm text-gray-500 hover:text-gray-900"
        >
          ← Back
        </button>
      </div>

      {/* Document Info */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <div className="flex items-start justify-between">
          <div>
            <h1 className="text-xl font-semibold text-gray-900">
              {doc.fileName}
            </h1>
            <p className="text-sm text-gray-400 mt-1">
              Uploaded {new Date(doc.createdAt).toLocaleDateString()}
            </p>
          </div>
          <span
            className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColor(doc.status)}`}
          >
            {doc.status}
          </span>
        </div>

        <div className="grid grid-cols-3 gap-4 mt-6">
          <div>
            <p className="text-xs text-gray-400 uppercase tracking-wide">
              Schema
            </p>
            <p className="text-sm font-medium text-gray-900 mt-1">
              {doc.schema}
            </p>
          </div>
          <div>
            <p className="text-xs text-gray-400 uppercase tracking-wide">
              File Size
            </p>
            <p className="text-sm font-medium text-gray-900 mt-1">
              {(doc.sizeBytes / 1024).toFixed(1)} KB
            </p>
          </div>
          <div>
            <p className="text-xs text-gray-400 uppercase tracking-wide">
              Processed
            </p>
            <p className="text-sm font-medium text-gray-900 mt-1">
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
          <div className="px-6 py-8 text-sm text-gray-400 text-center">
            {doc.status === "Failed"
              ? "Extraction failed for this document."
              : "Extraction is still in progress. Check back shortly."}
          </div>
        ) : fieldsLoading ? (
          <div className="px-6 py-8 text-sm text-gray-400 text-center">
            Loading fields...
          </div>
        ) : !fields || fields.length === 0 ? (
          <div className="px-6 py-8 text-sm text-gray-400 text-center">
            No fields extracted.
          </div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="text-xs text-gray-500 border-b border-gray-100">
                <th className="text-left px-6 py-3 font-medium">Field</th>
                <th className="text-left px-6 py-3 font-medium">Value</th>
                <th className="text-left px-6 py-3 font-medium">Confidence</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {fields.map((field) => (
                <tr key={field.id}>
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    {field.fieldName}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-600">
                    {field.fieldValue}
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-2">
                      <div className="flex-1 bg-gray-100 rounded-full h-1.5 max-w-24">
                        <div
                          className="bg-blue-500 h-1.5 rounded-full"
                          style={{ width: `${field.confidence * 100}%` }}
                        />
                      </div>
                      <span className="text-xs text-gray-500">
                        {(field.confidence * 100).toFixed(0)}%
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

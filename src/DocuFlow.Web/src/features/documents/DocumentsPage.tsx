import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { getDocuments, uploadDocument } from "../../api/documents";

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

const DocumentsPage = () => {
  const [page, setPage] = useState(1);
  const [uploading, setUploading] = useState(false);
  const [dragOver, setDragOver] = useState(false);
  const [schema, setSchema] = useState("Invoice");
  const queryClient = useQueryClient();
  const [uploadError, setUploadError] = useState<string | null>(null);
  const navigate = useNavigate();

  const { data, isLoading } = useQuery({
    queryKey: ["documents", page, 10],
    queryFn: () => getDocuments(page, 10),
  });

  const docs = data?.items ?? [];
  const totalPages = data?.totalPages ?? 1;

  const handleUpload = async (file: File) => {
    setUploadError(null);
    setUploading(true);
    try {
      await uploadDocument(file, schema);
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    } catch (err: unknown) {
      const error = err as {
        response?: { data?: { error?: string } };
        message?: string;
      };
      const message =
        error?.response?.data?.error ??
        error?.message ??
        "Upload failed. Please try again.";
      setUploadError(message);
    } finally {
      setUploading(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    const file = e.dataTransfer.files[0];
    if (file) handleUpload(file);
  };

  const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) handleUpload(file);
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900">Documents</h1>

      {/* Upload Area */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-sm font-semibold text-gray-900 mb-4">
          Upload Document
        </h2>

        <div className="flex items-center gap-3 mb-4">
          <label className="text-sm font-medium text-gray-600">Schema</label>
          <select
            value={schema}
            onChange={(e) => setSchema(e.target.value)}
            className="text-sm border border-gray-300 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
          >
            <option>Invoice</option>
            <option>Contract</option>
            <option>Receipt</option>
            <option>IdDocument</option>
          </select>
        </div>

        <div
          onDragOver={(e) => {
            e.preventDefault();
            setDragOver(true);
          }}
          onDragLeave={() => setDragOver(false)}
          onDrop={handleDrop}
          className={`border-2 border-dashed rounded-xl p-10 text-center transition-colors ${
            dragOver
              ? "border-blue-400 bg-blue-50"
              : "border-gray-200 hover:border-blue-300 hover:bg-slate-50"
          }`}
        >
          {uploading ? (
            <div className="flex flex-col items-center gap-2">
              <div className="w-8 h-8 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
              <p className="text-sm text-blue-600 font-medium">Uploading...</p>
            </div>
          ) : (
            <>
              <div className="flex justify-center mb-3">
                <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center">
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
                      d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
                    />
                  </svg>
                </div>
              </div>
              <p className="text-sm text-gray-700 mb-1">
                Drag and drop a file here, or
              </p>
              <label className="cursor-pointer text-sm text-blue-600 font-medium hover:underline">
                browse to upload
                <input
                  type="file"
                  className="hidden"
                  onChange={handleFileInput}
                  accept=".pdf,.txt,.csv,.xlsx,.xls"
                />
              </label>
              <p className="text-xs text-gray-600 mt-2">
                PDF, TXT, CSV, Excel up to 5MB
              </p>
            </>
          )}
        </div>

        {uploadError && (
          <p className="mt-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-4 py-2">
            {uploadError}
          </p>
        )}
      </div>

      {/* Documents Table */}
      <div className="bg-white rounded-xl border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-100">
          <h2 className="text-sm font-semibold text-gray-900">All Documents</h2>
        </div>

        {isLoading ? (
          <div className="px-6 py-10 text-sm text-gray-600 text-center">
            Loading...
          </div>
        ) : docs.length === 0 ? (
          <div className="px-6 py-10 text-sm text-gray-600 text-center">
            No documents found. Upload one above.
          </div>
        ) : (
          <>
            <table className="w-full">
              <thead>
                <tr className="text-xs text-gray-800 border-b border-gray-100 uppercase tracking-wide">
                  <th className="text-left px-6 py-3 font-medium">File Name</th>
                  <th className="text-left px-6 py-3 font-medium">Schema</th>
                  <th className="text-left px-6 py-3 font-medium">Uploaded</th>
                  <th className="text-left px-6 py-3 font-medium">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {docs.map((doc) => (
                  <tr
                    key={doc.id}
                    onClick={() => navigate(`/documents/${doc.id}`)}
                    className="hover:bg-slate-50 transition-colors cursor-pointer"
                  >
                    <td className="px-6 py-4">
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
                        <span className="text-sm font-medium text-gray-900">
                          {doc.fileName}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-700">
                      {doc.schema}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-700">
                      {new Date(doc.createdAt).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColor(doc.status)}`}
                      >
                        {doc.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="px-6 py-4 border-t border-gray-100 flex items-center justify-between">
              <p className="text-xs text-gray-600">
                Page {page} of {totalPages}
              </p>
              <div className="flex gap-2">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="text-sm px-3 py-1.5 border border-gray-200 rounded-lg disabled:opacity-40 hover:bg-slate-50 cursor-pointer transition-colors"
                >
                  Previous
                </button>
                <button
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="text-sm px-3 py-1.5 border border-gray-200 rounded-lg disabled:opacity-40 hover:bg-slate-50 cursor-pointer transition-colors"
                >
                  Next
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default DocumentsPage;

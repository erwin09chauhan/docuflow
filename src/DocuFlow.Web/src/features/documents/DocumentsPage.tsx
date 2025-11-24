import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
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

  const { data, isLoading } = useQuery({
    queryKey: ["documents", page, 10],
    queryFn: () => getDocuments(page, 10),
  });

  const docs = data?.items ?? [];
  const totalPages = data?.totalPages ?? 1;

  const handleUpload = async (file: File) => {
    setUploading(true);
    try {
      await uploadDocument(file, schema);
      queryClient.invalidateQueries({ queryKey: ["documents"] });
    } catch {
      alert("Upload failed. Please try again.");
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
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Documents</h1>
      </div>

      {/* Upload Area */}
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-sm font-semibold text-gray-900 mb-4">
          Upload Document
        </h2>
        <div className="flex items-center gap-4 mb-4">
          <label className="text-sm text-gray-600 font-medium">Schema</label>
          <select
            value={schema}
            onChange={(e) => setSchema(e.target.value)}
            className="text-sm border border-gray-300 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500"
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
              : "border-gray-200 hover:border-gray-300"
          }`}
        >
          {uploading ? (
            <p className="text-sm text-blue-600 font-medium">Uploading...</p>
          ) : (
            <>
              <p className="text-sm text-gray-500 mb-2">
                Drag and drop a file here, or
              </p>
              <label className="cursor-pointer text-sm text-blue-600 font-medium hover:underline">
                browse to upload
                <input
                  type="file"
                  className="hidden"
                  onChange={handleFileInput}
                  accept=".pdf,.png,.jpg,.jpeg"
                />
              </label>
              <p className="text-xs text-gray-400 mt-2">
                PDF, PNG, JPG up to 10MB
              </p>
            </>
          )}
        </div>
      </div>

      {/* Documents Table */}
      <div className="bg-white rounded-xl border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-100">
          <h2 className="text-sm font-semibold text-gray-900">All Documents</h2>
        </div>

        {isLoading ? (
          <div className="px-6 py-8 text-sm text-gray-400 text-center">
            Loading...
          </div>
        ) : docs.length === 0 ? (
          <div className="px-6 py-8 text-sm text-gray-400 text-center">
            No documents found. Upload one above.
          </div>
        ) : (
          <>
            <table className="w-full">
              <thead>
                <tr className="text-xs text-gray-500 border-b border-gray-100">
                  <th className="text-left px-6 py-3 font-medium">File Name</th>
                  <th className="text-left px-6 py-3 font-medium">Schema</th>
                  <th className="text-left px-6 py-3 font-medium">Uploaded</th>
                  <th className="text-left px-6 py-3 font-medium">Status</th>
                  <th className="px-6 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {docs.map((doc) => (
                  <tr
                    key={doc.id}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 text-sm font-medium text-gray-900">
                      {doc.fileName}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {doc.schema}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {new Date(doc.uploadedAt).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColor(doc.status)}`}
                      >
                        {doc.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right">
                      <Link
                        to={`/documents/${doc.id}`}
                        className="text-sm text-blue-600 hover:underline"
                      >
                        View
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            {/* Pagination */}
            <div className="px-6 py-4 border-t border-gray-100 flex items-center justify-between">
              <p className="text-xs text-gray-500">
                Page {page} of {totalPages}
              </p>
              <div className="flex gap-2">
                <button
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  className="text-sm px-3 py-1.5 border border-gray-200 rounded-lg disabled:opacity-40 hover:bg-gray-50"
                >
                  Previous
                </button>
                <button
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  className="text-sm px-3 py-1.5 border border-gray-200 rounded-lg disabled:opacity-40 hover:bg-gray-50"
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

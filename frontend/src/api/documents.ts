import apiClient from "../lib/axios";
import type {
  Document,
  DocumentStats,
  ExtractedField,
  PaginatedList,
} from "../types";

export const getDocuments = async (
  pageNumber = 1,
  pageSize = 10,
): Promise<PaginatedList<Document>> => {
  const response = await apiClient.get<PaginatedList<Document>>("/documents", {
    params: { pageNumber, pageSize },
  });
  return response.data;
};

export const getDocumentById = async (id: string): Promise<Document> => {
  const response = await apiClient.get<Document>(`/documents/${id}`);
  return response.data;
};

export const uploadDocument = async (
  file: File,
  schema: string,
): Promise<Document> => {
  const formData = new FormData();
  formData.append("file", file);
  formData.append("schema", schema);
  const response = await apiClient.post<Document>(
    "/documents/upload",
    formData,
    {
      headers: { "Content-Type": "multipart/form-data" },
    },
  );
  return response.data;
};

export const getExtractedFields = async (
  documentId: string,
): Promise<ExtractedField[]> => {
  const response = await apiClient.get<ExtractedField[]>(
    `/extractions/document/${documentId}`,
  );
  return response.data;
};

export const getDocumentStats = async (): Promise<DocumentStats> => {
  const response = await apiClient.get<DocumentStats>("/documents/stats");
  return response.data;
};

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: string;
  tenantId: string;
}

export interface AuthResponse {
  accessToken: string;
  user: User;
}

export interface Document {
  id: string;
  fileName: string;
  fileSize: number;
  status: "Pending" | "Processing" | "Completed" | "Failed";
  schema: string;
  uploadedAt: string;
  processedAt?: string;
  tenantId: string;
}

export interface ExtractedField {
  id: string;
  fieldName: string;
  fieldValue: string;
  confidence: number;
  documentId: string;
}

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}

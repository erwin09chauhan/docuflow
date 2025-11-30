import apiClient, { setAccessToken } from "../lib/axios";
import type { AuthResponse } from "../types";

export const login = async (
  email: string,
  password: string,
): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>("/auth/login", {
    email,
    password,
  });
  setAccessToken(response.data.accessToken);
  return response.data;
};

export const register = async (
  email: string,
  password: string,
  firstName: string,
  lastName: string,
  tenantName: string,
): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>("/auth/register", {
    email,
    password,
    firstName,
    lastName,
    tenantSlug: tenantName.toLowerCase().replace(/\s+/g, "-"),
  });
  setAccessToken(response.data.accessToken);
  return response.data;
};

export const logout = async (): Promise<void> => {
  await apiClient.post("/auth/logout");
  setAccessToken(null);
};

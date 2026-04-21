import axios from "axios";
import { useAuthStore } from "@/store/auth.store";

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5000/api/v1",
  headers: { "Content-Type": "application/json" },
});

http.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

http.interceptors.response.use(
  (res) => res,
  (error) => {
    const isAuthEndpoint = error.config?.url?.includes("/auth/");
    if (error.response?.status === 401 && !isAuthEndpoint) {
      useAuthStore.getState().clearSession();
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

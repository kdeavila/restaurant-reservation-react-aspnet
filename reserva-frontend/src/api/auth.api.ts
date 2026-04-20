import { http } from "./http";
import type { ApiResponse, AuthUser, CreateUserDto } from "@/types";

export const authApi = {
  login: async (dto: { email: string; password: string }) => {
    const { data } = await http.post<ApiResponse<AuthUser>>("/auth/login", dto);
    return data;
  },
  register: async (dto: CreateUserDto) => {
    const { data } = await http.post<ApiResponse<AuthUser>>("/auth/register", dto);
    return data;
  },
};

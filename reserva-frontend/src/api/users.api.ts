import { http } from "./http";
import type { ApiResponse, User } from "@/types";

export const usersApi = {
  list: async () => {
    const { data } = await http.get<ApiResponse<User[]>>("/users");
    return data;
  },
  detail: async (id: number) => {
    const { data } = await http.get<ApiResponse<User>>(`/users/${id}`);
    return data;
  },
  remove: async (id: number) => {
    const { data } = await http.delete<ApiResponse<void>>(`/users/${id}`);
    return data;
  },
};

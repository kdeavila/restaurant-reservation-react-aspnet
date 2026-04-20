import { http } from "./http";
import type { ApiResponse, TableType } from "@/types";

export const tableTypesApi = {
  list: async () => {
    const { data } = await http.get<ApiResponse<TableType[]>>("/table-types");
    return data;
  },
  detail: async (id: number) => {
    const { data } = await http.get<ApiResponse<TableType>>(`/table-types/${id}`);
    return data;
  },
  create: async (dto: { name: string; basePricePerHour: number; description?: string }) => {
    const { data } = await http.post<ApiResponse<TableType>>("/table-types", dto);
    return data;
  },
  update: async (id: number, dto: { name?: string; basePricePerHour?: number; description?: string; isActive?: boolean }) => {
    const { data } = await http.patch<ApiResponse<TableType>>(`/table-types/${id}`, dto);
    return data;
  },
  remove: async (id: number) => {
    const { data } = await http.delete<ApiResponse<void>>(`/table-types/${id}`);
    return data;
  },
};

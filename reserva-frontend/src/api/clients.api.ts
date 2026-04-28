import type { ApiResponse, Client, CreateClientDto, Pagination } from "@/types"
import { http } from "./http"

export const clientsApi = {
  list: async (params: Record<string, unknown>) => {
    const { data } = await http.get<ApiResponse<Client[] & { pagination: Pagination }>>(
      "/clients",
      { params },
    )
    return data
  },
  detail: async (id: number) => {
    const { data } = await http.get<ApiResponse<Client>>(`/clients/${id}`)
    return data
  },
  create: async (dto: CreateClientDto) => {
    const { data } = await http.post<ApiResponse<Client>>("/clients", dto)
    return data
  },
  update: async (id: number, dto: Partial<CreateClientDto>) => {
    const { data } = await http.patch<ApiResponse<Client>>(`/clients/${id}`, dto)
    return data
  },
  remove: async (id: number) => {
    const { data } = await http.delete<ApiResponse<void>>(`/clients/${id}`)
    return data
  },
}

import type { ApiResponse, CreateTableDto, Pagination, TableDetailed } from "@/types"
import { http } from "./http"

export const tablesApi = {
  list: async (params: Record<string, unknown>) => {
    const { data } = await http.get<ApiResponse<TableDetailed[] & { pagination: Pagination }>>(
      "/tables",
      { params },
    )
    return data
  },
  available: async (params: {
    date: string
    startTime: string
    endTime: string
    numberOfGuests: number
  }) => {
    const { data } = await http.get<ApiResponse<TableDetailed[]>>("/tables/available", { params })
    return data
  },
  detail: async (id: number) => {
    const { data } = await http.get<ApiResponse<TableDetailed>>(`/tables/${id}`)
    return data
  },
  create: async (dto: CreateTableDto) => {
    const { data } = await http.post<ApiResponse<TableDetailed>>("/tables", dto)
    return data
  },
  update: async (id: number, dto: Partial<Omit<CreateTableDto, "tableTypeId">>) => {
    const { data } = await http.patch<ApiResponse<TableDetailed>>(`/tables/${id}`, dto)
    return data
  },
  remove: async (id: number) => {
    const { data } = await http.delete<ApiResponse<void>>(`/tables/${id}`)
    return data
  },
}

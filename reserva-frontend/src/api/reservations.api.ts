import { http } from "./http";
import type { ApiResponse, Reservation, CreateReservationDto, UpdateReservationDto, Pagination } from "@/types";

export const reservationsApi = {
  list: async (params: Record<string, unknown>) => {
    const { data } = await http.get<ApiResponse<Reservation[] & { pagination: Pagination }>>("/reservations", { params });
    return data;
  },
  detail: async (id: number) => {
    const { data } = await http.get<ApiResponse<Reservation>>(`/reservations/${id}`);
    return data;
  },
  create: async (dto: CreateReservationDto) => {
    const { data } = await http.post<ApiResponse<Reservation>>("/reservations", dto);
    return data;
  },
  update: async (id: number, dto: UpdateReservationDto) => {
    const { data } = await http.patch<ApiResponse<Reservation>>(`/reservations/${id}`, dto);
    return data;
  },
  remove: async (id: number) => {
    const { data } = await http.delete<ApiResponse<void>>(`/reservations/${id}`);
    return data;
  },
};

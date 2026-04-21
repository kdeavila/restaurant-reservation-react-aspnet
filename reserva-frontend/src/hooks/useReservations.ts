import { useQuery, useMutation, useQueryClient, keepPreviousData } from "@tanstack/react-query";
import { reservationsApi } from "@/api/reservations.api";
import { qk } from "./query-keys";
import { toast } from "sonner";

export const useReservationList = (params: Record<string, unknown>) =>
  useQuery({
    queryKey: qk.reservations(params),
    queryFn: () => reservationsApi.list(params),
    placeholderData: keepPreviousData,
  });

export const useReservationDetail = (id: number) =>
  useQuery({ queryKey: qk.reservation(id), queryFn: () => reservationsApi.detail(id), enabled: !!id });

export const useCreateReservation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: reservationsApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.reservations() });
      toast.success("Reserva creada");
    },
    onError: (e: Error) => toast.error(e.message),
  });
};

export const useUpdateReservation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: Record<string, unknown> }) =>
      reservationsApi.update(id, dto),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: qk.reservations() });
      qc.invalidateQueries({ queryKey: qk.reservation(id) });
      toast.success("Reserva actualizada");
    },
    onError: (e: Error) => toast.error(e.message),
  });
};

export const useDeleteReservation = () => {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: reservationsApi.remove,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.reservations() });
      toast.success("Reserva eliminada");
    },
    onError: (e: Error) => toast.error(e.message),
  });
};

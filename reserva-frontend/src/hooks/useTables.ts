import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { tablesApi } from "@/api/tables.api"
import { qk } from "./query-keys"

export const useTableList = (params: Record<string, unknown>) =>
  useQuery({ queryKey: qk.tables(params), queryFn: () => tablesApi.list(params) })

export const useAvailableTables = (
  params: { date: string; startTime: string; endTime: string; numberOfGuests: number },
  options?: { enabled?: boolean },
) =>
  useQuery({
    queryKey: qk.available(params),
    queryFn: () => tablesApi.available(params),
    enabled: options?.enabled ?? false,
  })

export const useTableDetail = (id: number) =>
  useQuery({ queryKey: qk.tables({ id }), queryFn: () => tablesApi.detail(id), enabled: !!id })

export const useCreateTable = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: tablesApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.tables() })
      toast.success("Mesa creada")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

export const useUpdateTable = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: Record<string, unknown> }) =>
      tablesApi.update(id, dto),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: qk.tables() })
      qc.invalidateQueries({ queryKey: qk.tables({ id }) })
      toast.success("Mesa actualizada")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

export const useDeleteTable = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: tablesApi.remove,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.tables() })
      toast.success("Mesa eliminada")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { tableTypesApi } from "@/api/table-types.api"
import { qk } from "./query-keys"

export const useTableTypeList = () =>
  useQuery({ queryKey: qk.tableTypes(), queryFn: () => tableTypesApi.list() })

export const useTableTypeDetail = (id: number) =>
  useQuery({ queryKey: qk.tableType(id), queryFn: () => tableTypesApi.detail(id), enabled: !!id })

export const useCreateTableType = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: tableTypesApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.tableTypes() })
      toast.success("Tipo de mesa creado")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

export const useUpdateTableType = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: Record<string, unknown> }) =>
      tableTypesApi.update(id, dto),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: qk.tableTypes() })
      qc.invalidateQueries({ queryKey: qk.tableType(id) })
      toast.success("Tipo de mesa actualizado")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

export const useDeleteTableType = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: tableTypesApi.remove,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.tableTypes() })
      toast.success("Tipo de mesa eliminado")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

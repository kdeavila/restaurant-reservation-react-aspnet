import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { clientsApi } from "@/api/clients.api"
import { qk } from "./query-keys"

export const useClientList = (params: Record<string, unknown>) =>
  useQuery({ queryKey: qk.clients(params), queryFn: () => clientsApi.list(params) })

export const useClientDetail = (id: number) =>
  useQuery({ queryKey: qk.client(id), queryFn: () => clientsApi.detail(id), enabled: !!id })

export const useCreateClient = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: clientsApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.clients() })
      toast.success("Cliente creado")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

export const useUpdateClient = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, dto }: { id: number; dto: Partial<Record<string, unknown>> }) =>
      clientsApi.update(id, dto),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: qk.clients() })
      qc.invalidateQueries({ queryKey: qk.client(id) })
      toast.success("Cliente actualizado")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

export const useDeleteClient = () => {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: clientsApi.remove,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: qk.clients() })
      toast.success("Cliente eliminado")
    },
    onError: (e: Error) => toast.error(e.message),
  })
}

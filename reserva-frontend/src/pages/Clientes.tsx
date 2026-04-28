import axios from "axios"
import { Plus, Search, X } from "lucide-react"
import { useCallback, useMemo, useState } from "react"
import { ConfirmDialog } from "@/components/molecules/ConfirmDialog"
import { DataTable } from "@/components/molecules/DataTable"
import { EmptyState } from "@/components/molecules/EmptyState"
import { ErrorState } from "@/components/molecules/ErrorState"
import { PageHeader } from "@/components/molecules/PageHeader"
import { Pagination } from "@/components/molecules/Pagination"
import { ClientFormDialog } from "@/components/organisms/ClientFormDialog"
import { ClientReservationsDialog } from "@/components/organisms/ClientReservationsDialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  useClientList,
  useCreateClient,
  useDeleteClient,
  useUpdateClient,
} from "@/hooks/useClients"
import { useClientsTableColumns } from "@/hooks/useClientsTableColumns"
import { useReservationList } from "@/hooks/useReservations"
import { useAuthStore } from "@/store/auth.store"
import type { Client } from "@/types"

type ClientFormValues = Omit<Client, "id" | "status" | "totalReservations" | "createdAt">

export default function Clientes() {
  const role = useAuthStore((state) => state.role)
  const createMutation = useCreateClient()
  const updateMutation = useUpdateClient()
  const deleteMutation = useDeleteClient()

  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingClient, setEditingClient] = useState<Client | null>(null)
  const [clientToDelete, setClientToDelete] = useState<Client | null>(null)
  const [viewingClientId, setViewingClientId] = useState<number | null>(null)
  const [formError, setFormError] = useState<string | null>(null)

  const queryParams = useMemo(
    () => ({
      firstName: search,
      email: search,
      phone: search,
      page,
      pageSize,
    }),
    [search, page, pageSize],
  )

  const clientsQuery = useClientList(queryParams)
  const clients = clientsQuery.data?.data ?? []
  const pagination = clientsQuery.data?.pagination

  const reservationsQuery = useReservationList({
    clientId: viewingClientId,
    pageSize: 100,
  })
  const clientReservations = (reservationsQuery.data?.data ?? []).filter(
    (r) => r.client.id === viewingClientId,
  )

  const columns = useClientsTableColumns(
    role,
    setViewingClientId,
    (client) => {
      setEditingClient(client)
      setFormOpen(true)
    },
    setClientToDelete,
  )

  const handleFormSubmit = useCallback(
    async (values: ClientFormValues) => {
      setFormError(null)
      try {
        if (editingClient) {
          await updateMutation.mutateAsync({
            id: editingClient.id,
            dto: values,
          })
        } else {
          await createMutation.mutateAsync(values)
        }
        setFormOpen(false)
        setEditingClient(null)
        setFormError(null)
      } catch (error) {
        if (axios.isAxiosError(error)) {
          const msg = error.response?.data?.message || error.message
          if (msg?.includes("email")) {
            setFormError("Este email ya está registrado")
          } else {
            setFormError(msg || "Error al guardar cliente")
          }
        } else if (error instanceof Error) {
          setFormError(error.message)
        } else {
          setFormError("Error desconocido")
        }
      }
    },
    [editingClient, createMutation, updateMutation],
  )

  const handleFormOpenChange = useCallback((open: boolean) => {
    setFormOpen(open)
    if (!open) {
      setEditingClient(null)
      setFormError(null)
    }
  }, [])

  const handleDelete = useCallback(async () => {
    if (!clientToDelete) return
    try {
      await deleteMutation.mutateAsync(clientToDelete.id)
      setClientToDelete(null)
    } catch (error) {
      console.error("Delete error:", error)
    }
  }, [clientToDelete, deleteMutation])

  const hasError = clientsQuery.isError
  const isLoading = clientsQuery.isLoading
  const isFetching = clientsQuery.isFetching

  return (
    <main>
      <PageHeader
        title="Clientes"
        description="Gestiona la base de clientes del restaurante."
        actions={
          <Button onClick={() => setFormOpen(true)}>
            <Plus className="size-4" />
            Nuevo cliente
          </Button>
        }
      />

      <div className="filter-bar px-8 py-4 flex items-center gap-3">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-fg pointer-events-none" />
          <Input
            placeholder="Buscar por nombre, correo o teléfono…"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value)
              setPage(1)
            }}
            className="w-[320px] pl-9"
          />
        </div>
        {search && (
          <Button variant="ghost" size="sm" onClick={() => setSearch("")}>
            <X className="size-4" />
            Limpiar
          </Button>
        )}
      </div>

      <section className="px-8 py-8 space-y-6">
        {hasError ? (
          <ErrorState
            message="No se pudieron cargar los clientes."
            onRetry={() => clientsQuery.refetch()}
          />
        ) : clients.length === 0 && !isLoading && !isFetching ? (
          <EmptyState
            icon={<Plus className="size-7 text-primary" />}
            title="Sin clientes"
            description="Crea el primer cliente para empezar a registrar reservas."
            onAction={() => setFormOpen(true)}
          />
        ) : (
          <div className={isFetching && !isLoading && clients.length > 0 ? "opacity-50" : ""}>
            <DataTable
              columns={columns}
              data={clients}
              loading={isLoading || (isFetching && clients.length === 0)}
            />
          </div>
        )}

        {pagination && clients.length > 0 && (
          <Pagination
            page={pagination.page}
            pageSize={pagination.pageSize}
            totalCount={pagination.totalCount}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
          />
        )}
      </section>

      <ClientFormDialog
        open={formOpen}
        onOpenChange={handleFormOpenChange}
        client={editingClient}
        onSubmit={handleFormSubmit}
        isPending={createMutation.isPending || updateMutation.isPending}
        error={formError}
      />

      <ClientReservationsDialog
        open={viewingClientId !== null}
        onOpenChange={() => setViewingClientId(null)}
        reservations={clientReservations}
      />

      <ConfirmDialog
        open={Boolean(clientToDelete)}
        onOpenChange={(open) => !open && setClientToDelete(null)}
        title="Eliminar cliente"
        description="Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        variant="destructive"
        loading={deleteMutation.isPending}
        onConfirm={handleDelete}
      />
    </main>
  )
}

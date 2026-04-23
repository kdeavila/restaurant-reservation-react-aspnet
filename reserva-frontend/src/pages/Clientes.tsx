import { useMemo, useState } from "react";
import { useForm, type SubmitHandler } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { createColumnHelper } from "@tanstack/react-table";
import { Eye, Pencil, Plus, Search, Trash2, X } from "lucide-react";
import { useAuthStore } from "@/store/auth.store";
import {
  useClientList,
  useCreateClient,
  useUpdateClient,
  useDeleteClient,
} from "@/hooks/useClients";
import { useReservationList } from "@/hooks/useReservations";
import { PageHeader } from "@/components/molecules/PageHeader";
import { DataTable } from "@/components/molecules/DataTable";
import { Pagination } from "@/components/molecules/Pagination";
import { EmptyState } from "@/components/molecules/EmptyState";
import { ErrorState } from "@/components/molecules/ErrorState";
import { ConfirmDialog } from "@/components/molecules/ConfirmDialog";
import { FormField } from "@/components/molecules/FormField";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { StatusBadge } from "@/components/atoms/StatusBadge";
import { can } from "@/lib/permissions";
import { formatDate } from "@/lib/format";
import type { Client } from "@/types";

const clientSchema = z.object({
  firstName: z.string().min(1, "Nombre requerido"),
  lastName: z.string().min(1, "Apellido requerido"),
  email: z.string().email("Email inválido"),
  phone: z.string().optional(),
});

type ClientFormValues = z.infer<typeof clientSchema>;

const columnHelper = createColumnHelper<Client>();

export default function Clientes() {
  const role = useAuthStore((state) => state.role);
  const createMutation = useCreateClient();
  const updateMutation = useUpdateClient();
  const deleteMutation = useDeleteClient();

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [formOpen, setFormOpen] = useState(false);
  const [editingClient, setEditingClient] = useState<Client | null>(null);
  const [clientToDelete, setClientToDelete] = useState<Client | null>(null);
  const [viewingClientId, setViewingClientId] = useState<number | null>(null);
  const [formError, setFormError] = useState<string | null>(null);

  const queryParams = useMemo(
    () => ({
      firstName: search,
      email: search,
      phone: search,
      page,
      pageSize,
    }),
    [search, page, pageSize],
  );

  const clientsQuery = useClientList(queryParams);
  const clients = clientsQuery.data?.data ?? [];
  const pagination = clientsQuery.data?.pagination;

  const reservationsQuery = useReservationList({
    clientId: viewingClientId,
    pageSize: 100,
  });
  const clientReservations = (reservationsQuery.data?.data ?? []).filter(
    (r) => r.client.id === viewingClientId,
  );

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ClientFormValues>({
    resolver: zodResolver(clientSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      phone: "",
    },
  });

  const onSubmit: SubmitHandler<ClientFormValues> = async (values) => {
    setFormError(null);
    try {
      if (editingClient) {
        await updateMutation.mutateAsync({
          id: editingClient.id,
          dto: values,
        });
      } else {
        await createMutation.mutateAsync(values);
      }
      setFormOpen(false);
      setEditingClient(null);
      reset();
    } catch (error: any) {
      const msg = error.response?.data?.message || error.message;
      if (msg?.includes("email")) {
        setFormError("Este email ya está registrado");
      } else {
        setFormError(msg || "Error al guardar cliente");
      }
    }
  };

  const openForm = (client?: Client) => {
    if (client) {
      setEditingClient(client);
      reset({
        firstName: client.firstName,
        lastName: client.lastName,
        email: client.email,
        phone: client.phone ?? "",
      });
    } else {
      setEditingClient(null);
      reset();
    }
    setFormError(null);
    setFormOpen(true);
  };

  const closeForm = () => {
    setFormOpen(false);
    setEditingClient(null);
    setFormError(null);
    reset();
  };

  const handleDelete = async () => {
    if (!clientToDelete) return;
    await deleteMutation.mutateAsync(clientToDelete.id);
    setClientToDelete(null);
  };

  const columns = useMemo(
    () => [
      columnHelper.display({
        id: "avatar",
        cell: ({ row }) => (
          <div className="ml-auto h-8 w-8 rounded-full bg-muted flex items-center justify-center text-xs font-medium text-foreground">
            {row.original.firstName[0]}
            {row.original.lastName[0]}
          </div>
        ),
      }),
      columnHelper.accessor((row) => `${row.firstName} ${row.lastName}`, {
        id: "fullName",
        header: "Nombre completo",
        cell: ({ row }) => (
          <span className="font-medium">
            {row.original.firstName} {row.original.lastName}
          </span>
        ),
      }),
      columnHelper.accessor((row) => row.email, {
        id: "email",
        header: "Correo",
        cell: ({ row }) => (
          <span className="text-muted-fg">{row.original.email}</span>
        ),
      }),
      columnHelper.accessor((row) => row.phone, {
        id: "phone",
        header: "Teléfono",
        cell: ({ row }) => (
          <span className="text-muted-fg">{row.original.phone ?? "—"}</span>
        ),
      }),
      columnHelper.accessor((row) => row.status, {
        id: "status",
        header: "Estado",
        cell: ({ row }) => <StatusBadge status={row.original.status} />,
      }),
      columnHelper.accessor((row) => row.totalReservations, {
        id: "reservations",
        header: "Reservas",
        cell: ({ row }) => (
          <span className="tabular-nums text-right">
            {row.original.totalReservations}
          </span>
        ),
      }),
      columnHelper.accessor((row) => row.createdAt, {
        id: "created",
        header: "Registrado",
        cell: ({ row }) => (
          <span className="text-muted-fg">
            {formatDate(row.original.createdAt)}
          </span>
        ),
      }),
      columnHelper.display({
        id: "actions",
        header: "Acciones",
        cell: ({ row }) => (
          <div className="flex items-center justify-end gap-1">
            <Button
              size="sm"
              variant="ghost"
              onClick={() => setViewingClientId(row.original.id)}
            >
              <Eye className="size-4" />
            </Button>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => openForm(row.original)}
            >
              <Pencil className="size-4" />
            </Button>
            <Button
              size="sm"
              variant="ghost"
              disabled={!can.deleteClient(role ?? "Employee")}
              onClick={() => setClientToDelete(row.original)}
              className={
                !can.deleteClient(role ?? "Employee")
                  ? "text-muted-fg"
                  : "text-destructive hover:text-destructive"
              }
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        ),
      }),
    ],
    [role],
  );

  const hasError = clientsQuery.isError;
  const isLoading = clientsQuery.isLoading;
  const isFetching = clientsQuery.isFetching;

  return (
    <main>
      <PageHeader
        title="Clientes"
        description="Gestiona la base de clientes del restaurante."
        actions={
          <Button onClick={() => openForm()}>
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
              setSearch(e.target.value);
              setPage(1);
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
            onAction={() => openForm()}
          />
        ) : (
          <div
            className={
              isFetching && !isLoading && clients.length > 0 ? "opacity-50" : ""
            }
          >
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

      {/* Form Dialog */}
      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent className="sm:max-w-100">
          <DialogHeader>
            <DialogTitle>
              {editingClient ? "Editar cliente" : "Nuevo cliente"}
            </DialogTitle>
            <DialogDescription>
              {editingClient
                ? "Actualiza los datos del cliente"
                : "Ingresa los datos del nuevo cliente"}
            </DialogDescription>
          </DialogHeader>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            {formError && (
              <div className="rounded bg-destructive/10 p-3 text-sm text-destructive">
                {formError}
              </div>
            )}

            <FormField
              label="Nombre"
              required
              error={errors.firstName?.message}
            >
              <Input {...register("firstName")} placeholder="Ej: Juan" />
            </FormField>

            <FormField
              label="Apellido"
              required
              error={errors.lastName?.message}
            >
              <Input {...register("lastName")} placeholder="Ej: Pérez" />
            </FormField>

            <FormField
              label="Correo electrónico"
              required
              error={errors.email?.message}
            >
              <Input
                type="email"
                {...register("email")}
                placeholder="ej: juan@ejemplo.com"
              />
            </FormField>

            <FormField label="Teléfono" error={errors.phone?.message}>
              <Input
                {...register("phone")}
                placeholder="+34 612 345 678 (opcional)"
              />
            </FormField>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={closeForm}>
                Cancelar
              </Button>
              <Button
                type="submit"
                disabled={
                  isSubmitting ||
                  createMutation.isPending ||
                  updateMutation.isPending
                }
              >
                {isSubmitting ||
                createMutation.isPending ||
                updateMutation.isPending
                  ? "Guardando…"
                  : "Guardar"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Viewing history dialog */}
      <Dialog
        open={viewingClientId !== null}
        onOpenChange={() => setViewingClientId(null)}
      >
        <DialogContent className="sm:max-w-150">
          <DialogHeader>
            <DialogTitle>Historial de reservas</DialogTitle>
            <DialogDescription>
              Todas las reservas del cliente
            </DialogDescription>
          </DialogHeader>

          <div className="max-h-100 overflow-y-auto">
            {clientReservations.length === 0 ? (
              <div className="text-center py-8 text-muted-fg">
                No hay reservas para este cliente
              </div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Fecha</TableHead>
                    <TableHead>Horario</TableHead>
                    <TableHead>Mesa</TableHead>
                    <TableHead>Comensales</TableHead>
                    <TableHead>Estado</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {clientReservations.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell className="text-sm">{r.date}</TableCell>
                      <TableCell className="text-sm tabular-nums">
                        {r.startTime.slice(0, 5)} – {r.endTime.slice(0, 5)}
                      </TableCell>
                      <TableCell className="text-sm">{r.table.code}</TableCell>
                      <TableCell className="text-sm tabular-nums">
                        {r.numberOfGuests}
                      </TableCell>
                      <TableCell className="text-sm">
                        <StatusBadge status={r.status} />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </div>
        </DialogContent>
      </Dialog>

      {/* Delete confirm dialog */}
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
  );
}

import { useMemo, useState } from "react";
import { cn } from "@/lib/utils";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { createColumnHelper } from "@tanstack/react-table";
import { CalendarDays, Eye, Trash2, X } from "lucide-react";
import { useAuthStore } from "@/store/auth.store";
import { useReservationList, useDeleteReservation, useUpdateReservation } from "@/hooks/useReservations";
import { PageHeader } from "@/components/molecules/PageHeader";
import { DataTable } from "@/components/molecules/DataTable";
import { Pagination } from "@/components/molecules/Pagination";
import { EmptyState } from "@/components/molecules/EmptyState";
import { ErrorState } from "@/components/molecules/ErrorState";
import { ConfirmDialog } from "@/components/molecules/ConfirmDialog";
import { FormField } from "@/components/molecules/FormField";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { StatusBadge } from "@/components/atoms/StatusBadge";
import { can } from "@/lib/permissions";
import { formatCurrency, todayISO } from "@/lib/format";
import type { Reservation } from "@/types";

const filtersSchema = z.object({
  date: z.string().min(1),
  status: z.enum(["all", "Pending", "Confirmed", "Completed", "Cancelled"]),
});

type FiltersFormValues = z.infer<typeof filtersSchema>;

const columnHelper = createColumnHelper<Reservation>();

export default function Reservas() {
  const navigate = useNavigate();
  const role = useAuthStore((state) => state.role);
  const deleteReservationMutation = useDeleteReservation();
  const updateReservationMutation = useUpdateReservation();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [reservationToCancel, setReservationToCancel] = useState<Reservation | null>(null);
  const [reservationToDelete, setReservationToDelete] = useState<Reservation | null>(null);

  const {
    register,
    watch,
    setValue,
    formState: { errors },
  } = useForm<FiltersFormValues>({
    resolver: zodResolver(filtersSchema),
    defaultValues: { date: todayISO(), status: "all" },
  });

  const date = watch("date");
  const status = watch("status");

  const queryParams = useMemo(
    () => ({
      date,
      status: status === "all" ? undefined : status,
      page,
      pageSize,
    }),
    [date, page, pageSize, status],
  );

  const reservationsQuery = useReservationList(queryParams);
  const reservations = reservationsQuery.data?.data ?? [];
  const pagination = reservationsQuery.data?.pagination;

  const columns = useMemo(
    () => [
      columnHelper.accessor((row) => row.client, {
        id: "client",
        header: "Cliente",
        cell: ({ row }) => (
          <div>
            <div className="font-medium text-foreground">
              {row.original.client.firstName} {row.original.client.lastName}
            </div>
            <div className="text-xs text-muted-fg">{row.original.client.email}</div>
          </div>
        ),
      }),
      columnHelper.accessor((row) => row.table.code, {
        id: "table",
        header: "Mesa",
        cell: ({ row }) => <span className="font-medium">{row.original.table.code}</span>,
      }),
      columnHelper.accessor((row) => row.date, {
        id: "dateTime",
        header: "Fecha y horario",
        cell: ({ row }) => (
          <div>
            <div>{row.original.date}</div>
            <div className="text-xs text-muted-fg">
              {row.original.startTime.slice(0, 5)} - {row.original.endTime.slice(0, 5)}
            </div>
          </div>
        ),
      }),
      columnHelper.accessor((row) => row.numberOfGuests, {
        id: "guests",
        header: "Comensales",
        cell: ({ row }) => <span>{row.original.numberOfGuests}</span>,
      }),
      columnHelper.accessor((row) => row.totalPrice, {
        id: "total",
        header: "Total",
        cell: ({ row }) => <span className="font-medium">{formatCurrency(row.original.totalPrice)}</span>,
      }),
      columnHelper.accessor((row) => row.status, {
        id: "status",
        header: "Estado",
        cell: ({ row }) => <StatusBadge status={row.original.status} />,
      }),
      columnHelper.display({
        id: "actions",
        header: "Acciones",
        cell: ({ row }) => {
          const reservation = row.original;
          const canCancel = (reservation.status === "Pending" || reservation.status === "Confirmed") && can.cancelReservation(role ?? "Employee");
          const canDelete = role ? can.deleteReservation(role) : false;

          return (
            <div className="flex flex-wrap gap-2">
              <Button size="sm" variant="outline" onClick={() => navigate(`/reservas/${reservation.id}`)}>
                <Eye className="size-4" />
                Ver
              </Button>
              {canCancel && (
                <Button size="sm" variant="outline" onClick={() => setReservationToCancel(reservation)}>
                  <X className="size-4" />
                  Cancelar
                </Button>
              )}
              {canDelete && (
                <Button size="sm" variant="destructive" onClick={() => setReservationToDelete(reservation)}>
                  <Trash2 className="size-4" />
                  Eliminar
                </Button>
              )}
            </div>
          );
        },
      }),
    ],
    [navigate, role],
  );

  const clearFilters = () => {
    setValue("date", todayISO());
    setValue("status", "all");
    setPage(1);
  };

  const handleCancel = async () => {
    if (!reservationToCancel) {
      return;
    }

    await updateReservationMutation.mutateAsync({
      id: reservationToCancel.id,
      dto: { status: "Cancelled" },
    });

    setReservationToCancel(null);
  };

  const handleDelete = async () => {
    if (!reservationToDelete) {
      return;
    }

    await deleteReservationMutation.mutateAsync(reservationToDelete.id);
    setReservationToDelete(null);
  };

  const hasError = reservationsQuery.isError;
  const isLoading = reservationsQuery.isLoading;
  const isFetching = reservationsQuery.isFetching;

  return (
    <main>
      <PageHeader
        title="Reservas"
        actions={
          <Button onClick={() => navigate("/reservas/nueva")}>
            Nueva reserva
          </Button>
        }
      />

      <section className="filter-bar px-8 py-5">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end">
          <div className="grid flex-1 gap-4 md:grid-cols-[220px_220px]">
            <FormField label="Fecha" error={errors.date?.message} required>
              <Input type="date" min={todayISO()} {...register("date")} />
            </FormField>

            <FormField label="Estado" error={errors.status?.message} required>
              <Select value={status} onValueChange={(value) => setValue("status", value as FiltersFormValues["status"]) }>
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="Pending">Pending</SelectItem>
                  <SelectItem value="Confirmed">Confirmed</SelectItem>
                  <SelectItem value="Completed">Completed</SelectItem>
                  <SelectItem value="Cancelled">Cancelled</SelectItem>
                </SelectContent>
              </Select>
            </FormField>
          </div>

          <div className="flex gap-3">
            <Button type="button" variant="outline" onClick={clearFilters}>
              Limpiar filtros
            </Button>
          </div>
        </div>
      </section>

      <section className="px-8 py-8 space-y-6">
        {hasError ? (
          <ErrorState message="No se pudieron cargar las reservas." onRetry={() => reservationsQuery.refetch()} />
        ) : reservations.length === 0 && !isLoading && !isFetching ? (
          <EmptyState
            icon={<CalendarDays className="size-7 text-primary" />}
            title="Sin reservas"
            description="No hay reservas para los filtros actuales."
          />
        ) : (
          <div className={cn("transition-opacity duration-150", isFetching && !isLoading && reservations.length > 0 && "opacity-50")}>
            <DataTable columns={columns} data={reservations} loading={isLoading || (isFetching && reservations.length === 0)} />
          </div>
        )}

        {pagination && reservations.length > 0 && (
          <Pagination
            page={pagination.page}
            pageSize={pagination.pageSize}
            totalCount={pagination.totalCount}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
          />
        )}
      </section>

      <ConfirmDialog
        open={Boolean(reservationToCancel)}
        onOpenChange={(open) => !open && setReservationToCancel(null)}
        title="Cancelar reserva"
        description="Esta acción cambiará el estado de la reserva a Cancelled."
        confirmLabel="Cancelar reserva"
        variant="destructive"
        loading={updateReservationMutation.isPending}
        onConfirm={handleCancel}
      />

      <ConfirmDialog
        open={Boolean(reservationToDelete)}
        onOpenChange={(open) => !open && setReservationToDelete(null)}
        title="Eliminar reserva"
        description="Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        variant="destructive"
        loading={deleteReservationMutation.isPending}
        onConfirm={handleDelete}
      />
    </main>
  );
}

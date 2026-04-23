import { useMemo, useState } from "react";
import { cn } from "@/lib/utils";
import { useNavigate } from "react-router-dom";
import { createColumnHelper } from "@tanstack/react-table";
import { CalendarDays, Eye, Pencil, Search, X } from "lucide-react";
import { useAuthStore } from "@/store/auth.store";
import {
  useReservationList,
  useDeleteReservation,
  useUpdateReservation,
} from "@/hooks/useReservations";
import { PageHeader } from "@/components/molecules/PageHeader";
import { DataTable } from "@/components/molecules/DataTable";
import { Pagination } from "@/components/molecules/Pagination";
import { EmptyState } from "@/components/molecules/EmptyState";
import { ErrorState } from "@/components/molecules/ErrorState";
import { ConfirmDialog } from "@/components/molecules/ConfirmDialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { StatusBadge } from "@/components/atoms/StatusBadge";
import { can } from "@/lib/permissions";
import { formatCurrency } from "@/lib/format";
import type { Reservation } from "@/types";

type StatusFilter = "all" | "Pending" | "Confirmed" | "Completed" | "Cancelled";

const columnHelper = createColumnHelper<Reservation>();

export default function Reservas() {
  const navigate = useNavigate();
  const role = useAuthStore((state) => state.role);
  const deleteReservationMutation = useDeleteReservation();
  const updateReservationMutation = useUpdateReservation();

  const [date, setDate] = useState("");
  const [status, setStatus] = useState<StatusFilter>("all");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [reservationToCancel, setReservationToCancel] =
    useState<Reservation | null>(null);
  const [reservationToDelete, setReservationToDelete] =
    useState<Reservation | null>(null);

  const queryParams = useMemo(
    () => ({
      date: date || undefined,
      status: status === "all" ? undefined : status,
      page,
      pageSize,
      sortBy: "createdAt",
      sortOrder: "desc",
    }),
    [date, page, pageSize, status],
  );

  const reservationsQuery = useReservationList(queryParams);
  const reservations = reservationsQuery.data?.data ?? [];
  const pagination = reservationsQuery.data?.pagination;

  const displayReservations = useMemo(() => {
    if (!search.trim()) return reservations;
    const q = search.toLowerCase();
    return reservations.filter(
      (r) =>
        `${r.client.firstName} ${r.client.lastName}`
          .toLowerCase()
          .includes(q) ||
        r.client.email.toLowerCase().includes(q) ||
        r.table.code.toLowerCase().includes(q),
    );
  }, [reservations, search]);

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
            <div className="text-xs text-muted-fg">
              {row.original.client.email}
            </div>
          </div>
        ),
      }),
      columnHelper.accessor((row) => row.table.code, {
        id: "table",
        header: "Mesa",
        cell: ({ row }) => (
          <div>
            <div className="font-medium">{row.original.table.code}</div>
            {row.original.table.tableType?.name && (
              <div className="text-xs text-muted-fg">
                {row.original.table.tableType.name}
              </div>
            )}
          </div>
        ),
      }),
      columnHelper.accessor((row) => row.date, {
        id: "dateTime",
        header: "Fecha y horario",
        cell: ({ row }) => (
          <div>
            <div>{row.original.date}</div>
            <div className="text-xs text-muted-fg tabular-nums">
              {row.original.startTime.slice(0, 5)} –{" "}
              {row.original.endTime.slice(0, 5)}
            </div>
          </div>
        ),
      }),
      columnHelper.accessor((row) => row.numberOfGuests, {
        id: "guests",
        header: "Comensales",
        cell: ({ row }) => (
          <span className="tabular-nums">{row.original.numberOfGuests}</span>
        ),
      }),
      columnHelper.accessor((row) => row.totalPrice, {
        id: "total",
        header: "Total",
        cell: ({ row }) => (
          <span className="font-medium tabular-nums">
            {formatCurrency(row.original.totalPrice)}
          </span>
        ),
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
          const canCancel =
            (reservation.status === "Pending" ||
              reservation.status === "Confirmed") &&
            can.cancelReservation(role ?? "Employee");
          const canEdit =
            reservation.status !== "Completed" &&
            reservation.status !== "Cancelled";
          const canDelete = role ? can.deleteReservation(role) : false;

          return (
            <div className="flex items-center justify-end gap-1">
              <Button
                size="sm"
                variant="ghost"
                onClick={() => navigate(`/reservas/${reservation.id}`)}
              >
                <Eye className="size-4" />
              </Button>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => navigate(`/reservas/${reservation.id}`)}
                disabled={!canEdit}
              >
                <Pencil className="size-4" />
              </Button>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => setReservationToCancel(reservation)}
                disabled={!canCancel && !canDelete}
                className={cn(
                  !canCancel && !canDelete
                    ? "text-muted-fg"
                    : "text-destructive hover:text-destructive"
                )}
              >
                <X className="size-4" />
              </Button>
            </div>
          );
        },
      }),
    ],
    [navigate, role],
  );

  const clearFilters = () => {
    setDate("");
    setStatus("all");
    setSearch("");
    setPage(1);
  };

  const handleCancel = async () => {
    if (!reservationToCancel) return;
    await updateReservationMutation.mutateAsync({
      id: reservationToCancel.id,
      dto: { status: "Cancelled" },
    });
    setReservationToCancel(null);
  };

  const handleDelete = async () => {
    if (!reservationToDelete) return;
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
        description="Consulta, filtra y opera todas las reservas del sistema."
      />

      <div className="filter-bar px-8 py-4 flex items-center gap-3 flex-wrap">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-fg pointer-events-none" />
          <Input
            placeholder="Buscar por cliente o mesa…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-65 pl-9"
          />
        </div>

        <Select
          value={status}
          onValueChange={(v) => setStatus(v as StatusFilter)}
        >
          <SelectTrigger className="w-45">
            <SelectValue placeholder="Todos los estados" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos los estados</SelectItem>
            <SelectItem value="Pending">Pendiente</SelectItem>
            <SelectItem value="Confirmed">Confirmada</SelectItem>
            <SelectItem value="Completed">Completada</SelectItem>
            <SelectItem value="Cancelled">Cancelada</SelectItem>
          </SelectContent>
        </Select>

        <Input
          type="date"
          value={date}
          onChange={(e) => {
            setDate(e.target.value);
            setPage(1);
          }}
          className="w-40"
        />

        <Button variant="ghost" size="sm" onClick={clearFilters}>
          <X className="size-4" />
          Limpiar filtros
        </Button>
      </div>

      <section className="px-8 py-8 space-y-6">
        {hasError ? (
          <ErrorState
            message="No se pudieron cargar las reservas."
            onRetry={() => reservationsQuery.refetch()}
          />
        ) : displayReservations.length === 0 && !isLoading && !isFetching ? (
          <EmptyState
            icon={<CalendarDays className="size-7 text-primary" />}
            title="Sin reservas"
            description="No hay reservas para los filtros actuales."
          />
        ) : (
          <div
            className={cn(
              "transition-opacity duration-150",
              isFetching &&
                !isLoading &&
                reservations.length > 0 &&
                "opacity-50",
            )}
          >
            <DataTable
              columns={columns}
              data={displayReservations}
              loading={isLoading || (isFetching && reservations.length === 0)}
            />
          </div>
        )}

        {pagination && displayReservations.length > 0 && (
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
        description="Esta acción cambiará el estado de la reserva a Cancelada."
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

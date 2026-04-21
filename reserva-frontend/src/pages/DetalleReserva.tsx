import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm, type Resolver, type SubmitHandler } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { AlertTriangle, ArrowLeft } from "lucide-react";
import { PageHeader } from "@/components/molecules/PageHeader";
import { FormField } from "@/components/molecules/FormField";
import { ErrorState } from "@/components/molecules/ErrorState";
import { ConfirmDialog } from "@/components/molecules/ConfirmDialog";
import { PriceSummary } from "@/components/atoms/PriceSummary";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useReservationDetail, useUpdateReservation } from "@/hooks/useReservations";
import { useAvailableTables } from "@/hooks/useTables";
import { durationHours, formatDate, todayISO } from "@/lib/format";
import type { ReservationStatus, TableDetailed, UpdateReservationDto } from "@/types";

const detailSchema = z
  .object({
    tableId: z.coerce.number().int().min(1),
    date: z.string().min(1),
    startTime: z.string().min(1),
    endTime: z.string().min(1),
    numberOfGuests: z.coerce.number().int().min(1),
    notes: z.string().optional(),
    status: z.enum(["Pending", "Confirmed", "Completed", "Cancelled"]),
  })
  .refine((v) => durationHours(v.startTime, v.endTime) >= 0.5, {
    path: ["endTime"],
    message: "La duración mínima es 30 minutos",
  })
  .refine((v) => v.date >= todayISO(), {
    path: ["date"],
    message: "La fecha no puede ser anterior a hoy",
  });

type DetailFormValues = z.infer<typeof detailSchema>;

const nextStatusMap: Record<ReservationStatus, ReservationStatus[]> = {
  Pending: ["Confirmed", "Cancelled"],
  Confirmed: ["Cancelled", "Completed"],
  Completed: [],
  Cancelled: [],
};

const statusLabel: Record<ReservationStatus, string> = {
  Pending: "Pendiente",
  Confirmed: "Confirmada",
  Completed: "Completada",
  Cancelled: "Cancelada",
};

const toInputTime = (value: string) => value.slice(0, 5);
const toBackendTime = (value: string) => (value.length === 5 ? `${value}:00` : value);
const getTableTypeName = (table: TableDetailed | undefined) => table?.tableType?.name ?? "Tipo no disponible";

export default function DetalleReserva() {
  const navigate = useNavigate();
  const { id } = useParams();
  const reservationId = Number(id);
  const [openCancelDialog, setOpenCancelDialog] = useState(false);

  const detailQuery = useReservationDetail(Number.isFinite(reservationId) ? reservationId : 0);
  const updateMutation = useUpdateReservation();

  const reservation = detailQuery.data?.data;
  const currentStatus = reservation?.status;
  const isLocked = currentStatus === "Completed" || currentStatus === "Cancelled";

  const {
    register,
    watch,
    reset,
    handleSubmit,
    setValue,
    formState: { errors, isDirty },
  } = useForm<DetailFormValues>({
    resolver: zodResolver(detailSchema) as Resolver<DetailFormValues>,
    defaultValues: {
      tableId: 0,
      date: todayISO(),
      startTime: "19:00",
      endTime: "21:00",
      numberOfGuests: 2,
      notes: "",
      status: "Pending",
    },
  });

  useEffect(() => {
    if (!reservation) {
      return;
    }

    reset({
      tableId: reservation.table.id,
      date: reservation.date,
      startTime: toInputTime(reservation.startTime),
      endTime: toInputTime(reservation.endTime),
      numberOfGuests: reservation.numberOfGuests,
      notes: reservation.notes ?? "",
      status: reservation.status,
    });
  }, [reservation, reset]);

  const watchDate = watch("date");
  const watchStartTime = watch("startTime");
  const watchEndTime = watch("endTime");
  const watchGuests = watch("numberOfGuests");
  const watchTableId = watch("tableId");
  const watchStatus = watch("status");

  const availableTablesQuery = useAvailableTables(
    {
      date: watchDate,
      startTime: toBackendTime(watchStartTime),
      endTime: toBackendTime(watchEndTime),
      numberOfGuests: watchGuests,
    },
    { enabled: Boolean(!isLocked && reservation) },
  );

  const availableTables = availableTablesQuery.data?.data ?? [];

  const tableOptions = useMemo(() => {
    if (!reservation) {
      return [] as TableDetailed[];
    }

    const map = new Map<number, TableDetailed>();
    map.set(reservation.table.id, reservation.table);

    for (const table of availableTables) {
      map.set(table.id, table);
    }

    return Array.from(map.values());
  }, [availableTables, reservation]);

  const selectedTable = useMemo(
    () => tableOptions.find((table) => table.id === watchTableId) ?? reservation?.table,
    [reservation?.table, tableOptions, watchTableId],
  );

  const currentAndNextStatuses = useMemo(() => {
    if (!reservation) {
      return [] as ReservationStatus[];
    }

    return [reservation.status, ...nextStatusMap[reservation.status]];
  }, [reservation]);

  const onSubmit: SubmitHandler<DetailFormValues> = async (values) => {
    if (!reservation || isLocked) {
      return;
    }

    const payload: UpdateReservationDto = {
      tableId: values.tableId,
      date: values.date,
      startTime: toBackendTime(values.startTime),
      endTime: toBackendTime(values.endTime),
      numberOfGuests: values.numberOfGuests,
      notes: values.notes,
      status: values.status,
    };

    await updateMutation.mutateAsync({ id: reservation.id, dto: payload });
  };

  const handleCancelReservation = async () => {
    if (!reservation || (reservation.status !== "Pending" && reservation.status !== "Confirmed")) {
      return;
    }

    await updateMutation.mutateAsync({
      id: reservation.id,
      dto: { status: "Cancelled" },
    });

    setOpenCancelDialog(false);
  };

  if (!Number.isFinite(reservationId) || reservationId <= 0) {
    return (
      <main className="px-8 py-8">
        <ErrorState message="ID de reserva inválido." />
      </main>
    );
  }

  if (detailQuery.isLoading) {
    return (
      <main className="px-8 py-8">
        <div className="surface-card h-64 animate-pulse" />
      </main>
    );
  }

  if (detailQuery.isError || !reservation) {
    return (
      <main className="px-8 py-8">
        <ErrorState message="No se pudo cargar la reserva." onRetry={() => detailQuery.refetch()} />
      </main>
    );
  }

  const canCancel = reservation.status === "Pending" || reservation.status === "Confirmed";

  return (
    <main>
      <PageHeader
        title={`Reserva #${reservation.id}`}
        description={`${reservation.client.firstName} ${reservation.client.lastName}`}
      />

      <section className="grid grid-cols-1 gap-8 px-8 py-8 lg:grid-cols-[3fr_2fr]">
        <form className="space-y-8" onSubmit={handleSubmit(onSubmit)} noValidate>
          {isLocked && (
            <div className="surface-card flex items-center gap-3 border border-destructive/30 bg-destructive/5 p-4 text-destructive">
              <AlertTriangle className="size-4" />
              <p className="text-sm font-medium">Esta reserva no puede modificarse</p>
            </div>
          )}

          <div className="surface-card p-5">
            <h3 className="mb-4 text-lg font-semibold">Cliente</h3>
            <dl className="grid gap-3 text-sm">
              <div className="flex flex-col gap-1">
                <dt className="text-muted-fg">Nombre</dt>
                <dd className="font-medium text-foreground">
                  {reservation.client.firstName} {reservation.client.lastName}
                </dd>
              </div>
              <div className="flex flex-col gap-1">
                <dt className="text-muted-fg">Correo electrónico</dt>
                <dd className="font-medium text-foreground">{reservation.client.email}</dd>
              </div>
              <div className="flex flex-col gap-1">
                <dt className="text-muted-fg">Teléfono</dt>
                <dd className="font-medium text-foreground">{reservation.client.phone ?? "No disponible"}</dd>
              </div>
            </dl>
          </div>

          <div className="surface-card p-5">
            <h3 className="mb-4 text-lg font-semibold">Mesa</h3>
            <dl className="mb-5 grid gap-3 text-sm">
              <div className="flex flex-col gap-1">
                <dt className="text-muted-fg">Código</dt>
                <dd className="font-medium text-foreground">{reservation.table.code}</dd>
              </div>
              <div className="flex flex-col gap-1">
                <dt className="text-muted-fg">Tipo</dt>
                <dd className="font-medium text-foreground">{getTableTypeName(reservation.table)}</dd>
              </div>
              <div className="flex flex-col gap-1">
                <dt className="text-muted-fg">Ubicación</dt>
                <dd className="font-medium text-foreground">{reservation.table.location}</dd>
              </div>
              <div className="flex flex-col gap-1">
                <dt className="text-muted-fg">Capacidad</dt>
                <dd className="font-medium text-foreground">{reservation.table.capacity} personas</dd>
              </div>
            </dl>

            <FormField label="Mesa" required error={errors.tableId?.message}>
              <Select
                value={String(watchTableId)}
                onValueChange={(value) => setValue("tableId", Number(value), { shouldDirty: true })}
                disabled={isLocked}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Seleccionar mesa" />
                </SelectTrigger>
                <SelectContent>
                  {tableOptions.map((table) => (
                    <SelectItem key={table.id} value={String(table.id)}>
                      {table.code} - {getTableTypeName(table)} - {table.capacity} pers.
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </FormField>

            {!isLocked && availableTablesQuery.isFetching && (
              <p className="mt-2 text-xs text-muted-fg">Actualizando mesas disponibles...</p>
            )}
          </div>

          <div className="surface-card p-5">
            <h3 className="mb-4 text-lg font-semibold">Edición</h3>

            <div className="grid gap-4 md:grid-cols-2">
              <FormField label="Fecha" required error={errors.date?.message}>
                <Input type="date" min={todayISO()} disabled={isLocked} {...register("date")} />
              </FormField>
              <FormField label="Comensales" required error={errors.numberOfGuests?.message}>
                <Input type="number" min={1} disabled={isLocked} {...register("numberOfGuests")} />
              </FormField>
              <FormField label="Hora inicio" required error={errors.startTime?.message}>
                <Input type="time" disabled={isLocked} {...register("startTime")} />
              </FormField>
              <FormField label="Hora fin" required error={errors.endTime?.message}>
                <Input type="time" disabled={isLocked} {...register("endTime")} />
              </FormField>
            </div>

            <div className="mt-4">
              <FormField label="Notas" error={errors.notes?.message}>
                <Textarea disabled={isLocked} {...register("notes")} />
              </FormField>
            </div>

            <div className="mt-4">
              <FormField label="Estado" error={errors.status?.message} required>
                <Select
                  value={watchStatus}
                  onValueChange={(value) =>
                    setValue("status", value as DetailFormValues["status"], { shouldDirty: true })
                  }
                  disabled={isLocked}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {currentAndNextStatuses.map((status) => (
                      <SelectItem key={status} value={status}>
                        {statusLabel[status]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormField>
            </div>
          </div>
        </form>

        <aside className="space-y-6 lg:sticky lg:top-8 lg:h-fit">
          <div className="surface-card p-6">
            <h3 className="mb-4 text-lg font-semibold">Resumen de precio</h3>
            <PriceSummary
              basePrice={reservation.basePrice}
              totalPrice={reservation.totalPrice}
              hours={durationHours(toInputTime(reservation.startTime), toInputTime(reservation.endTime))}
            />
            <p className="mt-3 rounded bg-accent p-2 text-xs text-muted-fg">
              El precio se recalculará al guardar cambios.
            </p>

            <div className="mt-6 space-y-3">
              {!isLocked && isDirty && (
                <Button
                  className="w-full"
                  onClick={handleSubmit(onSubmit)}
                  disabled={updateMutation.isPending}
                >
                  {updateMutation.isPending ? "Guardando..." : "Guardar cambios"}
                </Button>
              )}

              {canCancel && (
                <Button
                  className="w-full"
                  variant="destructive"
                  onClick={() => setOpenCancelDialog(true)}
                  disabled={updateMutation.isPending}
                >
                  Cancelar reserva
                </Button>
              )}

              <Button className="w-full" variant="outline" onClick={() => navigate("/reservas")}>
                <ArrowLeft className="mr-2 size-4" />
                Volver al listado
              </Button>
            </div>
          </div>

          <div className="surface-card p-5 text-sm">
            <h4 className="font-semibold">Datos actuales</h4>
            <p className="mt-2 text-muted-fg">
              Fecha: {formatDate(reservation.date)}
            </p>
            <p className="text-muted-fg">
              Horario: {toInputTime(reservation.startTime)} - {toInputTime(reservation.endTime)}
            </p>
            <p className="text-muted-fg">
              Mesa seleccionada: {selectedTable?.code ?? reservation.table.code}
            </p>
          </div>
        </aside>
      </section>

      <ConfirmDialog
        open={openCancelDialog}
        onOpenChange={setOpenCancelDialog}
        title="Cancelar reserva"
        description="Esta acción cambiará el estado de la reserva a Cancelada."
        confirmLabel="Cancelar reserva"
        variant="destructive"
        loading={updateMutation.isPending}
        onConfirm={handleCancelReservation}
      />
    </main>
  );
}

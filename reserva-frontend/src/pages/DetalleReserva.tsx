import { zodResolver } from "@hookform/resolvers/zod"
import { AlertCircle, ArrowLeft, Lock, Save, Search, X } from "lucide-react"
import { useEffect, useMemo, useState } from "react"
import { type Resolver, type SubmitHandler, useForm } from "react-hook-form"
import { useNavigate, useParams } from "react-router-dom"
import { z } from "zod"
import { PriceSummary } from "@/components/atoms/PriceSummary"
import { StatusBadge } from "@/components/atoms/StatusBadge"
import { ConfirmDialog } from "@/components/molecules/ConfirmDialog"
import { ErrorState } from "@/components/molecules/ErrorState"
import { PageHeader } from "@/components/molecules/PageHeader"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"
import { useReservationDetail, useUpdateReservation } from "@/hooks/useReservations"
import { useAvailableTables } from "@/hooks/useTables"
import { durationHours, formatCurrency, formatDate, todayISO } from "@/lib/format"
import type { ReservationStatus, TableDetailed, UpdateReservationDto } from "@/types"

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
  })

type DetailFormValues = z.infer<typeof detailSchema>

const nextStatusMap: Record<ReservationStatus, ReservationStatus[]> = {
  Pending: ["Confirmed", "Cancelled"],
  Confirmed: ["Cancelled", "Completed"],
  Completed: [],
  Cancelled: [],
}

const statusLabel: Record<ReservationStatus, string> = {
  Pending: "Pendiente",
  Confirmed: "Confirmada",
  Completed: "Completada",
  Cancelled: "Cancelada",
}

const toInputTime = (value: string) => value.slice(0, 5)
const toBackendTime = (value: string) => (value.length === 5 ? `${value}:00` : value)

export default function DetalleReserva() {
  const navigate = useNavigate()
  const { id } = useParams()
  const reservationId = Number(id)
  const [openCancelDialog, setOpenCancelDialog] = useState(false)

  const detailQuery = useReservationDetail(Number.isFinite(reservationId) ? reservationId : 0)
  const updateMutation = useUpdateReservation()

  const reservation = detailQuery.data?.data
  const currentStatus = reservation?.status
  const isLocked = currentStatus === "Completed" || currentStatus === "Cancelled"

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
  })

  useEffect(() => {
    if (!reservation) return
    reset(
      {
        tableId: reservation.table.id,
        date: reservation.date,
        startTime: toInputTime(reservation.startTime),
        endTime: toInputTime(reservation.endTime),
        numberOfGuests: reservation.numberOfGuests,
        notes: reservation.notes ?? "",
        status: reservation.status,
      },
      {
        keepDefaultValues: true,
        keepDirty: false,
        keepValues: false,
      },
    )
  }, [reservation, reset])

  const watchDate = watch("date")
  const watchStartTime = watch("startTime")
  const watchEndTime = watch("endTime")
  const watchGuests = watch("numberOfGuests")
  const watchTableId = watch("tableId")
  const watchStatus = watch("status")

  const availableTablesQuery = useAvailableTables(
    {
      date: watchDate,
      startTime: toBackendTime(watchStartTime),
      endTime: toBackendTime(watchEndTime),
      numberOfGuests: watchGuests,
    },
    { enabled: false },
  )

  const handleFindTables = () => {
    availableTablesQuery.refetch()
  }

  const availableTables = useMemo(
    () => availableTablesQuery.data?.data ?? [],
    [availableTablesQuery.data],
  )

  const tableOptions = useMemo(() => {
    if (!reservation) return [] as TableDetailed[]
    const map = new Map<number, TableDetailed>()
    map.set(reservation.table.id, reservation.table)
    for (const table of availableTables) map.set(table.id, table)
    return Array.from(map.values())
  }, [availableTables, reservation])

  const selectedTable = useMemo(
    () => tableOptions.find((t) => t.id === watchTableId) ?? reservation?.table,
    [reservation?.table, tableOptions, watchTableId],
  )

  const currentAndNextStatuses = useMemo(() => {
    if (!reservation) return [] as ReservationStatus[]
    return [reservation.status, ...nextStatusMap[reservation.status]]
  }, [reservation])

  const onSubmit: SubmitHandler<DetailFormValues> = async (values) => {
    if (!reservation || isLocked) return
    const payload: UpdateReservationDto = {
      tableId: values.tableId,
      date: values.date,
      startTime: toBackendTime(values.startTime),
      endTime: toBackendTime(values.endTime),
      numberOfGuests: values.numberOfGuests,
      notes: values.notes,
      status: values.status,
    }
    await updateMutation.mutateAsync({ id: reservation.id, dto: payload })
  }

  const handleCancelReservation = async () => {
    if (!reservation || (reservation.status !== "Pending" && reservation.status !== "Confirmed"))
      return
    await updateMutation.mutateAsync({ id: reservation.id, dto: { status: "Cancelled" } })
    setOpenCancelDialog(false)
  }

  if (!Number.isFinite(reservationId) || reservationId <= 0) {
    return (
      <main className="px-8 py-8">
        <ErrorState message="ID de reserva inválido." />
      </main>
    )
  }

  if (detailQuery.isLoading) {
    return (
      <main className="px-8 py-8">
        <div className="surface-card h-64 animate-pulse" />
      </main>
    )
  }

  if (detailQuery.isError || !reservation) {
    return (
      <main className="px-8 py-8">
        <ErrorState message="No se pudo cargar la reserva." onRetry={() => detailQuery.refetch()} />
      </main>
    )
  }

  const canCancel = reservation.status === "Pending" || reservation.status === "Confirmed"
  const hours = durationHours(toInputTime(reservation.startTime), toInputTime(reservation.endTime))

  return (
    <main>
      <PageHeader
        title={`Reserva #${reservation.id}`}
        description={[
          reservation.createdAt && `Creada el ${formatDate(reservation.createdAt)}`,
          reservation.user?.username && `por ${reservation.user.username}`,
        ]
          .filter(Boolean)
          .join(" ")}
        actions={
          <>
            <Button variant="ghost" size="sm" onClick={() => navigate("/reservas")}>
              <ArrowLeft className="size-4" />
              Listado
            </Button>
            <StatusBadge status={reservation.status} />
          </>
        }
      />

      {isLocked && (
        <div className="mx-8 mt-6 flex items-center gap-3 rounded-lg border bg-surface-elevated px-4 py-3 text-sm text-muted-fg">
          <Lock className="size-4 shrink-0" />
          <span>
            Esta reserva está{" "}
            <strong className="text-foreground">
              {currentStatus === "Completed" ? "completada" : "cancelada"}
            </strong>{" "}
            y no puede modificarse.
          </span>
        </div>
      )}

      <div className="p-8 grid grid-cols-1 lg:grid-cols-[1fr_360px] gap-6">
        <div className="space-y-5">
          {/* Cliente — solo lectura */}
          <section className="surface-card p-5">
            <h3 className="font-display text-lg font-semibold mb-4">Cliente</h3>
            <dl className="grid grid-cols-1 sm:grid-cols-3 gap-4 text-sm">
              <Field label="Nombre">
                {reservation.client.firstName} {reservation.client.lastName}
              </Field>
              <Field label="Correo">{reservation.client.email}</Field>
              <Field label="Teléfono">{reservation.client.phone ?? "—"}</Field>
            </dl>
          </section>

          {/* Mesa — solo lectura */}
          <section className="surface-card p-5">
            <h3 className="font-display text-lg font-semibold mb-4">Mesa actual</h3>
            <dl className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-sm">
              <Field label="Código">{reservation.table.code}</Field>
              <Field label="Tipo">{reservation.table.tableType?.name ?? "—"}</Field>
              <Field label="Capacidad">{reservation.table.capacity} personas</Field>
              <Field label="Ubicación">{reservation.table.location}</Field>
            </dl>
          </section>

          {/* Datos editables */}
          <section className="surface-card p-5">
            <h3 className="font-display text-lg font-semibold mb-4">Datos de la reserva</h3>
            <fieldset disabled={isLocked} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="col-span-2 space-y-1.5">
                  <p className="text-sm font-medium">Mesa</p>
                  <div className="flex gap-2">
                    <Select
                      value={String(watchTableId)}
                      onValueChange={(v) => setValue("tableId", Number(v), { shouldDirty: true })}
                    >
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Seleccionar mesa" />
                      </SelectTrigger>
                      <SelectContent>
                        {tableOptions.map((t) => (
                          <SelectItem key={t.id} value={String(t.id)}>
                            {t.code} · {t.tableType?.name ?? "—"} · {t.capacity}p
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    {!isLocked && (
                      <Button
                        type="button"
                        variant="outline"
                        size="icon"
                        onClick={handleFindTables}
                        disabled={availableTablesQuery.isFetching}
                        title="Buscar mesas disponibles"
                      >
                        <Search className="size-4" />
                      </Button>
                    )}
                  </div>
                  {!isLocked && availableTablesQuery.isFetching && (
                    <p className="text-xs text-muted-fg">Actualizando mesas disponibles…</p>
                  )}
                </div>

                <div className="space-y-1.5">
                  <label htmlFor="detail-reservation-date" className="text-sm font-medium">
                    Fecha
                  </label>
                  <Input
                    id="detail-reservation-date"
                    type="date"
                    min={todayISO()}
                    {...register("date")}
                  />
                  {errors.date && <p className="text-xs text-destructive">{errors.date.message}</p>}
                </div>
                <div className="space-y-1.5">
                  <p className="text-sm font-medium">Comensales</p>
                  <Input type="number" min={1} {...register("numberOfGuests")} />
                  {errors.numberOfGuests && (
                    <p className="text-xs text-destructive">{errors.numberOfGuests.message}</p>
                  )}
                </div>
                <div className="space-y-1.5">
                  <p className="text-sm font-medium">Hora inicio</p>
                  <Input type="time" {...register("startTime")} />
                  {errors.startTime && (
                    <p className="text-xs text-destructive">{errors.startTime.message}</p>
                  )}
                </div>
                <div className="space-y-1.5">
                  <p className="text-sm font-medium">Hora fin</p>
                  <Input type="time" {...register("endTime")} />
                  {errors.endTime && (
                    <p className="text-xs text-destructive">{errors.endTime.message}</p>
                  )}
                </div>

                <div className="col-span-2 space-y-1.5">
                  <p className="text-sm font-medium">Estado</p>
                  <Select
                    value={watchStatus}
                    onValueChange={(v) =>
                      setValue("status", v as DetailFormValues["status"], { shouldDirty: true })
                    }
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {currentAndNextStatuses.map((s) => (
                        <SelectItem key={s} value={s}>
                          {statusLabel[s]}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="col-span-2 space-y-1.5">
                  <p className="text-sm font-medium">Notas</p>
                  <Textarea className="min-h-22.5" {...register("notes")} />
                </div>
              </div>

              {selectedTable && watchGuests > selectedTable.capacity && (
                <div className="flex items-start gap-2 rounded-md border border-destructive/20 bg-destructive/5 px-3 py-2 text-sm text-destructive">
                  <AlertCircle className="size-4 mt-0.5 shrink-0" />
                  El número de comensales supera la capacidad de la mesa ({selectedTable.capacity}).
                </div>
              )}
            </fieldset>
          </section>
        </div>

        {/* Aside sticky */}
        <aside className="space-y-4">
          <div className="surface-card p-5 lg:sticky lg:top-6">
            <h3 className="font-display text-lg font-semibold mb-4">Resumen de precios</h3>

            <PriceSummary
              basePrice={reservation.basePrice}
              totalPrice={reservation.totalPrice}
              hours={hours}
            />

            <p className="mt-3 text-xs text-muted-fg">
              El precio se recalculará al guardar cambios.
            </p>

            <div className="mt-5 flex items-center justify-between text-sm">
              <span className="text-muted-fg">Estado actual</span>
              <StatusBadge status={reservation.status} />
            </div>

            <div className="mt-5 space-y-2">
              {!isLocked && isDirty && (
                <Button
                  className="w-full"
                  onClick={handleSubmit(onSubmit)}
                  disabled={updateMutation.isPending}
                >
                  <Save className="size-4" />
                  {updateMutation.isPending ? "Guardando…" : "Guardar cambios"}
                </Button>
              )}

              {canCancel && (
                <Button
                  variant="outline"
                  className="w-full text-destructive hover:text-destructive"
                  onClick={() => setOpenCancelDialog(true)}
                  disabled={updateMutation.isPending}
                >
                  <X className="size-4" />
                  Cancelar reserva
                </Button>
              )}

              <Button variant="outline" className="w-full" onClick={() => navigate("/reservas")}>
                <ArrowLeft className="size-4" />
                Volver al listado
              </Button>
            </div>

            <div className="mt-5 space-y-1 border-t pt-5 text-xs text-muted-fg">
              <div className="flex justify-between">
                <span>Precio base</span>
                <span className="tabular-nums font-medium text-foreground">
                  {formatCurrency(reservation.basePrice)}
                </span>
              </div>
              <div className="flex justify-between">
                <span>Total cobrado</span>
                <span className="tabular-nums font-medium text-foreground">
                  {formatCurrency(reservation.totalPrice)}
                </span>
              </div>
            </div>
          </div>
        </aside>
      </div>

      <ConfirmDialog
        open={openCancelDialog}
        onOpenChange={setOpenCancelDialog}
        title="¿Cancelar la reserva?"
        description="La mesa quedará disponible y la reserva pasará a estado Cancelada."
        confirmLabel="Sí, cancelar"
        variant="destructive"
        loading={updateMutation.isPending}
        onConfirm={handleCancelReservation}
      />
    </main>
  )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <dt className="text-[11px] uppercase tracking-wider text-muted-fg mb-1">{label}</dt>
      <dd className="font-medium">{children}</dd>
    </div>
  )
}

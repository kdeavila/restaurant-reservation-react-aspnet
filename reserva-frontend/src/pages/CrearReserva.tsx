import { useEffect, useState } from "react";
import { useForm, type Resolver, type SubmitHandler } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useLocation } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { FormField } from "@/components/molecules/FormField";
import { PageHeader } from "@/components/molecules/PageHeader";
import { ClientSearchSelect } from "@/components/molecules/ClientSearchSelect";
import { PriceSummary } from "@/components/atoms/PriceSummary";
import { useCreateReservation } from "@/hooks/useReservations";
import { useTableDetail } from "@/hooks/useTables";
import { durationHours, formatCurrency, formatDate, todayISO } from "@/lib/format";
import type { Client, TableDetailed } from "@/types";

const createReservaSchema = z
  .object({
    clientId: z.coerce.number().int().min(1),
    tableId: z.coerce.number().int().min(1),
    date: z.string().min(1),
    startTime: z.string().min(1),
    endTime: z.string().min(1),
    numberOfGuests: z.coerce.number().int().min(1),
    notes: z.string().optional(),
  })
  .refine((v) => v.date >= todayISO(), {
    path: ["date"],
    message: "La fecha no puede ser anterior a hoy",
  })
  .refine((v) => durationHours(v.startTime, v.endTime) >= 0.5, {
    path: ["endTime"],
    message: "La duración mínima es 30 minutos",
  });

type CreateReservaFormValues = z.infer<typeof createReservaSchema>;

interface LocationState {
  tableId?: number;
  date?: string;
  startTime?: string;
  endTime?: string;
  numberOfGuests?: number;
}

export default function CrearReserva() {
  const navigate = useNavigate();
  const location = useLocation();
  const state = (location.state || {}) as LocationState;

  const [selectedClient, setSelectedClient] = useState<Client | null>(null);
  const [selectedTable, setSelectedTable] = useState<TableDetailed | null>(null);

  const createMutation = useCreateReservation();
  const tableDetailQuery = useTableDetail(state.tableId ?? 0);

  const defaultDate = state.date ?? todayISO();
  const defaultStartTime = state.startTime ?? "19:00";
  const defaultEndTime = state.endTime ?? "21:00";
  const defaultGuests = state.numberOfGuests ?? 2;

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<CreateReservaFormValues>({
    resolver: zodResolver(createReservaSchema) as Resolver<CreateReservaFormValues>,
    defaultValues: {
      clientId: 0,
      tableId: state.tableId ?? 0,
      date: defaultDate,
      startTime: defaultStartTime,
      endTime: defaultEndTime,
      numberOfGuests: defaultGuests,
      notes: "",
    },
  });

  const watchDate = watch("date");
  const watchStartTime = watch("startTime");
  const watchEndTime = watch("endTime");
  const watchClientId = watch("clientId");

  useEffect(() => {
    if (state.tableId && tableDetailQuery.data?.data) {
      setSelectedTable(tableDetailQuery.data.data);
    }
  }, [state.tableId, tableDetailQuery.data]);

  const hours = durationHours(watchStartTime, watchEndTime);
  const basePrice = selectedTable
    ? selectedTable.tableType.basePricePerHour * Math.max(0, hours)
    : 0;

  const onSubmit: SubmitHandler<CreateReservaFormValues> = async (values) => {
    try {
      const response = await createMutation.mutateAsync({
        clientId: values.clientId,
        tableId: values.tableId,
        date: values.date,
        startTime: values.startTime,
        endTime: values.endTime,
        numberOfGuests: values.numberOfGuests,
        notes: values.notes,
      });

      if (response.data?.id) {
        navigate(`/reservas/${response.data.id}`);
      }
    } catch (error) {
      console.error("Error creating reservation:", error);
    }
  };

  return (
    <main>
      <PageHeader
        title="Nueva reserva"
        actions={
          <Button variant="outline" size="sm" onClick={() => navigate(-1)}>
            <ArrowLeft className="mr-2 size-4" />
            Volver
          </Button>
        }
      />

      <section className="grid grid-cols-1 lg:grid-cols-[1fr_380px] gap-8 px-8 py-8">
        <form className="space-y-4" onSubmit={handleSubmit(onSubmit)} noValidate>
          <section className="surface-card p-5 space-y-4">
            <div className="flex items-center gap-2.5">
              <div className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-primary text-xs font-semibold shrink-0">1</div>
              <h3 className="font-display text-lg font-semibold">Cliente</h3>
            </div>
            <ClientSearchSelect
              value={watchClientId || null}
              onChange={(_, client) => {
                setSelectedClient(client);
              }}
            />
          </section>

          <section className="surface-card p-5 space-y-4">
            <div className="flex items-center gap-2.5">
              <div className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-primary text-xs font-semibold shrink-0">2</div>
              <h3 className="font-display text-lg font-semibold">Mesa y horario</h3>
            </div>

            {state.tableId && selectedTable ? (
              <div className="flex items-center justify-between gap-4 rounded-lg border border-border bg-muted/50 px-4 py-3">
                <div className="min-w-0">
                  <div className="font-semibold">{selectedTable.code}</div>
                  <div className="mt-0.5 flex flex-wrap items-center gap-x-2 text-sm text-muted-fg">
                    <span>{selectedTable.tableType.name}</span>
                    <span aria-hidden>·</span>
                    <span>{selectedTable.capacity} personas</span>
                    <span aria-hidden>·</span>
                    <span>{selectedTable.location}</span>
                    <span aria-hidden>·</span>
                    <span>{formatCurrency(selectedTable.tableType.basePricePerHour)}/h</span>
                  </div>
                </div>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="shrink-0"
                  onClick={() => navigate("/disponibilidad")}
                >
                  Cambiar mesa
                </Button>
              </div>
            ) : null}

            <div className="grid grid-cols-2 gap-4">
              <FormField label="Fecha" error={errors.date?.message} required>
                <Input type="date" min={todayISO()} {...register("date")} />
              </FormField>
              <FormField label="Comensales" error={errors.numberOfGuests?.message} required>
                <Input type="number" min={1} {...register("numberOfGuests")} />
              </FormField>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <FormField label="Hora inicio" error={errors.startTime?.message} required>
                <Input type="time" {...register("startTime")} />
              </FormField>
              <FormField label="Hora fin" error={errors.endTime?.message} required>
                <Input type="time" {...register("endTime")} />
              </FormField>
            </div>
          </section>

          <section className="surface-card p-5 space-y-4">
            <div className="flex items-center gap-2.5">
              <div className="flex h-7 w-7 items-center justify-center rounded-full bg-primary/10 text-primary text-xs font-semibold shrink-0">3</div>
              <h3 className="font-display text-lg font-semibold">Notas</h3>
            </div>
            <FormField label="Notas adicionales" error={errors.notes?.message}>
              <Textarea placeholder="Ej: aniversario, petición especial..." {...register("notes")} />
            </FormField>
          </section>
        </form>

        <aside className="lg:sticky lg:top-8 lg:h-fit space-y-6">
          <div className="surface-card p-6 space-y-4">
            <h4 className="font-semibold">Resumen</h4>

            {selectedClient && (
              <div className="space-y-1 pb-4 border-b border-border">
                <div className="text-sm text-muted-fg">Cliente</div>
                <div className="font-medium">
                  {selectedClient.firstName} {selectedClient.lastName}
                </div>
              </div>
            )}

            {selectedTable && (
              <div className="space-y-1 pb-4 border-b border-border">
                <div className="text-sm text-muted-fg">Mesa</div>
                <div className="font-medium">{selectedTable.code}</div>
              </div>
            )}

            {watchDate && (
              <div className="space-y-1 pb-4 border-b border-border">
                <div className="text-sm text-muted-fg">Fecha y horario</div>
                <div className="font-medium">
                  {formatDate(watchDate)} • {watchStartTime} - {watchEndTime}
                </div>
              </div>
            )}

            <PriceSummary
              basePrice={basePrice}
              hours={hours}
              pendingNote
              className="pt-2"
            />

            <Button
              type="submit"
              className="w-full mt-6"
              disabled={createMutation.isPending || !selectedClient || !selectedTable}
              onClick={handleSubmit(onSubmit)}
            >
              {createMutation.isPending ? "Confirmando..." : "Confirmar reserva"}
            </Button>
          </div>
        </aside>
      </section>
    </main>
  );
}

import { useEffect, useState } from "react";
import { useForm, type Resolver, type SubmitHandler } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { CalendarDays, Clock, Search, Sparkles, Users } from "lucide-react";
import { PageHeader } from "@/components/molecules/PageHeader";
import { EmptyState } from "@/components/molecules/EmptyState";
import { FormField } from "@/components/molecules/FormField";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { useAvailableTables } from "@/hooks/useTables";
import { durationHours, todayISO } from "@/lib/format";
import { TableCard } from "@/components/organisms/TableCard";

const availabilitySchema = z
  .object({
    date: z.string().min(1),
    startTime: z.string().min(1),
    endTime: z.string().min(1),
    numberOfGuests: z.coerce.number().int().min(1),
  })
  .refine((values) => values.date >= todayISO(), {
    path: ["date"],
    message: "La fecha no puede ser anterior a hoy",
  })
  .refine((values) => durationHours(values.startTime, values.endTime) >= 0.5, {
    path: ["endTime"],
    message: "La duración mínima es de 30 minutos",
  });

type AvailabilityFormValues = z.infer<typeof availabilitySchema>;

const defaultValues: AvailabilityFormValues = {
  date: todayISO(),
  startTime: "19:00",
  endTime: "21:00",
  numberOfGuests: 2,
};

export default function Disponibilidad() {
  const [hasSearched, setHasSearched] = useState(false);
  const [searchParams, setSearchParams] = useState(defaultValues);

  const {
    register,
    handleSubmit,
    formState: { isValid },
  } = useForm<AvailabilityFormValues>({
    resolver: zodResolver(
      availabilitySchema,
    ) as Resolver<AvailabilityFormValues>,
    defaultValues,
    mode: "onChange",
  });

  const availabilityQuery = useAvailableTables(searchParams, {
    enabled: false,
  });

  useEffect(() => {
    if (!hasSearched) {
      return;
    }

    void availabilityQuery.refetch();
  }, [hasSearched, searchParams]);

  const onSubmit: SubmitHandler<AvailabilityFormValues> = (values) => {
    setSearchParams(values);
    setHasSearched(true);
  };

  const tables = availabilityQuery.data?.data ?? [];
  const isLoading = availabilityQuery.isFetching;

  return (
    <main>
      <PageHeader
        title="Disponibilidad"
        description="Consulta mesas disponibles y crea una reserva con un clic."
      />

      <section className="filter-bar px-8 py-4">
        <form
          className="flex flex-wrap items-end justify-between gap-3"
          onSubmit={handleSubmit(onSubmit)}
          noValidate
        >
          <div className="flex flex-wrap items-end gap-3">
            <FormField
              label="Fecha"
              icon={<CalendarDays className="size-3.5" />}
              required
            >
              <Input
                type="date"
                min={todayISO()}
                className="w-40"
                {...register("date")}
              />
            </FormField>

            <FormField
              label="Inicio"
              icon={<Clock className="size-3.5" />}
              required
            >
              <Input type="time" className="w-30" {...register("startTime")} />
            </FormField>

            <FormField label="Fin" icon={<Clock className="size-3.5" />} required>
              <Input type="time" className="w-30" {...register("endTime")} />
            </FormField>

            <FormField
              label="Comensales"
              icon={<Users className="size-3.5" />}
              required
            >
              <Input
                type="number"
                min={1}
                className="w-25"
                {...register("numberOfGuests")}
              />
            </FormField>
          </div>

          <Button type="submit" disabled={!isValid}>
            <Search className="size-4" />
            Buscar disponibilidad
          </Button>
        </form>
      </section>

      <section className="px-8 py-8">
        {!hasSearched ? (
          <EmptyState
            icon={<CalendarDays className="size-7 text-primary" />}
            title="Busca mesas disponibles"
            description="Indica fecha, horario y comensales para ver opciones libres."
          />
        ) : (
          <>
            <div className="mb-6 flex items-center gap-2 text-sm text-muted-fg">
              <span className="font-medium text-foreground">{tables.length} mesas disponibles</span>
              <span>·</span>
              <span>duración {durationHours(searchParams.startTime, searchParams.endTime).toFixed(1)}h</span>
              <span>·</span>
              <span>{searchParams.numberOfGuests} comensales</span>
            </div>

            {isLoading ? (
              <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
                {Array.from({ length: 6 }).map((_, index) => (
                  <Skeleton key={index} className="surface-card h-48" />
                ))}
              </div>
            ) : tables.length === 0 ? (
              <EmptyState
                icon={<Sparkles className="size-7 text-primary" />}
                title="No hay mesas disponibles para este horario"
                description="Prueba otro horario o reduce comensales para ampliar opciones."
              />
            ) : (
              <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
                {tables.map((table) => (
                  <TableCard
                    key={table.id}
                    table={table}
                    date={searchParams.date}
                    startTime={searchParams.startTime}
                    endTime={searchParams.endTime}
                    numberOfGuests={searchParams.numberOfGuests}
                  />
                ))}
              </div>
            )}
          </>
        )}
      </section>
    </main>
  );
}

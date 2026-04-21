import { useEffect, useState } from "react";
import { useForm, type Resolver, type SubmitHandler } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { CalendarDays, Search, Sparkles } from "lucide-react";
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
  const navigate = useNavigate();
  const [hasSearched, setHasSearched] = useState(false);
  const [searchParams, setSearchParams] = useState(defaultValues);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AvailabilityFormValues>({
    resolver: zodResolver(availabilitySchema) as Resolver<AvailabilityFormValues>,
    defaultValues,
  });

  const availabilityQuery = useAvailableTables(searchParams, { enabled: false });

  useEffect(() => {
    if (!hasSearched) {
      return;
    }

    void availabilityQuery.refetch();
  }, [hasSearched, searchParams, availabilityQuery.refetch]);

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
        actions={
          <Button asChild>
            <button type="button" onClick={() => navigate("/reservas/nueva")}>Nueva reserva</button>
          </Button>
        }
      />

      <section className="filter-bar px-8 py-5">
        <form className="grid gap-4 lg:grid-cols-[1.1fr_1fr_1fr_0.8fr_auto]" onSubmit={handleSubmit(onSubmit)} noValidate>
          <FormField label="Fecha" error={errors.date?.message} required>
            <Input type="date" min={todayISO()} {...register("date")} />
          </FormField>

          <FormField label="Hora inicio" error={errors.startTime?.message} required>
            <Input type="time" {...register("startTime")} />
          </FormField>

          <FormField label="Hora fin" error={errors.endTime?.message} required>
            <Input type="time" {...register("endTime")} />
          </FormField>

          <FormField label="Comensales" error={errors.numberOfGuests?.message} required>
            <Input type="number" min={1} {...register("numberOfGuests")} />
          </FormField>

          <div className="flex items-end">
            <Button type="submit" className="w-full lg:w-auto" size="lg">
              <Search className="mr-2 size-4" />
              Buscar disponibilidad
            </Button>
          </div>
        </form>
      </section>

      <section className="px-8 py-8">
        {!hasSearched ? (
          <EmptyState
            icon={<CalendarDays className="size-7 text-primary" />}
            title="Busca mesas disponibles"
            description="Indica fecha, horario y comensales para ver opciones libres."
          />
        ) : isLoading ? (
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
          <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
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
      </section>
    </main>
  );
}

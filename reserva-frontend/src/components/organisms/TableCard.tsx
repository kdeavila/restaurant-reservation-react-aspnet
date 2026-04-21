import { useNavigate } from "react-router-dom";
import type { TableDetailed } from "@/types";
import { formatCurrency } from "@/lib/format";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

interface TableCardProps {
  table: TableDetailed;
  date: string;
  startTime: string;
  endTime: string;
  numberOfGuests: number;
}

export function TableCard({ table, date, startTime, endTime, numberOfGuests }: TableCardProps) {
  const navigate = useNavigate();

  return (
    <article className="surface-card flex h-full flex-col justify-between p-5 transition-shadow hover:shadow-elevated">
      <div className="space-y-4">
        <div className="flex items-start justify-between gap-3">
          <div>
            <h3 className="font-display text-2xl font-bold tracking-tight">{table.code}</h3>
            <p className="mt-1 text-sm text-muted-fg">Mesa disponible</p>
          </div>
          <Badge variant="outline" className="rounded-full px-3 py-1 text-xs">
            {table.tableType.name}
          </Badge>
        </div>

        <dl className="grid gap-3 text-sm text-muted-fg">
          <div className="flex items-center justify-between gap-4">
            <dt>Capacidad</dt>
            <dd className="font-medium text-foreground">{table.capacity} personas</dd>
          </div>
          <div className="flex items-center justify-between gap-4">
            <dt>Ubicación</dt>
            <dd className="font-medium text-foreground">{table.location}</dd>
          </div>
          <div className="flex items-center justify-between gap-4">
            <dt>Precio base/hora</dt>
            <dd className="font-medium text-foreground">
              {formatCurrency(table.tableType.basePricePerHour)}
            </dd>
          </div>
        </dl>
      </div>

      <Button
        className="mt-6 w-full"
        variant="secondary"
        onClick={() =>
          navigate("/reservas/nueva", {
            state: { tableId: table.id, date, startTime, endTime, numberOfGuests },
          })
        }
      >
        Reservar esta mesa
      </Button>
    </article>
  );
}
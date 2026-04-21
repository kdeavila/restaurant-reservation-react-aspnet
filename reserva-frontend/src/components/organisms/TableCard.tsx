import { useNavigate } from "react-router-dom";
import { MapPin, Users } from "lucide-react";
import type { TableDetailed } from "@/types";
import { formatCurrency } from "@/lib/format";
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
            <span className="mt-1 inline-block text-[11px] font-medium uppercase tracking-wider text-secondary bg-secondary/10 px-2 py-0.5 rounded-full">
              {table.tableType.name}
            </span>
          </div>
          <div className="text-right">
            <div className="font-display text-xl font-bold text-primary tabular-nums">
              {formatCurrency(table.tableType.basePricePerHour)}
            </div>
            <div className="text-[11px] text-muted-fg uppercase tracking-wider">por hora</div>
          </div>
        </div>

        <div className="space-y-1.5 text-sm text-muted-fg">
          <div className="flex items-center gap-2">
            <Users className="size-4 shrink-0" />
            <span>Capacidad: {table.capacity} personas</span>
          </div>
          <div className="flex items-center gap-2">
            <MapPin className="size-4 shrink-0" />
            <span>{table.location}</span>
          </div>
        </div>
      </div>

      <Button
        className="mt-5 w-full"
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
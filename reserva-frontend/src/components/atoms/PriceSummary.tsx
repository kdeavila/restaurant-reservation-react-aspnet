import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/format";

interface PriceSummaryProps {
  basePrice: number;
  totalPrice?: number;
  hours?: number;
  pendingNote?: boolean;
  className?: string;
}

export function PriceSummary({
  basePrice,
  totalPrice,
  hours,
  pendingNote,
  className,
}: PriceSummaryProps): React.ReactNode {
  const surcharge =
    totalPrice != null ? Math.max(0, totalPrice - basePrice) : null;

  return (
    <dl className={cn("space-y-2.5 text-sm", className)}>
      {hours !== undefined && (
        <div className="flex justify-between text-[color:var(--color-muted-fg)]">
          <dt>Duración</dt>
          <dd className="tabular-nums font-medium">{hours.toFixed(1)} h</dd>
        </div>
      )}

      <div className="flex justify-between">
        <dt className="text-[color:var(--color-muted-fg)]">Precio base</dt>
        <dd className="tabular-nums font-medium">
          {formatCurrency(Math.max(0, basePrice))}
        </dd>
      </div>

      {surcharge !== null && surcharge > 0 && (
        <div className="flex justify-between">
          <dt className="text-[color:var(--color-muted-fg)]">
            Recargos aplicados
          </dt>
          <dd className="tabular-nums font-medium text-[color:var(--color-primary)]">
            + {formatCurrency(surcharge)}
          </dd>
        </div>
      )}

      <div className="border-t border-[color:var(--color-border)] pt-2.5 flex justify-between items-baseline">
        <dt className="font-display font-semibold">Total</dt>
        <dd className="font-display text-2xl font-bold tabular-nums">
          {formatCurrency(Math.max(0, totalPrice ?? basePrice))}
        </dd>
      </div>

      {pendingNote && (
        <p className="text-xs text-[color:var(--color-muted-fg)] pt-1">
          El precio final con recargos se calculará al confirmar.
        </p>
      )}
    </dl>
  );
}

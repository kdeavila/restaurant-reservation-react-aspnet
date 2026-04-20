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
}: PriceSummaryProps) {
  const surcharge = totalPrice ? Math.max(0, totalPrice - basePrice) : 0;

  return (
    <div className={cn("space-y-2 text-sm", className)}>
      {hours !== undefined && (
        <div className="flex justify-between text-muted-fg">
          <span>Duración:</span>
          <span>{hours.toFixed(1)} h</span>
        </div>
      )}

      <div className="flex justify-between text-muted-fg">
        <span>Precio base:</span>
        <span>{formatCurrency(Math.max(0, basePrice))}</span>
      </div>

      {surcharge > 0 && (
        <div className="flex justify-between text-muted-fg">
          <span>Recargo:</span>
          <span className="text-destructive">
            {formatCurrency(surcharge)}
          </span>
        </div>
      )}

      {totalPrice !== undefined && (
        <div className="flex justify-between font-semibold border-t border-border pt-2">
          <span>Total:</span>
          <span>{formatCurrency(Math.max(0, totalPrice))}</span>
        </div>
      )}

      {pendingNote && (
        <div className="mt-3 p-2 bg-accent rounded text-muted-fg text-xs">
          El precio final se calculará al confirmar.
        </div>
      )}
    </div>
  );
}

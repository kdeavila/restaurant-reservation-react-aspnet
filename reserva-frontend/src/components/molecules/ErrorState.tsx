import { AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface ErrorStateProps {
  message?: string;
  onRetry?: () => void;
  className?: string;
}

export function ErrorState({
  message = "Algo salió mal. Por favor, intenta de nuevo.",
  onRetry,
  className,
}: ErrorStateProps) {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center gap-4 py-12 text-center",
        className
      )}
    >
      <div className="flex items-center justify-center w-16 h-16 rounded-full bg-destructive/10">
        <AlertCircle className="w-8 h-8 text-destructive" />
      </div>
      <div className="space-y-2">
        <h3 className="font-semibold text-lg">Error</h3>
        <p className="text-muted-fg text-sm max-w-xs">{message}</p>
      </div>
      {onRetry && (
        <Button onClick={onRetry} size="sm" variant="outline" className="mt-2">
          Reintentar
        </Button>
      )}
    </div>
  );
}

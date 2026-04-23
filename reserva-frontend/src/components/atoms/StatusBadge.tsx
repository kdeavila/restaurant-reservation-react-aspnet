import { cn } from "@/lib/utils";
import type { ReservationStatus, TableStatus, EntityStatus } from "@/types";

type Status = ReservationStatus | TableStatus | EntityStatus;

interface StatusBadgeProps {
  status: Status;
  className?: string;
  customLabel?: string;
}

const statusConfig: Record<Status, { bg: string; text: string; label: string }> = {
  Pending: {
    bg: "bg-status-pending-bg",
    text: "text-status-pending",
    label: "Pendiente",
  },
  Confirmed: {
    bg: "bg-status-confirmed-bg",
    text: "text-status-confirmed",
    label: "Confirmado",
  },
  Completed: {
    bg: "bg-status-completed-bg",
    text: "text-status-completed",
    label: "Completado",
  },
  Cancelled: {
    bg: "bg-status-cancelled-bg",
    text: "text-status-cancelled",
    label: "Cancelado",
  },
  Active: {
    bg: "bg-status-active-bg",
    text: "text-status-active",
    label: "Activo",
  },
  Inactive: {
    bg: "bg-status-inactive-bg",
    text: "text-status-inactive",
    label: "Inactivo",
  },
  Maintenance: {
    bg: "bg-status-maintenance-bg",
    text: "text-status-maintenance",
    label: "Mantenimiento",
  },
};

export function StatusBadge({
  status,
  className,
  customLabel,
}: StatusBadgeProps) {
  const config = statusConfig[status];
  const label = customLabel || config.label;

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 px-3 py-0.5 rounded-full text-sm font-medium",
        config.bg,
        config.text,
        className
      )}
    >
      <span className={cn("w-1.5 h-1.5 rounded-full bg-current")} />
      {label}
    </span>
  );
}

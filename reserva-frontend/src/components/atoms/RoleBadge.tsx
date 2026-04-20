import { cn } from "@/lib/utils";
import type { Role } from "@/types";

interface RoleBadgeProps {
  role: Role;
  className?: string;
}

const roleConfig: Record<Role, { bg: string; text: string; label: string }> = {
  Admin: {
    bg: "bg-primary/10",
    text: "text-primary",
    label: "Administrador",
  },
  Manager: {
    bg: "bg-secondary/10",
    text: "text-secondary",
    label: "Gestor",
  },
  Employee: {
    bg: "bg-muted",
    text: "text-muted-fg",
    label: "Empleado",
  },
};

export function RoleBadge({ role, className }: RoleBadgeProps) {
  const config = roleConfig[role];

  return (
    <span
      className={cn(
        "inline-flex items-center px-2.5 py-1 rounded-full text-sm font-medium",
        config.bg,
        config.text,
        className
      )}
    >
      {config.label}
    </span>
  );
}

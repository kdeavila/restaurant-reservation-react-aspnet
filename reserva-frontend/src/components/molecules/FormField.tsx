import type { ReactNode } from "react"
import { cn } from "@/lib/utils"

interface FormFieldProps {
  label: string
  icon?: ReactNode
  error?: string
  required?: boolean
  children: ReactNode
  className?: string
}

export function FormField({ label, icon, error, required, children, className }: FormFieldProps) {
  return (
    <div className={cn("space-y-1.5", className)}>
      <div
        className={cn(
          "flex items-center gap-1.5",
          icon
            ? "text-[11px] font-medium uppercase tracking-wider text-muted-fg"
            : "text-sm font-medium",
        )}
      >
        {icon}
        {label}
        {required && <span className="text-destructive ml-0.5">*</span>}
      </div>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  )
}

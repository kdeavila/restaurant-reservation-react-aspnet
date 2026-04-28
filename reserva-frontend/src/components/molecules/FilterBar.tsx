import type { ReactNode } from "react"
import { cn } from "@/lib/utils"

interface FilterBarProps {
  children: ReactNode
  className?: string
}

export function FilterBar({ children, className }: FilterBarProps) {
  return <div className={cn("filter-bar px-8 py-4", className)}>{children}</div>
}

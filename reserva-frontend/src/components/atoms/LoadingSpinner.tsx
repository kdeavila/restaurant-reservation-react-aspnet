import { cn } from "@/lib/utils"

interface LoadingSpinnerProps {
  className?: string
}

export function LoadingSpinner({ className }: LoadingSpinnerProps) {
  return (
    <div
      className={cn(
        "w-6 h-6 border-2 border-border rounded-full animate-spin",
        "border-t-primary border-r-primary",
        className,
      )}
    />
  )
}

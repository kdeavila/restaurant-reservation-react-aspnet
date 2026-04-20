import { cn } from "@/lib/utils";

interface DayChipsProps {
  value: number[];
  onChange: (days: number[]) => void;
  readonly?: boolean;
  className?: string;
}

const dayLabels = ["Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb"];

export function DayChips({
  value,
  onChange,
  readonly,
  className,
}: DayChipsProps) {
  const handleToggle = (index: number) => {
    if (readonly) return;
    const newValue = value.includes(index)
      ? value.filter((d) => d !== index)
      : [...value, index].sort((a, b) => a - b);
    onChange(newValue);
  };

  return (
    <div className={cn("flex flex-wrap gap-2", className)}>
      {dayLabels.map((label, index) => (
        <button
          key={index}
          onClick={() => handleToggle(index)}
          disabled={readonly}
          className={cn(
            "px-3 py-1 rounded-full text-sm font-medium transition-colors",
            value.includes(index)
              ? "bg-primary text-primary-fg"
              : "bg-muted text-muted-fg",
            !readonly && "cursor-pointer hover:opacity-80",
            readonly && "cursor-default"
          )}
        >
          {label}
        </button>
      ))}
    </div>
  );
}

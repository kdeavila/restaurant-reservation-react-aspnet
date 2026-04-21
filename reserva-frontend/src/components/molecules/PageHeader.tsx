import type { ReactNode } from "react";

interface PageHeaderProps {
  title: string;
  description?: string;
  actions?: ReactNode;
}

export function PageHeader({ title, description, actions }: PageHeaderProps) {
  return (
    <div className="border-b border-border/60 px-8 pt-8 pb-6">
      <div className="flex justify-between items-start">
        <div className="flex-1">
          <h1 className="text-3xl font-bold font-display">{title}</h1>
          {description && <p className="mt-2 text-muted-fg">{description}</p>}
        </div>
        {actions && <div className="ml-4">{actions}</div>}
      </div>
    </div>
  );
}

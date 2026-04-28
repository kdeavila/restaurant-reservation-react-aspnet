import { createColumnHelper } from "@tanstack/react-table"
import { Eye, Pencil, Trash2 } from "lucide-react"
import { useCallback, useMemo } from "react"
import { StatusBadge } from "@/components/atoms/StatusBadge"
import { Button } from "@/components/ui/button"
import { formatDate } from "@/lib/format"
import { can } from "@/lib/permissions"
import type { Client, Role } from "@/types"

const columnHelper = createColumnHelper<Client>()

export const useClientsTableColumns = (
  role: Role | null,
  onView: (id: number) => void,
  onEdit: (client: Client) => void,
  onDelete: (client: Client) => void,
) => {
  const handleViewClick = useCallback((id: number) => onView(id), [onView])
  const handleEditClick = useCallback((client: Client) => onEdit(client), [onEdit])
  const handleDeleteClick = useCallback((client: Client) => onDelete(client), [onDelete])

  return useMemo(
    () => [
      columnHelper.display({
        id: "avatar",
        cell: ({ row }) => (
          <div className="ml-auto h-8 w-8 rounded-full bg-muted flex items-center justify-center text-xs font-medium text-foreground">
            {row.original.firstName[0]}
            {row.original.lastName[0]}
          </div>
        ),
      }),
      columnHelper.accessor((row) => `${row.firstName} ${row.lastName}`, {
        id: "fullName",
        header: "Nombre completo",
        cell: ({ row }) => (
          <span className="font-medium">
            {row.original.firstName} {row.original.lastName}
          </span>
        ),
      }),
      columnHelper.accessor((row) => row.email, {
        id: "email",
        header: "Correo",
        cell: ({ row }) => <span className="text-muted-fg">{row.original.email}</span>,
      }),
      columnHelper.accessor((row) => row.phone, {
        id: "phone",
        header: "Teléfono",
        cell: ({ row }) => <span className="text-muted-fg">{row.original.phone ?? "—"}</span>,
      }),
      columnHelper.accessor((row) => row.status, {
        id: "status",
        header: "Estado",
        cell: ({ row }) => <StatusBadge status={row.original.status} />,
      }),
      columnHelper.accessor((row) => row.totalReservations, {
        id: "reservations",
        header: "Reservas",
        cell: ({ row }) => (
          <span className="tabular-nums text-right">{row.original.totalReservations}</span>
        ),
      }),
      columnHelper.accessor((row) => row.createdAt, {
        id: "created",
        header: "Registrado",
        cell: ({ row }) => (
          <span className="text-muted-fg">{formatDate(row.original.createdAt)}</span>
        ),
      }),
      columnHelper.display({
        id: "actions",
        header: "Acciones",
        cell: ({ row }) => (
          <div className="flex items-center justify-end gap-1">
            <Button size="sm" variant="ghost" onClick={() => handleViewClick(row.original.id)}>
              <Eye className="size-4" />
            </Button>
            <Button size="sm" variant="ghost" onClick={() => handleEditClick(row.original)}>
              <Pencil className="size-4" />
            </Button>
            <Button
              size="sm"
              variant="ghost"
              disabled={!can.deleteClient(role ?? "Employee")}
              onClick={() => handleDeleteClick(row.original)}
              className={
                !can.deleteClient(role ?? "Employee")
                  ? "text-muted-fg"
                  : "text-destructive hover:text-destructive"
              }
            >
              <Trash2 className="size-4" />
            </Button>
          </div>
        ),
      }),
    ],
    [role, handleViewClick, handleEditClick, handleDeleteClick],
  )
}

import { useMemo } from "react"
import { StatusBadge } from "@/components/atoms/StatusBadge"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import type { Reservation } from "@/types"

interface ClientReservationsDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  reservations: Reservation[]
}

export const ClientReservationsDialog = function ClientReservationsDialog({
  open,
  onOpenChange,
  reservations,
}: ClientReservationsDialogProps) {
  const displayReservations = useMemo(() => reservations, [reservations])

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-150">
        <DialogHeader>
          <DialogTitle>Historial de reservas</DialogTitle>
          <DialogDescription>Todas las reservas del cliente</DialogDescription>
        </DialogHeader>

        <div className="max-h-100 overflow-y-auto">
          {displayReservations.length === 0 ? (
            <div className="text-center py-8 text-muted-fg">No hay reservas para este cliente</div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Horario</TableHead>
                  <TableHead>Mesa</TableHead>
                  <TableHead>Comensales</TableHead>
                  <TableHead>Estado</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {displayReservations.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="text-sm">{r.date}</TableCell>
                    <TableCell className="text-sm tabular-nums">
                      {r.startTime.slice(0, 5)} – {r.endTime.slice(0, 5)}
                    </TableCell>
                    <TableCell className="text-sm">{r.table.code}</TableCell>
                    <TableCell className="text-sm tabular-nums">{r.numberOfGuests}</TableCell>
                    <TableCell className="text-sm">
                      <StatusBadge status={r.status} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </div>
      </DialogContent>
    </Dialog>
  )
}

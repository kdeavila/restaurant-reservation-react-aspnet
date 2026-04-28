import { zodResolver } from "@hookform/resolvers/zod"
import { useCallback, useEffect } from "react"
import { type SubmitHandler, useForm } from "react-hook-form"
import { z } from "zod"
import { FormField } from "@/components/molecules/FormField"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import type { Client } from "@/types"

const clientSchema = z.object({
  firstName: z.string().min(1, "Nombre requerido"),
  lastName: z.string().min(1, "Apellido requerido"),
  email: z.string().email("Email inválido"),
  phone: z.string().optional(),
})

type ClientFormValues = z.infer<typeof clientSchema>

interface ClientFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  client: Client | null
  onSubmit: (values: ClientFormValues) => Promise<void>
  isPending: boolean
  error: string | null
}

export const ClientFormDialog = function ClientFormDialog({
  open,
  onOpenChange,
  client,
  onSubmit,
  isPending,
  error,
}: ClientFormDialogProps) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ClientFormValues>({
    resolver: zodResolver(clientSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      phone: "",
    },
  })

  useEffect(() => {
    if (open && client) {
      reset({
        firstName: client.firstName,
        lastName: client.lastName,
        email: client.email,
        phone: client.phone ?? "",
      })
    } else if (open) {
      reset({
        firstName: "",
        lastName: "",
        email: "",
        phone: "",
      })
    }
  }, [open, client, reset])

  const handleClose = useCallback(() => {
    onOpenChange(false)
  }, [onOpenChange])

  const handleFormSubmit: SubmitHandler<ClientFormValues> = async (values) => {
    await onSubmit(values)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-100">
        <DialogHeader>
          <DialogTitle>{client ? "Editar cliente" : "Nuevo cliente"}</DialogTitle>
          <DialogDescription>
            {client ? "Actualiza los datos del cliente" : "Ingresa los datos del nuevo cliente"}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          {error && (
            <div className="rounded bg-destructive/10 p-3 text-sm text-destructive">{error}</div>
          )}

          <FormField label="Nombre" required error={errors.firstName?.message}>
            <Input {...register("firstName")} placeholder="Ej: Juan" />
          </FormField>

          <FormField label="Apellido" required error={errors.lastName?.message}>
            <Input {...register("lastName")} placeholder="Ej: Pérez" />
          </FormField>

          <FormField label="Correo electrónico" required error={errors.email?.message}>
            <Input type="email" {...register("email")} placeholder="ej: juan@ejemplo.com" />
          </FormField>

          <FormField label="Teléfono" error={errors.phone?.message}>
            <Input {...register("phone")} placeholder="+34 612 345 678 (opcional)" />
          </FormField>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancelar
            </Button>
            <Button type="submit" disabled={isSubmitting || isPending}>
              {isSubmitting || isPending ? "Guardando…" : "Guardar"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

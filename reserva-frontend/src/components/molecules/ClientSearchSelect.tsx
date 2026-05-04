import { Check, ChevronsUpDown, Search, X } from "lucide-react"
import { useMemo, useState } from "react"
import { StatusBadge } from "@/components/atoms/StatusBadge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import { useClientList } from "@/hooks/useClients"
import { usePacerDebouncedValue } from "@/hooks/usePacerDebouncedValue"
import { cn } from "@/lib/utils"
import type { Client } from "@/types"

interface ClientSearchSelectProps {
  value: number | null
  onChange: (id: number | null, client: Client | null) => void
  className?: string
}

const EMPTY_CLIENTS: Client[] = []
const CLIENT_RESULTS_LIMIT = 10

export function ClientSearchSelect({ value, onChange, className }: ClientSearchSelectProps) {
  const [open, setOpen] = useState(false)
  const [search, setSearch] = useState("")
  const [selectedClientState, setSelectedClientState] = useState<Client | null>(null)

  const {
    debouncedValue: debouncedSearch,
    schedule: scheduleSearch,
    setImmediate: setSearchImmediate,
  } = usePacerDebouncedValue("", { wait: 350 })

  const normalizedSearch = debouncedSearch.trim()
  const { data: listData } = useClientList({
    searchParam: normalizedSearch || undefined,
    pageSize: CLIENT_RESULTS_LIMIT,
  })

  const clients = useMemo(() => listData?.data ?? EMPTY_CLIENTS, [listData?.data])

  const selectedClient = useMemo(() => {
    if (value == null) {
      return null
    }

    if (selectedClientState?.id === value) {
      return selectedClientState
    }

    return clients.find((c) => c.id === value) ?? null
  }, [clients, selectedClientState, value])

  const results: Client[] = useMemo(() => {
    if (!search.trim()) return clients.slice(0, CLIENT_RESULTS_LIMIT)
    const term = search.trim().toLowerCase()
    return clients
      .filter(
        (c) =>
          `${c.firstName} ${c.lastName}`.toLowerCase().includes(term) ||
          c.email.toLowerCase().includes(term),
      )
      .slice(0, CLIENT_RESULTS_LIMIT)
  }, [clients, search])

  const handleSearchChange = (value: string) => {
    setSearch(value)
    scheduleSearch(value)
  }

  const handleSelect = (client: Client) => {
    if (client.status === "Inactive") return
    setSelectedClientState(client)
    onChange(client.id, client)
    setOpen(false)
    setSearchImmediate("")
    setSearch("")
  }

  const handleClear = () => {
    setSelectedClientState(null)
    onChange(null, null)
    setSearchImmediate("")
    setSearch("")
  }

  // Estado seleccionado: mostrar tarjeta con datos
  if (selectedClient) {
    return (
      <div
        className={cn(
          "flex items-center gap-3 rounded-lg border border-(--color-border) bg-(--color-surface-card) px-4 py-3",
          className,
        )}
      >
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-(--color-primary)/10 text-(--color-primary) text-sm font-semibold shrink-0">
          {selectedClient.firstName[0]}
          {selectedClient.lastName[0]}
        </div>
        <div className="flex-1 min-w-0">
          <div className="text-sm font-medium truncate">
            {selectedClient.firstName} {selectedClient.lastName}
          </div>
          <div className="text-xs text-(--color-muted-fg) truncate">{selectedClient.email}</div>
        </div>
        {selectedClient.status === "Inactive" && <StatusBadge status="Inactive" />}
        <Button variant="ghost" size="sm" className="shrink-0 h-8 px-2" onClick={handleClear}>
          <X className="h-4 w-4" /> Cambiar
        </Button>
      </div>
    )
  }

  // Estado sin seleccionar: input de búsqueda
  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          className={cn(
            "w-full justify-between font-normal text-(--color-muted-fg) h-11",
            className,
          )}
        >
          <span className="flex items-center gap-2">
            <Search className="h-4 w-4" />
            Buscar cliente por nombre o correo
          </span>
          <ChevronsUpDown className="h-4 w-4 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-(--radix-popover-trigger-width) p-0" align="start">
        <div className="p-2 border-b border-(--color-border)">
          <Input
            autoFocus
            placeholder="Escribe para filtrar..."
            value={search}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="h-9"
          />
        </div>
        <div className="max-h-72 overflow-auto py-1">
          {results.length === 0 ? (
            <div className="px-3 py-6 text-center text-sm text-(--color-muted-fg)">
              Sin coincidencias
            </div>
          ) : (
            results.map((client: Client) => (
              <button
                key={client.id}
                type="button"
                onClick={() => handleSelect(client)}
                disabled={client.status === "Inactive"}
                className={cn(
                  "w-full text-left px-3 py-2.5 flex items-center gap-3 hover:bg-accent transition-colors",
                  "disabled:opacity-50 disabled:cursor-not-allowed",
                )}
              >
                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-xs font-medium shrink-0">
                  {client.firstName[0]}
                  {client.lastName[0]}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-medium truncate">
                    {client.firstName} {client.lastName}
                  </div>
                  <div className="text-xs text-(--color-muted-fg) truncate">{client.email}</div>
                </div>
                {client.status === "Inactive" ? (
                  <StatusBadge status="Inactive" />
                ) : (
                  <Check className="h-4 w-4 opacity-0 shrink-0" />
                )}
              </button>
            ))
          )}
        </div>
      </PopoverContent>
    </Popover>
  )
}

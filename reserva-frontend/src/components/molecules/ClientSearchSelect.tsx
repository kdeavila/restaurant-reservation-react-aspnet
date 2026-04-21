import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { ChevronsUpDown, UserRound } from "lucide-react";
import { cn } from "@/lib/utils";
import type { Client } from "@/types";
import { useClientList } from "@/hooks/useClients";

interface ClientSearchSelectProps {
  value: number | null;
  onChange: (id: number, client: Client) => void;
  className?: string;
}

export function ClientSearchSelect({
  value,
  onChange,
  className,
}: ClientSearchSelectProps) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);

  const { data: listData } = useClientList({
    firstName: search,
    pageSize: 10,
  });

  const clients = listData?.data || [];

  useEffect(() => {
    if (value && !selectedClient) {
      const found = clients.find((c) => c.id === value);
      if (found) setSelectedClient(found);
    }
  }, [value, clients, selectedClient]);

  const handleSelect = (client: Client) => {
    setSelectedClient(client);
    onChange(client.id, client);
    setOpen(false);
    setSearch("");
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          className={cn(
            "w-full justify-between",
            !selectedClient && "text-muted-fg",
            className,
          )}
        >
          {selectedClient
            ? `${selectedClient.firstName} ${selectedClient.lastName} (${selectedClient.email})`
            : "Seleccionar cliente..."}
          <ChevronsUpDown className="ml-2 h-4 w-4 opacity-50 shrink-0" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-full p-0" align="start">
        <Command className="bg-surface-card p-0">
          <div className="p-2 border-b">
            <Input
              placeholder="Buscar por nombre..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="h-8"
            />
          </div>
          <CommandList>
            <CommandEmpty>No se encontraron clientes.</CommandEmpty>
            <CommandGroup className="p-0">
              {clients.map((client) => (
                <CommandItem
                  key={client.id}
                  value={String(client.id)}
                  onSelect={() => handleSelect(client)}
                  className="cursor-pointer rounded-none border-b border-border/70 bg-transparent px-3 py-2.5 data-[selected=true]:bg-accent/45 last:border-b-0 [&>svg:last-child]:hidden"
                >
                  <UserRound className="size-4 mr-2 text-muted-fg" />
                  <div className="flex flex-1 flex-col gap-y-0.5">
                    <span className="font-medium">
                      {client.firstName} {client.lastName}
                    </span>
                    <span className="text-xs text-muted-fg">
                      {client.email}
                    </span>
                  </div>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}

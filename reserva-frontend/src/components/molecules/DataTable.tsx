import { type ColumnDef, flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { cn } from "@/lib/utils"

interface DataTableProps<T> {
  columns: ColumnDef<T, unknown>[]
  data: T[]
  loading?: boolean
  className?: string
}

export function DataTable<T>({ columns, data, loading, className }: DataTableProps<T>) {
  const table = useReactTable({
    data: loading ? [] : data,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  const skeletonRows = ["row-0", "row-1", "row-2", "row-3", "row-4"]

  return (
    <div className={cn("w-full surface-card overflow-hidden", className)}>
      <div className="w-full overflow-auto">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id} className="hover:bg-transparent">
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id} className="font-medium text-muted-fg">
                    {header.isPlaceholder
                      ? null
                      : flexRender(header.column.columnDef.header, header.getContext())}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {loading
              ? skeletonRows.map((rowKey) => (
                  <TableRow key={rowKey}>
                    {table.getAllColumns().map((column) => (
                      <TableCell key={`${rowKey}-${column.id}`}>
                        <div>
                          <Skeleton className="h-5 w-2/3" />
                        </div>
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              : table.getRowModel().rows.length
                ? table.getRowModel().rows.map((row) => (
                    <TableRow key={row.id} className="table-row-hover">
                      {row.getVisibleCells().map((cell) => (
                        <TableCell key={cell.id} className="text-sm text-foreground">
                          {flexRender(cell.column.columnDef.cell, cell.getContext())}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                : null}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}

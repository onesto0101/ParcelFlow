import { useEffect, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router'
import { ArrowRightIcon, PackageSearchIcon, PlusIcon, SearchIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { Pagination } from '@/shared/components/Pagination'
import { StatusBadge } from '@/shared/components/StatusBadge'
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue'
import { formatDateTime, formatMoney } from '@/shared/lib/format'
import { useShipments } from '../api/hooks'
import type { ShipmentStatus } from '../api/types'

const STATUS_OPTIONS: { value: ShipmentStatus | 'all'; label: string }[] = [
  { value: 'all', label: 'Todos los estados' },
  { value: 'Registered', label: 'Registrado' },
  { value: 'Delivered', label: 'Entregado' },
  { value: 'Cancelled', label: 'Cancelado' },
]

const PAGE_SIZE = 15

/**
 * Página "Envíos": tabla con filtros y paginación (GET /api/shipments).
 *
 * Los filtros viven en la URL (?status=Cancelled&q=mty&page=2), no en useState: así se puede
 * recargar, compartir el enlace o volver atrás desde un detalle sin perder la búsqueda.
 */
export function ShipmentsPage() {
  const navigate = useNavigate()
  const [params, setParams] = useSearchParams()

  const status = (params.get('status') as ShipmentStatus | null) ?? undefined
  const page = Number(params.get('page') ?? '1')

  // El input se actualiza al teclear; la URL (y la petición) solo cuando se deja de teclear.
  const [searchInput, setSearchInput] = useState(params.get('q') ?? '')
  const search = useDebouncedValue(searchInput.trim())

  function updateParams(changes: Record<string, string | undefined>) {
    setParams(
      (current) => {
        const next = new URLSearchParams(current)
        for (const [key, value] of Object.entries(changes)) {
          if (value) next.set(key, value)
          else next.delete(key)
        }
        return next
      },
      { replace: true }, // no llenar el historial del navegador con cada letra
    )
  }

  // Cuando la búsqueda (ya "debounced") cambia, se copia a la URL y se vuelve a la página 1:
  // la página 3 de la búsqueda anterior no tiene sentido para la nueva.
  useEffect(() => {
    setParams(
      (current) => {
        if ((current.get('q') ?? '') === search) return current
        const next = new URLSearchParams(current)
        if (search) next.set('q', search)
        else next.delete('q')
        next.delete('page')
        return next
      },
      { replace: true },
    )
  }, [search, setParams])

  const { data, isPending, isFetching, error } = useShipments({ status, search: search || undefined, page, pageSize: PAGE_SIZE })

  return (
    <>
      <PageHeader
        title="Envíos"
        description="Consulta y administra los envíos registrados"
        actions={
          <Button asChild size="lg">
            <Link to="/envios/nuevo">
              <PlusIcon /> Registrar envío
            </Link>
          </Button>
        }
      />

      <Card className="gap-0 overflow-hidden py-0">
        <div className="flex flex-wrap items-center gap-3 border-b p-4">
          <div className="relative min-w-64 flex-1">
            <SearchIcon className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              placeholder="Buscar por guía, nombre o ciudad…"
              className="h-9 pl-9"
              aria-label="Buscar envíos"
            />
          </div>
          <Select
            value={status ?? 'all'}
            onValueChange={(value) => updateParams({ status: value === 'all' ? undefined : value, page: undefined })}
          >
            <SelectTrigger className="h-9 w-52" aria-label="Filtrar por estado">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {STATUS_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {isFetching && !isPending && <span className="text-xs text-muted-foreground">Actualizando…</span>}
        </div>

        {isPending ? (
          <TableSkeleton />
        ) : error ? (
          <EmptyState icon={<PackageSearchIcon className="size-6" />} title="No se pudo cargar el listado">
            {error.message}
          </EmptyState>
        ) : data.items.length === 0 ? (
          <EmptyState icon={<PackageSearchIcon className="size-6" />} title="No hay envíos que coincidan">
            Prueba con otra búsqueda o quita el filtro de estado.
          </EmptyState>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-muted/50 text-left text-xs font-semibold tracking-wider text-muted-foreground uppercase">
                  <tr>
                    <th className="px-4 py-3">Guía</th>
                    <th className="px-4 py-3">Estado</th>
                    <th className="px-4 py-3">Remitente</th>
                    <th className="px-4 py-3">Destinatario</th>
                    <th className="px-4 py-3 text-right">Costo</th>
                    <th className="px-4 py-3">Registrado</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {data.items.map((s) => (
                    <tr
                      key={s.id}
                      onClick={() => navigate(`/envios/${s.trackingNumber}`)}
                      className="cursor-pointer transition-colors hover:bg-muted/40"
                    >
                      <td className="px-4 py-3">
                        {/* Link real (y no solo onClick en la fila) para poder abrir en otra pestaña y navegar con teclado. */}
                        <Link to={`/envios/${s.trackingNumber}`} className="font-mono font-semibold tabular hover:underline">
                          {s.trackingNumber}
                        </Link>
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={s.status} />
                      </td>
                      <td className="px-4 py-3">
                        <div className="font-medium">{s.senderName}</div>
                        <div className="text-xs text-muted-foreground">{s.originCity}</div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-1.5 font-medium">
                          <ArrowRightIcon className="size-3.5 text-muted-foreground" />
                          {s.recipientName}
                        </div>
                        <div className="pl-5 text-xs text-muted-foreground">{s.destinationCity}</div>
                      </td>
                      <td className="px-4 py-3 text-right tabular">{formatMoney(s.cost, s.currency)}</td>
                      <td className="px-4 py-3 whitespace-nowrap text-muted-foreground">{formatDateTime(s.createdAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <Pagination result={data} onPageChange={(p) => updateParams({ page: p === 1 ? undefined : String(p) })} />
          </>
        )}
      </Card>
    </>
  )
}

function TableSkeleton() {
  return (
    <div className="space-y-3 p-4">
      {Array.from({ length: 6 }, (_, i) => (
        <Skeleton key={i} className="h-10 w-full" />
      ))}
    </div>
  )
}

import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router'
import { ArchiveIcon, SearchIcon, UserRoundIcon } from 'lucide-react'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { Pagination } from '@/shared/components/Pagination'
import { StatusBadge } from '@/shared/components/StatusBadge'
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue'
import { formatDateTime } from '@/shared/lib/format'
import { useDeliveries } from '../api/hooks'
import type { DeliveryStatus } from '../api/types'

const TABS: { value: DeliveryStatus | 'all'; label: string }[] = [
  { value: 'Pending', label: 'Pendientes' },
  { value: 'Assigned', label: 'En reparto' },
  { value: 'Delivered', label: 'Entregadas' },
  { value: 'Failed', label: 'Fallidas' },
  { value: 'all', label: 'Todas' },
]

const PAGE_SIZE = 15

/**
 * Página "Entregas" (DE-001 pendientes, DE-003 por repartidor).
 * Pestaña por defecto: Pendientes, la cola de trabajo del despachador (las más antiguas primero).
 */
export function DeliveriesPage() {
  const navigate = useNavigate()
  const [params, setParams] = useSearchParams()
  const tab = (params.get('status') as DeliveryStatus | 'all' | null) ?? 'Pending'
  const page = Number(params.get('page') ?? '1')

  const [searchInput, setSearchInput] = useState('')
  const [courierInput, setCourierInput] = useState('')
  const search = useDebouncedValue(searchInput.trim())
  const courierId = useDebouncedValue(courierInput.trim())

  const { data, isPending, error } = useDeliveries({
    status: tab === 'all' ? undefined : tab,
    search: search || undefined,
    // La API valida CourierId (mínimo 2 caracteres): no se manda mientras se está escribiendo el primero.
    courierId: courierId.length >= 2 ? courierId : undefined,
    page,
    pageSize: PAGE_SIZE,
  })

  function setTab(value: string) {
    setParams(value === 'Pending' ? {} : { status: value }, { replace: true })
  }

  function setPage(p: number) {
    setParams(
      (current) => {
        const next = new URLSearchParams(current)
        if (p === 1) next.delete('page')
        else next.set('page', String(p))
        return next
      },
      { replace: true },
    )
  }

  return (
    <>
      <PageHeader title="Entregas" description="Asigna repartidores y registra los intentos de entrega" />

      <Card className="gap-0 overflow-hidden py-0">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b p-4">
          <nav className="flex flex-wrap gap-1 rounded-lg bg-muted p-1" aria-label="Estado de la entrega">
            {TABS.map((t) => (
              <button
                key={t.value}
                type="button"
                onClick={() => setTab(t.value)}
                aria-current={tab === t.value ? 'page' : undefined}
                className={cn(
                  'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
                  tab === t.value ? 'bg-white text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground',
                )}
              >
                {t.label}
              </button>
            ))}
          </nav>

          <div className="flex flex-wrap gap-2">
            <div className="relative w-48">
              <SearchIcon className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={searchInput}
                onChange={(e) => {
                  setSearchInput(e.target.value)
                  setPage(1)
                }}
                placeholder="Guía…"
                className="h-9 pl-9 font-mono uppercase"
                aria-label="Buscar por guía"
              />
            </div>
            <div className="relative w-48">
              <UserRoundIcon className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={courierInput}
                onChange={(e) => {
                  setCourierInput(e.target.value)
                  setPage(1)
                }}
                placeholder="Repartidor…"
                className="h-9 pl-9"
                aria-label="Filtrar por repartidor"
              />
            </div>
          </div>
        </div>

        {isPending ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 5 }, (_, i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
        ) : error ? (
          <EmptyState icon={<ArchiveIcon className="size-6" />} title="No se pudo cargar el listado">
            {error.message}
          </EmptyState>
        ) : data.items.length === 0 ? (
          <EmptyState icon={<ArchiveIcon className="size-6" />} title="No hay entregas aquí">
            {tab === 'Pending'
              ? 'Una entrega aparece cuando el paquete llega a un centro de distribución (Rastreo → "Llegó a centro de distribución").'
              : 'Prueba con otra pestaña o quita los filtros.'}
          </EmptyState>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-muted/50 text-left text-xs font-semibold tracking-wider text-muted-foreground uppercase">
                  <tr>
                    <th className="px-4 py-3">Guía</th>
                    <th className="px-4 py-3">Estado</th>
                    <th className="px-4 py-3">Repartidor</th>
                    <th className="px-4 py-3 text-center">Intentos</th>
                    <th className="px-4 py-3">Lista desde</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {data.items.map((d) => (
                    <tr key={d.id} onClick={() => navigate(`/entregas/${d.id}`)} className="cursor-pointer transition-colors hover:bg-muted/40">
                      <td className="px-4 py-3">
                        <Link to={`/entregas/${d.id}`} className="font-mono font-semibold tabular hover:underline">
                          {d.trackingNumber}
                        </Link>
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={d.status} />
                      </td>
                      <td className="px-4 py-3">{d.courierId ?? <span className="text-muted-foreground">Sin asignar</span>}</td>
                      <td className="px-4 py-3 text-center tabular">{d.attemptCount} / 3</td>
                      <td className="px-4 py-3 whitespace-nowrap text-muted-foreground">{formatDateTime(d.createdAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <Pagination result={data} onPageChange={setPage} />
          </>
        )}
      </Card>
    </>
  )
}

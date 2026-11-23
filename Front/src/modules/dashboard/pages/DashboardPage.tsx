import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { ArrowRightIcon, CircleAlertIcon, CircleCheckIcon, PackageIcon, PlusIcon, TruckIcon, WarehouseIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { useDeliveries } from '@/modules/deliveries/api/hooks'
import { useShipments } from '@/modules/shipments/api/hooks'
import { PageHeader } from '@/shared/components/PageHeader'
import { StatusBadge } from '@/shared/components/StatusBadge'
import { formatDateTime } from '@/shared/lib/format'
import { useDashboard } from '../api/dashboardApi'

/**
 * Dashboard (§8.2): los 5 indicadores + lo más reciente de Envíos y la cola de Entregas.
 *
 * Reutiliza los hooks de los otros módulos del front (useShipments, useDeliveries) en vez de
 * pedir un "endpoint de dashboard" gigante: el backend solo compone lo que de verdad hay que
 * calcular en servidor (los conteos).
 */
export function DashboardPage() {
  const { data, isPending, error } = useDashboard()

  return (
    <>
      <PageHeader
        title="Dashboard"
        description="Resumen de la operación en tiempo real"
        actions={
          <Button asChild size="lg">
            <Link to="/envios/nuevo">
              <PlusIcon /> Registrar envío
            </Link>
          </Button>
        }
      />

      {error && (
        <Card className="mb-6 border-rose-200 bg-rose-50">
          <CardContent className="text-sm text-rose-700">No se pudieron cargar los indicadores: {error.message}</CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-5">
        <Kpi loading={isPending} label="Envíos registrados" value={data?.totalShipments} icon={<PackageIcon />} tone="slate" to="/envios" />
        <Kpi loading={isPending} label="En tránsito" value={data?.inTransit} icon={<WarehouseIcon />} tone="blue" hint="Recolectados, en camino o en centro" />
        <Kpi loading={isPending} label="En reparto" value={data?.outForDelivery} icon={<TruckIcon />} tone="amber" to="/entregas?status=Assigned" />
        <Kpi loading={isPending} label="Entregados" value={data?.delivered} icon={<CircleCheckIcon />} tone="emerald" to="/envios?status=Delivered" />
        <Kpi loading={isPending} label="Con problemas" value={data?.withDeliveryProblems} icon={<CircleAlertIcon />} tone="rose" hint="Último intento de entrega fallido" />
      </div>

      <div className="mt-6 grid grid-cols-1 gap-6 xl:grid-cols-2">
        <RecentShipments />
        <PendingDeliveries pending={data?.deliveries.pending} />
      </div>
    </>
  )
}

const TONES = {
  slate: 'bg-slate-100 text-slate-700',
  blue: 'bg-blue-100 text-blue-700',
  amber: 'bg-amber-100 text-amber-700',
  emerald: 'bg-emerald-100 text-emerald-700',
  rose: 'bg-rose-100 text-rose-700',
}

function Kpi({
  label,
  value,
  icon,
  tone,
  hint,
  to,
  loading,
}: {
  label: string
  value?: number
  icon: ReactNode
  tone: keyof typeof TONES
  hint?: string
  to?: string
  loading: boolean
}) {
  const content = (
    <Card className={cn('h-full', to && 'transition-shadow hover:shadow-md')}>
      <CardContent className="space-y-3">
        <div className={cn('flex size-10 items-center justify-center rounded-lg [&_svg]:size-5', TONES[tone])}>{icon}</div>
        <div>
          <div className="text-sm text-muted-foreground">{label}</div>
          {loading ? <Skeleton className="mt-1 h-9 w-16" /> : <div className="text-3xl font-bold tabular">{value ?? '—'}</div>}
          {hint && <div className="mt-1 text-xs text-muted-foreground">{hint}</div>}
        </div>
      </CardContent>
    </Card>
  )

  return to ? <Link to={to}>{content}</Link> : content
}

function RecentShipments() {
  const { data, isPending } = useShipments({ pageSize: 5 })

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Envíos recientes</CardTitle>
        <Button asChild variant="ghost" size="sm">
          <Link to="/envios">
            Ver todos <ArrowRightIcon />
          </Link>
        </Button>
      </CardHeader>
      <CardContent>
        {isPending ? (
          <ListSkeleton />
        ) : !data || data.items.length === 0 ? (
          <p className="text-sm text-muted-foreground">Aún no hay envíos.</p>
        ) : (
          <ul className="divide-y">
            {data.items.map((s) => (
              <li key={s.id}>
                <Link to={`/envios/${s.trackingNumber}`} className="flex items-center justify-between gap-3 py-2.5 hover:bg-muted/40">
                  <div className="min-w-0">
                    <div className="font-mono text-sm font-semibold tabular">{s.trackingNumber}</div>
                    <div className="truncate text-xs text-muted-foreground">
                      {s.originCity} → {s.destinationCity} · {formatDateTime(s.createdAt)}
                    </div>
                  </div>
                  <StatusBadge status={s.status} />
                </Link>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}

function PendingDeliveries({ pending }: { pending?: number }) {
  const { data, isPending } = useDeliveries({ status: 'Pending', pageSize: 5 })

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>
          Entregas por asignar {pending !== undefined && <span className="ml-1 text-muted-foreground tabular">({pending})</span>}
        </CardTitle>
        <Button asChild variant="ghost" size="sm">
          <Link to="/entregas">
            Ir a Entregas <ArrowRightIcon />
          </Link>
        </Button>
      </CardHeader>
      <CardContent>
        {isPending ? (
          <ListSkeleton />
        ) : !data || data.items.length === 0 ? (
          <p className="text-sm text-muted-foreground">No hay entregas pendientes de asignar.</p>
        ) : (
          <ul className="divide-y">
            {data.items.map((d) => (
              <li key={d.id}>
                <Link to={`/entregas/${d.id}`} className="flex items-center justify-between gap-3 py-2.5 hover:bg-muted/40">
                  <div>
                    <div className="font-mono text-sm font-semibold tabular">{d.trackingNumber}</div>
                    <div className="text-xs text-muted-foreground">
                      Lista desde {formatDateTime(d.createdAt)} · intentos {d.attemptCount}/3
                    </div>
                  </div>
                  <StatusBadge status={d.status} />
                </Link>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}

function ListSkeleton() {
  return (
    <div className="space-y-2">
      {Array.from({ length: 4 }, (_, i) => (
        <Skeleton key={i} className="h-11 w-full" />
      ))}
    </div>
  )
}

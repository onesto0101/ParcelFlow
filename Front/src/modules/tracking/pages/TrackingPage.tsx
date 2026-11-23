import { useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { ClockIcon, CompassIcon, ExternalLinkIcon, MapPinIcon, PackageSearchIcon, SearchIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { ApiError } from '@/shared/api/http'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { StatusBadge } from '@/shared/components/StatusBadge'
import { TrackingNumber } from '@/shared/components/TrackingNumber'
import { formatDateTime, normalizeTrackingNumber } from '@/shared/lib/format'
import { useTracking } from '../api/hooks'
import type { Tracking } from '../api/types'
import { RegisterMovementDialog } from '../components/RegisterMovementDialog'
import { TrackingTimeline } from '../components/TrackingTimeline'

/**
 * Página "Rastreo" (TR-002, TR-003). Una sola página para dos rutas:
 *   /rastreo                → solo el buscador
 *   /rastreo/:trackingNumber → buscador + estado actual + timeline + registrar movimiento
 */
export function TrackingPage() {
  const trackingNumber = normalizeTrackingNumber(useParams().trackingNumber ?? '')

  return (
    <>
      <PageHeader title="Rastreo" description="Estado actual e historial de movimientos de un paquete" />
      <div className="space-y-6">
        {/* key: si la guía de la URL cambia (ej. desde el buscador superior), el input se reinicia con ella. */}
        <SearchCard key={trackingNumber} initial={trackingNumber} />
        {trackingNumber ? (
          <TrackingResult trackingNumber={trackingNumber} />
        ) : (
          <Card>
            <EmptyState icon={<CompassIcon className="size-6" />} title="Busca un paquete">
              Escribe un número de guía para ver dónde está y todo su recorrido.
            </EmptyState>
          </Card>
        )}
      </div>
    </>
  )
}

function SearchCard({ initial }: { initial: string }) {
  const navigate = useNavigate()
  const [query, setQuery] = useState(initial)

  function onSubmit(event: FormEvent) {
    event.preventDefault()
    const trackingNumber = normalizeTrackingNumber(query)
    if (trackingNumber) navigate(`/rastreo/${trackingNumber}`)
  }

  return (
    <Card>
      <CardContent>
        <form onSubmit={onSubmit} className="flex gap-2">
          <div className="relative flex-1">
            <SearchIcon className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="PF7K3M9QXR2A"
              className="h-10 pl-9 font-mono uppercase"
              aria-label="Número de guía"
            />
          </div>
          <Button type="submit" size="lg" className="h-10" disabled={!query.trim()}>
            Rastrear
          </Button>
        </form>
      </CardContent>
    </Card>
  )
}

function TrackingResult({ trackingNumber }: { trackingNumber: string }) {
  const { data: tracking, isPending, error } = useTracking(trackingNumber)

  if (isPending) {
    return (
      <div className="grid gap-6 lg:grid-cols-[1fr_2fr]">
        <Skeleton className="h-64" />
        <Skeleton className="h-96" />
      </div>
    )
  }

  if (error) {
    const notFound = error instanceof ApiError && error.status === 404
    return (
      <Card>
        <EmptyState
          icon={<PackageSearchIcon className="size-6" />}
          title={notFound ? `No hay seguimiento para ${trackingNumber}` : 'No se pudo cargar el seguimiento'}
        >
          {notFound ? 'Revisa el número de guía. Si el envío se acaba de registrar, espera unos segundos.' : error.message}
        </EmptyState>
      </Card>
    )
  }

  return (
    <div className="grid items-start gap-6 lg:grid-cols-[1fr_2fr]">
      <CurrentStatusCard tracking={tracking} />
      <Card>
        <CardHeader className="flex flex-row items-center justify-between gap-4">
          <CardTitle>Historial de movimientos</CardTitle>
          <span className="text-sm text-muted-foreground tabular">{tracking.events.length} movimientos</span>
        </CardHeader>
        <CardContent>
          <TrackingTimeline events={tracking.events} />
        </CardContent>
      </Card>
    </div>
  )
}

function CurrentStatusCard({ tracking }: { tracking: Tracking }) {
  return (
    <Card>
      <CardHeader>
        <div className="text-[11px] font-semibold tracking-wider text-muted-foreground uppercase">Estado actual</div>
        <CardTitle className="pt-1">
          <TrackingNumber value={tracking.trackingNumber} size="lg" />
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <StatusBadge status={tracking.currentStatus} className="px-3 py-1.5 text-xs" />

        <dl className="space-y-3 text-sm">
          <div className="flex items-start gap-2">
            <MapPinIcon className="mt-0.5 size-4 text-muted-foreground" />
            <div>
              <dt className="text-xs text-muted-foreground">Ubicación</dt>
              <dd className="font-medium">{tracking.currentLocation}</dd>
            </div>
          </div>
          <div className="flex items-start gap-2">
            <ClockIcon className="mt-0.5 size-4 text-muted-foreground" />
            <div>
              <dt className="text-xs text-muted-foreground">Última actualización</dt>
              <dd className="font-medium">{formatDateTime(tracking.lastUpdatedAt)}</dd>
            </div>
          </div>
        </dl>

        <div className="flex flex-col gap-2 border-t pt-4">
          <RegisterMovementDialog tracking={tracking} />
          {!tracking.isFinal && tracking.nextManualStatuses.length === 0 && (
            <p className="text-xs text-muted-foreground">
              El siguiente paso lo registra el módulo de Entregas (asignar repartidor, confirmar o reportar un fallo).
            </p>
          )}
          <Button asChild variant="outline">
            <Link to={`/envios/${tracking.trackingNumber}`}>
              <ExternalLinkIcon /> Ver detalle del envío
            </Link>
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

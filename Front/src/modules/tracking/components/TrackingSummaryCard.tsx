import { Link } from 'react-router'
import { ArrowRightIcon, CompassIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { useTracking } from '../api/hooks'
import { TrackingTimeline } from './TrackingTimeline'

/**
 * Resumen del seguimiento para incrustar en otras páginas (el detalle del envío).
 *
 * Es la "puerta" pública del módulo tracking del front hacia los demás: Shipments no importa
 * hooks ni tipos de tracking, solo este componente. Misma idea que los IntegrationEvents
 * en el backend: una superficie pequeña y explícita entre módulos.
 */
export function TrackingSummaryCard({ trackingNumber }: { trackingNumber: string }) {
  const { data: tracking, isPending, error } = useTracking(trackingNumber)

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="flex size-10 items-center justify-center rounded-lg bg-secondary text-secondary-foreground">
            <CompassIcon className="size-5" />
          </div>
          <CardTitle>Seguimiento</CardTitle>
        </div>
        <Button asChild variant="ghost" size="sm">
          <Link to={`/rastreo/${trackingNumber}`}>
            Ver rastreo completo <ArrowRightIcon />
          </Link>
        </Button>
      </CardHeader>
      <CardContent>
        {isPending ? (
          <div className="space-y-3">
            <Skeleton className="h-14 w-full" />
            <Skeleton className="h-14 w-full" />
          </div>
        ) : error ? (
          <p className="text-sm text-muted-foreground">
            El seguimiento aún no está disponible. Si el envío se acaba de registrar, aparecerá en unos segundos.
          </p>
        ) : (
          <TrackingTimeline events={tracking.events} limit={3} />
        )}
      </CardContent>
    </Card>
  )
}

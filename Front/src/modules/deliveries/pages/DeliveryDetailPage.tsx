import { Link, useParams } from 'react-router'
import { ArchiveIcon, CircleCheckIcon, CircleXIcon, CompassIcon, ExternalLinkIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { StatusBadge } from '@/shared/components/StatusBadge'
import { TrackingNumber } from '@/shared/components/TrackingNumber'
import { formatDateTime } from '@/shared/lib/format'
import { useDelivery } from '../api/hooks'
import type { Delivery, DeliveryAttempt } from '../api/types'
import { AssignCourierDialog, ConfirmDeliveryDialog, ReportFailureDialog } from '../components/DeliveryActionDialogs'
import { FAILURE_REASONS } from '../lib/failureReasons'

export function DeliveryDetailPage() {
  const id = useParams().id ?? ''
  const { data: delivery, isPending, error } = useDelivery(id)
  const breadcrumbs = [{ label: 'Entregas', to: '/entregas' }, { label: delivery?.trackingNumber ?? 'Detalle' }]

  if (isPending) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-64" />
        <Skeleton className="h-40 w-full" />
        <Skeleton className="h-56 w-full" />
      </div>
    )
  }

  if (error) {
    return (
      <>
        <PageHeader breadcrumbs={breadcrumbs} title="Detalle de entrega" />
        <Card>
          <EmptyState icon={<ArchiveIcon className="size-6" />} title="No se pudo cargar la entrega">
            {error.message}
          </EmptyState>
        </Card>
      </>
    )
  }

  return (
    <>
      <PageHeader breadcrumbs={breadcrumbs} title="Detalle de entrega" />
      <div className="space-y-6">
        <SummaryCard delivery={delivery} />
        <AttemptsCard delivery={delivery} />
      </div>
    </>
  )
}

function SummaryCard({ delivery }: { delivery: Delivery }) {
  return (
    <Card>
      <CardContent className="space-y-5">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="space-y-2">
            <div className="flex flex-wrap items-center gap-3">
              <TrackingNumber value={delivery.trackingNumber} size="lg" />
              <StatusBadge status={delivery.status} />
            </div>
            <p className="text-sm text-muted-foreground">Lista para reparto desde el {formatDateTime(delivery.createdAt)}</p>
          </div>

          {/* Qué botones se ven lo decide el backend (canAssign / canRegisterAttempt). */}
          <div className="flex flex-wrap gap-2">
            {delivery.canRegisterAttempt && <ConfirmDeliveryDialog delivery={delivery} />}
            {delivery.canRegisterAttempt && <ReportFailureDialog delivery={delivery} />}
            {delivery.canAssign && <AssignCourierDialog delivery={delivery} />}
          </div>
        </div>

        <dl className="grid grid-cols-2 gap-3 md:grid-cols-4">
          <Stat label="Repartidor" value={delivery.courierId ?? 'Sin asignar'} />
          <Stat label="Asignada" value={delivery.assignedAt ? formatDateTime(delivery.assignedAt) : '—'} />
          <Stat label="Intentos restantes" value={`${delivery.remainingAttempts} de ${delivery.maxAttempts}`} />
          <Stat label="Finalizada" value={delivery.completedAt ? formatDateTime(delivery.completedAt) : '—'} />
        </dl>

        <div className="flex flex-wrap gap-2 border-t pt-4">
          <Button asChild variant="outline" size="sm">
            <Link to={`/rastreo/${delivery.trackingNumber}`}>
              <CompassIcon /> Ver rastreo
            </Link>
          </Button>
          <Button asChild variant="outline" size="sm">
            <Link to={`/envios/${delivery.trackingNumber}`}>
              <ExternalLinkIcon /> Ver envío
            </Link>
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}

/** DE-009: historial de intentos. */
function AttemptsCard({ delivery }: { delivery: Delivery }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Intentos de entrega</CardTitle>
      </CardHeader>
      <CardContent>
        {delivery.attempts.length === 0 ? (
          <p className="text-sm text-muted-foreground">Todavía no hay intentos registrados.</p>
        ) : (
          <ol className="space-y-3">
            {delivery.attempts.map((attempt) => (
              <AttemptRow key={attempt.attemptNumber} attempt={attempt} />
            ))}
          </ol>
        )}
      </CardContent>
    </Card>
  )
}

function AttemptRow({ attempt }: { attempt: DeliveryAttempt }) {
  const Icon = attempt.succeeded ? CircleCheckIcon : CircleXIcon
  return (
    <li className={cn('flex gap-3 rounded-lg border p-3', attempt.succeeded ? 'border-emerald-200 bg-emerald-50/50' : 'border-rose-200 bg-rose-50/50')}>
      <Icon className={cn('mt-0.5 size-5 shrink-0', attempt.succeeded ? 'text-emerald-600' : 'text-rose-600')} />
      <div className="min-w-0 flex-1 text-sm">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <span className="font-semibold">
            Intento {attempt.attemptNumber} · {attempt.succeeded ? 'Entregado' : 'Fallido'}
          </span>
          <time className="text-xs text-muted-foreground tabular">{formatDateTime(attempt.attemptedAt)}</time>
        </div>
        <p className="text-muted-foreground">Repartidor: {attempt.courierId}</p>
        {attempt.succeeded ? (
          <p>
            Recibió: <strong>{attempt.receivedBy}</strong>
            {attempt.notes && <span className="text-muted-foreground"> — {attempt.notes}</span>}
          </p>
        ) : (
          <p>
            Motivo: <strong>{attempt.failureCode ? FAILURE_REASONS[attempt.failureCode] : '—'}</strong>
            {attempt.failureDetails && <span className="text-muted-foreground"> — {attempt.failureDetails}</span>}
          </p>
        )}
      </div>
    </li>
  )
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg bg-muted/60 p-3">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="font-semibold">{value}</dd>
    </div>
  )
}

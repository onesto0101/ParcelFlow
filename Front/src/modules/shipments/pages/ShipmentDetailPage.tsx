import type { ReactNode } from 'react'
import { Link, useParams } from 'react-router'
import { ArrowRightIcon, BanIcon, BoxIcon, ClockIcon, PackageSearchIcon, PhoneIcon, PlaneLandingIcon, PlaneTakeoffIcon, ReceiptIcon } from 'lucide-react'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { ApiError } from '@/shared/api/http'
import { PageHeader } from '@/shared/components/PageHeader'
import { StatusBadge } from '@/shared/components/StatusBadge'
import { TrackingNumber } from '@/shared/components/TrackingNumber'
import { TrackingSummaryCard } from '@/modules/tracking/components/TrackingSummaryCard'
import { formatDateTime, formatKg, formatMoney, normalizeTrackingNumber } from '@/shared/lib/format'
import { useShipment } from '../api/hooks'
import type { ContactInput, Shipment } from '../api/types'
import { CancelShipmentDialog } from '../components/CancelShipmentDialog'
import { BASE_FEE, estimateCost } from '../lib/shippingCost'

export function ShipmentDetailPage() {
  const trackingNumber = normalizeTrackingNumber(useParams().trackingNumber ?? '')
  const { data: shipment, isPending, error } = useShipment(trackingNumber)

  const breadcrumbs = [{ label: 'Envíos', to: '/envios' }, { label: trackingNumber }]

  if (isPending) return <DetailSkeleton />

  if (error) {
    return (
      <>
        <PageHeader breadcrumbs={breadcrumbs} title="Detalle de envío" />
        <NotFound trackingNumber={trackingNumber} error={error} />
      </>
    )
  }

  return (
    <>
      <PageHeader breadcrumbs={breadcrumbs} title="Detalle de envío" />
      <div className="space-y-6">
        <SummaryHeader shipment={shipment} />

        {shipment.status === 'Cancelled' && (
          <Alert className="border-slate-300 bg-slate-50">
            <BanIcon />
            <AlertTitle>Envío cancelado el {formatDateTime(shipment.cancelledAt!)}</AlertTitle>
            <AlertDescription>Motivo: {shipment.cancellationReason}</AlertDescription>
          </Alert>
        )}

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_auto_1fr]">
          <ContactCard role="Origen del envío" title="Remitente" icon={<PlaneTakeoffIcon className="size-5" />} contact={shipment.sender} />
          <div className="hidden flex-col items-center justify-center gap-2 lg:flex">
            <div className="flex size-11 items-center justify-center rounded-full bg-primary text-primary-foreground">
              <ArrowRightIcon className="size-5" />
            </div>
            <span className="text-xs font-semibold text-muted-foreground uppercase">Trayecto</span>
          </div>
          <ContactCard role="Destino del envío" title="Destinatario" icon={<PlaneLandingIcon className="size-5" />} contact={shipment.recipient} />
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <PackageCard shipment={shipment} />
          <CostCard shipment={shipment} />
        </div>

        <TrackingSummaryCard trackingNumber={shipment.trackingNumber} />
      </div>
    </>
  )
}

function SummaryHeader({ shipment }: { shipment: Shipment }) {
  return (
    <Card>
      <CardContent className="flex flex-wrap items-center justify-between gap-4">
        <div className="space-y-2">
          <div className="flex flex-wrap items-center gap-3">
            <TrackingNumber value={shipment.trackingNumber} size="lg" />
            <StatusBadge status={shipment.status} />
          </div>
          <p className="flex items-center gap-1.5 text-sm text-muted-foreground">
            <ClockIcon className="size-4" />
            Registrado el {formatDateTime(shipment.createdAt)}
            <span className="mx-1">·</span>
            {shipment.sender.address.city} <ArrowRightIcon className="size-3.5" /> {shipment.recipient.address.city}
          </p>
        </div>
        {/* Solo se puede cancelar un envío "Registrado" (misma regla que Shipment.Cancel). */}
        {shipment.status === 'Registered' && <CancelShipmentDialog shipment={shipment} />}
      </CardContent>
    </Card>
  )
}

function ContactCard({ role, title, icon, contact }: { role: string; title: string; icon: ReactNode; contact: ContactInput }) {
  const { address } = contact
  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <IconBox>{icon}</IconBox>
        <div>
          <div className="text-[11px] font-semibold tracking-wider text-muted-foreground uppercase">{role}</div>
          <CardTitle>{title}</CardTitle>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        <div>
          <div className="text-lg font-semibold">{contact.name}</div>
          <div className="flex items-center gap-1.5 text-sm text-muted-foreground tabular">
            <PhoneIcon className="size-3.5" /> {contact.phone}
          </div>
        </div>
        <div className="rounded-lg bg-muted/60 p-3 text-sm">
          <div className="font-medium">{address.street}</div>
          <div className="text-muted-foreground">
            {address.city}, {address.state} — <span className="font-semibold text-foreground tabular">CP {address.postalCode}</span>
          </div>
          <div className="text-xs font-semibold tracking-wider text-muted-foreground">{address.country}</div>
        </div>
      </CardContent>
    </Card>
  )
}

function PackageCard({ shipment }: { shipment: Shipment }) {
  const volumetricKg = (shipment.lengthCm * shipment.widthCm * shipment.heightCm) / 5000
  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <IconBox>
          <BoxIcon className="size-5" />
        </IconBox>
        <CardTitle>Paquete</CardTitle>
      </CardHeader>
      <CardContent className="grid grid-cols-2 gap-3">
        <Stat label="Peso real" value={formatKg(shipment.weightKg)} />
        <Stat label="Peso volumétrico" value={formatKg(volumetricKg)} hint="Factor ÷ 5000" />
        <Stat
          className="col-span-2"
          label="Dimensiones (L × A × H)"
          value={`${shipment.lengthCm} × ${shipment.widthCm} × ${shipment.heightCm} cm`}
        />
      </CardContent>
    </Card>
  )
}

function CostCard({ shipment }: { shipment: Shipment }) {
  // Desglose reconstruido con la misma fórmula del dominio. El total mostrado es el de la API.
  const estimate = estimateCost(shipment.weightKg, shipment.lengthCm, shipment.widthCm, shipment.heightCm)
  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <IconBox>
          <ReceiptIcon className="size-5" />
        </IconBox>
        <CardTitle>Costo</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="rounded-lg bg-secondary p-4 text-center">
          <div className="text-xs font-semibold tracking-wider text-muted-foreground uppercase">Total</div>
          <div className="text-3xl font-bold tabular">{formatMoney(shipment.cost, shipment.currency)}</div>
        </div>
        {estimate && (
          <div className="space-y-1.5 text-sm">
            <div className="flex justify-between">
              <span>Tarifa base</span>
              <span className="tabular">{formatMoney(BASE_FEE)}</span>
            </div>
            <div className="flex justify-between">
              <span>15 × {formatKg(estimate.billableKg)} (peso cobrado)</span>
              <span className="tabular">{formatMoney(estimate.weightFee)}</span>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

function NotFound({ trackingNumber, error }: { trackingNumber: string; error: Error }) {
  const isNotFound = error instanceof ApiError && error.status === 404
  const isInvalid = error instanceof ApiError && error.status === 400

  return (
    <Card>
      <CardContent className="flex flex-col items-center gap-3 py-14 text-center">
        <div className="flex size-12 items-center justify-center rounded-full bg-muted text-muted-foreground">
          <PackageSearchIcon className="size-6" />
        </div>
        <h2 className="text-lg font-semibold">
          {isNotFound || isInvalid ? `No encontramos un envío con el número ${trackingNumber}` : 'No se pudo cargar el envío'}
        </h2>
        <p className="max-w-md text-muted-foreground">
          {isInvalid ? 'Verifica que el formato sea PF seguido de 10 caracteres.' : error.message}
        </p>
        <Button asChild variant="outline">
          <Link to="/envios">Volver a envíos</Link>
        </Button>
      </CardContent>
    </Card>
  )
}

function Stat({ label, value, hint, className }: { label: string; value: string; hint?: string; className?: string }) {
  return (
    <div className={`rounded-lg bg-muted/60 p-3 ${className ?? ''}`}>
      <div className="text-xs text-muted-foreground">{label}</div>
      <div className="text-xl font-semibold tabular">{value}</div>
      {hint && <div className="text-xs text-muted-foreground">{hint}</div>}
    </div>
  )
}

function IconBox({ children }: { children: ReactNode }) {
  return <div className="flex size-10 items-center justify-center rounded-lg bg-secondary text-secondary-foreground">{children}</div>
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-64" />
      <Skeleton className="h-28 w-full" />
      <div className="grid grid-cols-2 gap-6">
        <Skeleton className="h-56" />
        <Skeleton className="h-56" />
      </div>
    </div>
  )
}

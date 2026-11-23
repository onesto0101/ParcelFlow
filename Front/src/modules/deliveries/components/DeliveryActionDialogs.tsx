import { useState, type ReactNode } from 'react'
import { toast } from 'sonner'
import { CircleAlertIcon, CircleCheckIcon, CircleXIcon, UserRoundIcon } from 'lucide-react'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/shared/components/FormField'
import { useDeliveryAction, type DeliveryAction } from '../api/hooks'
import type { Delivery, FailureCode } from '../api/types'
import { FAILURE_REASONS } from '../lib/failureReasons'

/**
 * Los tres diálogos de acciones sobre una entrega. Comparten estructura (ActionDialog) y la misma
 * mutación; cada uno solo define sus campos y qué manda.
 */

export function AssignCourierDialog({ delivery }: { delivery: Delivery }) {
  const [courierId, setCourierId] = useState(delivery.courierId ?? '')
  const isReassign = delivery.status === 'Assigned'

  return (
    <ActionDialog
      deliveryId={delivery.id}
      trigger={
        <Button size="lg" variant={isReassign ? 'outline' : 'default'}>
          <UserRoundIcon /> {isReassign ? 'Reasignar repartidor' : 'Asignar repartidor'}
        </Button>
      }
      title={isReassign ? 'Reasignar repartidor' : 'Asignar repartidor'}
      description={`Entrega de la guía ${delivery.trackingNumber}. Al asignar, el paquete sale a reparto.`}
      submitLabel="Asignar"
      canSubmit={courierId.trim().length >= 2}
      buildAction={() => ({ type: 'assign', courierId: courierId.trim() })}
      successMessage={`Repartidor ${courierId.trim()} asignado`}
      onReset={() => setCourierId(delivery.courierId ?? '')}
    >
      <FormField id="courier-id" label="Repartidor" required hint="Identificador del repartidor, ej. REP-014">
        <Input id="courier-id" value={courierId} onChange={(e) => setCourierId(e.target.value)} maxLength={50} autoFocus />
      </FormField>
    </ActionDialog>
  )
}

export function ConfirmDeliveryDialog({ delivery }: { delivery: Delivery }) {
  const [receivedBy, setReceivedBy] = useState('')
  const [notes, setNotes] = useState('')

  return (
    <ActionDialog
      deliveryId={delivery.id}
      trigger={
        <Button size="lg" className="bg-emerald-600 text-white hover:bg-emerald-700">
          <CircleCheckIcon /> Confirmar entrega
        </Button>
      }
      title="Confirmar entrega"
      description={`Guía ${delivery.trackingNumber} · repartidor ${delivery.courierId}`}
      submitLabel="Confirmar entrega"
      canSubmit={receivedBy.trim().length >= 2}
      buildAction={() => ({ type: 'confirm', receivedBy: receivedBy.trim(), notes: notes.trim() || undefined })}
      successMessage="Entrega confirmada"
      onReset={() => {
        setReceivedBy('')
        setNotes('')
      }}
    >
      <FormField id="received-by" label="Recibió" required hint="Nombre de la persona que recibió el paquete">
        <Input id="received-by" value={receivedBy} onChange={(e) => setReceivedBy(e.target.value)} maxLength={150} autoFocus />
      </FormField>
      <FormField id="delivery-notes" label="Notas">
        <Textarea id="delivery-notes" value={notes} onChange={(e) => setNotes(e.target.value)} maxLength={500} rows={2} />
      </FormField>
    </ActionDialog>
  )
}

export function ReportFailureDialog({ delivery }: { delivery: Delivery }) {
  const [code, setCode] = useState<FailureCode>('RecipientAbsent')
  const [details, setDetails] = useState('')
  const isLastAttempt = delivery.remainingAttempts === 1
  // Misma regla que FailureReason en el dominio: "Other" exige detalles.
  const detailsRequired = code === 'Other'

  return (
    <ActionDialog
      deliveryId={delivery.id}
      trigger={
        <Button size="lg" variant="outline" className="border-rose-200 text-rose-600 hover:bg-rose-50 hover:text-rose-700">
          <CircleXIcon /> Reportar fallo
        </Button>
      }
      title="Reportar entrega fallida"
      description={`Intento ${delivery.attempts.length + 1} de ${delivery.maxAttempts} · guía ${delivery.trackingNumber}`}
      submitLabel="Registrar fallo"
      destructive
      canSubmit={!detailsRequired || details.trim().length > 0}
      buildAction={() => ({ type: 'fail', failureCode: code, details: details.trim() || undefined })}
      successMessage={isLastAttempt ? 'Se agotaron los intentos: la entrega quedó como fallida' : 'Intento fallido registrado'}
      onReset={() => {
        setCode('RecipientAbsent')
        setDetails('')
      }}
    >
      {isLastAttempt && (
        <Alert className="border-rose-200 bg-rose-50 text-rose-700">
          <CircleAlertIcon />
          <AlertDescription className="text-rose-700">
            Es el <strong>último intento</strong>. Si falla, la entrega quedará como fallida definitivamente.
          </AlertDescription>
        </Alert>
      )}
      <FormField id="failure-code" label="Motivo" required>
        <Select value={code} onValueChange={(value) => setCode(value as FailureCode)}>
          <SelectTrigger id="failure-code" className="h-9 w-full">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {Object.entries(FAILURE_REASONS).map(([value, label]) => (
              <SelectItem key={value} value={value}>
                {label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>
      <FormField id="failure-details" label="Detalles" required={detailsRequired}>
        <Textarea id="failure-details" value={details} onChange={(e) => setDetails(e.target.value)} maxLength={500} rows={2} />
      </FormField>
    </ActionDialog>
  )
}

/** Estructura común: botón → modal con campos → error de la API → Volver / Enviar. */
function ActionDialog({
  deliveryId,
  trigger,
  title,
  description,
  submitLabel,
  destructive,
  canSubmit,
  buildAction,
  successMessage,
  onReset,
  children,
}: {
  deliveryId: string
  trigger: ReactNode
  title: string
  description: string
  submitLabel: string
  destructive?: boolean
  canSubmit: boolean
  buildAction: () => DeliveryAction
  successMessage: string
  onReset: () => void
  children: ReactNode
}) {
  const [open, setOpen] = useState(false)
  const action = useDeliveryAction(deliveryId)

  function onOpenChange(next: boolean) {
    setOpen(next)
    if (!next) {
      onReset()
      action.reset()
    }
  }

  function submit() {
    action.mutate(buildAction(), {
      onSuccess: () => {
        toast.success(successMessage)
        onOpenChange(false)
      },
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogTrigger asChild>{trigger}</DialogTrigger>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4">{children}</div>

        {action.error && (
          <Alert variant="destructive">
            <CircleAlertIcon />
            <AlertDescription>{action.error.message}</AlertDescription>
          </Alert>
        )}

        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline" size="lg">
              Volver
            </Button>
          </DialogClose>
          <Button
            size="lg"
            className={destructive ? 'bg-rose-600 text-white hover:bg-rose-700' : undefined}
            disabled={!canSubmit || action.isPending}
            onClick={submit}
          >
            {action.isPending ? 'Guardando…' : submitLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

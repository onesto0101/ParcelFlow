import { useState } from 'react'
import { toast } from 'sonner'
import { CircleAlertIcon, TriangleAlertIcon, XCircleIcon } from 'lucide-react'
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
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { useCancelShipment } from '../api/hooks'
import type { Shipment } from '../api/types'

const REASON_MAX_LENGTH = 500 // Shipment.CancellationReasonMaxLength

/** Botón "Cancelar envío" + modal de confirmación con motivo obligatorio. */
export function CancelShipmentDialog({ shipment }: { shipment: Shipment }) {
  const [open, setOpen] = useState(false)
  const [reason, setReason] = useState('')
  const cancel = useCancelShipment()

  const trimmed = reason.trim()

  function onOpenChange(next: boolean) {
    setOpen(next)
    if (!next) {
      setReason('')
      cancel.reset() // limpia el error de un intento anterior
    }
  }

  function confirm() {
    cancel.mutate(
      { shipmentId: shipment.id, reason: trimmed },
      {
        onSuccess: () => {
          toast.success(`Envío ${shipment.trackingNumber} cancelado`)
          onOpenChange(false)
        },
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogTrigger asChild>
        <Button variant="outline" size="lg" className="border-rose-200 text-rose-600 hover:bg-rose-50 hover:text-rose-700">
          <XCircleIcon />
          Cancelar envío
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <DialogHeader className="flex-row items-start gap-3">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-rose-100 text-rose-600">
            <TriangleAlertIcon className="size-5" />
          </div>
          <div className="space-y-1 text-left">
            <DialogTitle>Cancelar envío {shipment.trackingNumber}</DialogTitle>
            <DialogDescription>Esta acción detiene el envío.</DialogDescription>
          </div>
        </DialogHeader>

        <Alert className="border-rose-200 bg-rose-50 text-rose-700">
          <CircleAlertIcon />
          <AlertDescription className="text-rose-700">
            El envío se marcará como <strong>cancelado</strong>. Esta acción no se puede deshacer, pero el historial se conservará.
          </AlertDescription>
        </Alert>

        <div className="space-y-1.5">
          <Label htmlFor="cancel-reason">
            Motivo de la cancelación <span className="text-destructive">*</span>
          </Label>
          <Textarea
            id="cancel-reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            maxLength={REASON_MAX_LENGTH}
            rows={4}
            placeholder="Ej. El cliente ya no necesita el envío"
          />
          <p className="text-right text-xs text-muted-foreground tabular">
            {reason.length} / {REASON_MAX_LENGTH} caracteres
          </p>
        </div>

        {/* Errores de la API, ej. 409 "El envío ya está cancelado" si alguien se adelantó. */}
        {cancel.error && (
          <Alert variant="destructive">
            <CircleAlertIcon />
            <AlertDescription>{cancel.error.message}</AlertDescription>
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
            className="bg-rose-600 text-white hover:bg-rose-700"
            disabled={!trimmed || cancel.isPending}
            onClick={confirm}
          >
            {cancel.isPending ? 'Cancelando…' : 'Confirmar cancelación'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

import { useState } from 'react'
import { toast } from 'sonner'
import { CircleAlertIcon, PlusIcon } from 'lucide-react'
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
import { Textarea } from '@/components/ui/textarea'
import { cn } from '@/lib/utils'
import { FormField } from '@/shared/components/FormField'
import { useRegisterTrackingEvent } from '../api/hooks'
import type { ManualTrackingStatus, Tracking } from '../api/types'
import { MANUAL_STATUS_INFO } from '../lib/statusInfo'

// Mismos límites que Location y TrackingDescription en el dominio.
const LOCATION_MAX = 150
const DESCRIPTION_MAX = 500

/**
 * Registrar un movimiento (TR-001, TR-004…TR-008). Solo ofrece los estados de
 * `tracking.nextManualStatuses`: el front no conoce la máquina de estados, se la pregunta a la API.
 *
 * Formulario pequeño → useState en vez de React Hook Form + Zod: 3 campos no justifican la librería.
 * La validación de verdad la hace el backend; aquí solo se evita mandar campos vacíos.
 */
export function RegisterMovementDialog({ tracking }: { tracking: Tracking }) {
  const options = tracking.nextManualStatuses
  const [open, setOpen] = useState(false)
  const [status, setStatus] = useState<ManualTrackingStatus | undefined>()
  const [location, setLocation] = useState('')
  const [description, setDescription] = useState('')
  const register = useRegisterTrackingEvent(tracking.trackingNumber)

  if (options.length === 0) return null

  function onOpenChange(next: boolean) {
    setOpen(next)
    if (next) {
      // Valores iniciales útiles: el primer estado posible y la ubicación actual.
      const first = options[0]
      setStatus(first)
      setLocation(tracking.currentLocation)
      setDescription(MANUAL_STATUS_INFO[first].suggestion)
    } else {
      register.reset()
    }
  }

  function selectStatus(next: ManualTrackingStatus) {
    // Si la descripción era la sugerencia del estado anterior, se cambia por la del nuevo.
    if (!status || description === MANUAL_STATUS_INFO[status].suggestion) setDescription(MANUAL_STATUS_INFO[next].suggestion)
    setStatus(next)
  }

  const canSubmit = status && location.trim() && description.trim() && !register.isPending

  function submit() {
    if (!status) return
    register.mutate(
      { status, location: location.trim(), description: description.trim() },
      {
        onSuccess: () => {
          toast.success(
            status === 'AtDistributionCenter'
              ? 'Movimiento registrado. El paquete quedó listo para reparto en Entregas.'
              : 'Movimiento registrado',
          )
          onOpenChange(false)
        },
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogTrigger asChild>
        <Button size="lg">
          <PlusIcon /> Registrar movimiento
        </Button>
      </DialogTrigger>

      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Registrar movimiento</DialogTitle>
          <DialogDescription>Guía {tracking.trackingNumber}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-1.5">
            <span className="text-sm font-medium">Nuevo estado</span>
            <div className="grid gap-2" role="radiogroup">
              {options.map((option) => (
                <button
                  key={option}
                  type="button"
                  role="radio"
                  aria-checked={status === option}
                  onClick={() => selectStatus(option)}
                  className={cn(
                    'rounded-lg border px-3 py-2.5 text-left text-sm font-medium transition-colors',
                    status === option ? 'border-primary bg-primary/5 text-primary' : 'hover:bg-muted',
                  )}
                >
                  {MANUAL_STATUS_INFO[option].action}
                </button>
              ))}
            </div>
          </div>

          <FormField id="movement-location" label="Ubicación" required>
            <Input
              id="movement-location"
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              maxLength={LOCATION_MAX}
              placeholder="Ej. CEDIS Guadalajara"
            />
          </FormField>

          <FormField id="movement-description" label="Descripción" required>
            <Textarea
              id="movement-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              maxLength={DESCRIPTION_MAX}
              rows={3}
            />
          </FormField>
        </div>

        {register.error && (
          <Alert variant="destructive">
            <CircleAlertIcon />
            <AlertDescription>{register.error.message}</AlertDescription>
          </Alert>
        )}

        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline" size="lg">
              Volver
            </Button>
          </DialogClose>
          <Button size="lg" disabled={!canSubmit} onClick={submit}>
            {register.isPending ? 'Registrando…' : 'Registrar'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}

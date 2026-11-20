import { FormProvider, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router'
import { toast } from 'sonner'
import { BoxIcon, CircleAlertIcon, MapPinIcon, SendIcon, TruckIcon } from 'lucide-react'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { ApiError } from '@/shared/api/http'
import { FormField } from '@/shared/components/FormField'
import { PageHeader } from '@/shared/components/PageHeader'
import { useCreateShipment } from '../api/hooks'
import { ContactFields } from '../components/ContactFields'
import { CostSummary } from '../components/CostSummary'
import { createShipmentSchema, type CreateShipmentForm } from '../lib/shipmentSchema'
import { estimateCost } from '../lib/shippingCost'

const emptyContact = { name: '', phone: '', address: { street: '', city: '', state: '', postalCode: '', country: 'MX' } }

// Campo del dominio (ProblemDetails.field) → campo del formulario, para marcar el input correcto
// cuando la API rechaza algo que Zod dejó pasar.
const SERVER_FIELD_TO_FORM: Partial<Record<string, keyof CreateShipmentForm>> = {
  Kilograms: 'weightKg',
  Length: 'lengthCm',
  Width: 'widthCm',
  Height: 'heightCm',
}

export function CreateShipmentPage() {
  const navigate = useNavigate()
  const createShipment = useCreateShipment()

  const form = useForm<CreateShipmentForm>({
    resolver: zodResolver(createShipmentSchema),
    defaultValues: { sender: emptyContact, recipient: emptyContact },
    mode: 'onTouched', // valida al salir de cada campo, no en cada tecla
  })
  const { register, handleSubmit, setError, control, formState } = form
  const { errors } = formState

  // useWatch re-renderiza solo cuando cambian estos 4 campos → estimación "en vivo".
  const [weightKg, lengthCm, widthCm, heightCm] = useWatch({
    control,
    name: ['weightKg', 'lengthCm', 'widthCm', 'heightCm'],
  })
  const estimate = estimateCost(weightKg, lengthCm, widthCm, heightCm)

  async function onSubmit(values: CreateShipmentForm) {
    try {
      const result = await createShipment.mutateAsync(values)
      toast.success(`Envío ${result.trackingNumber} registrado`)
      navigate(`/envios/${result.trackingNumber}`)
    } catch (error) {
      const formField = error instanceof ApiError && error.field ? SERVER_FIELD_TO_FORM[error.field] : undefined
      // Error ligado a un campo → se muestra bajo ese input; si no, en el aviso general ("root").
      setError(formField ?? 'root', { message: (error as Error).message })
    }
  }

  const numberField = (name: 'weightKg' | 'lengthCm' | 'widthCm' | 'heightCm') =>
    // valueAsNumber: el input devuelve number (o NaN si está vacío) en lugar de string.
    register(name, { valueAsNumber: true })

  return (
    <FormProvider {...form}>
      <PageHeader
        breadcrumbs={[{ label: 'Envíos', to: '/envios' }, { label: 'Registrar envío' }]}
        title="Registrar nuevo envío"
        description="Ingresa los datos de origen, destino y del paquete para generar la guía y calcular la tarifa."
      />

      <form onSubmit={handleSubmit(onSubmit)} noValidate className="grid grid-cols-1 gap-6 xl:grid-cols-[1fr_360px]">
        <div className="space-y-6">
          <ContactFields
            prefix="sender"
            title="Datos del remitente"
            description="Punto de recolección y contacto de origen"
            icon={<MapPinIcon className="size-5" />}
          />
          <ContactFields
            prefix="recipient"
            title="Datos del destinatario"
            description="Dirección de entrega y receptor final"
            icon={<SendIcon className="size-5" />}
          />

          <Card>
            <CardHeader className="flex flex-row items-center gap-3">
              <div className="flex size-10 items-center justify-center rounded-lg bg-secondary text-secondary-foreground">
                <BoxIcon className="size-5" />
              </div>
              <div>
                <CardTitle>Especificaciones del paquete</CardTitle>
                <CardDescription>Peso y dimensiones para el cálculo de la tarifa</CardDescription>
              </div>
            </CardHeader>
            <CardContent className="grid grid-cols-3 gap-4">
              <FormField id="weightKg" label="Peso (kg)" required hint="Máximo 50 kg" error={errors.weightKg?.message} className="col-span-3 sm:col-span-1">
                <Input id="weightKg" type="number" step="0.001" min="0" className="tabular" aria-invalid={!!errors.weightKg} {...numberField('weightKg')} />
              </FormField>
              <div className="hidden sm:col-span-2 sm:block" />

              <FormField id="lengthCm" label="Largo (cm)" required error={errors.lengthCm?.message}>
                <Input id="lengthCm" type="number" step="0.01" className="tabular" aria-invalid={!!errors.lengthCm} {...numberField('lengthCm')} />
              </FormField>
              <FormField id="widthCm" label="Ancho (cm)" required error={errors.widthCm?.message}>
                <Input id="widthCm" type="number" step="0.01" className="tabular" aria-invalid={!!errors.widthCm} {...numberField('widthCm')} />
              </FormField>
              <FormField id="heightCm" label="Alto (cm)" required error={errors.heightCm?.message}>
                <Input id="heightCm" type="number" step="0.01" className="tabular" aria-invalid={!!errors.heightCm} {...numberField('heightCm')} />
              </FormField>
              <p className="col-span-3 text-xs text-muted-foreground">Entre 10 y 150 cm por lado.</p>
            </CardContent>
          </Card>
        </div>

        {/* Panel lateral "pegajoso": se queda a la vista al hacer scroll. */}
        <aside className="xl:sticky xl:top-22 xl:self-start">
          <Card>
            <CardHeader>
              <CardTitle>Resumen y costo</CardTitle>
              <CardDescription>Estimación en tiempo real</CardDescription>
            </CardHeader>
            <CardContent className="space-y-5">
              <CostSummary estimate={estimate} />

              {errors.root && (
                <Alert variant="destructive">
                  <CircleAlertIcon />
                  <AlertTitle>No se pudo registrar</AlertTitle>
                  <AlertDescription>{errors.root.message}</AlertDescription>
                </Alert>
              )}

              <div className="space-y-2">
                <Button type="submit" size="lg" className="h-10 w-full" disabled={formState.isSubmitting}>
                  <TruckIcon />
                  {formState.isSubmitting ? 'Registrando…' : 'Registrar envío'}
                </Button>
                <Button asChild variant="outline" size="lg" className="h-10 w-full">
                  <Link to="/envios">Cancelar</Link>
                </Button>
              </div>
            </CardContent>
          </Card>
        </aside>
      </form>
    </FormProvider>
  )
}

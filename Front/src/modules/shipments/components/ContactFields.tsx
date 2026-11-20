import type { ReactNode } from 'react'
import { useFormContext } from 'react-hook-form'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { FormField } from '@/shared/components/FormField'
import type { CreateShipmentForm } from '../lib/shipmentSchema'

/**
 * Bloque de datos de contacto. Se usa DOS veces (remitente y destinatario), igual que el value
 * object Contact del backend sirve para los dos roles. `prefix` dice qué parte del formulario edita.
 *
 * useFormContext: lee el formulario del <FormProvider> padre, así no hay que pasar
 * register/errors como props.
 */
export function ContactFields({
  prefix,
  title,
  description,
  icon,
}: {
  prefix: 'sender' | 'recipient'
  title: string
  description: string
  icon: ReactNode
}) {
  const {
    register,
    formState: { errors },
  } = useFormContext<CreateShipmentForm>()
  const e = errors[prefix]
  const id = (field: string) => `${prefix}-${field}`

  return (
    <Card>
      <CardHeader className="flex flex-row items-center gap-3">
        <div className="flex size-10 items-center justify-center rounded-lg bg-secondary text-secondary-foreground">
          {icon}
        </div>
        <div>
          <CardTitle>{title}</CardTitle>
          <CardDescription>{description}</CardDescription>
        </div>
      </CardHeader>

      <CardContent className="grid grid-cols-6 gap-4">
        <FormField id={id('name')} label="Nombre completo" required error={e?.name?.message} className="col-span-4">
          <Input id={id('name')} aria-invalid={!!e?.name} {...register(`${prefix}.name`)} />
        </FormField>

        <FormField id={id('phone')} label="Teléfono" required error={e?.phone?.message} className="col-span-2">
          <Input id={id('phone')} placeholder="81 1234 5678" aria-invalid={!!e?.phone} {...register(`${prefix}.phone`)} />
        </FormField>

        <FormField id={id('street')} label="Calle y número" required error={e?.address?.street?.message} className="col-span-6">
          <Input id={id('street')} aria-invalid={!!e?.address?.street} {...register(`${prefix}.address.street`)} />
        </FormField>

        <FormField id={id('city')} label="Ciudad" required error={e?.address?.city?.message} className="col-span-2">
          <Input id={id('city')} aria-invalid={!!e?.address?.city} {...register(`${prefix}.address.city`)} />
        </FormField>

        <FormField id={id('state')} label="Estado" required error={e?.address?.state?.message} className="col-span-2">
          <Input id={id('state')} aria-invalid={!!e?.address?.state} {...register(`${prefix}.address.state`)} />
        </FormField>

        <FormField id={id('postalCode')} label="C.P." required error={e?.address?.postalCode?.message} className="col-span-1">
          <Input id={id('postalCode')} className="tabular" aria-invalid={!!e?.address?.postalCode} {...register(`${prefix}.address.postalCode`)} />
        </FormField>

        <FormField id={id('country')} label="País" required error={e?.address?.country?.message} className="col-span-1">
          <Input id={id('country')} maxLength={2} className="uppercase" aria-invalid={!!e?.address?.country} {...register(`${prefix}.address.country`)} />
        </FormField>
      </CardContent>
    </Card>
  )
}

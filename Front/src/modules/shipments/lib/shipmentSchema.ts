import { z } from 'zod'

/**
 * Validación del formulario con Zod. Repite las reglas de los value objects del dominio
 * (Contact, Address, Weight, Dimensions) para avisar al usuario ANTES de enviar.
 *
 * Importante: esto es solo UX. La API vuelve a validar todo; nunca confíes solo en el front.
 */

const PHONE_SEPARATORS = /[\s\-()]/g

const required = (label: string, max: number, min = 1) =>
  z
    .string()
    .trim()
    .min(min, min > 1 ? `${label}: mínimo ${min} caracteres.` : `${label} es obligatorio.`)
    .max(max, `${label}: máximo ${max} caracteres.`)

const addressSchema = z.object({
  street: required('La calle', 200),
  city: required('La ciudad', 100),
  state: required('El estado', 100),
  postalCode: required('El código postal', 10, 3).regex(/^[A-Za-z0-9 -]+$/, 'Solo letras, dígitos, espacios y guiones.'),
  country: z.string().regex(/^[A-Za-z]{2}$/, 'Código ISO de 2 letras (ej. MX).'),
})

const contactSchema = z.object({
  name: required('El nombre', 150, 2),
  phone: z
    .string()
    .trim()
    .refine((value) => {
      const digits = value.replace(PHONE_SEPARATORS, '')
      return /^\+?\d{7,15}$/.test(digits)
    }, 'Teléfono inválido: entre 7 y 15 dígitos ("+" solo al inicio).'),
  address: addressSchema,
})

const dimension = (label: string) =>
  z
    .number({ error: `${label} es obligatorio.` })
    .min(10, `${label}: mínimo 10 cm.`)
    .max(150, `${label}: máximo 150 cm.`)

export const createShipmentSchema = z.object({
  sender: contactSchema,
  recipient: contactSchema,
  weightKg: z
    .number({ error: 'El peso es obligatorio.' })
    .gt(0, 'El peso debe ser mayor a 0 y menor o igual a 50 kg.')
    .max(50, 'El peso debe ser mayor a 0 y menor o igual a 50 kg.'),
  lengthCm: dimension('El largo'),
  widthCm: dimension('El ancho'),
  heightCm: dimension('El alto'),
})

export type CreateShipmentForm = z.infer<typeof createShipmentSchema>

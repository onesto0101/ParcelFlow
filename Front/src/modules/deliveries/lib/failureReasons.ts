import type { FailureCode } from '../api/types'

/** Motivos de entrega fallida (DeliveryFailureCode en el dominio). */
export const FAILURE_REASONS: Record<FailureCode, string> = {
  RecipientAbsent: 'Destinatario ausente',
  WrongAddress: 'Dirección incorrecta',
  Refused: 'Rechazado por el destinatario',
  Inaccessible: 'Domicilio inaccesible',
  Other: 'Otro motivo',
}

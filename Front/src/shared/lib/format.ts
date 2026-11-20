/** Formatos para México. Intl ya sabe poner "$", comas de miles y nombres de meses en español. */

export function formatMoney(amount: number, currency = 'MXN'): string {
  return new Intl.NumberFormat('es-MX', { style: 'currency', currency }).format(amount) + ` ${currency}`
}

export function formatDateTime(isoUtc: string): string {
  // La API manda fechas UTC ("...Z"); Intl las muestra en la zona horaria del navegador.
  return new Intl.DateTimeFormat('es-MX', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(isoUtc))
}

export function formatKg(kg: number): string {
  return `${kg.toLocaleString('es-MX', { maximumFractionDigits: 2 })} kg`
}

/** "pf7k 3m9q xr2a" → "PF7K3M9QXR2A" (mismo criterio que TrackingNumber.From en el backend). */
export function normalizeTrackingNumber(value: string): string {
  return value.replace(/\s+/g, '').toUpperCase()
}

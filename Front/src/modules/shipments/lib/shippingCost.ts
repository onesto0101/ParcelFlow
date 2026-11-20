/**
 * Copia de ShippingCostCalculator.cs SOLO para mostrar una estimación mientras se llena el
 * formulario. El costo que vale es el que devuelve la API (el dominio es la fuente de verdad).
 * Si cambian las tarifas en el backend, hay que actualizar esto también.
 */
export const BASE_FEE = 50
export const RATE_PER_KG = 15
export const VOLUMETRIC_DIVISOR = 5000

export interface CostEstimate {
  realKg: number
  volumetricKg: number
  billableKg: number
  weightFee: number
  total: number
}

const round2 = (n: number) => Math.round((n + Number.EPSILON) * 100) / 100

export function estimateCost(weightKg: number, lengthCm: number, widthCm: number, heightCm: number): CostEstimate | null {
  if (![weightKg, lengthCm, widthCm, heightCm].every((n) => Number.isFinite(n) && n > 0)) return null

  const volumetricKg = (lengthCm * widthCm * heightCm) / VOLUMETRIC_DIVISOR
  const billableKg = Math.max(weightKg, volumetricKg)
  const weightFee = round2(RATE_PER_KG * billableKg)

  return { realKg: weightKg, volumetricKg, billableKg, weightFee, total: round2(BASE_FEE + weightFee) }
}

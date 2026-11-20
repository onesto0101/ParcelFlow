/**
 * Cliente HTTP mínimo sobre fetch. Todas las llamadas a la API pasan por aquí.
 *
 * Su trabajo principal: convertir las respuestas de error (ProblemDetails, el formato que
 * devuelve ExceptionToProblemDetailsHandler en el backend) en un ApiError tipado, para que
 * las pantallas puedan decidir qué hacer según `status`, `code` o `field`.
 */

const API_URL = import.meta.env.VITE_API_URL

/** Forma del cuerpo de error que devuelve la API (RFC 9457 + nuestros campos extra). */
interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  code?: string
  field?: string
}

export class ApiError extends Error {
  readonly status: number
  /** Código estable del backend, ej. "shipment.not_found" o "validation.invalid". */
  readonly code?: string
  /** Campo del dominio que falló, ej. "Kilograms". */
  readonly field?: string

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `Error HTTP ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.code = problem.code
    this.field = problem.field
  }
}

export async function http<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response
  try {
    response = await fetch(`${API_URL}${path}`, {
      ...init,
      headers: { 'Content-Type': 'application/json', ...init?.headers },
    })
  } catch {
    // fetch solo lanza si no hubo respuesta: API apagada, sin red, o CORS bloqueado.
    throw new ApiError(0, {
      title: 'Sin conexión',
      detail: 'No se pudo contactar con la API. ¿Está arrancada en ' + API_URL + '?',
      code: 'network.unreachable',
    })
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails
    throw new ApiError(response.status, problem)
  }

  // 204 No Content (ej. cancelar) no trae cuerpo.
  if (response.status === 204) return undefined as T

  return (await response.json()) as T
}

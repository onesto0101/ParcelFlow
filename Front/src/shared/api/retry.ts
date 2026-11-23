import { ApiError } from './http'

/**
 * Política de reintentos para useQuery: un 400 o un 404 no se arreglan reintentando (el dato está
 * mal o no existe); un fallo de red o un 500 quizá sí. Antes estaba copiada dentro de useShipment.
 */
export function retryUnlessClientError(failureCount: number, error: Error): boolean {
  const isClientError = error instanceof ApiError && error.status >= 400 && error.status < 500
  return !isClientError && failureCount < 2
}

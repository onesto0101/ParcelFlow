import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiError } from '@/shared/api/http'
import { trackingApi } from './trackingApi'
import type { RegisterTrackingEventRequest } from './types'

export const trackingKeys = {
  all: ['tracking'] as const,
  byTrackingNumber: (trackingNumber: string) => ['tracking', trackingNumber] as const,
}

/**
 * Seguimiento de una guía.
 *
 * ⚠️ Consistencia eventual: el seguimiento lo crea Tracking cuando procesa el evento
 * ShipmentCreated, una fracción de segundo DESPUÉS de crear el envío. Si se abre justo
 * al crearlo, el primer GET puede dar 404. Por eso aquí un 404 SÍ se reintenta (2 veces, con
 * 1 s de espera) antes de mostrar "no encontrado". Es la cara visible del outbox en la UI.
 */
export function useTracking(trackingNumber: string, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: trackingKeys.byTrackingNumber(trackingNumber),
    queryFn: () => trackingApi.get(trackingNumber),
    enabled: options?.enabled ?? true,
    retry: (failureCount, error) => {
      if (error instanceof ApiError && error.status === 404) return failureCount < 2
      if (error instanceof ApiError && error.status >= 400 && error.status < 500) return false
      return failureCount < 2
    },
    retryDelay: 1000,
  })
}

export function useRegisterTrackingEvent(trackingNumber: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: RegisterTrackingEventRequest) => trackingApi.registerEvent(trackingNumber, request),
    onSuccess: (_, request) => {
      queryClient.invalidateQueries({ queryKey: trackingKeys.byTrackingNumber(trackingNumber) })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      // Llegar al centro crea una entrega en Deliveries (por evento, un instante después).
      if (request.status === 'AtDistributionCenter') {
        setTimeout(() => queryClient.invalidateQueries({ queryKey: ['deliveries'] }), 1000)
      }
    },
  })
}

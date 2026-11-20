import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiError } from '@/shared/api/http'
import { shipmentsApi } from './shipmentsApi'
import type { CreateShipmentRequest } from './types'

/**
 * Hooks de TanStack Query: el "estado del servidor" del módulo.
 * - useQuery  = leer (cachea, reintenta, sabe si está cargando).  ≈ IQuery en el backend
 * - useMutation = cambiar algo.                                    ≈ ICommand en el backend
 * CQRS también en el front.
 */

// Claves de caché centralizadas: así invalidar no depende de escribir el mismo array a mano.
export const shipmentKeys = {
  all: ['shipments'] as const,
  byTrackingNumber: (trackingNumber: string) => ['shipments', 'tracking', trackingNumber] as const,
}

export function useShipment(trackingNumber: string) {
  return useQuery({
    queryKey: shipmentKeys.byTrackingNumber(trackingNumber),
    queryFn: () => shipmentsApi.getByTrackingNumber(trackingNumber),
    // Un 404 o 400 no se arregla reintentando; un fallo de red quizá sí.
    retry: (failureCount, error) => !(error instanceof ApiError && error.status >= 400 && error.status < 500) && failureCount < 2,
  })
}

export function useCreateShipment() {
  return useMutation({
    mutationFn: (request: CreateShipmentRequest) => shipmentsApi.create(request),
  })
}

export function useCancelShipment(trackingNumber: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ shipmentId, reason }: { shipmentId: string; reason: string }) =>
      shipmentsApi.cancel(shipmentId, reason),
    // Tras cancelar, el envío en caché está desactualizado: se marca como "viejo" y se recarga.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: shipmentKeys.byTrackingNumber(trackingNumber) }),
  })
}

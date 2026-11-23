import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { retryUnlessClientError } from '@/shared/api/retry'
import { shipmentsApi } from './shipmentsApi'
import type { CreateShipmentRequest, ShipmentsFilter } from './types'

/**
 * Hooks de TanStack Query: el "estado del servidor" del módulo.
 * - useQuery  = leer (cachea, reintenta, sabe si está cargando).  ≈ IQuery en el backend
 * - useMutation = cambiar algo.                                    ≈ ICommand en el backend
 * CQRS también en el front.
 */

// Claves de caché centralizadas y JERÁRQUICAS: invalidar ['shipments'] invalida también
// ['shipments', 'list', ...] y ['shipments', 'tracking', ...] de un golpe.
export const shipmentKeys = {
  all: ['shipments'] as const,
  list: (filter: ShipmentsFilter) => ['shipments', 'list', filter] as const,
  byTrackingNumber: (trackingNumber: string) => ['shipments', 'tracking', trackingNumber] as const,
}

export function useShipments(filter: ShipmentsFilter) {
  return useQuery({
    queryKey: shipmentKeys.list(filter),
    queryFn: () => shipmentsApi.list(filter),
    // Al cambiar de página o filtro, sigue mostrando la página anterior mientras llega la nueva
    // (en vez de vaciar la tabla y "parpadear").
    placeholderData: keepPreviousData,
    retry: retryUnlessClientError,
  })
}

export function useShipment(trackingNumber: string) {
  return useQuery({
    queryKey: shipmentKeys.byTrackingNumber(trackingNumber),
    queryFn: () => shipmentsApi.getByTrackingNumber(trackingNumber),
    retry: retryUnlessClientError,
  })
}

export function useCreateShipment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (request: CreateShipmentRequest) => shipmentsApi.create(request),
    // El listado en caché ya no incluye el nuevo envío.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: shipmentKeys.all }),
  })
}

export function useCancelShipment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ shipmentId, reason }: { shipmentId: string; reason: string }) =>
      shipmentsApi.cancel(shipmentId, reason),
    // Tras cancelar están desactualizados el detalle y las filas del listado: se invalida todo
    // lo de envíos. También el seguimiento, que pasará a "Cancelado" cuando Tracking procese el evento.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: shipmentKeys.all })
      queryClient.invalidateQueries({ queryKey: ['tracking'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

import { createBrowserRouter } from 'react-router'
import { ComingSoonPage } from '@/modules/placeholder/ComingSoonPage'
import { CreateShipmentPage } from '@/modules/shipments/pages/CreateShipmentPage'
import { ShipmentDetailPage } from '@/modules/shipments/pages/ShipmentDetailPage'
import { ShipmentsPage } from '@/modules/shipments/pages/ShipmentsPage'
import { AppLayout } from './AppLayout'

/**
 * Mapa de URLs → páginas. Todas cuelgan de AppLayout (barra lateral + superior).
 * El orden importa poco: React Router elige la ruta más específica ("nuevo" gana a ":trackingNumber").
 */
export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      {
        path: '/',
        element: <ComingSoonPage title="Dashboard" missing="Las métricas necesitan un endpoint de estadísticas que combine Shipments, Tracking y Deliveries." />,
      },
      { path: '/envios', element: <ShipmentsPage /> },
      { path: '/envios/nuevo', element: <CreateShipmentPage /> },
      { path: '/envios/:trackingNumber', element: <ShipmentDetailPage /> },
      {
        path: '/rastreo',
        element: <ComingSoonPage title="Rastreo" missing="El historial de movimientos necesita la API del módulo Tracking (Application + Infrastructure + endpoints)." />,
      },
      {
        path: '/entregas',
        element: <ComingSoonPage title="Entregas" missing="La asignación de repartidores y los intentos de entrega necesitan la API del módulo Deliveries." />,
      },
      {
        path: '*',
        element: <ComingSoonPage title="Página no encontrada" missing="La dirección que buscas no existe." />,
      },
    ],
  },
])

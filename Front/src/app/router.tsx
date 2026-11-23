import { createBrowserRouter } from 'react-router'
import { DashboardPage } from '@/modules/dashboard/pages/DashboardPage'
import { DeliveriesPage } from '@/modules/deliveries/pages/DeliveriesPage'
import { DeliveryDetailPage } from '@/modules/deliveries/pages/DeliveryDetailPage'
import { ComingSoonPage } from '@/modules/placeholder/ComingSoonPage'
import { CreateShipmentPage } from '@/modules/shipments/pages/CreateShipmentPage'
import { ShipmentDetailPage } from '@/modules/shipments/pages/ShipmentDetailPage'
import { ShipmentsPage } from '@/modules/shipments/pages/ShipmentsPage'
import { TrackingPage } from '@/modules/tracking/pages/TrackingPage'
import { AppLayout } from './AppLayout'

/**
 * Mapa de URLs → páginas. Todas cuelgan de AppLayout (barra lateral + superior).
 * El orden importa poco: React Router elige la ruta más específica ("nuevo" gana a ":trackingNumber").
 */
export const router = createBrowserRouter([
  {
    element: <AppLayout />,
    children: [
      { path: '/', element: <DashboardPage /> },
      { path: '/envios', element: <ShipmentsPage /> },
      { path: '/envios/nuevo', element: <CreateShipmentPage /> },
      { path: '/envios/:trackingNumber', element: <ShipmentDetailPage /> },
      { path: '/rastreo', element: <TrackingPage /> },
      { path: '/rastreo/:trackingNumber', element: <TrackingPage /> },
      { path: '/entregas', element: <DeliveriesPage /> },
      { path: '/entregas/:id', element: <DeliveryDetailPage /> },
      {
        path: '*',
        element: <ComingSoonPage title="Página no encontrada" missing="La dirección que buscas no existe." />,
      },
    ],
  },
])

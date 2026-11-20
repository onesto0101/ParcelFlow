import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router'
import { Toaster } from '@/components/ui/sonner'
import { router } from '@/app/router'
import './index.css'

// Caché de datos del servidor (TanStack Query). Una sola instancia para toda la app.
const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000 }, // un dato se considera "fresco" 30 s: no se vuelve a pedir al navegar
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
      <Toaster richColors position="top-right" />
    </QueryClientProvider>
  </StrictMode>,
)

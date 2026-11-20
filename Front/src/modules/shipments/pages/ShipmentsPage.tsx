import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { PlusIcon, SearchIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { PageHeader } from '@/shared/components/PageHeader'
import { normalizeTrackingNumber } from '@/shared/lib/format'

/**
 * Página "Envíos". El diseño tiene una tabla con filtros y paginación, pero la API todavía no
 * tiene un endpoint de listado (GET /api/shipments). Mientras tanto: buscar por guía + registrar.
 */
export function ShipmentsPage() {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')

  function onSubmit(event: FormEvent) {
    event.preventDefault()
    const trackingNumber = normalizeTrackingNumber(query)
    if (trackingNumber) navigate(`/envios/${trackingNumber}`)
  }

  return (
    <>
      <PageHeader
        title="Envíos"
        description="Consulta y administra los envíos registrados"
        actions={
          <Button asChild size="lg">
            <Link to="/envios/nuevo">
              <PlusIcon /> Registrar envío
            </Link>
          </Button>
        }
      />

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Buscar envío</CardTitle>
          <CardDescription>Ingresa el número de guía (PF seguido de 10 caracteres).</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={onSubmit} className="flex gap-2">
            <div className="relative flex-1">
              <SearchIcon className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder="PF7K3M9QXR2A"
                className="h-10 pl-9 font-mono uppercase"
                aria-label="Número de guía"
              />
            </div>
            <Button type="submit" size="lg" className="h-10" disabled={!query.trim()}>
              Buscar
            </Button>
          </form>
          <p className="mt-4 text-xs text-muted-foreground">
            El listado completo con filtros llegará cuando la API tenga el endpoint de listado de envíos.
          </p>
        </CardContent>
      </Card>
    </>
  )
}

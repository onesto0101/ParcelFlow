import { ConstructionIcon } from 'lucide-react'
import { Card, CardContent } from '@/components/ui/card'
import { PageHeader } from '@/shared/components/PageHeader'

/**
 * Página provisional para las vistas cuyo backend aún no existe. Es honesta: dice qué falta,
 * en lugar de mostrar datos inventados que luego habría que borrar.
 */
export function ComingSoonPage({ title, missing }: { title: string; missing: string }) {
  return (
    <>
      <PageHeader title={title} />
      <Card>
        <CardContent className="flex flex-col items-center gap-3 py-16 text-center">
          <div className="flex size-12 items-center justify-center rounded-full bg-secondary text-secondary-foreground">
            <ConstructionIcon className="size-6" />
          </div>
          <h2 className="text-lg font-semibold">Próximamente</h2>
          <p className="max-w-md text-muted-foreground">{missing}</p>
        </CardContent>
      </Card>
    </>
  )
}

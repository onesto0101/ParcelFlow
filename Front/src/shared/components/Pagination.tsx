import { ChevronLeftIcon, ChevronRightIcon } from 'lucide-react'
import { Button } from '@/components/ui/button'
import type { PagedResult } from '@/shared/api/paging'

/** "Mostrando 21–40 de 57" + Anterior / Siguiente. Funciona con cualquier PagedResult. */
export function Pagination({
  result,
  onPageChange,
}: {
  result: Pick<PagedResult<unknown>, 'page' | 'pageSize' | 'totalCount' | 'totalPages'>
  onPageChange: (page: number) => void
}) {
  const { page, pageSize, totalCount, totalPages } = result
  if (totalCount === 0) return null

  const from = (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, totalCount)

  return (
    <div className="flex items-center justify-between gap-4 border-t px-4 py-3 text-sm text-muted-foreground">
      <span className="tabular">
        Mostrando {from}–{to} de {totalCount}
      </span>
      <div className="flex items-center gap-2">
        <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
          <ChevronLeftIcon /> Anterior
        </Button>
        <span className="tabular">
          {page} / {Math.max(totalPages, 1)}
        </span>
        <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => onPageChange(page + 1)}>
          Siguiente <ChevronRightIcon />
        </Button>
      </div>
    </div>
  )
}

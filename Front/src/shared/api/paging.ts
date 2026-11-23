/** Refleja PagedResult<T> de BuildingBlocks.Application. Lo devuelven todos los listados. */
export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

/**
 * { status: 'Pending', search: '', page: 2 } → "?status=Pending&page=2".
 * Omite los valores vacíos: la API los trataría igual ("sin filtro"), pero así la URL queda limpia.
 */
export function toQueryString(params: Record<string, string | number | null | undefined>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') search.set(key, String(value))
  }
  const text = search.toString()
  return text ? `?${text}` : ''
}

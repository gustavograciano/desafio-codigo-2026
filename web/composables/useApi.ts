import type { FetchError } from 'ofetch'

export interface Categoria {
  id: number
  nome: string
  descricao: string | null
}

export interface ProdutoCategoria {
  id: number
  nome: string
}

export interface Produto {
  id: number
  nome: string
  descricao: string | null
  preco: number
  categoriaId: number
  categoria: ProdutoCategoria | null
}

export interface CategoriaPayload {
  nome: string
  descricao: string | null
}

export interface ProdutoPayload {
  nome: string
  descricao: string | null
  preco: number
  categoriaId: number
}

interface ApiErrorBody {
  mensagem?: string
  title?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  status: number
  details: ApiErrorBody

  constructor(message: string, status: number, details: ApiErrorBody) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.details = details
  }
}

function extractMessage(err: FetchError): { message: string; body: ApiErrorBody } {
  const body = (err.data ?? {}) as ApiErrorBody
  if (body?.mensagem) return { message: body.mensagem, body }
  if (body?.errors) {
    const first = Object.values(body.errors)[0]?.[0]
    if (first) return { message: first, body }
  }
  if (body?.title) return { message: body.title, body }
  return { message: err.message || 'Falha na comunicação com o servidor.', body }
}

export function useApi() {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiBase as string

  async function request<T>(path: string, options: Parameters<typeof $fetch>[1] = {}): Promise<T> {
    try {
      return await $fetch<T>(path, { baseURL, ...options })
    } catch (raw) {
      const err = raw as FetchError
      const { message, body } = extractMessage(err)
      throw new ApiError(message, err.status ?? 0, body)
    }
  }

  return {
    // Categorias
    listarCategorias: () => request<Categoria[]>('/api/categorias'),
    criarCategoria: (payload: CategoriaPayload) =>
      request<Categoria>('/api/categorias', { method: 'POST', body: payload }),
    atualizarCategoria: (id: number, payload: CategoriaPayload) =>
      request<Categoria>(`/api/categorias/${id}`, { method: 'PUT', body: payload }),
    excluirCategoria: (id: number) =>
      request<void>(`/api/categorias/${id}`, { method: 'DELETE' }),

    // Produtos
    listarProdutos: () => request<Produto[]>('/api/produtos'),
    criarProduto: (payload: ProdutoPayload) =>
      request<Produto>('/api/produtos', { method: 'POST', body: payload }),
    atualizarProduto: (id: number, payload: ProdutoPayload) =>
      request<Produto>(`/api/produtos/${id}`, { method: 'PUT', body: payload }),
    excluirProduto: (id: number) =>
      request<void>(`/api/produtos/${id}`, { method: 'DELETE' })
  }
}

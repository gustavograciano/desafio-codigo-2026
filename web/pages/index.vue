<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { ApiError, useApi, type Categoria, type Produto } from '~/composables/useApi'
import { useFeedback } from '~/composables/useFeedback'

definePageMeta({ title: 'Visão geral' })

const api = useApi()
const feedback = useFeedback()

const categorias = ref<Categoria[]>([])
const produtos = ref<Produto[]>([])
const carregando = ref(false)

const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL'
})

const valorInventario = computed(() =>
  produtos.value.reduce((acc, p) => acc + Number(p.preco || 0), 0)
)

const ticketMedio = computed(() =>
  produtos.value.length === 0 ? 0 : valorInventario.value / produtos.value.length
)

const categoriasAtivas = computed(() => {
  const ids = new Set(produtos.value.map((p) => p.categoriaId))
  return ids.size
})

const distribuicaoPorCategoria = computed(() => {
  const map = new Map<number, { nome: string; total: number; valor: number }>()
  for (const cat of categorias.value) {
    map.set(cat.id, { nome: cat.nome, total: 0, valor: 0 })
  }
  for (const prod of produtos.value) {
    const item = map.get(prod.categoriaId)
    if (item) {
      item.total += 1
      item.valor += Number(prod.preco)
    }
  }
  return Array.from(map.values())
    .sort((a, b) => b.total - a.total)
    .slice(0, 6)
})

const produtosRecentes = computed(() =>
  [...produtos.value].sort((a, b) => b.id - a.id).slice(0, 5)
)

const maxProdutosCategoria = computed(() =>
  Math.max(1, ...distribuicaoPorCategoria.value.map((c) => c.total))
)

async function carregar() {
  carregando.value = true
  try {
    const [c, p] = await Promise.all([api.listarCategorias(), api.listarProdutos()])
    categorias.value = c
    produtos.value = p
  } catch (err) {
    if (err instanceof ApiError) feedback.error(err.message)
  } finally {
    carregando.value = false
  }
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageHeader
      eyebrow="Dashboard"
      title="Visão geral do catálogo"
      subtitle="Acompanhe o estado do inventário comercial em tempo real."
    >
      <template #actions>
        <v-btn
          variant="outlined"
          prepend-icon="mdi-refresh"
          :loading="carregando"
          @click="carregar"
        >
          Atualizar
        </v-btn>
        <v-btn color="primary" prepend-icon="mdi-plus" to="/produtos">
          Novo produto
        </v-btn>
      </template>
    </PageHeader>

    <v-row dense>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Categorias"
          :value="categorias.length"
          hint="Total cadastrado"
          icon="mdi-tag-multiple-outline"
          color="primary"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Produtos"
          :value="produtos.length"
          hint="Itens no catálogo"
          icon="mdi-package-variant-closed"
          color="info"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Valor do inventário"
          :value="formatadorMoeda.format(valorInventario)"
          hint="Soma dos preços cadastrados"
          icon="mdi-cash-multiple"
          color="success"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Ticket médio"
          :value="formatadorMoeda.format(ticketMedio)"
          :hint="`${categoriasAtivas} categorias com produtos`"
          icon="mdi-chart-line-variant"
          color="warning"
          :loading="carregando"
        />
      </v-col>
    </v-row>

    <v-row class="mt-2" dense>
      <v-col cols="12" lg="7">
        <v-card class="pa-6 h-100">
          <div class="d-flex align-center justify-space-between mb-1">
            <div>
              <div class="text-h6 font-weight-bold">Produtos recentes</div>
              <div class="app-subtitle">Últimos itens cadastrados no catálogo.</div>
            </div>
            <v-btn variant="text" size="small" to="/produtos" append-icon="mdi-arrow-right">
              Ver todos
            </v-btn>
          </div>

          <v-divider class="my-3" style="opacity: 0.5;" />

          <v-table v-if="produtosRecentes.length" hover density="comfortable">
            <thead>
              <tr>
                <th>Produto</th>
                <th>Categoria</th>
                <th class="text-end">Preço</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in produtosRecentes" :key="p.id">
                <td>
                  <div class="font-weight-semibold">{{ p.nome }}</div>
                  <div
                    class="text-caption"
                    style="color: rgb(var(--v-theme-on-surface-variant));"
                  >
                    ID #{{ p.id }}
                  </div>
                </td>
                <td>
                  <v-chip
                    color="primary"
                    variant="tonal"
                    size="small"
                    :prepend-icon="'mdi-tag-outline'"
                  >
                    {{ p.categoria?.nome ?? '—' }}
                  </v-chip>
                </td>
                <td class="text-end font-weight-bold">{{ formatadorMoeda.format(p.preco) }}</td>
              </tr>
            </tbody>
          </v-table>

          <EmptyState
            v-else
            icon="mdi-package-variant"
            title="Nenhum produto cadastrado ainda"
            message="Crie sua primeira categoria e adicione produtos para vê-los aqui."
            cta-label="Ir para Produtos"
            cta-icon="mdi-arrow-right"
            @cta="$router.push('/produtos')"
          />
        </v-card>
      </v-col>

      <v-col cols="12" lg="5">
        <v-card class="pa-6 h-100">
          <div class="d-flex align-center justify-space-between mb-1">
            <div>
              <div class="text-h6 font-weight-bold">Por categoria</div>
              <div class="app-subtitle">Top 6 com mais produtos.</div>
            </div>
            <v-btn variant="text" size="small" to="/categorias" append-icon="mdi-arrow-right">
              Gerenciar
            </v-btn>
          </div>

          <v-divider class="my-3" style="opacity: 0.5;" />

          <div v-if="distribuicaoPorCategoria.length" class="d-flex flex-column ga-4 pt-2">
            <div
              v-for="cat in distribuicaoPorCategoria"
              :key="cat.nome"
              class="d-flex flex-column ga-1"
            >
              <div class="d-flex align-center justify-space-between">
                <div class="font-weight-semibold">{{ cat.nome }}</div>
                <div
                  class="text-caption font-weight-medium"
                  style="color: rgb(var(--v-theme-on-surface-variant));"
                >
                  {{ cat.total }} {{ cat.total === 1 ? 'produto' : 'produtos' }} ·
                  {{ formatadorMoeda.format(cat.valor) }}
                </div>
              </div>
              <v-progress-linear
                :model-value="(cat.total / maxProdutosCategoria) * 100"
                color="primary"
                height="6"
                rounded
                bg-color="surface-variant"
              />
            </div>
          </div>

          <EmptyState
            v-else
            icon="mdi-tag-outline"
            title="Sem categorias cadastradas"
            message="Crie categorias para organizar seus produtos."
            cta-label="Criar categoria"
            cta-icon="mdi-plus"
            @cta="$router.push('/categorias')"
          />
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

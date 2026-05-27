<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import {
  ApiError,
  useApi,
  type Categoria,
  type Produto,
  type ProdutoPayload
} from '~/composables/useApi'
import { useFeedback } from '~/composables/useFeedback'

definePageMeta({ title: 'Produtos' })

const api = useApi()
const feedback = useFeedback()

const produtos = ref<Produto[]>([])
const categorias = ref<Categoria[]>([])
const carregando = ref(false)
const carregandoCategorias = ref(false)
const salvando = ref(false)
const excluindo = ref(false)

const busca = ref('')
const filtroCategoriaId = ref<number | null>(null)

const dialogFormAberto = ref(false)
const modoEdicao = ref(false)
const idEmEdicao = ref<number | null>(null)
const form = reactive<ProdutoPayload>({
  nome: '',
  descricao: '',
  preco: 0,
  categoriaId: 0
})

const dialogConfirmacaoAberto = ref(false)
const produtoParaExcluir = ref<Produto | null>(null)

const headers = [
  { title: 'Produto', key: 'nome', sortable: true },
  { title: 'Categoria', key: 'categoria.nome', sortable: true, width: 200 },
  { title: 'Preço', key: 'preco', sortable: true, align: 'end' as const, width: 160 },
  { title: '', key: 'acoes', sortable: false, width: 130, align: 'end' as const }
]

const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL'
})

function formatarPreco(valor: number) {
  return formatadorMoeda.format(valor)
}

const produtosFiltrados = computed(() => {
  let lista = produtos.value
  if (filtroCategoriaId.value) {
    lista = lista.filter((p) => p.categoriaId === filtroCategoriaId.value)
  }
  const termo = busca.value.trim().toLowerCase()
  if (termo) {
    lista = lista.filter(
      (p) =>
        p.nome.toLowerCase().includes(termo) ||
        (p.descricao ?? '').toLowerCase().includes(termo) ||
        (p.categoria?.nome ?? '').toLowerCase().includes(termo)
    )
  }
  return lista
})

const totalProdutos = computed(() => produtos.value.length)
const valorInventario = computed(() =>
  produtos.value.reduce((acc, p) => acc + Number(p.preco || 0), 0)
)
const ticketMedio = computed(() =>
  totalProdutos.value === 0 ? 0 : valorInventario.value / totalProdutos.value
)
const categoriasComProdutos = computed(() => {
  const ids = new Set(produtos.value.map((p) => p.categoriaId))
  return ids.size
})

const opcoesCategoriaFiltro = computed(() => [
  { id: null as number | null, nome: 'Todas as categorias' },
  ...categorias.value
])

const nomeValido = computed(() => form.nome.trim().length >= 5)
const precoValido = computed(() => Number(form.preco) > 0)
const categoriaValida = computed(() => Number(form.categoriaId) > 0)
const podeSalvar = computed(
  () => nomeValido.value && precoValido.value && categoriaValida.value && !salvando.value
)

async function carregarProdutos() {
  carregando.value = true
  try {
    produtos.value = await api.listarProdutos()
  } catch (err) {
    if (err instanceof ApiError) feedback.error(err.message)
  } finally {
    carregando.value = false
  }
}

async function carregarCategorias() {
  carregandoCategorias.value = true
  try {
    categorias.value = await api.listarCategorias()
  } catch (err) {
    if (err instanceof ApiError) feedback.error(err.message)
  } finally {
    carregandoCategorias.value = false
  }
}

async function abrirNovoCadastro() {
  modoEdicao.value = false
  idEmEdicao.value = null
  form.nome = ''
  form.descricao = ''
  form.preco = 0
  form.categoriaId = 0
  if (!categorias.value.length) await carregarCategorias()
  dialogFormAberto.value = true
}

async function abrirEdicao(produto: Produto) {
  modoEdicao.value = true
  idEmEdicao.value = produto.id
  form.nome = produto.nome
  form.descricao = produto.descricao ?? ''
  form.preco = produto.preco
  form.categoriaId = produto.categoriaId
  if (!categorias.value.length) await carregarCategorias()
  dialogFormAberto.value = true
}

async function salvar() {
  if (!podeSalvar.value) return

  const payload: ProdutoPayload = {
    nome: form.nome.trim(),
    descricao: form.descricao?.trim() || null,
    preco: Number(form.preco),
    categoriaId: Number(form.categoriaId)
  }

  salvando.value = true
  try {
    if (modoEdicao.value && idEmEdicao.value !== null) {
      const atualizado = await api.atualizarProduto(idEmEdicao.value, payload)
      const idx = produtos.value.findIndex((p) => p.id === atualizado.id)
      if (idx >= 0) produtos.value.splice(idx, 1, atualizado)
      feedback.success('Produto atualizado com sucesso.')
    } else {
      const novo = await api.criarProduto(payload)
      produtos.value.push(novo)
      produtos.value.sort((a, b) => a.nome.localeCompare(b.nome))
      feedback.success('Produto criado com sucesso.')
    }
    dialogFormAberto.value = false
  } catch (err) {
    if (err instanceof ApiError) feedback.error(err.message)
  } finally {
    salvando.value = false
  }
}

function pedirConfirmacaoExclusao(produto: Produto) {
  produtoParaExcluir.value = produto
  dialogConfirmacaoAberto.value = true
}

async function confirmarExclusao() {
  if (!produtoParaExcluir.value) return
  excluindo.value = true
  try {
    const id = produtoParaExcluir.value.id
    await api.excluirProduto(id)
    produtos.value = produtos.value.filter((p) => p.id !== id)
    feedback.success('Produto excluído com sucesso.')
    dialogConfirmacaoAberto.value = false
    produtoParaExcluir.value = null
  } catch (err) {
    if (err instanceof ApiError) feedback.error(err.message)
    dialogConfirmacaoAberto.value = false
  } finally {
    excluindo.value = false
  }
}

onMounted(async () => {
  await Promise.all([carregarProdutos(), carregarCategorias()])
})
</script>

<template>
  <div>
    <PageHeader
      eyebrow="Catálogo"
      title="Produtos"
      subtitle="Cadastre, edite e organize os itens disponíveis no inventário."
    >
      <template #actions>
        <v-btn
          variant="outlined"
          prepend-icon="mdi-refresh"
          :loading="carregando"
          @click="carregarProdutos"
        >
          Atualizar
        </v-btn>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="abrirNovoCadastro">
          Novo produto
        </v-btn>
      </template>
    </PageHeader>

    <v-row dense>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Total"
          :value="totalProdutos"
          hint="Produtos cadastrados"
          icon="mdi-package-variant-closed"
          color="primary"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Valor do inventário"
          :value="formatarPreco(valorInventario)"
          hint="Somatório dos preços"
          icon="mdi-cash-multiple"
          color="success"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Ticket médio"
          :value="formatarPreco(ticketMedio)"
          hint="Preço médio por produto"
          icon="mdi-chart-line-variant"
          color="warning"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="6" lg="3">
        <StatCard
          label="Categorias ativas"
          :value="categoriasComProdutos"
          :hint="`de ${categorias.length} cadastradas`"
          icon="mdi-tag-multiple-outline"
          color="info"
          :loading="carregando"
        />
      </v-col>
    </v-row>

    <v-card class="mt-4 pa-2">
      <div class="d-flex align-center ga-3 pa-3 pb-2 flex-wrap">
        <v-text-field
          v-model="busca"
          prepend-inner-icon="mdi-magnify"
          placeholder="Buscar por nome, descrição ou categoria..."
          hide-details
          density="compact"
          style="max-width: 380px;"
          clearable
        />
        <v-select
          v-model="filtroCategoriaId"
          :items="opcoesCategoriaFiltro"
          item-title="nome"
          item-value="id"
          prepend-inner-icon="mdi-filter-variant"
          hide-details
          density="compact"
          style="max-width: 260px;"
        />
        <v-spacer />
        <v-chip variant="tonal" size="small" color="primary">
          {{ produtosFiltrados.length }} de {{ produtos.length }}
        </v-chip>
      </div>

      <v-data-table
        :headers="headers"
        :items="produtosFiltrados"
        :loading="carregando"
        items-per-page="10"
        loading-text="Carregando produtos..."
      >
        <template #item.nome="{ item }">
          <div class="d-flex align-center ga-3 py-1">
            <div
              class="d-flex align-center justify-center"
              style="
                width: 36px;
                height: 36px;
                border-radius: 10px;
                background: rgba(var(--v-theme-info), 0.1);
                color: rgb(var(--v-theme-info));
              "
            >
              <v-icon icon="mdi-package-variant-closed" size="18" />
            </div>
            <div style="min-width: 0;">
              <div class="font-weight-semibold text-truncate" style="max-width: 320px;">
                {{ item.nome }}
              </div>
              <div
                class="text-caption text-truncate"
                style="color: rgb(var(--v-theme-on-surface-variant)); max-width: 320px;"
              >
                {{ item.descricao || `ID #${item.id}` }}
              </div>
            </div>
          </div>
        </template>

        <template #item.categoria.nome="{ item }">
          <v-chip
            color="primary"
            variant="tonal"
            size="small"
            prepend-icon="mdi-tag-outline"
          >
            {{ item.categoria?.nome ?? '—' }}
          </v-chip>
        </template>

        <template #item.preco="{ item }">
          <span class="font-weight-bold">{{ formatarPreco(item.preco) }}</span>
        </template>

        <template #item.acoes="{ item }">
          <div class="d-flex justify-end ga-1">
            <v-btn
              icon="mdi-pencil-outline"
              variant="text"
              color="primary"
              size="small"
              density="comfortable"
              aria-label="Editar produto"
              @click="abrirEdicao(item)"
            />
            <v-btn
              icon="mdi-delete-outline"
              variant="text"
              color="error"
              size="small"
              density="comfortable"
              aria-label="Excluir produto"
              @click="pedirConfirmacaoExclusao(item)"
            />
          </div>
        </template>

        <template #no-data>
          <EmptyState
            v-if="!busca && !filtroCategoriaId"
            icon="mdi-package-variant-closed"
            title="Nenhum produto cadastrado"
            message="Comece adicionando seu primeiro produto ao catálogo."
            cta-label="Novo produto"
            @cta="abrirNovoCadastro"
          />
          <EmptyState
            v-else
            icon="mdi-filter-variant-remove"
            title="Nenhum produto encontrado"
            message="Tente ajustar a busca ou o filtro de categoria."
            cta-label="Limpar filtros"
            cta-icon="mdi-close"
            @cta="busca = ''; filtroCategoriaId = null"
          />
        </template>
      </v-data-table>
    </v-card>

    <v-dialog v-model="dialogFormAberto" max-width="660" persistent>
      <v-card class="pa-2">
        <div class="pa-4 pb-2">
          <div class="d-flex align-center ga-3">
            <div
              class="d-flex align-center justify-center"
              style="
                width: 44px;
                height: 44px;
                border-radius: 12px;
                background: rgba(var(--v-theme-info), 0.1);
                color: rgb(var(--v-theme-info));
              "
            >
              <v-icon :icon="modoEdicao ? 'mdi-pencil-outline' : 'mdi-plus'" size="22" />
            </div>
            <div>
              <div class="text-h6 font-weight-bold">
                {{ modoEdicao ? 'Editar produto' : 'Novo produto' }}
              </div>
              <div class="app-subtitle" style="font-size: 0.8125rem;">
                {{ modoEdicao
                  ? 'Atualize os dados do produto.'
                  : 'Cadastre um novo produto e vincule a uma categoria.'
                }}
              </div>
            </div>
          </div>
        </div>

        <v-card-text class="pt-3">
          <v-form @submit.prevent="salvar">
            <v-text-field
              v-model="form.nome"
              label="Nome do produto"
              :counter="160"
              maxlength="160"
              :error-messages="
                form.nome.length > 0 && !nomeValido
                  ? ['Mínimo de 5 caracteres.']
                  : []
              "
              :hint="form.nome.length === 0 ? 'Ex.: Notebook Dell Inspiron 15' : ''"
              persistent-hint
              autofocus
              required
            />
            <v-textarea
              v-model="form.descricao"
              label="Descrição (opcional)"
              rows="3"
              :counter="1000"
              maxlength="1000"
              class="mt-2"
              auto-grow
            />
            <v-row dense class="mt-1">
              <v-col cols="12" sm="6">
                <v-text-field
                  v-model.number="form.preco"
                  label="Preço (R$)"
                  type="number"
                  step="0.01"
                  min="0.01"
                  prepend-inner-icon="mdi-currency-brl"
                  :error-messages="
                    form.preco !== 0 && !precoValido
                      ? ['O preço deve ser maior que zero.']
                      : []
                  "
                  required
                />
              </v-col>
              <v-col cols="12" sm="6">
                <v-select
                  v-model.number="form.categoriaId"
                  label="Categoria"
                  :items="categorias"
                  item-title="nome"
                  item-value="id"
                  prepend-inner-icon="mdi-tag-outline"
                  :loading="carregandoCategorias"
                  :no-data-text="
                    carregandoCategorias ? 'Carregando...' : 'Cadastre uma categoria primeiro.'
                  "
                  required
                />
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>

        <v-card-actions class="px-4 pb-4">
          <v-spacer />
          <v-btn variant="text" :disabled="salvando" @click="dialogFormAberto = false">
            Cancelar
          </v-btn>
          <v-btn
            color="primary"
            :disabled="!podeSalvar"
            :loading="salvando"
            prepend-icon="mdi-check"
            @click="salvar"
          >
            {{ modoEdicao ? 'Salvar alterações' : 'Criar produto' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <ConfirmDialog
      v-model="dialogConfirmacaoAberto"
      title="Excluir produto"
      :message="`Deseja realmente excluir o produto “${produtoParaExcluir?.nome ?? ''}”? Esta ação não pode ser desfeita.`"
      confirm-text="Excluir produto"
      icon="mdi-delete-alert-outline"
      confirm-color="error"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

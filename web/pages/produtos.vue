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
  { title: 'ID', key: 'id', sortable: true, width: 80 },
  { title: 'Nome', key: 'nome', sortable: true },
  { title: 'Categoria', key: 'categoria.nome', sortable: true },
  { title: 'Preço', key: 'preco', sortable: true, align: 'end' as const, width: 140 },
  { title: 'Ações', key: 'acoes', sortable: false, width: 180, align: 'end' as const }
]

const nomeValido = computed(() => form.nome.trim().length >= 5)
const precoValido = computed(() => Number(form.preco) > 0)
const categoriaValida = computed(() => Number(form.categoriaId) > 0)
const podeSalvar = computed(
  () => nomeValido.value && precoValido.value && categoriaValida.value && !salvando.value
)

const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL'
})

function formatarPreco(valor: number) {
  return formatadorMoeda.format(valor)
}

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
    <div class="d-flex align-center mb-4">
      <div>
        <div class="text-h5 font-weight-bold">Produtos</div>
        <div class="text-body-2 text-medium-emphasis">
          Gerencie os produtos do catálogo e seus vínculos com categorias.
        </div>
      </div>
      <v-spacer />
      <v-btn color="primary" prepend-icon="mdi-plus" @click="abrirNovoCadastro">
        Novo produto
      </v-btn>
    </div>

    <v-card>
      <v-data-table
        :headers="headers"
        :items="produtos"
        :loading="carregando"
        items-per-page="10"
        density="comfortable"
        no-data-text="Nenhum produto cadastrado."
        loading-text="Carregando produtos..."
      >
        <template #item.categoria.nome="{ item }">
          <v-chip color="primary" variant="tonal" size="small">
            {{ item.categoria?.nome ?? '—' }}
          </v-chip>
        </template>

        <template #item.preco="{ item }">
          <span class="font-weight-medium">{{ formatarPreco(item.preco) }}</span>
        </template>

        <template #item.acoes="{ item }">
          <v-btn
            icon="mdi-pencil"
            variant="text"
            color="primary"
            size="small"
            density="comfortable"
            aria-label="Editar produto"
            @click="abrirEdicao(item)"
          />
          <v-btn
            icon="mdi-delete"
            variant="text"
            color="error"
            size="small"
            density="comfortable"
            aria-label="Excluir produto"
            @click="pedirConfirmacaoExclusao(item)"
          />
        </template>
      </v-data-table>
    </v-card>

    <v-dialog v-model="dialogFormAberto" max-width="640" persistent>
      <v-card>
        <v-card-title class="text-h6">
          {{ modoEdicao ? 'Editar produto' : 'Novo produto' }}
        </v-card-title>

        <v-card-text>
          <v-form @submit.prevent="salvar">
            <v-text-field
              v-model="form.nome"
              label="Nome *"
              :counter="160"
              maxlength="160"
              :error-messages="
                form.nome.length > 0 && !nomeValido
                  ? ['O Nome deve possuir no mínimo 5 caracteres.']
                  : []
              "
              autofocus
            />
            <v-textarea
              v-model="form.descricao"
              label="Descrição"
              rows="3"
              :counter="1000"
              maxlength="1000"
            />
            <div class="d-flex ga-3">
              <v-text-field
                v-model.number="form.preco"
                label="Preço (R$) *"
                type="number"
                step="0.01"
                min="0.01"
                :error-messages="
                  Number(form.preco) > 0 || form.preco === 0
                    ? form.preco !== 0 && !precoValido
                      ? ['O preço deve ser maior que zero.']
                      : []
                    : ['Informe um preço válido.']
                "
              />
              <v-select
                v-model.number="form.categoriaId"
                label="Categoria *"
                :items="categorias"
                item-title="nome"
                item-value="id"
                :loading="carregandoCategorias"
                :no-data-text="
                  carregandoCategorias ? 'Carregando...' : 'Cadastre uma categoria primeiro.'
                "
                :error-messages="
                  form.categoriaId > 0 || !dialogFormAberto
                    ? []
                    : (modoEdicao
                      ? []
                      : ['Selecione uma categoria.'])
                "
              />
            </div>
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
            @click="salvar"
          >
            Salvar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <ConfirmDialog
      v-model="dialogConfirmacaoAberto"
      title="Excluir produto"
      :message="`Deseja realmente excluir o produto “${produtoParaExcluir?.nome ?? ''}”? Esta ação não pode ser desfeita.`"
      confirm-text="Excluir"
      confirm-color="error"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

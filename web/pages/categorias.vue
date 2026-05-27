<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ApiError, useApi, type Categoria, type CategoriaPayload, type Produto } from '~/composables/useApi'
import { useFeedback } from '~/composables/useFeedback'

definePageMeta({ title: 'Categorias' })

const api = useApi()
const feedback = useFeedback()

const categorias = ref<Categoria[]>([])
const produtos = ref<Produto[]>([])
const carregando = ref(false)
const salvando = ref(false)
const excluindo = ref(false)
const busca = ref('')

const dialogFormAberto = ref(false)
const modoEdicao = ref(false)
const idEmEdicao = ref<number | null>(null)
const form = reactive<CategoriaPayload>({ nome: '', descricao: '' })

const dialogConfirmacaoAberto = ref(false)
const categoriaParaExcluir = ref<Categoria | null>(null)

const headers = [
  { title: 'Categoria', key: 'nome', sortable: true },
  { title: 'Descrição', key: 'descricao', sortable: false },
  { title: 'Produtos vinculados', key: 'totalProdutos', sortable: true, align: 'end' as const, width: 200 },
  { title: '', key: 'acoes', sortable: false, width: 130, align: 'end' as const }
]

const contagemPorCategoria = computed(() => {
  const map = new Map<number, number>()
  for (const p of produtos.value) {
    map.set(p.categoriaId, (map.get(p.categoriaId) ?? 0) + 1)
  }
  return map
})

const categoriasComTotal = computed(() =>
  categorias.value.map((c) => ({
    ...c,
    totalProdutos: contagemPorCategoria.value.get(c.id) ?? 0
  }))
)

const categoriasFiltradas = computed(() => {
  const termo = busca.value.trim().toLowerCase()
  if (!termo) return categoriasComTotal.value
  return categoriasComTotal.value.filter(
    (c) =>
      c.nome.toLowerCase().includes(termo) ||
      (c.descricao ?? '').toLowerCase().includes(termo)
  )
})

const totalCategorias = computed(() => categorias.value.length)
const totalComProdutos = computed(
  () => categoriasComTotal.value.filter((c) => c.totalProdutos > 0).length
)
const totalSemProdutos = computed(() => totalCategorias.value - totalComProdutos.value)
const totalComDescricao = computed(
  () => categorias.value.filter((c) => (c.descricao ?? '').trim().length > 0).length
)

const nomeValido = computed(() => form.nome.trim().length >= 5)
const podeSalvar = computed(() => nomeValido.value && !salvando.value)

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

function abrirNovoCadastro() {
  modoEdicao.value = false
  idEmEdicao.value = null
  form.nome = ''
  form.descricao = ''
  dialogFormAberto.value = true
}

function abrirEdicao(categoria: Categoria) {
  modoEdicao.value = true
  idEmEdicao.value = categoria.id
  form.nome = categoria.nome
  form.descricao = categoria.descricao ?? ''
  dialogFormAberto.value = true
}

async function salvar() {
  if (!podeSalvar.value) return

  const payload: CategoriaPayload = {
    nome: form.nome.trim(),
    descricao: form.descricao?.trim() || null
  }

  salvando.value = true
  try {
    if (modoEdicao.value && idEmEdicao.value !== null) {
      const atualizada = await api.atualizarCategoria(idEmEdicao.value, payload)
      const idx = categorias.value.findIndex((c) => c.id === atualizada.id)
      if (idx >= 0) categorias.value.splice(idx, 1, atualizada)
      feedback.success('Categoria atualizada com sucesso.')
    } else {
      const nova = await api.criarCategoria(payload)
      categorias.value.push(nova)
      categorias.value.sort((a, b) => a.nome.localeCompare(b.nome))
      feedback.success('Categoria criada com sucesso.')
    }
    dialogFormAberto.value = false
  } catch (err) {
    if (err instanceof ApiError) feedback.error(err.message)
  } finally {
    salvando.value = false
  }
}

function pedirConfirmacaoExclusao(categoria: Categoria) {
  categoriaParaExcluir.value = categoria
  dialogConfirmacaoAberto.value = true
}

async function confirmarExclusao() {
  if (!categoriaParaExcluir.value) return
  excluindo.value = true
  try {
    const id = categoriaParaExcluir.value.id
    await api.excluirCategoria(id)
    categorias.value = categorias.value.filter((c) => c.id !== id)
    feedback.success('Categoria excluída com sucesso.')
    dialogConfirmacaoAberto.value = false
    categoriaParaExcluir.value = null
  } catch (err) {
    if (err instanceof ApiError) feedback.error(err.message)
    dialogConfirmacaoAberto.value = false
  } finally {
    excluindo.value = false
  }
}

onMounted(carregar)
</script>

<template>
  <div>
    <PageHeader
      eyebrow="Catálogo"
      title="Categorias"
      subtitle="Organize os agrupamentos dos produtos do inventário."
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
        <v-btn color="primary" prepend-icon="mdi-plus" @click="abrirNovoCadastro">
          Nova categoria
        </v-btn>
      </template>
    </PageHeader>

    <v-row dense>
      <v-col cols="12" sm="4">
        <StatCard
          label="Total"
          :value="totalCategorias"
          hint="Categorias cadastradas"
          icon="mdi-tag-multiple-outline"
          color="primary"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="4">
        <StatCard
          label="Em uso"
          :value="totalComProdutos"
          hint="Possuem produtos vinculados"
          icon="mdi-link-variant"
          color="success"
          :loading="carregando"
        />
      </v-col>
      <v-col cols="12" sm="4">
        <StatCard
          label="Vazias"
          :value="totalSemProdutos"
          :hint="`${totalComDescricao} com descrição preenchida`"
          icon="mdi-tag-off-outline"
          color="warning"
          :loading="carregando"
        />
      </v-col>
    </v-row>

    <v-card class="mt-4 pa-2">
      <div class="d-flex align-center ga-3 pa-3 pb-2 flex-wrap">
        <v-text-field
          v-model="busca"
          prepend-inner-icon="mdi-magnify"
          placeholder="Buscar por nome ou descrição..."
          hide-details
          density="compact"
          style="max-width: 380px;"
          clearable
        />
        <v-spacer />
        <v-chip variant="tonal" size="small" color="primary">
          {{ categoriasFiltradas.length }} de {{ categorias.length }}
        </v-chip>
      </div>

      <v-data-table
        :headers="headers"
        :items="categoriasFiltradas"
        :loading="carregando"
        items-per-page="10"
        no-data-text=""
        loading-text="Carregando categorias..."
      >
        <template #item.nome="{ item }">
          <div class="d-flex align-center ga-3 py-1">
            <div
              class="d-flex align-center justify-center"
              style="
                width: 36px;
                height: 36px;
                border-radius: 10px;
                background: rgba(var(--v-theme-primary), 0.1);
                color: rgb(var(--v-theme-primary));
              "
            >
              <v-icon icon="mdi-tag-outline" size="18" />
            </div>
            <div>
              <div class="font-weight-semibold">{{ item.nome }}</div>
              <div
                class="text-caption"
                style="color: rgb(var(--v-theme-on-surface-variant));"
              >
                ID #{{ item.id }}
              </div>
            </div>
          </div>
        </template>

        <template #item.descricao="{ item }">
          <span
            :class="{ 'text-medium-emphasis font-italic': !item.descricao }"
            style="font-size: 0.875rem;"
          >
            {{ item.descricao || 'Sem descrição' }}
          </span>
        </template>

        <template #item.totalProdutos="{ item }">
          <v-chip
            v-if="item.totalProdutos > 0"
            color="success"
            variant="tonal"
            size="small"
            prepend-icon="mdi-package-variant-closed"
          >
            {{ item.totalProdutos }} {{ item.totalProdutos === 1 ? 'produto' : 'produtos' }}
          </v-chip>
          <span
            v-else
            class="text-caption"
            style="color: rgb(var(--v-theme-on-surface-variant));"
          >
            —
          </span>
        </template>

        <template #item.acoes="{ item }">
          <div class="d-flex justify-end ga-1">
            <v-btn
              icon="mdi-pencil-outline"
              variant="text"
              color="primary"
              size="small"
              density="comfortable"
              aria-label="Editar categoria"
              @click="abrirEdicao(item)"
            />
            <v-btn
              icon="mdi-delete-outline"
              variant="text"
              color="error"
              size="small"
              density="comfortable"
              aria-label="Excluir categoria"
              @click="pedirConfirmacaoExclusao(item)"
            />
          </div>
        </template>

        <template #no-data>
          <EmptyState
            v-if="!busca"
            icon="mdi-tag-multiple-outline"
            title="Nenhuma categoria cadastrada"
            message="Crie sua primeira categoria para começar a organizar os produtos do catálogo."
            cta-label="Nova categoria"
            @cta="abrirNovoCadastro"
          />
          <EmptyState
            v-else
            icon="mdi-magnify"
            title="Nenhuma categoria encontrada"
            :message="`Sem resultados para “${busca}”. Tente outro termo.`"
            cta-label="Limpar busca"
            cta-icon="mdi-close"
            @cta="busca = ''"
          />
        </template>
      </v-data-table>
    </v-card>

    <v-dialog v-model="dialogFormAberto" max-width="560" persistent>
      <v-card class="pa-2">
        <div class="pa-4 pb-2">
          <div class="d-flex align-center ga-3">
            <div
              class="d-flex align-center justify-center"
              style="
                width: 44px;
                height: 44px;
                border-radius: 12px;
                background: rgba(var(--v-theme-primary), 0.1);
                color: rgb(var(--v-theme-primary));
              "
            >
              <v-icon :icon="modoEdicao ? 'mdi-pencil-outline' : 'mdi-plus'" size="22" />
            </div>
            <div>
              <div class="text-h6 font-weight-bold">
                {{ modoEdicao ? 'Editar categoria' : 'Nova categoria' }}
              </div>
              <div class="app-subtitle" style="font-size: 0.8125rem;">
                {{ modoEdicao ? 'Atualize as informações abaixo.' : 'Preencha os dados para criar uma nova categoria.' }}
              </div>
            </div>
          </div>
        </div>

        <v-card-text class="pt-3">
          <v-form @submit.prevent="salvar">
            <v-text-field
              v-model="form.nome"
              label="Nome"
              :counter="120"
              maxlength="120"
              :error-messages="
                form.nome.length > 0 && !nomeValido
                  ? ['Mínimo de 5 caracteres.']
                  : []
              "
              :hint="form.nome.length === 0 ? 'Ex.: Eletrônicos, Ferramentas, Vestuário' : ''"
              persistent-hint
              autofocus
              required
            />
            <v-textarea
              v-model="form.descricao"
              label="Descrição (opcional)"
              rows="3"
              :counter="500"
              maxlength="500"
              class="mt-2"
              auto-grow
            />
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
            {{ modoEdicao ? 'Salvar alterações' : 'Criar categoria' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <ConfirmDialog
      v-model="dialogConfirmacaoAberto"
      title="Excluir categoria"
      :message="`Deseja realmente excluir a categoria “${categoriaParaExcluir?.nome ?? ''}”? Esta ação não pode ser desfeita.`"
      confirm-text="Excluir categoria"
      icon="mdi-delete-alert-outline"
      confirm-color="error"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

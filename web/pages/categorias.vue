<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ApiError, useApi, type Categoria, type CategoriaPayload } from '~/composables/useApi'
import { useFeedback } from '~/composables/useFeedback'

definePageMeta({ title: 'Categorias' })

const api = useApi()
const feedback = useFeedback()

const categorias = ref<Categoria[]>([])
const carregando = ref(false)
const salvando = ref(false)
const excluindo = ref(false)

const dialogFormAberto = ref(false)
const modoEdicao = ref(false)
const idEmEdicao = ref<number | null>(null)
const form = reactive<CategoriaPayload>({ nome: '', descricao: '' })

const dialogConfirmacaoAberto = ref(false)
const categoriaParaExcluir = ref<Categoria | null>(null)

const headers = [
  { title: 'ID', key: 'id', sortable: true, width: 90 },
  { title: 'Nome', key: 'nome', sortable: true },
  { title: 'Descrição', key: 'descricao', sortable: false },
  { title: 'Ações', key: 'acoes', sortable: false, width: 180, align: 'end' as const }
]

const nomeValido = computed(() => form.nome.trim().length >= 5)
const podeSalvar = computed(() => nomeValido.value && !salvando.value)

async function carregarCategorias() {
  carregando.value = true
  try {
    categorias.value = await api.listarCategorias()
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

onMounted(carregarCategorias)
</script>

<template>
  <div>
    <div class="d-flex align-center mb-4">
      <div>
        <div class="text-h5 font-weight-bold">Categorias</div>
        <div class="text-body-2 text-medium-emphasis">
          Gerencie as categorias de produtos do catálogo.
        </div>
      </div>
      <v-spacer />
      <v-btn color="primary" prepend-icon="mdi-plus" @click="abrirNovoCadastro">
        Nova categoria
      </v-btn>
    </div>

    <v-card>
      <v-data-table
        :headers="headers"
        :items="categorias"
        :loading="carregando"
        items-per-page="10"
        density="comfortable"
        no-data-text="Nenhuma categoria cadastrada."
        loading-text="Carregando categorias..."
      >
        <template #item.descricao="{ item }">
          <span :class="{ 'text-medium-emphasis': !item.descricao }">
            {{ item.descricao || '—' }}
          </span>
        </template>

        <template #item.acoes="{ item }">
          <v-btn
            icon="mdi-pencil"
            variant="text"
            color="primary"
            size="small"
            density="comfortable"
            aria-label="Editar categoria"
            @click="abrirEdicao(item)"
          />
          <v-btn
            icon="mdi-delete"
            variant="text"
            color="error"
            size="small"
            density="comfortable"
            aria-label="Excluir categoria"
            @click="pedirConfirmacaoExclusao(item)"
          />
        </template>
      </v-data-table>
    </v-card>

    <v-dialog v-model="dialogFormAberto" max-width="560" persistent>
      <v-card>
        <v-card-title class="text-h6">
          {{ modoEdicao ? 'Editar categoria' : 'Nova categoria' }}
        </v-card-title>

        <v-card-text>
          <v-form @submit.prevent="salvar">
            <v-text-field
              v-model="form.nome"
              label="Nome *"
              :counter="120"
              maxlength="120"
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
              :counter="500"
              maxlength="500"
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
            @click="salvar"
          >
            Salvar
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <ConfirmDialog
      v-model="dialogConfirmacaoAberto"
      title="Excluir categoria"
      :message="`Deseja realmente excluir a categoria “${categoriaParaExcluir?.nome ?? ''}”? Esta ação não pode ser desfeita.`"
      confirm-text="Excluir"
      confirm-color="error"
      :loading="excluindo"
      @confirm="confirmarExclusao"
    />
  </div>
</template>

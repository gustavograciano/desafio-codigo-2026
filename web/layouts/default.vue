<script setup lang="ts">
import { onMounted } from 'vue'
import { useFeedback } from '~/composables/useFeedback'
import { useAppTheme } from '~/composables/useAppTheme'

const route = useRoute()
const { feedback, dismiss } = useFeedback()
const { isDark, toggle, restoreFromStorage } = useAppTheme()

onMounted(restoreFromStorage)

const navSections = [
  {
    label: 'Operação',
    items: [
      { title: 'Visão geral', to: '/', icon: 'mdi-view-dashboard-outline' },
      { title: 'Categorias', to: '/categorias', icon: 'mdi-tag-multiple-outline' },
      { title: 'Produtos', to: '/produtos', icon: 'mdi-package-variant-closed' }
    ]
  }
]

const pageTitle = computed(() => {
  if (route.path === '/') return 'Visão geral'
  if (route.path.startsWith('/categorias')) return 'Categorias'
  if (route.path.startsWith('/produtos')) return 'Produtos'
  return 'Inventário'
})
</script>

<template>
  <v-app>
    <v-navigation-drawer
      permanent
      width="260"
      class="app-nav"
      :border="0"
      color="surface"
    >
      <div class="pa-5 pb-2">
        <div class="d-flex align-center ga-3">
          <div
            class="d-flex align-center justify-center"
            style="
              width: 40px;
              height: 40px;
              border-radius: 12px;
              background: linear-gradient(135deg, rgb(var(--v-theme-primary)), rgb(var(--v-theme-accent)));
              color: white;
            "
          >
            <v-icon icon="mdi-cube-outline" size="22" />
          </div>
          <div>
            <div class="font-weight-bold" style="font-size: 0.95rem; letter-spacing: -0.01em;">
              Inventário
            </div>
            <div class="text-caption" style="color: rgb(var(--v-theme-on-surface-variant));">
              Catálogo comercial
            </div>
          </div>
        </div>
      </div>

      <v-divider class="my-3 mx-4" style="opacity: 0.5;" />

      <div v-for="section in navSections" :key="section.label" class="px-2 mb-2">
        <div
          class="px-4 pb-2 text-caption font-weight-bold text-uppercase"
          style="letter-spacing: 0.08em; color: rgb(var(--v-theme-on-surface-variant));"
        >
          {{ section.label }}
        </div>
        <v-list density="compact" nav class="pa-0">
          <v-list-item
            v-for="item in section.items"
            :key="item.to"
            :to="item.to"
            :prepend-icon="item.icon"
            :title="item.title"
            color="primary"
            exact
          />
        </v-list>
      </div>

      <template #append>
        <div class="pa-4">
          <v-card
            color="surface-variant"
            class="pa-3"
            rounded="lg"
            variant="flat"
            style="border: 1px solid rgba(var(--v-theme-on-surface), 0.06);"
          >
            <div class="d-flex align-center ga-3">
              <v-avatar size="36" color="primary">
                <span class="text-white font-weight-bold">GG</span>
              </v-avatar>
              <div class="flex-grow-1" style="min-width: 0;">
                <div
                  class="font-weight-bold text-body-2 text-truncate"
                  style="line-height: 1.2;"
                >
                  Gustavo Graciano
                </div>
                <div
                  class="text-caption text-truncate"
                  style="color: rgb(var(--v-theme-on-surface-variant));"
                >
                  Operador do catálogo
                </div>
              </div>
            </div>
          </v-card>
        </div>
      </template>
    </v-navigation-drawer>

    <v-app-bar
      :elevation="0"
      class="app-bar"
      height="64"
    >
      <div class="d-flex align-center ga-2 px-2">
        <v-icon icon="mdi-chevron-right" size="18" style="opacity: 0.4;" />
        <span class="text-body-2 font-weight-medium">{{ pageTitle }}</span>
      </div>

      <v-spacer />

      <div class="d-flex align-center ga-2 px-4">
        <v-btn
          variant="text"
          density="comfortable"
          :prepend-icon="isDark ? 'mdi-weather-sunny' : 'mdi-weather-night'"
          @click="toggle"
        >
          {{ isDark ? 'Claro' : 'Escuro' }}
        </v-btn>

        <v-divider vertical class="mx-2" style="opacity: 0.4;" />

        <v-btn
          icon="mdi-bell-outline"
          variant="text"
          density="comfortable"
          aria-label="Notificações"
        />
        <v-btn
          icon="mdi-help-circle-outline"
          variant="text"
          density="comfortable"
          href="https://github.com/gustavograciano/desafio-codigo-2026"
          target="_blank"
          rel="noopener"
          aria-label="Documentação"
        />
      </div>
    </v-app-bar>

    <v-main>
      <v-container fluid class="px-8 py-8" style="max-width: 1400px;">
        <slot />
      </v-container>
    </v-main>

    <v-snackbar
      v-model="feedback.visible"
      :color="feedback.color"
      :timeout="feedback.timeout"
      location="bottom right"
      multi-line
      rounded="lg"
    >
      <div class="d-flex align-center ga-2">
        <v-icon :icon="feedback.icon" />
        <span class="font-weight-medium">{{ feedback.message }}</span>
      </div>
      <template #actions>
        <v-btn icon="mdi-close" variant="text" size="small" @click="dismiss" />
      </template>
    </v-snackbar>
  </v-app>
</template>

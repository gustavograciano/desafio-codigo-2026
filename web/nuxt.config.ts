export default defineNuxtConfig({
  compatibilityDate: '2025-01-01',
  devtools: { enabled: true },

  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5228'
    }
  },

  modules: ['vuetify-nuxt-module'],

  vuetify: {
    vuetifyOptions: {
      icons: {
        defaultSet: 'mdi'
      },
      theme: {
        defaultTheme: 'light',
        themes: {
          light: {
            dark: false,
            colors: {
              primary: '#1976D2',
              secondary: '#424242',
              accent: '#82B1FF',
              error: '#D32F2F',
              success: '#388E3C',
              warning: '#F57C00',
              info: '#0288D1'
            }
          }
        }
      },
      defaults: {
        VBtn: { variant: 'flat' },
        VTextField: { variant: 'outlined', density: 'comfortable' },
        VTextarea: { variant: 'outlined', density: 'comfortable' },
        VSelect: { variant: 'outlined', density: 'comfortable' },
        VCard: { rounded: 'lg' }
      }
    }
  },

  css: [
    '@mdi/font/css/materialdesignicons.css'
  ]
})

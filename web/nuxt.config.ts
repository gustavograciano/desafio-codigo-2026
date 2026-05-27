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
      icons: { defaultSet: 'mdi' },
      theme: {
        defaultTheme: 'light',
        themes: {
          light: {
            dark: false,
            colors: {
              background: '#F7F8FA',
              surface: '#FFFFFF',
              'surface-bright': '#FFFFFF',
              'surface-variant': '#F1F3F7',
              'on-surface-variant': '#4B5563',
              primary: '#4F46E5',
              'primary-darken-1': '#4338CA',
              secondary: '#0F172A',
              accent: '#22D3EE',
              error: '#DC2626',
              success: '#16A34A',
              warning: '#F59E0B',
              info: '#0EA5E9'
            },
            variables: {
              'border-color': '#E5E7EB',
              'border-opacity': 1,
              'high-emphasis-opacity': 0.92,
              'medium-emphasis-opacity': 0.66
            }
          },
          dark: {
            dark: true,
            colors: {
              background: '#0B0F17',
              surface: '#111723',
              'surface-bright': '#161D2C',
              'surface-variant': '#1A2233',
              'on-surface-variant': '#94A3B8',
              primary: '#818CF8',
              'primary-darken-1': '#6366F1',
              secondary: '#E2E8F0',
              accent: '#22D3EE',
              error: '#F87171',
              success: '#4ADE80',
              warning: '#FBBF24',
              info: '#38BDF8'
            },
            variables: {
              'border-color': '#1F2937',
              'border-opacity': 1,
              'high-emphasis-opacity': 0.95,
              'medium-emphasis-opacity': 0.7
            }
          }
        }
      },
      defaults: {
        VBtn: {
          variant: 'flat',
          rounded: 'lg',
          style: 'text-transform: none; letter-spacing: 0; font-weight: 600;'
        },
        VTextField: {
          variant: 'outlined',
          density: 'comfortable',
          color: 'primary'
        },
        VTextarea: {
          variant: 'outlined',
          density: 'comfortable',
          color: 'primary'
        },
        VSelect: {
          variant: 'outlined',
          density: 'comfortable',
          color: 'primary'
        },
        VCard: {
          rounded: 'xl',
          variant: 'flat'
        },
        VChip: {
          rounded: 'md'
        },
        VDataTable: {
          density: 'comfortable',
          hover: true
        }
      }
    }
  },

  css: [
    '@mdi/font/css/materialdesignicons.css',
    '~/assets/css/main.css'
  ],

  app: {
    head: {
      title: 'Inventário Comercial',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Plataforma de gestão de catálogo comercial' }
      ],
      link: [
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap'
        }
      ]
    }
  }
})

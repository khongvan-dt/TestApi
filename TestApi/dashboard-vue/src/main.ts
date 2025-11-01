import './assets/css/main.css'
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ui from '@nuxt/ui/vue-plugin'
import App from './App.vue'

// === Router ===

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: () => import('./pages/index.vue') },
    {
      path: '/settingjob',
      component: () => import('./pages/settingJob.vue'),
      children: [
        { path: '', component: () => import('./pages/settingsjob/index.vue') },
        { path: 'databases', component: () => import('./pages/settingsjob/FormDbConnection.vue') },
      ]
    }
  ]
})

// === App ===
const app = createApp(App)
app.use(router)
app.use(ui)
app.use(PrimeVue)          // ✅ Use PrimeVue
app.use(ToastService)      // ✅ Use ToastService
app.mount('#app')
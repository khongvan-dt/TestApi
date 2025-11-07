import './assets/css/main.css'
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ui from '@nuxt/ui/vue-plugin'
import App from './App.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: () => import('./pages/index.vue') },
    {
      path: '/settingjob',
      component: () => import('./pages/settingJob.vue'),
      children: [
        { path: '', component: () => import('./pages/settingsjob/Scheduling.vue') },
        { path: 'databases', component: () => import('./pages/settingsjob/FormDbConnection.vue') },
      ]
    }
  ]
})

const app = createApp(App)
app.use(router)
app.use(ui)
app.use(PrimeVue)   
app.use(ToastService)
app.mount('#app')
import './assets/css/main.css'
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
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
        { path: 'notifications', component: () => import('./pages/settingsjob/notifications.vue') },
        { path: 'security', component: () => import('./pages/settingsjob/security.vue') },
      ]
    }
  ]
})

// === App ===
const app = createApp(App)
app.use(router)   
app.use(ui)
app.mount('#app')

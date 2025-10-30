import './assets/css/main.css'

import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'
import ui from '@nuxt/ui/vue-plugin'

import App from './App.vue'

const app = createApp(App)
 
app.use(createRouter({
  routes: [
     { path: '/', component: () => import('./pages/index.vue') },
          { path: '/dashboard', component: () => import('./pages/Dashboard.vue') },

      {
      path: '/settingjob',
      component: () => import('./pages/settingJob.vue'),
      children: [
        { path: '', component: () => import('./pages/settingsjob/index.vue') },
         { path: 'notifications', component: () => import('./pages/settingsjob/notifications.vue') },
        { path: 'security', component: () => import('./pages/settingsjob/security.vue') },
      ]
    }
  ],
  history: createWebHistory()
}))

app.use(ui)

app.mount('#app')

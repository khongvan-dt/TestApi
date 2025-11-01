<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useAuth } from './composables/useAuth'
import { useStorage } from '@vueuse/core'
import type { NavigationMenuItem } from '@nuxt/ui'
import Toast from 'primevue/toast'

const { isAuthenticated } = useAuth()
const toast = useToast()

const open = ref(false)
const showLoginModal = ref(false)  

 onMounted(() => {
  if (!isAuthenticated.value) {
    showLoginModal.value = true
  }
})

 watch(isAuthenticated, (newVal) => {
  if (!newVal) {
     showLoginModal.value = true
  } else {
     showLoginModal.value = false
  }
})

const links = [[{
  label: 'Home',
  icon: 'i-lucide-house',
  to: '/',
  onSelect: () => {
    open.value = false
  }
}, {
  label: 'Setting Job',
  to: '/settingjob',
  icon: 'i-lucide-settings',
  defaultOpen: true,
  type: 'trigger',
  children: [{
    label: 'API Test Suites',
    to: '/settingjob',
    exact: true,
    onSelect: () => {
      open.value = false
    }
  }, {
    label: 'DB Connection',
    to: '/settingjob/databases',
    onSelect: () => {
      open.value = false
    }
  }
]
}]] satisfies NavigationMenuItem[][]

const handleLoginSuccess = () => {
  showLoginModal.value = false
}
</script>

<template>
  <Suspense>
    <template #default>
      <UApp>
        <!-- Chỉ hiển thị dashboard khi đã đăng nhập -->
        <UDashboardGroup v-if="isAuthenticated" unit="rem" storage="local">
          <UDashboardSidebar 
            id="default" 
            v-model:open="open" 
            collapsible 
            resizable 
            class="bg-elevated/25"
            :ui="{ footer: 'lg:border-t lg:border-default' }"
          >
            <template #default="{ collapsed }">
              <UNavigationMenu 
                :collapsed="collapsed" 
                :items="links[0]" 
                orientation="vertical" 
                tooltip 
                popover 
              />

              <UNavigationMenu 
                :collapsed="collapsed" 
                :items="links[1]" 
                orientation="vertical" 
                tooltip 
                class="mt-auto" 
              />
            </template>

            <template #footer="{ collapsed }">
              <UserMenu :collapsed="collapsed" />
            </template>
          </UDashboardSidebar>

          <RouterView />

          <NotificationsSlideover />
        </UDashboardGroup>

        <!-- Nếu chưa đăng nhập - hiển thị placeholder hoặc để trống -->
        <div v-else class="flex items-center justify-center h-screen bg-gray-50 dark:bg-gray-900">
          <div class="text-center">
            <div class="animate-pulse">
              <div class="w-16 h-16 bg-blue-600 rounded-full mx-auto mb-4"></div>
            </div>
            <p class="text-gray-600 dark:text-gray-400">Please login to continue</p>
          </div>
        </div>

        <!-- Login Modal - auto show khi chưa đăng nhập -->
        <LoginModal 
          v-model="showLoginModal" 
          @login-success="handleLoginSuccess"
        />
      </UApp>
    </template>

    <!-- Fallback khi Suspense loading -->
    <template #fallback>
      <div class="flex items-center justify-center h-screen">
        <div class="text-center">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p class="mt-4 text-gray-600 dark:text-gray-400">Loading...</p>
        </div>
      </div>
    </template>
  </Suspense>
</template>
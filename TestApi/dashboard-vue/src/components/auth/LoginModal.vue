<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useAuth } from '../../composables/useAuth'
import { useUserData } from '../../composables/useUserData'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'switchToRegister'): void
  (e: 'loginSuccess'): void
}>()

const { login, isAuthenticated } = useAuth() // ✅ Get isAuthenticated
const { fetchUserData } = useUserData()

const usernameOrEmail = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

// ✅ Prevent đóng modal nếu chưa đăng nhập
const canClose = computed(() => isAuthenticated.value)

const closeModal = () => {
  // ✅ Chỉ cho phép đóng nếu đã đăng nhập
  if (!canClose.value) {
    return
  }
  
  emit('update:modelValue', false)
  setTimeout(() => {
    usernameOrEmail.value = ''
    password.value = ''
    error.value = ''
  }, 300)
}

const handleLogin = async () => {
  if (!usernameOrEmail.value || !password.value) {
    error.value = 'Please enter username/email and password'
    return
  }

  loading.value = true
  error.value = ''

  try {
    const result = await login(usernameOrEmail.value, password.value)

    if (result.success) {
       await fetchUserData()
      
      emit('loginSuccess')
      emit('update:modelValue', false)
      
      // Clear form
      usernameOrEmail.value = ''
      password.value = ''
      error.value = ''
    } else {
      error.value = result.message || 'Login failed'
    }
  } catch (err: any) {
    error.value = err.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}

 watch(() => props.modelValue, (value) => {
  if (value) {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && canClose.value) {
        closeModal()
      }
    }
    document.addEventListener('keydown', handleEscape)
    return () => document.removeEventListener('keydown', handleEscape)
  }
})
</script>

<template>
  <!-- Backdrop - không cho click outside nếu chưa login -->
  <Transition
    enter-active-class="transition-opacity duration-200"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition-opacity duration-200"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div
      v-if="modelValue"
      class="fixed inset-0 bg-black/50 backdrop-blur-sm z-40"
      @click="canClose ? closeModal() : null"
    />
  </Transition>

  <!-- Modal -->
  <Transition
    enter-active-class="transition-all duration-200"
    enter-from-class="opacity-0 scale-95"
    enter-to-class="opacity-100 scale-100"
    leave-active-class="transition-all duration-200"
    leave-from-class="opacity-100 scale-100"
    leave-to-class="opacity-0 scale-95"
  >
    <div
      v-if="modelValue"
      class="fixed inset-0 z-50 flex items-center justify-center p-4"
      @click.self="canClose ? closeModal() : null"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <div class="p-6">
          <!-- Header -->
          <div class="flex items-center justify-between mb-6">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
              Sign in to your account
            </h2>
            <!-- ✅ Chỉ hiển thị nút close nếu đã login -->
            <button
              v-if="canClose"
              type="button"
              @click="closeModal"
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- ✅ Warning message nếu bắt buộc phải login -->
          <div
            v-if="!canClose"
            class="mb-4 p-3 rounded-md bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800"
          >
            <div class="flex items-start gap-2">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p class="text-sm text-blue-800 dark:text-blue-300">
                Please login to access the application
              </p>
            </div>
          </div>

          <!-- Form -->
          <form @submit.prevent="handleLogin" class="space-y-4">
            <!-- Error Message -->
            <div
              v-if="error"
              class="p-3 rounded-md bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800"
            >
              <div class="flex items-start gap-2">
                <svg class="w-5 h-5 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p class="text-sm text-red-800 dark:text-red-300">{{ error }}</p>
              </div>
            </div>

            <!-- Username/Email Input -->
            <div class="space-y-2">
              <label for="usernameOrEmail" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Username or Email
              </label>
              <input
                id="usernameOrEmail"
                v-model="usernameOrEmail"
                type="text"
                placeholder="Enter your username or email"
                :disabled="loading"
                autofocus
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Password Input -->
            <div class="space-y-2">
              <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Password
              </label>
              <input
                id="password"
                v-model="password"
                type="password"
                placeholder="Enter your password"
                :disabled="loading"
                @keyup.enter="handleLogin"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              :disabled="loading"
              class="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              <svg v-if="loading" class="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              {{ loading ? 'Signing in...' : 'Sign in' }}
            </button>

            <!-- Register Link -->
            <div class="text-center text-sm text-gray-600 dark:text-gray-400">
              Don't have an account?
              <button
                type="button"
                @click="emit('switchToRegister')"
                class="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 font-medium"
              >
                Sign up
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </Transition>
</template>
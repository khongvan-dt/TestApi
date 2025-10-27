<script setup lang="ts">
import { ref, watch } from 'vue'
import { useApiClient } from '../../composables/useApiClient'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  'switchToLogin': []
}>()

const { auth } = useApiClient()

const username = ref('')
const email = ref('')
const password = ref('')
const fullName = ref('')
const loading = ref(false)
const error = ref('')
const success = ref(false)

const closeModal = () => {
  emit('update:modelValue', false)
  setTimeout(() => {
    username.value = ''
    email.value = ''
    password.value = ''
    fullName.value = ''
    error.value = ''
    success.value = false
  }, 300)
}

const handleRegister = async () => {
  if (!username.value || !email.value || !password.value) {
    error.value = 'Please fill in all required fields'
    return
  }

  loading.value = true
  error.value = ''

  try {
    const result = await auth.register({
      username: username.value,
      email: email.value,
      password: password.value,
      fullName: fullName.value
    })

    if (result.success) {
      success.value = true
      setTimeout(() => {
        closeModal()
        emit('switchToLogin')
      }, 2000)
    } else {
      error.value = result.message || 'Registration failed'
    }
  } catch (err: any) {
    error.value = err.message || 'An error occurred'
  } finally {
    loading.value = false
  }
}

// Close on Escape
watch(() => props.modelValue, (value) => {
  if (value) {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeModal()
    }
    document.addEventListener('keydown', handleEscape)
    return () => document.removeEventListener('keydown', handleEscape)
  }
})
</script>

<template>
  <!-- Backdrop -->
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
      @click="closeModal"
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
      @click.self="closeModal"
    >
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <!-- Header -->
          <div class="flex items-center justify-between mb-6">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
              Create your account
            </h2>
            <button
              type="button"
              @click="closeModal"
              class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
            >
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- Form -->
          <form @submit.prevent="handleRegister" class="space-y-4">
            <!-- Success Message -->
            <div
              v-if="success"
              class="p-3 rounded-md bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800"
            >
              <div class="flex items-start gap-2">
                <svg class="w-5 h-5 text-green-600 dark:text-green-400 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                </svg>
                <p class="text-sm text-green-800 dark:text-green-300">Registration successful! Redirecting to login...</p>
              </div>
            </div>

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

            <!-- Username Input -->
            <div class="space-y-2">
              <label for="username" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Username <span class="text-red-500">*</span>
              </label>
              <input
                id="username"
                v-model="username"
                type="text"
                placeholder="Enter your username"
                :disabled="loading || success"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Email Input -->
            <div class="space-y-2">
              <label for="email" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Email <span class="text-red-500">*</span>
              </label>
              <input
                id="email"
                v-model="email"
                type="email"
                placeholder="Enter your email"
                :disabled="loading || success"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Full Name Input -->
            <div class="space-y-2">
              <label for="fullName" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Full Name
              </label>
              <input
                id="fullName"
                v-model="fullName"
                type="text"
                placeholder="Enter your full name"
                :disabled="loading || success"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Password Input -->
            <div class="space-y-2">
              <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                Password <span class="text-red-500">*</span>
              </label>
              <input
                id="password"
                v-model="password"
                type="password"
                placeholder="Enter your password"
                :disabled="loading || success"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white disabled:opacity-50 disabled:cursor-not-allowed"
              />
            </div>

            <!-- Submit Button -->
            <button
              type="submit"
              :disabled="loading || success"
              class="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              <svg v-if="loading" class="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              {{ loading ? 'Creating account...' : 'Sign up' }}
            </button>

            <!-- Login Link -->
            <div class="text-center text-sm text-gray-600 dark:text-gray-400">
              Already have an account?
              <button
                type="button"
                @click="emit('switchToLogin')"
                class="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 font-medium"
              >
                Sign in
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </Transition>
</template>
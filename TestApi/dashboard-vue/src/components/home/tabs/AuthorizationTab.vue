<script setup lang="ts">
import { ref } from 'vue'

const authType = ref('none')
const bearerToken = ref('')
const username = ref('')
const password = ref('')

defineExpose({
  getAuth: () => {
    if (authType.value === 'none') return null
    
    if (authType.value === 'bearer') {
      return {
        type: 'bearer',
        token: bearerToken.value
      }
    }
    
    if (authType.value === 'basic') {
      return {
        type: 'basic',
        username: username.value,
        password: password.value
      }
    }
    
    return null
  }
})
</script>

<template>
  <div class="space-y-4">
    <div>
      <label class="text-sm font-medium text-gray-700 mb-2 block">Authentication Type</label>
      <select 
        v-model="authType"
        class="px-3 py-2 text-sm border border-gray-300 rounded-md w-full focus:outline-none focus:ring-2 focus:ring-blue-500"
      >
        <option value="none">No Auth</option>
        <option value="bearer">Bearer Token</option>
        <option value="basic">Basic Auth</option>
      </select>
    </div>

    <!-- No Auth -->
    <div v-if="authType === 'none'" class="text-center py-8 text-gray-400">
      <svg class="w-12 h-12 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
      </svg>
      <p class="text-sm">This request does not use any authorization</p>
    </div>

    <!-- Bearer Token -->
    <div v-if="authType === 'bearer'" class="space-y-2">
      <label class="text-sm font-medium text-gray-700">Token</label>
      <textarea
        v-model="bearerToken"
        rows="3"
        placeholder="Enter bearer token (e.g., eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...)"
        class="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono"
      />
      <p class="text-xs text-gray-500">Token will be sent as: Authorization: Bearer {token}</p>
    </div>

    <!-- Basic Auth -->
    <div v-if="authType === 'basic'" class="space-y-3">
      <div>
        <label class="text-sm font-medium text-gray-700 mb-1 block">Username</label>
        <input 
          v-model="username"
          type="text"
          placeholder="Enter username"
          class="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
      <div>
        <label class="text-sm font-medium text-gray-700 mb-1 block">Password</label>
        <input 
          v-model="password"
          type="password"
          placeholder="Enter password"
          class="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
      <p class="text-xs text-gray-500">Credentials will be Base64 encoded and sent as: Authorization: Basic {encoded}</p>
    </div>
  </div>
</template>
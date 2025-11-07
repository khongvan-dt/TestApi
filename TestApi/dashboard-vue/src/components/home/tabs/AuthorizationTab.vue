<script setup lang="ts">
import { ref } from 'vue'

 interface Props {
  initialAuthType?: string
  initialBearerToken?: string
}

const props = withDefaults(defineProps<Props>(), {
  initialAuthType: 'bearer-token',
  initialBearerToken: ''
})

 const authType = ref(props.initialAuthType)
const bearerToken = ref(props.initialBearerToken)

const authTypes = [
  { value: 'no-auth', label: 'No Auth' },
  { value: 'bearer-token', label: 'Bearer Token' }
]

function getAuthData() {

  
  return {
    authType: authType.value,
    bearerToken: bearerToken.value
  }
}

 function updateAuthData(newAuthType: string, newBearerToken: string) {
  authType.value = newAuthType || 'bearer-token'
  bearerToken.value = newBearerToken || ''
}

defineExpose({
  getAuth: () => {
    switch (authType.value) {
      case 'bearer-token':
        return { type: 'bearer', token: bearerToken.value }
      default:
        return null
    }
  },
  getAuthData,      
  updateAuthData
})
</script>

<template>
  <div  >
    <!-- Luôn hiển thị hàng Type + nội dung tương ứng -->
    <div class="flex items-start gap-4">
      <!-- Type select -->
      <div class="w-1/5">
        <label class="block text-sm font-medium text-gray-700 mb-2">Type</label>
        <select
          v-model="authType"
          class="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
        >
          <option
            v-for="type in authTypes"
            :key="type.value"
            :value="type.value"
          >
            {{ type.label }}
          </option>
        </select>
      </div>

      <!-- Nội dung bên phải -->
      <div class="flex-1">
        <!-- No Auth -->
        <div v-if="authType === 'no-auth'" class="flex flex-col items-center justify-center h-full text-gray-500 border border-dashed border-gray-300 rounded-md py-8">
          <svg
            class="w-12 h-12 mb-2 text-gray-300"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
            />
          </svg>
          <p class="text-sm">This request does not use any authorization.</p>
        </div>

        <!-- Bearer Token -->
        <div v-else-if="authType === 'bearer-token'">
          <label class="block text-sm font-medium text-gray-700 mb-2">Token</label>
          <input
            v-model="bearerToken"
            type="text"
            placeholder="Enter your bearer token"
            class="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          
        </div>
      </div>
    </div>
  </div>
</template>
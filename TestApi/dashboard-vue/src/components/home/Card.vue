<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import ParamsTab from './tabs/ParamsTab.vue'
import AuthorizationTab from './tabs/AuthorizationTab.vue'
import HeadersTab from './tabs/HeadersTab.vue'
import BodyTab from './tabs/BodyTab.vue'

interface Props {
  title?: string
  defaultMethod?: string
  defaultUrl?: string
  defaultBody?: string
}

const props = withDefaults(defineProps<Props>(), {
  title: 'API Request',
  defaultMethod: 'POST',
  defaultUrl: '',
  defaultBody: '{}'
})

const url = ref(props.defaultUrl)
const method = ref(props.defaultMethod)
const body = ref(props.defaultBody)
const response = ref('')
const loading = ref(false)
const activeTab = ref('Body')

const tabs = ['Params', 'Authorization', 'Headers', 'Body']

// Refs to tab components
const paramsTabRef = ref()
const authTabRef = ref()
const headersTabRef = ref()
const bodyTabRef = ref()

// Watch URL changes
watch(() => props.defaultUrl, (newUrl) => {
  console.log('🔗 Card received new URL:', newUrl)
  url.value = newUrl
})

// Watch Method changes
watch(() => props.defaultMethod, (newMethod) => {
  console.log('⚙️ Card received new method:', newMethod)
  method.value = newMethod
})

// Watch Body changes
watch(() => props.defaultBody, (newBody) => {
  console.log('📦 Card received new body:', newBody)
  body.value = newBody
  
  nextTick(() => {
    if (bodyTabRef.value?.updateBody) {
      console.log('🔄 Updating BodyTab...')
      bodyTabRef.value.updateBody(newBody)
    }
  })
}, { immediate: true })

const handleSend = () => {
  const params = paramsTabRef.value?.getParams() || []
  const headers = headersTabRef.value?.getHeaders() || []
  const auth = authTabRef.value?.getAuth() || null
  const requestBody = bodyTabRef.value?.getBody() || body.value

  console.log('Sending request:', { url: url.value, method: method.value, body: requestBody })
}

// Expose methods
defineExpose({
  setActiveTab: (tab: string) => {
    activeTab.value = tab
  },
  focusBody: () => {
    nextTick(() => {
      bodyTabRef.value?.focus?.()
    })
  }
})
</script>

<template>
  <div class="h-full flex flex-col bg-white">
    <!-- Tabs -->
    <div class="px-4 border-b border-gray-200 bg-white flex-shrink-0">
      <div class="flex gap-6 -mb-px">
        <button 
          v-for="tab in tabs" 
          :key="tab" 
          @click="activeTab = tab" 
          :class="[
            'px-1 py-3 text-sm font-medium border-b-2 transition-colors',
            activeTab === tab
              ? 'border-blue-600 text-blue-600'
              : 'text-gray-500 border-transparent hover:text-gray-700 hover:border-gray-300'
          ]"
        >
          {{ tab }}
        </button>
      </div>
    </div>

    <!-- Request Section -->
    <div class="p-4 bg-white flex-shrink-0 border-b border-gray-200">
      <!-- Method & URL Bar -->
      <div class="flex items-stretch gap-2 mb-4">
        <select 
          v-model="method"
          class="px-3 py-2 text-sm font-semibold border border-gray-300 rounded-md bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
        >
          <option value="GET">GET</option>
          <option value="POST">POST</option>
          <option value="PUT">PUT</option>
          <option value="PATCH">PATCH</option>
          <option value="DELETE">DELETE</option>
        </select>

        <input 
          v-model="url" 
          type="text" 
          placeholder="Enter request URL"
          class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" 
        />

        <button 
          @click="handleSend" 
          :disabled="loading || !url" 
          :class="[
            'px-6 py-2 text-sm font-semibold rounded-md transition-all',
            loading || !url
              ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
              : 'bg-blue-600 text-white hover:bg-blue-700'
          ]"
        >
          {{ loading ? 'Sending...' : 'Send' }}
        </button>
      </div>

      <!-- Tab Content -->
      <div class="max-h-[400px] overflow-y-auto">
        <ParamsTab v-if="activeTab === 'Params'" ref="paramsTabRef" />
        <AuthorizationTab v-else-if="activeTab === 'Authorization'" ref="authTabRef" />
        <HeadersTab v-else-if="activeTab === 'Headers'" ref="headersTabRef" />
        <BodyTab 
          v-else-if="activeTab === 'Body'" 
          ref="bodyTabRef" 
          :modelValue="body"
        />
      </div>
    </div>

    <!-- Response Section -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <div class="px-4 py-2.5 flex items-center justify-between bg-gray-50 border-b border-gray-200 flex-shrink-0">
        <h3 class="text-sm font-semibold text-gray-700">Response</h3>
      </div>

      <div class="flex-1 overflow-y-auto p-4">
        <div v-if="!response && !loading" class="text-center py-12 text-gray-400">
          <p class="text-sm">Click Send to get a response</p>
        </div>
        <div v-else-if="loading" class="text-center py-12">
          <p class="text-sm text-gray-600">Sending request...</p>
        </div>
        <pre 
          v-else
          class="bg-white border border-gray-200 rounded-lg p-4 text-xs font-mono whitespace-pre-wrap break-words"
        >{{ response }}</pre>
      </div>
    </div>
  </div>
</template>
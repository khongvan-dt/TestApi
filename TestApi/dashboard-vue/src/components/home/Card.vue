<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import ParamsTab from './tabs/ParamsTab.vue'
import AuthorizationTab from './tabs/AuthorizationTab.vue'
import HeadersTab from './tabs/HeadersTab.vue'
import BodyTab from './tabs/BodyTab.vue'
import SaveRequestModal from '../home/popup/SaveRequestModal.vue'
import { useApiClient } from '../../composables/useApiClient'
import { createExecutionHistory } from '../../composables/useExecutionHistory'

interface Props {
  title?: string
  defaultMethod?: string
  defaultUrl?: string
  defaultBody?: string
  requestId?: number | null
  dataBaseTest?: string | null
  bodyId?: number
}

const props = withDefaults(defineProps<Props>(), {
  title: 'API Request',
  defaultMethod: 'POST',
  defaultUrl: '',
  defaultBody: '{}',
  requestId: null,
  dataBaseTest: null,
  bodyId: 0
})

const emit = defineEmits<{
  (e: 'update:url', value: string): void
  (e: 'update:method', value: string): void
  (e: 'update:body', value: string): void
  (e: 'stateChange', state: any): void
  (e: 'requestSaved', requestId: number): void
}>()

// State
const url = ref('')
const method = ref('POST')
const body = ref('{}')
const currentRequestId = ref<number | null>(null)
const response = ref('')
const loading = ref(false)
const activeTab = ref('Body')
const responseStatus = ref<number | null>(null)
const responseDuration = ref<number | null>(null)
const responseSize = ref<number | null>(null)
const showSaveModal = ref(false)
const requestHeight = ref(400)
const isResizing = ref(false)
const bodyKey = ref(0)
const isInternalUpdate = ref(false)

// Refs
const bodyTabRef = ref<any>(null)
const paramsTabRef = ref<any>(null)
const authTabRef = ref<any>(null)
const headersTabRef = ref<any>(null)
const containerRef = ref<HTMLDivElement>()

// Composables
const { sendRequest } = useApiClient()

// Constants
const tabs = ['Params', 'Authorization', 'Headers', 'Body']

// Watch props with internal update flag
 

watch(() => props.defaultUrl, (newUrl) => {
  if (newUrl === url.value) return
  isInternalUpdate.value = true
  url.value = newUrl || ''
  nextTick(() => { isInternalUpdate.value = false })
}, { immediate: true })

watch(() => props.defaultMethod, (newMethod) => {
  if (newMethod === method.value) return
  isInternalUpdate.value = true
  method.value = newMethod || 'POST'
  nextTick(() => { isInternalUpdate.value = false })
}, { immediate: true })

watch(() => props.defaultBody, (newBody) => {
  if (newBody === body.value) return
  isInternalUpdate.value = true
  body.value = newBody || '{}'
  bodyKey.value++
  nextTick(() => {
    bodyTabRef.value?.updateBody?.(newBody || '{}')
    isInternalUpdate.value = false
  })
}, { immediate: true })

watch(() => props.dataBaseTest, (val) => {
  nextTick(() => {
    bodyTabRef.value?.setDataBaseTest?.(val)
  })
}, { immediate: true })

watch(() => props.requestId, (newId) => {
  currentRequestId.value = newId
})

// Watch state changes
watch(url, (newVal) => { 
  if (!isInternalUpdate.value) {
    emit('update:url', newVal)
    emitStateChange()
  }
})

watch(method, (newVal) => { 
  if (!isInternalUpdate.value) {
    emit('update:method', newVal)
    emitStateChange()
  }
})

watch(body, (newVal) => { 
  if (!isInternalUpdate.value) {
    emit('update:body', newVal)
    emitStateChange()
  }
})

function emitStateChange() {
  emit('stateChange', {
    url: url.value,
    method: method.value,
    body: body.value,
    activeTab: activeTab.value
  })
}

// Resize handlers
function startResize(e: MouseEvent) {
  isResizing.value = true
  document.addEventListener('mousemove', handleResize)
  document.addEventListener('mouseup', stopResize)
  e.preventDefault()
}

function handleResize(e: MouseEvent) {
  if (!isResizing.value || !containerRef.value) return
  
  const containerRect = containerRef.value.getBoundingClientRect()
  const newHeight = e.clientY - containerRect.top
  const minHeight = 200
  const maxHeight = containerRect.height * 0.8
  
  requestHeight.value = Math.max(minHeight, Math.min(newHeight, maxHeight))
}

function stopResize() {
  isResizing.value = false
  document.removeEventListener('mousemove', handleResize)
  document.removeEventListener('mouseup', stopResize)
}

// Get request data
watch(() => props.bodyId, (newId) => {
  console.log('🟠 [Card.vue] Received bodyId prop:', newId)
  
  nextTick(() => {
    if (bodyTabRef.value?.setBodyId) {
      bodyTabRef.value.setBodyId(newId || 0)
      console.log('🟠 [Card.vue] Called setBodyId on BodyTab:', newId)
    } else {
      console.log('❌ [Card.vue] bodyTabRef not ready')
    }
  })
}, { immediate: true })

// Get request data
function getRequestData() {
  const params = paramsTabRef.value?.getParams?.() || []
  const headers = headersTabRef.value?.getHeaders?.() || []
  const auth = authTabRef.value?.getAuthData?.() || null

  let bodyData: any = null
  if (bodyTabRef.value) {
    const result = bodyTabRef.value.getBody?.()
    const bodyType = bodyTabRef.value.getBodyType?.() || 'none'

    console.log('🟠 [Card.vue] getRequestData - raw body result:', result)
    console.log('🟠 [Card.vue] getRequestData - bodyType:', bodyType)

    if (result && typeof result === 'object' && 'bodyType' in result && 'content' in result) {
      bodyData = result
      console.log('🟠 [Card.vue] getRequestData - using result directly:', bodyData)
    } else if (bodyType !== 'none') {
      bodyData = { 
        id: 0,
        bodyType, 
        content: result || '' 
      }
      console.log('🟠 [Card.vue] getRequestData - created new body:', bodyData)
    }
  }

  console.log('🟠 [Card.vue] getRequestData - final bodyData:', bodyData)

  return {
    url: url.value,
    method: method.value,
    body: bodyData,
    params,
    headers,
    auth,
    dataBaseTest: bodyTabRef.value?.getDataBaseTest?.() || null
  }
}
// Load request data
function loadRequestData(requestData: any) {
  url.value = requestData.url
  method.value = requestData.method
  
  if (requestData.body) {
    body.value = requestData.body.content || '{}'
    nextTick(() => {
      bodyTabRef.value?.setBodyId?.(requestData.body.id || 0)
    })
  }
}

// Format request body
function formatRequestBody() {
  let requestBody: any = null
  
  if (bodyTabRef.value) {
    const rawBody = bodyTabRef.value.getBody?.()
    const bodyType = bodyTabRef.value.getBodyType?.() || 'none'

    if (bodyType !== 'none' && rawBody != null) {
      requestBody = typeof rawBody === 'object' && 'bodyType' in rawBody && 'content' in rawBody
        ? rawBody
        : { bodyType, content: typeof rawBody === 'string' ? rawBody : JSON.stringify(rawBody) }
    }
  }
  
  return requestBody
}

// Build headers with auth
function buildHeaders() {
  const headers = headersTabRef.value?.getHeaders() || []
  const auth = authTabRef.value?.getAuth?.() || null

  if (auth?.type && auth?.token) {
    headers.push({
      key: 'Authorization',
      value: `${auth.type.charAt(0).toUpperCase() + auth.type.slice(1)} ${auth.token}`,
      enabled: true
    })
  }

  return headers
    .filter((h: any) => h.enabled !== false && h.key)
    .map((h: any) => ({ key: h.key, value: h.value }))
}

// Save history
async function saveHistory(result: any, requestBody: any, params: any[], headers: any[]) {
  const historyPayload = {
    requestId: currentRequestId.value,
    collectionId: 1,
    name: props.title || 'New Request',
    method: method.value,
    url: url.value,
    queryParams: JSON.stringify(params),
    headers: JSON.stringify(headers),
    body: JSON.stringify(requestBody || ''),
    statusCode: result.status ?? 0,
    statusText: result.statusText ?? '',
    responseHeaders: JSON.stringify(result.headers || {}),
    responseBody: result.success ? JSON.stringify(result.data) : '',
    responseTime: result.duration ?? 0,
    errorMessage: result.success ? '' : result.error || '',
    executedAt: new Date().toISOString(),
    userId: 0
  }

  try {
    await createExecutionHistory(historyPayload)
  } catch (err) {
    console.error('Save history failed', err)
  }
}

// Send request
async function handleSend() {
  if (!url.value) {
    alert('Please enter a URL')
    return
  }

  loading.value = true
  response.value = ''
  responseStatus.value = null
  responseDuration.value = null
  responseSize.value = null

  try {
    const params = paramsTabRef.value?.getParams() || []
    const headers = buildHeaders()
    const requestBody = formatRequestBody()

    const requestPayload = {
      requestId: currentRequestId.value,
      collectionId: 1,
      name: props.title ?? 'Untitled',
      method: method.value,
      url: url.value,
      queryParams: params.filter((p: any) => p.enabled !== false && p.key),
      headers,
      body: requestBody
    }

    const actualBody = requestPayload.body?.bodyType === 'raw'
      ? requestPayload.body.content
      : requestPayload.body

    const result = await sendRequest({ ...requestPayload, body: actualBody })

    responseStatus.value = result.status ?? null
    responseDuration.value = result.duration ?? null
    responseSize.value = result.size ?? null
    
    response.value = result.success
      ? JSON.stringify(result.data, null, 2)
      : JSON.stringify({
        error: result.error || 'Request failed',
        status: result.status,
        statusText: result.statusText,
        data: result.data
      }, null, 2)

    await saveHistory(result, requestBody, params, headers)
  } catch (error: any) {
    response.value = JSON.stringify({ error: error.message || 'Unknown error occurred' }, null, 2)
  } finally {
    loading.value = false
  }
}

function handleOpenSaveModal() {
  showSaveModal.value = true
}

function handleCloseSaveModal() {
  showSaveModal.value = false
}

function handleRequestSaved(requestId: number) {
  emit('requestSaved', requestId)
}

function clearResponse() {
  response.value = ''
  responseStatus.value = null
  responseDuration.value = null
  responseSize.value = null
}

// Expose methods
defineExpose({
  setActiveTab: (tab: string) => { activeTab.value = tab },
  focusBody: () => { nextTick(() => { bodyTabRef.value?.focus?.() }) },
  clearResponse,
  getState: () => ({ 
    url: url.value, 
    method: method.value, 
    body: body.value, 
    activeTab: activeTab.value 
  }),
  getRequestData,
  loadRequestData,
  get $refs() {
    return {
      paramsTabRef: paramsTabRef.value,
      headersTabRef: headersTabRef.value,
      authTabRef: authTabRef.value,
      bodyTabRef: bodyTabRef.value
    }
  }
})
</script>

<template>
  <div ref="containerRef" class="h-full flex flex-col bg-white">
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
          ]">
          {{ tab }}
        </button>
      </div>
    </div>

    <!-- Request Section - Resizable -->
    <div 
      class="bg-white flex-shrink-0 border-b border-gray-200 overflow-hidden flex flex-col"
      :style="{ height: `${requestHeight}px` }">
      <div class="p-4 flex-shrink-0">
        <!-- Method & URL Bar -->
        <div class="flex items-stretch gap-2 mb-4">
          <select 
            v-model="method"
            class="px-3 py-2 text-sm font-semibold border border-gray-300 rounded-md bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer">
            <option value="GET">GET</option>
            <option value="POST">POST</option>
            <option value="PUT">PUT</option>
            <option value="PATCH">PATCH</option>
            <option value="DELETE">DELETE</option>
          </select>

          <!-- ✅ BỎ @input="emitStateChange" -->
          <input 
            v-model="url"
            type="text"
            placeholder="Enter request URL (e.g., https://api.example.com/users)"
            class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" />

          <button 
            @click="handleOpenSaveModal"
            class="px-4 py-2 text-sm font-semibold bg-green-50 text-green-600 border border-green-300 rounded-md hover:bg-green-100 transition-colors flex items-center gap-2"
            title="Save request">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" />
            </svg>
            <span class="hidden sm:inline">Save</span>
          </button>

          <button 
            @click="handleSend" 
            :disabled="loading || !url" 
            :class="[
              'px-6 py-2 text-sm font-semibold rounded-md transition-all',
              loading || !url
                ? 'bg-gray-300 text-gray-500 cursor-not-allowed'
                : 'bg-blue-600 text-white hover:bg-blue-700 active:bg-blue-800'
            ]">
            {{ loading ? 'Sending...' : 'Send' }}
          </button>
        </div>
      </div>

      <!-- Tab Content -->
      <div class="flex-1 overflow-y-auto px-4 pb-4">
        <ParamsTab 
          v-show="activeTab === 'Params'" 
          ref="paramsTabRef" 
          :paramsData="getRequestData()?.params || []" />

        <HeadersTab 
          v-show="activeTab === 'Headers'" 
          ref="headersTabRef"
          :headersData="getRequestData()?.headers || []" />

        <AuthorizationTab 
          v-show="activeTab === 'Authorization'" 
          ref="authTabRef" />

        <BodyTab 
          v-show="activeTab === 'Body'" 
          ref="bodyTabRef" 
          :key="bodyKey" 
          :modelValue="body"
          :bodyId="props.bodyId"  
          :dataBaseTest="props.dataBaseTest" 
          :requestId="currentRequestId" />
      </div>
    </div>

    <!-- Resize Handle -->
    <div 
      @mousedown="startResize" 
      :class="[
        'h-1 bg-gray-200 hover:bg-blue-400 cursor-row-resize transition-colors flex-shrink-0 relative group',
        isResizing ? 'bg-blue-500' : ''
      ]">
      <div class="absolute inset-x-0 -top-1 -bottom-1 flex items-center justify-center">
        <div class="w-12 h-1 rounded-full bg-gray-400 group-hover:bg-blue-500 transition-colors"></div>
      </div>
    </div>

    <!-- Response Section -->
    <div class="flex-1 flex flex-col overflow-hidden min-h-0">
      <div class="px-4 py-2.5 flex items-center justify-between bg-gray-50 border-b border-gray-200 flex-shrink-0">
        <h3 class="text-sm font-semibold text-gray-700">Response</h3>

        <!-- Response Stats -->
        <div v-if="responseStatus !== null" class="flex items-center gap-4 text-xs">
          <span :class="[
            'font-semibold px-2 py-1 rounded',
            responseStatus >= 200 && responseStatus < 300
              ? 'text-green-700 bg-green-50'
              : responseStatus >= 400 && responseStatus < 500
                ? 'text-orange-700 bg-orange-50'
                : 'text-red-700 bg-red-50'
          ]">
            {{ responseStatus }} {{ responseStatus >= 200 && responseStatus < 300 ? '✓' : '✗' }}
          </span>
          <span class="text-gray-600 flex items-center gap-1">
            <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {{ responseDuration }}ms
          </span>
          <span class="text-gray-600 flex items-center gap-1">
            <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
            {{ (responseSize || 0) }} B
          </span>
        </div>
      </div>

      <div class="flex-1 overflow-y-auto p-4 bg-gray-50">
        <!-- Empty State -->
        <div v-if="!response && !loading" class="text-center py-12 text-gray-400">
          <div class="mb-4">
            <svg class="w-16 h-16 mx-auto text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M8 9l3 3-3 3m5 0h3M5 20h14a2 2 0 002-2V6a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          <p class="text-sm font-medium">Click Send to get a response</p>
          <p class="text-xs mt-1 text-gray-400">Enter a URL and configure your request above</p>
        </div>

        <!-- Loading State -->
        <div v-else-if="loading" class="text-center py-12">
          <div class="mb-4">
            <svg class="animate-spin w-12 h-12 mx-auto text-blue-600" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
              </path>
            </svg>
          </div>
          <p class="text-sm text-gray-600 font-medium">Sending request...</p>
          <p class="text-xs text-gray-500 mt-1">Please wait</p>
        </div>

        <!-- Response Content -->
        <pre v-else
          class="bg-white border border-gray-200 rounded-lg p-4 text-xs font-mono whitespace-pre-wrap break-words shadow-sm">{{ response }}</pre>
      </div>
    </div>

    <SaveRequestModal 
      v-if="showSaveModal" 
      :current-url="url" 
      :current-method="method" 
      :current-body="body"
      :request-id="requestId" 
      :request-name="title" 
      :card-ref="{ getRequestData }" 
      @close="handleCloseSaveModal" 
      @saved="handleRequestSaved" />
  </div>
</template>
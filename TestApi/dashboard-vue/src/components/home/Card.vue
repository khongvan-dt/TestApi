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
}

const props = withDefaults(defineProps<Props>(), {
  title: 'API Request',
  defaultMethod: 'POST',
  defaultUrl: '',
  defaultBody: '{}',
  requestId: null
})

const emit = defineEmits<{
  (e: 'update:url', value: string): void
  (e: 'update:method', value: string): void
  (e: 'update:body', value: string): void
  (e: 'stateChange', state: any): void
  (e: 'requestSaved', requestId: number): void
}>()

const url = ref(props.defaultUrl)
const method = ref(props.defaultMethod)
const body = ref(props.defaultBody)
const response = ref('')
const loading = ref(false)
const activeTab = ref('Body')
const responseStatus = ref<number | null>(null)
const responseDuration = ref<number | null>(null)
const responseSize = ref<number | null>(null)

const showSaveModal = ref(false)

const tabs = ['Params', 'Authorization', 'Headers', 'Body']

// Refs to tab components
const paramsTabRef = ref()
const authTabRef = ref()
const headersTabRef = ref()
const bodyTabRef = ref()

// Resizable
const requestHeight = ref(400)
const isResizing = ref(false)
const containerRef = ref<HTMLDivElement>()

const bodyKey = ref(0)

const { sendRequest } = useApiClient()

// Watch changes và emit lên parent
watch(url, (newVal) => {
  emit('update:url', newVal)
  emitStateChange()
})

watch(method, (newVal) => {
  emit('update:method', newVal)
  emitStateChange()
})

watch(body, (newVal) => {
  emit('update:body', newVal)
  emitStateChange()
})

const emitStateChange = () => {
  emit('stateChange', {
    url: url.value,
    method: method.value,
    body: body.value,
    activeTab: activeTab.value
  })
}

watch(() => props.defaultUrl, (newUrl) => {
  url.value = newUrl
})

watch(() => props.defaultMethod, (newMethod) => {
  method.value = newMethod
})

watch(() => props.defaultBody, (newBody) => {
  body.value = newBody
  bodyKey.value++

  nextTick(() => {
    if (bodyTabRef.value?.updateBody) {
      bodyTabRef.value.updateBody(newBody)
    }
  })
}, { immediate: true })

const startResize = (e: MouseEvent) => {
  isResizing.value = true
  document.addEventListener('mousemove', handleResize)
  document.addEventListener('mouseup', stopResize)
  e.preventDefault()
}

const handleResize = (e: MouseEvent) => {
  if (!isResizing.value || !containerRef.value) return

  const containerRect = containerRef.value.getBoundingClientRect()
  const newHeight = e.clientY - containerRect.top

  const minHeight = 200
  const maxHeight = containerRect.height * 0.8

  requestHeight.value = Math.max(minHeight, Math.min(newHeight, maxHeight))
}

const stopResize = () => {
  isResizing.value = false
  document.removeEventListener('mousemove', handleResize)
  document.removeEventListener('mouseup', stopResize)
}

// const handleSend = async () => {
//   if (!url.value) {
//     alert('Please enter a URL')
//     return
//   }

//   loading.value = true
//   response.value = ''
//   responseStatus.value = null
//   responseDuration.value = null
//   responseSize.value = null

//   try {
//     // Lấy dữ liệu từ các tab
//     const params = paramsTabRef.value?.getParams() || []
//     const headers = headersTabRef.value?.getHeaders() || []
//     const auth = authTabRef.value?.getAuth() || null

//     let requestBody: any = null
//     if (bodyTabRef.value) {
//       const bodyType = bodyTabRef.value.getBodyType?.() || 'raw'
//       const content = bodyTabRef.value.getBody() || ''

//       if (bodyType === 'raw') {
//         requestBody = content
//         // Thêm Content-Type nếu chưa có
//         if (!headers.some((h: any) => h.key.toLowerCase() === 'content-type')) {
//           headers.push({ key: 'Content-Type', value: 'application/json', enabled: true })
//         }
//       } else if (bodyType === 'x-www-form-urlencoded') {
//         const urlParams = content || []
//         requestBody = urlParams
//           .filter((p: any) => p.enabled !== false && p.key)
//           .map((p: any) => `${encodeURIComponent(p.key)}=${encodeURIComponent(p.value || '')}`)
//           .join('&')
//         if (!headers.some((h: any) => h.key.toLowerCase() === 'content-type')) {
//           headers.push({ key: 'Content-Type', value: 'application/x-www-form-urlencoded', enabled: true })
//         }
//       } else if (bodyType === 'form-data') {
//         requestBody = content || []
//       }
//     }

//     // Thêm Authorization header nếu auth có
//     if (auth && auth.type && auth.token) {
//       headers.push({ key: 'Authorization', value: `${auth.type} ${auth.token}`, enabled: true })
//     }

//     interface Header {
//       key: string
//       value: string
//     }

//     // Format headers gửi request
//     const formattedHeaders: Header[] = headers.map((h: any) => ({ key: h.key, value: h.value }))

//     // Payload gửi request
//     const requestPayload = {
//       method: method.value,
//       url: url.value,
//       queryParams: params,
//       headers: formattedHeaders,
//       body: requestBody
//     }

//     // Gửi request
//     const result = await sendRequest(requestPayload)

//     // Hiển thị response
//     responseStatus.value = result.status
//     responseDuration.value = result.duration
//     responseSize.value = result.size
//     response.value = result.success
//       ? JSON.stringify(result.data, null, 2)
//       : JSON.stringify({
//         error: result.error || 'Request failed',
//         status: result.status,
//         statusText: result.statusText,
//         data: result.data
//       }, null, 2)

//     // Payload lưu history
//     const historyPayload = {
//       requestId: props.requestId ?? 0,
//       collectionId: 1,
//       name: props.title || 'New Request',
//       method: method.value,
//       url: url.value,
//       queryParams: JSON.stringify(params),
//       headers: JSON.stringify(formattedHeaders),
//       body: JSON.stringify(requestBody || ''),
//       statusCode: result.status ?? 0,
//       statusText: result.statusText ?? '',
//       responseHeaders: JSON.stringify(result.headers || {}),
//       responseBody: result.success ? JSON.stringify(result.data) : '',
//       responseTime: result.duration ?? 0,
//       errorMessage: result.success ? '' : result.error || '',
//       executedAt: new Date().toISOString(),
//       userId: 0
//     }

//     await createExecutionHistory(historyPayload)

//   } catch (error: any) {
//     response.value = JSON.stringify({
//       error: error.message || 'Unknown error occurred'
//     }, null, 2)
//   } finally {
//     loading.value = false
//   }
// }

const handleSend = async () => {
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
    // 1️⃣ Lấy dữ liệu từ các tab
    const params = paramsTabRef.value?.getParams() || []
    const headers = headersTabRef.value?.getHeaders() || []
    const auth = authTabRef.value?.getAuth() || null

    // 2️⃣ Xử lý body
    let requestBody: any = null
    if (bodyTabRef.value) {
      const bodyType = bodyTabRef.value.getBodyType?.() || 'raw'
      const content = bodyTabRef.value.getBody() || ''

      if (bodyType === 'raw') {
        requestBody = content
        if (!headers.some((h: any) => h.key.toLowerCase() === 'content-type')) {
          headers.push({ key: 'Content-Type', value: 'application/json', enabled: true })
        }
      } else if (bodyType === 'x-www-form-urlencoded') {
        const urlParams = content || []
        requestBody = urlParams
          .filter((p: any) => p.enabled !== false && p.key)
          .map((p: any) => `${encodeURIComponent(p.key)}=${encodeURIComponent(p.value || '')}`)
          .join('&')
        if (!headers.some((h: any) => h.key.toLowerCase() === 'content-type')) {
          headers.push({ key: 'Content-Type', value: 'application/x-www-form-urlencoded', enabled: true })
        }
      } else if (bodyType === 'form-data') {
        requestBody = content || []
      }
    }

    // 3️⃣ Thêm Authorization header nếu có
    if (auth && auth.type && auth.token) {
      headers.push({ key: 'Authorization', value: `${auth.type} ${auth.token}`, enabled: true })
    }

    interface Header { key: string; value: string }
    const formattedHeaders: Header[] = headers.map((h: any) => ({ key: h.key, value: h.value }))

    // 4️⃣ Payload gửi request
    const requestPayload = {
      method: method.value,
      url: url.value,
      queryParams: params,
      headers: formattedHeaders,
      body: requestBody
    }

    // 5️⃣ Gửi request thật và hiển thị response ngay
    const result = await sendRequest(requestPayload)

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

    // 6️⃣ Lưu lịch sử (không ảnh hưởng response)
    const historyPayload = {
      requestId: props.requestId ?? null,
      method: method.value,
      url: url.value,
      queryParams: JSON.stringify(params),
      headers: JSON.stringify(formattedHeaders),
      body: JSON.stringify(requestBody || ''),
      statusCode: result.status ?? 0,
      statusText: result.statusText ?? '',
      responseHeaders: JSON.stringify(result.headers || {}),
      responseBody: result.success ? JSON.stringify(result.data) : '',
      responseTime: result.duration ?? 0,
      errorMessage: result.success ? '' : result.error || '',
      executedAt: new Date().toISOString(),
    }

    // Chạy bất đồng bộ, không await để không block hiển thị
    createExecutionHistory(historyPayload).catch(err => console.error('Save history failed', err))

  } catch (error: any) {
    response.value = JSON.stringify({
      error: error.message || 'Unknown error occurred'
    }, null, 2)
  } finally {
    loading.value = false
  }
}


interface Header {
  key: string
  value: string
}
const headers: Header[] = headersTabRef.value?.getHeaders() || []

const getRequestData = () => {
  const params = paramsTabRef.value?.getParams() || []
  const headers = headersTabRef.value?.getHeaders() || []
  const auth = authTabRef.value?.getAuth() || null

  // Body
  let requestBody: any = null
  if (bodyTabRef.value) {
    const bodyType = bodyTabRef.value.getBodyType?.() || 'raw'
    const content = bodyTabRef.value.getBody?.() || ''

    requestBody = {
      bodyType,
      content
    }
  }

  const formattedHeaders: Header[] = headers.map((h: Header) => ({
    key: h.key,
    value: h.value
  }))

  if (requestBody?.bodyType === 'raw' && !formattedHeaders.some((h: Header) => h.key === 'Content-Type')) {
    formattedHeaders.push({ key: 'Content-Type', value: 'application/json' })
  }

  return {
    url: url.value,
    method: method.value,
    body: requestBody,
    params,
    headers: formattedHeaders,
    auth
  }
}

const handleOpenSaveModal = () => {
  showSaveModal.value = true
}

const handleCloseSaveModal = () => {
  showSaveModal.value = false
}

const handleRequestSaved = (requestId: number) => {
  emit('requestSaved', requestId)
}

defineExpose({
  setActiveTab: (tab: string) => {
    activeTab.value = tab
  },
  focusBody: () => {
    nextTick(() => {
      bodyTabRef.value?.focus?.()
    })
  },
  clearResponse: () => {
    response.value = ''
    responseStatus.value = null
    responseDuration.value = null
    responseSize.value = null
  },
  getState: () => ({
    url: url.value,
    method: method.value,
    body: body.value,
    activeTab: activeTab.value
  }),
  getRequestData,
  get $refs() {
    return {
      paramsTabRef: paramsTabRef.value,
      headersTabRef: headersTabRef.value,
      authTabRef: authTabRef.value,
      bodyTabRef: bodyTabRef.value
    }
  },
  activeTab: activeTab.value
})
</script>

<template>
  <div ref="containerRef" class="h-full flex flex-col bg-white">
    <!-- Tabs -->
    <div class="px-4 border-b border-gray-200 bg-white flex-shrink-0">
      <div class="flex gap-6 -mb-px">
        <button v-for="tab in tabs" :key="tab" @click="activeTab = tab" :class="[
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
    <div class="bg-white flex-shrink-0 border-b border-gray-200 overflow-hidden flex flex-col"
      :style="{ height: `${requestHeight}px` }">
      <div class="p-4 flex-shrink-0">
        <!-- Method & URL Bar -->
        <div class="flex items-stretch gap-2 mb-4">
          <select v-model="method"
            class="px-3 py-2 text-sm font-semibold border border-gray-300 rounded-md bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer">
            <option value="GET">GET</option>
            <option value="POST">POST</option>
            <option value="PUT">PUT</option>
            <option value="PATCH">PATCH</option>
            <option value="DELETE">DELETE</option>
          </select>

          <input v-model="url" @input="emitStateChange" type="text"
            placeholder="Enter request URL (e.g., https://api.example.com/users)"
            class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500" />

          <!-- ✅ NÚT SAVE -->
          <button @click="handleOpenSaveModal"
            class="px-4 py-2 text-sm font-semibold bg-green-50 text-green-600 border border-green-300 rounded-md hover:bg-green-100 transition-colors flex items-center gap-2"
            title="Save request">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" />
            </svg>
            <span class="hidden sm:inline">Save</span>
          </button>

          <button @click="handleSend" :disabled="loading || !url" :class="[
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
        <ParamsTab v-show="activeTab === 'Params'" ref="paramsTabRef" />
        <AuthorizationTab v-show="activeTab === 'Authorization'" ref="authTabRef" />
        <HeadersTab v-show="activeTab === 'Headers'" ref="headersTabRef" />
        <BodyTab v-show="activeTab === 'Body'" ref="bodyTabRef" :key="bodyKey" :modelValue="body" />
      </div>
    </div>

    <!-- Resize Handle -->
    <div @mousedown="startResize" :class="[
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
            {{ responseStatus }} {{ responseStatus >= 200 && responseStatus < 300 ? '✓' : '✗' }} </span>
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

    <SaveRequestModal v-if="showSaveModal" :current-url="url" :current-method="method" :current-body="body"
      :request-id="requestId" :request-name="title" :card-ref="$root" @close="handleCloseSaveModal"
      @saved="handleRequestSaved" />
  </div>
</template>
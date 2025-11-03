<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue'
import ParamsTab from './tabs/ParamsTab.vue'
import AuthorizationTab from './tabs/AuthorizationTab.vue'
import HeadersTab from './tabs/HeadersTab.vue'
import BodyTab from './tabs/BodyTab.vue'
import SaveRequestModal from '../home/popup/SaveRequestModal.vue'
import { useApiClient } from '../../composables/useApiClient'
import { createExecutionHistory } from '../../composables/useExecutionHistory'
import { executeSQLQuery, getMySQLConnections } from '../../composables/sqlConnectionService'

// ==================== INTERFACES ====================
interface Props {
  title?: string
  defaultMethod?: string
  defaultUrl?: string
  defaultBody?: string
  requestId?: number | null
  dataBaseTest?: string | null
  bodyId?: number
  collectionId?: number | null
  bodies?: Array<any>
}

const props = withDefaults(defineProps<Props>(), {
  title: 'API Request',
  defaultMethod: 'POST',
  defaultUrl: '',
  collectionId: null,
  defaultBody: '{}',
  requestId: null,
  dataBaseTest: null,
  bodyId: 0,
  bodies: () => []
})

const emit = defineEmits<{
  (e: 'requestSaved', requestId: number): void
}>()

// ==================== STATE ====================
const url = ref(props.defaultUrl || '')
const method = ref(props.defaultMethod || 'POST')
const body = ref(props.defaultBody || '{}')
const currentRequestId = ref<number | null>(props.requestId)
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

// ✅ State cho tabs data
const paramsData = ref<Array<{ key: string; value: string }> | null>(null)
const headersData = ref<Array<{ key: string; value: string }> | null>(null)

// ==================== REFS ====================
const bodyTabRef = ref<any>(null)
const paramsTabRef = ref<any>(null)
const authTabRef = ref<any>(null)
const headersTabRef = ref<any>(null)
const containerRef = ref<HTMLDivElement>()

// ==================== COMPOSABLES ====================
const { sendRequest } = useApiClient()

// ==================== CONSTANTS ====================
// const tabs = ['Params', 'Authorization', 'Headers', 'Body']
const tabs = ['Authorization', 'Body']

const sqlConnections = ref<any[]>([])

onMounted(async () => {
  const res = await getMySQLConnections()
  if (res?.success) {
    sqlConnections.value = res.data
  }
})

function getConnectionString(connectionId: number): string | null {
  const conn = sqlConnections.value.find(c => c.id === connectionId)
  return conn?.connectString || null
}
// ==================== UPDATE FROM PARENT ====================
function updateFromParent(data: {
  url?: string
  method?: string
  body?: string
  bodyId?: number
  dataBaseTest?: string | null
  requestId?: number | null
  collectionId?: number | null
  params?: Array<{ key: string; value: string; enabled: boolean }>
  headers?: Array<{ key: string; value: string; enabled: boolean }>
}) {

  if (data.url !== undefined) url.value = data.url
  if (data.method !== undefined) method.value = data.method
  if (data.requestId !== undefined) currentRequestId.value = data.requestId

  if (data.params !== undefined) {
    paramsData.value = data.params.map(p => ({ key: p.key, value: p.value }))
  }

  if (data.headers !== undefined) {
    headersData.value = data.headers.map(h => ({ key: h.key, value: h.value }))
  }

  if (data.body !== undefined) {
    body.value = data.body
    bodyKey.value++
    nextTick(() => {
      if (bodyTabRef.value?.updateBody) {
        bodyTabRef.value.updateBody(data.body || '{}')
      }
    })
  }

  if (data.bodyId !== undefined && bodyTabRef.value?.setBodyId) {
    nextTick(() => {
      bodyTabRef.value.setBodyId(data.bodyId || 0)
    })
  }

  if (data.dataBaseTest !== undefined && bodyTabRef.value?.setDataBaseTest) {
    nextTick(() => {
      bodyTabRef.value.setDataBaseTest(data.dataBaseTest)
    })
  }
}

// ==================== GET REQUEST DATA ====================
function getRequestData() {
  const params = paramsTabRef.value?.getParams?.() || []
  const headers = headersTabRef.value?.getHeaders?.() || []
  const auth = authTabRef.value?.getAuthData?.() || null

  let bodyData: any = null
  if (bodyTabRef.value) {
    const result = bodyTabRef.value.getBody?.()
    const bodyType = bodyTabRef.value.getBodyType?.() || 'none'

    if (result && typeof result === 'object' && 'bodyType' in result && 'content' in result) {
      bodyData = result
    } else if (bodyType !== 'none') {
      bodyData = {
        id: 0,
        bodyType,
        content: result || ''
      }
    }
  }

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

// ==================== LOAD REQUEST DATA ====================
function loadRequestData(requestData: any) {

  url.value = requestData.url
  method.value = requestData.method

  if (requestData.body) {
    body.value = requestData.body.content || '{}'
    nextTick(() => {
      if (bodyTabRef.value?.setBodyId) {
        bodyTabRef.value.setBodyId(requestData.body.id || 0)
      }
    })
  }
}

// ==================== BUILD HEADERS ====================
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

// ==================== SAVE HISTORY ====================
async function saveHistory(result: any, requestBody: any, params: any[], headers: any[]) {
  const historyPayload = {
    requestId: currentRequestId.value ?? 0,
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
    console.error('[Card] Save history failed:', err)
  }
}

// ==================== PARSE & MERGE ====================
function parseRawBody(rawStr: string): any {
  const trimmed = rawStr.trim()

  if (trimmed === '{}' || trimmed === '') {
    return {}
  }

  if (trimmed.startsWith('[') && trimmed.endsWith(']')) {
    try {
      return JSON.parse(trimmed)
    } catch {
      return trimmed
    }
  }

  if (/\}\s*,\s*\{/.test(trimmed)) {
    try {
      const wrapped = `[${trimmed}]`
      return JSON.parse(wrapped)
    } catch {
      return trimmed
    }
  }

  try {
    return JSON.parse(trimmed)
  } catch {
    return trimmed
  }
}

function mergeTestData(baseDataStr: string | null, rawBodyStr: string): any {
  if (!baseDataStr) {
    return parseRawBody(rawBodyStr)
  }

  try {
    const baseData = JSON.parse(baseDataStr)
    const trimmed = rawBodyStr.trim()

    if (trimmed === '{}' || trimmed === '') {
      return baseData
    }

    let overrides = parseRawBody(rawBodyStr)

    if (!Array.isArray(overrides)) {
      overrides = [overrides]
    }

    const mergedTestCases = overrides.map((override: any) => {
      if (Object.keys(override).length === 0) {
        return baseData
      }
      return { ...baseData, ...override }
    })

    return mergedTestCases
  } catch (error) {
    return parseRawBody(rawBodyStr)
  }
}

// ==================== HANDLE SEND ====================
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
    const bodyData = bodyTabRef.value?.getBody?.()
    const bodyType = bodyTabRef.value?.getBodyType?.() || 'none'
    const baseData = bodyTabRef.value?.getDataBaseTest?.() || null


    let requestBody: any = null

    if (bodyType === 'form-data' || (bodyData && bodyData.bodyType === 'form-data')) {
      const formRef = bodyTabRef.value?.$refs?.formRef
      const sqlItems = formRef?.getSQLItems?.() || []


 
      if (sqlItems.length > 0) {
         const allSQLData: any[] = []

        for (const sqlItem of sqlItems) {
          const connectionString = getConnectionString(sqlItem.connectionId)

          if (!connectionString) {
            alert(`Connection ID ${sqlItem.connectionId} not found`)
            continue
          }

          console.log('🔵 [Card] Executing SQL:', sqlItem.query)

          const sqlResult = await executeSQLQuery(connectionString, sqlItem.query)

          if (sqlResult.success && sqlResult.data) {
            // ✅ Lưu cả key và data
            allSQLData.push({
              key: sqlItem.key,        // ✅ Key từ form-data
              values: sqlResult.data   // ✅ Array values từ SQL
            })
          } else {
            alert(`SQL Error: ${sqlResult.message}`)
            return
          }
        }

        console.log('🔵 [Card] All SQL Data:', allSQLData)

        // ✅ Build body đúng format
        const results = []

        // Lấy item đầu tiên để build
        const firstSQLItem = allSQLData[0]

        if (firstSQLItem && firstSQLItem.values.length > 0) {
          // Loop qua từng value trong data array
          for (let i = 0; i < firstSQLItem.values.length; i++) {
            const sqlValue = firstSQLItem.values[i]

            // ✅ Build body đúng format: { [key]: sqlValue }
            const requestBody = {
              [firstSQLItem.key]: sqlValue
            }

            const requestPayload = {
              requestId: currentRequestId.value,
              collectionId: 1,
              name: props.title ?? 'Untitled',
              method: method.value,
              url: url.value,
              queryParams: params.filter((p: any) => p.enabled !== false && p.key),
              headers,
              body: requestBody  // ✅ Body đúng format
            }

            console.log(`🔵 [Card] Sending request ${i + 1}/${firstSQLItem.values.length}:`, requestPayload)

            const result = await sendRequest(requestPayload)
            results.push({
              testCase: i + 1,
              sqlData: sqlValue,
              requestBody: requestBody,  // ✅ Thêm để debug
              result: result[0] || result
            })
          }
        }

        // Hiển thị tất cả kết quả
        responseStatus.value = results[0]?.result?.status ?? null
        responseDuration.value = results[0]?.result?.duration ?? null
        response.value = JSON.stringify(results, null, 2)

        return
      }
    }

    if (bodyType === 'raw' && bodyData?.content) {
      requestBody = mergeTestData(baseData, bodyData.content)
    } else if (bodyType === 'base-data' && baseData) {
      requestBody = baseData
    } else if (bodyData?.content) {
      requestBody = bodyData.content
    }

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


    const result = await sendRequest(requestPayload)

    if (Array.isArray(result)) {
      const allResults = result.map((r, index) => ({
        testCase: index + 1,
        success: r.success,
        status: r.status,
        statusText: r.statusText,
        duration: r.duration,
         data: r.data
      }))

      responseStatus.value = result[0]?.status ?? null
      responseDuration.value = result[0]?.duration ?? null
 
      response.value = JSON.stringify(allResults, null, 2)
    } else if (result) {
      response.value = JSON.stringify(result, null, 2)
    }

    await saveHistory(result, requestBody, params, headers)
  } catch (error: any) {
    response.value = JSON.stringify({
      error: error.message || 'Unknown error occurred'
    }, null, 2)
  } finally {
    loading.value = false
  }
}

// ==================== RESIZE HANDLERS ====================
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

// ==================== MODAL HANDLERS ====================
function handleOpenSaveModal() {
  showSaveModal.value = true
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

// ==================== EXPOSE ====================
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
  updateFromParent,
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

    <!-- ==================== HEADER ==================== -->
    <div class="border-b bg-gray-50 px-4 py-3">
      <div class="flex items-center justify-between">
        <h2 class="text-base font-semibold text-gray-800">{{ title }}</h2>
        <div class="flex gap-2">
          <button @click="clearResponse"
            class="px-3 py-1.5 text-xs text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded transition-colors">
            Clear
          </button>
        </div>
      </div>
    </div>

    <!-- ==================== REQUEST SECTION ==================== -->
    <div class="border-b bg-white" :style="{ height: requestHeight + 'px' }">

      <!-- Method & URL Row -->
      <div class="flex gap-2 p-3 border-b">
        <select v-model="method"
          class="border border-gray-300 rounded px-3 py-2 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-blue-500">
          <option>GET</option>
          <option>POST</option>
          <option>PUT</option>
          <option>DELETE</option>
          <option>PATCH</option>
        </select>

        <input v-model="url" placeholder="Enter request URL"
          class="flex-1 border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />

        <button @click="handleSend" :disabled="loading"
          class="px-6 py-2 bg-blue-600 text-white text-sm font-medium rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors">
          {{ loading ? 'Sending...' : 'Send' }}
        </button>

        <button @click="handleOpenSaveModal"
          class="px-4 py-2 bg-green-600 text-white text-sm font-medium rounded hover:bg-green-700 transition-colors">
          Save
        </button>
      </div>

      <!-- Tabs Header -->
      <div class="flex border-b bg-gray-50">
        <button v-for="tab in tabs" :key="tab" @click="activeTab = tab"
          class="px-4 py-2.5 text-sm font-medium transition-colors relative" :class="activeTab === tab
            ? 'text-blue-600 bg-white border-b-2 border-blue-600'
            : 'text-gray-600 hover:text-gray-800 hover:bg-gray-100'">
          {{ tab }}
        </button>
      </div>

      <div class="flex-1 overflow-auto" style="height: calc(100% - 100px);">
        <!-- Params Tab -->
        <!-- <ParamsTab 
          v-show="activeTab === 'Params'"
          ref="paramsTabRef"
          :paramsData="paramsData" /> -->

        <!-- Authorization Tab -->
        <AuthorizationTab v-show="activeTab === 'Authorization'" ref="authTabRef" />

        <!-- Headers Tab -->
        <!-- <HeadersTab 
          v-show="activeTab === 'Headers'"
          ref="headersTabRef"
          :headersData="headersData" /> -->

        <!-- Body Tab -->
        <BodyTab v-show="activeTab === 'Body'" ref="bodyTabRef" :key="bodyKey" />
      </div>
    </div>

    <!-- ==================== RESIZE HANDLE ==================== -->
    <div @mousedown="startResize" class="h-1 bg-gray-200 hover:bg-blue-400 cursor-row-resize transition-colors">
    </div>

    <!-- ==================== RESPONSE SECTION ==================== -->
    <div class="flex-1 flex flex-col bg-gray-50 overflow-hidden">
      <div class="px-4 py-2 border-b bg-white flex items-center justify-between">
        <div class="flex items-center gap-4">
          <span class="text-sm font-medium text-gray-700">Response</span>
          <div v-if="responseStatus" class="flex items-center gap-3 text-xs">
            <span class="px-2 py-1 rounded"
              :class="responseStatus >= 200 && responseStatus < 300 ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'">
              Status: {{ responseStatus }}
            </span>
            <span v-if="responseDuration" class="text-gray-600">
              Time: {{ responseDuration }}ms
            </span>
            <span v-if="responseSize" class="text-gray-600">
              Size: {{ responseSize }}B
            </span>
          </div>
        </div>
      </div>

      <div class="flex-1 overflow-auto p-4">
        <pre v-if="response" class="text-xs font-mono text-gray-800 whitespace-pre-wrap">{{ response }}</pre>
        <div v-else class="flex items-center justify-center h-full text-gray-400 text-sm">
          No response yet. Click "Send" to make a request.
        </div>
      </div>
    </div>

    <!-- ==================== SAVE MODAL ==================== -->
    <SaveRequestModal v-if="showSaveModal" :currentUrl="url" :currentMethod="method"
      :currentBody="{ bodyType: 'raw', content: body }" :requestId="requestId" :requestName="title"
      :collectionId="collectionId" :cardRef="{ getRequestData }" @close="showSaveModal = false"
      @saved="handleRequestSaved" />
  </div>
</template>
<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useUserData } from '../../../composables/useUserData'
import { getMyCollections, createCollection } from '../../../composables/useCollection'
interface KeyValuePair {
  key: string
  value: string
  enabled?: boolean
}
interface Collection {
  id: number
  userId: number
  name: string
  description: string
  createdAt: string
  requestsCount: number
}

interface Props {
  currentUrl: string
  currentMethod: string
  currentBody: any
  requestId?: number | null
  requestName?: string
  cardRef?: any
  collectionId?: number | null
}

const props = defineProps<Props>()
const emit = defineEmits<{
  (e: 'close'): void
  (e: 'saved', requestId: number): void
}>()

const { saveRequest, loading, error } = useUserData()
const saveResult = ref<string | null>(null)

// State
const selectedCollectionId = ref<number>(0)
const requestName = ref('')
const collections = ref<Collection[]>([])
const loadingCollections = ref(false)
const showCreateCollection = ref(false)
const newCollectionName = ref('')
const newCollectionDescription = ref('')
const creatingCollection = ref(false)

watch(() => props.collectionId, (newVal) => {
  if (newVal && newVal > 0) selectedCollectionId.value = newVal
}, { immediate: true })

watch(() => props.requestName, (newVal) => {
  requestName.value = newVal || ''
}, { immediate: true })

onMounted(loadCollections)

async function loadCollections() {
  loadingCollections.value = true
  try {
    collections.value = await getMyCollections() || []
    if (props.collectionId && selectedCollectionId.value === 0) {
      selectedCollectionId.value = props.collectionId
    }
  } catch (err) {
    error.value = 'Failed to load collections'
  } finally {
    loadingCollections.value = false
  }
}

function toggleCreateCollection() {
  showCreateCollection.value = !showCreateCollection.value
  if (showCreateCollection.value) {
    newCollectionName.value = ''
    newCollectionDescription.value = ''
  }
}

async function handleCreateCollection() {
  const name = newCollectionName.value.trim()
  if (!name) {
    error.value = 'Please enter collection name'
    return
  }

  creatingCollection.value = true
  error.value = null

  try {
    const result = await createCollection({ name, description: newCollectionDescription.value })
    if (result?.id) {
      await loadCollections()
      selectedCollectionId.value = result.id
      showCreateCollection.value = false
      newCollectionName.value = ''
      newCollectionDescription.value = ''
    }
  } catch (err: any) {
    error.value = err.message || 'Failed to create collection'
  } finally {
    creatingCollection.value = false
  }
}
function getCardData() {
 
  
  //  CHECK: cardRef có tồn tại không
  if (!props.cardRef) {
    console.error(' [SaveRequestModal] cardRef is null/undefined!')
    return {
      formDataItems: [],
      authData: null,
      queryParams: [],
      headers: [],
      currentBodyType: 'raw',
      currentBodyContent: props.currentBody?.content || '{}',
      dataBaseTest: null
    }
  }
   
  if (!props.cardRef.getCurrentData) {
    console.error(' [SaveRequestModal] getCurrentData method NOT FOUND!')
    return {
      formDataItems: [],
      authData: null,
      queryParams: [],
      headers: [],
      currentBodyType: 'raw',
      currentBodyContent: props.currentBody?.content || '{}',
      dataBaseTest: null
    }
  }

  //  GỌI getCurrentData
   const currentData = props.cardRef.getCurrentData()
 
  // Lấy dữ liệu từ Card.vue/getCurrentData()
  const formDataItems = currentData.formDataItems || []
  const authData = currentData.authData || null
  const queryParams = (currentData.params || []).filter((p: any) => p.enabled !== false && p.key)
  const headers = (currentData.headers || []).filter((h: any) => h.enabled !== false && h.key)

  // Lấy Body Data chính xác từ Card
  const currentBodyType = currentData.bodyType || 'none'
  const currentBodyContent = currentData.body || '{}'
  const dataBaseTest = currentData.dataBaseTest || null
 

  return { formDataItems, authData, queryParams, headers, currentBodyType, currentBodyContent, dataBaseTest }
}


async function handleSave() {
   
  const validationError = validateSaveRequest()
  if (validationError) {
    error.value = validationError
    return
  }

  error.value = null

   const cardData = getCardData()
   
  const { formDataItems, authData, queryParams, headers, currentBodyType, currentBodyContent, dataBaseTest } = cardData

  // BASE REQUEST
  const baseRequest = {
    requestId: props.requestId || 0,
    collectionId: selectedCollectionId.value,
    name: requestName.value,
    method: props.currentMethod,
    url: props.currentUrl,
    queryParams: queryParams.map((p: KeyValuePair) => ({ key: p.key, value: p.value })),
    headers: headers.map((h: KeyValuePair) => ({ key: h.key, value: h.value }))
  }

  let bodyForSave: any = null
  let dataBaseTestForSave: string | null = null

 

  // 1. XỬ LÝ FORM-DATA
  if (currentBodyType === 'form-data' && formDataItems.length > 0) {
     
    const filtered = formDataItems
      .filter((item: any) => item.enabled && item.key)
      .map((item: any) => ({
        key: item.key,
        type: item.type,
        value: item.value,
        sqlConnectionId: item.sqlConnectionId || null,
        description: item.description || '',
        enabled: item.enabled !== false
      }))

 
    if (filtered.length > 0) {
      const bodyTypeValue = filtered[0]?.type || 'text'

      bodyForSave = {
        id: 0,
        bodyType: 'form-data',
        value: JSON.stringify(filtered),
        type: bodyTypeValue
      }
      
     }
  }
  // 2. XỬ LÝ BASE-DATA
  else if (currentBodyType === 'base-data' && dataBaseTest?.trim()) {
     dataBaseTestForSave = dataBaseTest
    bodyForSave = null
  }
  // 3. XỬ LÝ RAW
  else if (currentBodyType === 'raw' && currentBodyContent?.trim() && currentBodyContent.trim() !== '{}') {
     bodyForSave = {
      id: 0,
      bodyType: 'raw',
      value: currentBodyContent,
      type: 'raw'
    }
  } else {
    console.log(' [SaveRequestModal]  No body to save (type:', currentBodyType, ')')
  }

  const requestData = {
    ...baseRequest,
    authType: authData?.authType || '',
    authValue: authData?.bearerToken || '',
    dataBaseTest: dataBaseTestForSave,
    body: bodyForSave
  }

  const payload = [requestData]
 
  try {
    const result = await saveRequest(payload)

    if (result?.[0]?.success) {
      saveResult.value = result[0].isNew ? 'created' : 'updated'

      setTimeout(() => {
        emit('saved', result[0].requestId)
        emit('close')
      }, 1500)
    } else {
      error.value = result?.[0]?.message || 'Save failed'
    }
  } catch (err: any) {
    console.error(' [SaveRequestModal] Error:', err)
    error.value = err.message || 'Error'
  }
}
function validateSaveRequest(): string | null {
  if (!requestName.value.trim()) return 'Please enter a request name'
  if (selectedCollectionId.value === 0) return 'Please select a collection'
  if (!props.currentUrl.trim()) return 'URL cannot be empty'
  return null
}
</script>



<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto">
      <!-- Header -->
      <div class="flex items-center justify-between p-6 border-b border-gray-200 sticky top-0 bg-white z-10">
        <h2 class="text-xl font-semibold text-gray-900">
          {{ props.requestId ? 'Update Request' : 'Save Request' }}
        </h2>
        <button @click="emit('close')" class="text-gray-400 hover:text-gray-600 transition-colors">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Content -->
      <div class="p-6 space-y-4">
        <!-- Error Message -->
        <div v-if="error" class="p-3 bg-red-50 border border-red-200 rounded-lg">
          <div class="flex items-center justify-between">
            <p class="text-sm text-red-600">{{ error }}</p>
            <button @click="error = null" class="text-red-400 hover:text-red-600">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        <!-- Success Message -->
        <div v-if="saveResult" class="p-3 bg-green-50 border border-green-200 rounded-lg">
          <div class="flex items-center gap-2">
            <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <p class="text-sm text-green-600 font-medium">
              Request {{ saveResult === 'created' ? 'created' : 'updated' }} successfully!
            </p>
          </div>
        </div>

        <!-- Request Name -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Request Name <span class="text-red-500">*</span>
          </label>
          <input v-model="requestName" type="text" placeholder="e.g., Get Users, Create Post..."
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>

        <!-- Collection Select -->
        <div>
          <div class="flex items-center justify-between mb-2">
            <label class="block text-sm font-medium text-gray-700">
              Collection <span class="text-red-500">*</span>
            </label>
            <button @click="toggleCreateCollection"
              class="text-xs text-blue-600 hover:text-blue-700 font-medium flex items-center gap-1">
              <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
              </svg>
              {{ showCreateCollection ? 'Cancel' : 'New Collection' }}
            </button>
          </div>

          <!-- New Collection Form -->
          <div v-if="showCreateCollection" class="mb-3 p-3 bg-blue-50 border border-blue-200 rounded-lg space-y-2">
            <input v-model="newCollectionName" type="text" placeholder="Collection name *"
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              @keydown.enter="handleCreateCollection" />
            <input v-model="newCollectionDescription" type="text" placeholder="Description (optional)"
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              @keydown.enter="handleCreateCollection" />
            <button @click="handleCreateCollection" :disabled="creatingCollection || !newCollectionName.trim()"
              class="w-full px-3 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors font-medium">
              <span v-if="creatingCollection" class="flex items-center justify-center gap-2">
                <svg class="animate-spin w-3 h-3" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
                  </path>
                </svg>
                Creating...
              </span>
              <span v-else>Create Collection</span>
            </button>
          </div>

          <!-- Collections Loading -->
          <div v-if="loadingCollections" class="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50">
            <div class="flex items-center gap-2 text-gray-500">
              <svg class="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
                </path>
              </svg>
              <span class="text-sm">Loading collections...</span>
            </div>
          </div>

          <!-- Select Dropdown -->
          <select v-else v-model="selectedCollectionId"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            :disabled="collections.length === 0">
            <option :value="0" disabled>
              {{ collections.length === 0 ? 'No collections available' : 'Select a collection' }}
            </option>
            <option v-for="col in collections" :key="col.id" :value="col.id">
              {{ col.name }} ({{ col.requestsCount }} requests)
            </option>
          </select>
        </div>

        <!-- Preview -->
        <div class="p-3 bg-gray-50 rounded-lg border border-gray-200">
          <p class="text-xs font-medium text-gray-500 mb-2">Preview:</p>
          <div class="flex items-center gap-2 mb-1">
            <span class="text-xs font-bold px-2 py-0.5 rounded" :class="{
              'bg-green-100 text-green-700': props.currentMethod === 'POST',
              'bg-blue-100 text-blue-700': props.currentMethod === 'GET',
              'bg-orange-100 text-orange-700': props.currentMethod === 'PUT',
              'bg-purple-100 text-purple-700': props.currentMethod === 'PATCH',
              'bg-red-100 text-red-700': props.currentMethod === 'DELETE'
            }">
              {{ props.currentMethod }}
            </span>
            <span class="text-sm text-gray-900 font-medium truncate">{{ requestName || 'Untitled' }}</span>
          </div>
          <p class="text-xs text-gray-500 truncate">{{ props.currentUrl || 'No URL' }}</p>
        </div>
      </div>

      <!-- Footer -->
      <div class="flex gap-3 p-6 border-t border-gray-200 bg-gray-50 sticky bottom-0">
        <button @click="emit('close')"
          class="flex-1 px-4 py-2 text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors font-medium">
          Cancel
        </button>
        <button @click="handleSave" :disabled="loading || !!saveResult || loadingCollections"
          class="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors font-medium">
          <span v-if="loading" class="flex items-center justify-center gap-2">
            <svg class="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
              </path>
            </svg>
            Saving...
          </span>
          <span v-else>{{ props.requestId ? 'Update' : 'Save' }}</span>
        </button>
      </div>
    </div>
  </div>
</template>
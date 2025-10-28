<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useUserData } from '../../composables/useUserData'
import type { Collection, RequestItem, RequestBody } from '../../composables/useUserData'
import { deleteRequest } from '../../composables/useCollection'

const emit = defineEmits<{
  (e: 'selectRequest', payload: {
    url: string
    method: string
    name: string
    body: RequestBody | null
    headers: Array<{ key: string; value: string }>
    queryParams: Array<{ key: string; value: string }>
  }): void
  (e: 'addNewTab', collectionId: number): void
  (e: 'openExportImport'): void
}>()

const { data, loading, error, fetchUserData, loadCachedData } = useUserData()
const selectedCollection = ref<number | null>(null)
const selectedRequest = ref<number | null>(null)
const deletingRequestId = ref<number | null>(null)

const collections = computed<Collection[]>(() => data.value?.collections || [])

onMounted(async () => {
  loadCachedData()
  await fetchUserData()
})

const toggleCollection = (id: number) => {
  selectedCollection.value = selectedCollection.value === id ? null : id
  selectedRequest.value = null
}

const toggleRequest = (id: number, request: RequestItem) => {
  selectedRequest.value = selectedRequest.value === id ? null : id

  if (selectedRequest.value === id) {
    emit('selectRequest', {
      url: request.url,
      method: request.method,
      name: request.name,
      body: request.body,
      headers: request.headers,
      queryParams: request.queryParams
    })
  }
}

const handleDeleteRequest = async (requestId: number, requestName: string, event: Event) => {
  event.stopPropagation()
  if (!confirm(`Are you sure you want to delete "${requestName}"?`)) return

  deletingRequestId.value = requestId
  try {
    const result = await deleteRequest(requestId)
    if (result?.success) {
       await fetchUserData()
      if (selectedRequest.value === requestId) selectedRequest.value = null
    } else {
      alert(result?.message || 'Failed to delete request')
    }
  } catch (err: any) {
     alert('Failed to delete request: ' + (err.message || 'Unknown error'))
  } finally {
    deletingRequestId.value = null
  }
}

const handleAddTab = (collectionId: number, event: Event) => {
  event.stopPropagation()
  emit('addNewTab', collectionId)
}

const refreshData = async () => await fetchUserData()

defineExpose({ refreshData })
</script>


<template>
  <div class="h-full flex flex-col bg-white overflow-hidden">
    <!-- Loading State -->
    <div v-if="loading && collections.length === 0" class="flex-1 flex items-center justify-center p-4">
      <div class="text-center">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-2"></div>
        <p class="text-sm text-gray-600">Loading collections...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error && collections.length === 0" class="flex-1 flex items-center justify-center p-4">
      <div class="text-center">
        <svg class="w-12 h-12 text-red-500 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <p class="text-sm text-red-600 mb-2">{{ error }}</p>
        <button @click="refreshData" class="px-3 py-1 bg-blue-600 text-white text-xs rounded hover:bg-blue-700">
          Try Again
        </button>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="collections.length === 0" class="flex-1 flex items-center justify-center p-4">
      <div class="text-center">
        <svg class="w-12 h-12 text-gray-400 mx-auto mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
        </svg>
        <p class="text-sm text-gray-600 mb-1">No collections yet</p>
        <p class="text-xs text-gray-400">Create your first collection to get started</p>
      </div>
    </div>

    <!-- Collections List -->
    <div v-else class="flex-1 overflow-y-auto overflow-x-hidden p-2 text-sm">
      <!-- ✅ Action Buttons -->
      <div class="mb-2 px-2 space-y-2">
        <!-- Refresh Button -->
        <button @click="refreshData" :disabled="loading"
          class="w-full px-3 py-1.5 bg-blue-50 hover:bg-blue-100 text-blue-600 text-xs rounded-md transition-colors disabled:opacity-50 flex items-center justify-center gap-2">
          <svg class="w-4 h-4" :class="{ 'animate-spin': loading }" fill="none" stroke="currentColor"
            viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          <span>{{ loading ? 'Refreshing...' : 'Refresh' }}</span>
        </button>

        <!-- Export/Import Button -->
        <button @click="emit('openExportImport')"
          class="w-full px-3 py-1.5 bg-purple-50 hover:bg-purple-100 text-purple-600 text-xs rounded-md transition-colors flex items-center justify-center gap-2">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" />
          </svg>
          <span>Export / Import</span>
        </button>
      </div>

      <div v-for="col in collections" :key="col.id" class="py-2">
        <!-- COLLECTION HEADER -->
        <div
          class="flex items-center justify-between cursor-pointer hover:bg-gray-50 px-2 py-2 rounded-md border-l-2 transition-colors group"
          :class="selectedCollection === col.id ? 'border-blue-500 bg-blue-50' : 'border-transparent'"
          @click="toggleCollection(col.id)">
          <div class="flex-1 mr-2 min-w-0">
            <div class="font-semibold text-gray-800 text-xs truncate">
              {{ col.name }}
            </div>
            <div class="text-gray-400 text-xs truncate">
              {{ col.description }}
            </div>
          </div>

          <!-- NÚT + -->
          <button @click="handleAddTab(col.id, $event)"
            class="opacity-0 group-hover:opacity-100 text-gray-400 hover:text-blue-600 hover:bg-blue-100 rounded p-1 transition-all flex-shrink-0"
            title="New tab">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
          </button>

        </div>

        <!-- REQUEST LIST -->
        <div v-if="selectedCollection === col.id" class="pl-4 border-l border-gray-200 ml-2 mt-1 space-y-1">
          <div v-for="req in col.requests" :key="req.id">
            <div class="cursor-pointer px-2 py-1.5 hover:bg-gray-50 rounded-md transition-colors relative"
              :class="selectedRequest === req.id ? 'bg-blue-50 border border-blue-200' : 'border border-transparent'"
              @click="toggleRequest(req.id, req)">
              <div class="flex items-center gap-2 mb-0.5">
                <span class="text-xs font-bold px-1.5 py-0.5 rounded flex-shrink-0" :class="{
                  'bg-green-100 text-green-700': req.method === 'POST',
                  'bg-blue-100 text-blue-700': req.method === 'GET',
                  'bg-orange-100 text-orange-700': req.method === 'PUT',
                  'bg-purple-100 text-purple-700': req.method === 'PATCH',
                  'bg-red-100 text-red-700': req.method === 'DELETE'
                }">
                  {{ req.method }}
                </span>
                <span class="text-gray-800 text-xs truncate flex-1 font-medium">{{ req.name }}</span>
              </div>
              <div class="text-gray-500 text-xs pl-1 truncate" :title="req.url">
                {{ req.url }}
              </div>

              <!-- DELETE BUTTON -->
              <button @click="handleDeleteRequest(req.id, req.name, $event)" :disabled="deletingRequestId === req.id"
                class="absolute right-1 top-1 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded p-1 flex-shrink-0"
                title="Delete request">
                <svg v-if="deletingRequestId === req.id" class="animate-spin w-3 h-3" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
                  </path>
                </svg>
                <svg v-else class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>

            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>
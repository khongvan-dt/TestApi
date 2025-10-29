<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import List from '../components/home/List.vue'
import Card from '../components/home/Card.vue'
import ExportImportModal from '../components/home/popup/ExportImportModal.vue'

interface Tab {
  id: string
  title: string
  method: string
  url: string
  body: string
  collectionId?: number
  requestId?: number
  params?: Array<{ key: string; value: string; enabled: boolean }>
  headers?: Array<{ key: string; value: string; enabled: boolean }>
  auth?: any
  activeSubTab?: string
  dataBaseTest?: string | null
}

// State
const tabs = ref<Tab[]>([
  {
    id: 'default',
    title: 'New Request',
    method: 'POST',
    url: '',
    body: '{}',
    collectionId: undefined,
    requestId: undefined,
    params: [],
    headers: [],
    auth: null,
    activeSubTab: 'Body'
  }
])

const activeTabId = ref('default')
const cardRef = ref()
const listRef = ref()
const showExportImportModal = ref(false)

const activeTab = computed(() =>
  tabs.value.find(t => t.id === activeTabId.value) || tabs.value[0]
)

// Handle state change từ Card
const handleStateChange = (state: any) => {
  const currentTab = tabs.value.find(t => t.id === activeTabId.value)
  if (currentTab) {
    currentTab.url = state.url
    currentTab.method = state.method
    currentTab.body = state.body
    currentTab.activeSubTab = state.activeTab
  }
}

// Handle chọn request từ sidebar
const handleSelectRequest = (payload: any) => {
  const currentTab = tabs.value.find(t => t.id === activeTabId.value)
  if (currentTab) {
    currentTab.title = payload.name
    currentTab.method = payload.method
    currentTab.url = payload.url
    currentTab.headers = payload.headers || []
    currentTab.params = payload.queryParams || []
    currentTab.requestId = payload.requestId
    currentTab.dataBaseTest = payload.dataBaseTest || null  // LƯU
    console.log("📦 handleSelectRequest:", payload.requestId)

    if (payload.body?.content) {
      try {
        const parsed = JSON.parse(payload.body.content)
        currentTab.body = JSON.stringify(parsed, null, 2)
      } catch {
        currentTab.body = payload.body.content
      }
    } else {
      currentTab.body = '{}'
    }

    // Tự động chọn tab base-data nếu có dataBaseTest
    if (payload.dataBaseTest) {
      currentTab.activeSubTab = 'Body'
      nextTick(() => {
        cardRef.value?.setActiveTab?.('Body')
      })
    }
  }
}

// Thêm tab mới
const handleAddNewTab = (collectionId: number) => {
  const newId = `tab-${Date.now()}`

  tabs.value.push({
    id: newId,
    title: 'New Request',
    method: 'POST',
    url: '',
    body: '{}',
    collectionId,
    requestId: undefined,
    params: [],
    headers: [],
    auth: null,
    activeSubTab: 'Body'
  })

  activeTabId.value = newId
}

// Đóng tab
const closeTab = (tabId: string) => {
  const index = tabs.value.findIndex(t => t.id === tabId)

  if (index > -1) {
    tabs.value.splice(index, 1)

    if (activeTabId.value === tabId && tabs.value.length > 0) {
      activeTabId.value = tabs.value[tabs.value.length - 1].id
    }
  }

  if (tabs.value.length === 0) {
    tabs.value.push({
      id: 'default',
      title: 'New Request',
      method: 'POST',
      url: '',
      body: '{}',
      collectionId: undefined,
      requestId: undefined,
      params: [],
      headers: [],
      auth: null,
      activeSubTab: 'Body'
    })
    activeTabId.value = 'default'
  }
}

// Switch tab
const switchTab = (tabId: string) => {
  if (activeTabId.value === tabId) return
  activeTabId.value = tabId
}

// Export/Import handlers
const handleOpenExportImport = () => {
  showExportImportModal.value = true
}

const handleCloseExportImport = () => {
  showExportImportModal.value = false
}

const handleImported = async () => {
  if (listRef.value?.refreshData) {
    await listRef.value.refreshData()
  }
}

const handleRequestSaved = async (requestId: number) => {

  // Refresh list
  if (listRef.value?.refreshData) {
    await listRef.value.refreshData()
  }

  const currentTab = tabs.value.find(t => t.id === activeTabId.value)
  if (currentTab) {
    currentTab.requestId = requestId
  }
}

// Save/Load từ localStorage
onMounted(() => {
  const savedTabs = localStorage.getItem('api-tabs')
  if (savedTabs) {
    try {
      const parsed = JSON.parse(savedTabs)
      if (parsed.tabs && parsed.tabs.length > 0) {
        tabs.value = parsed.tabs
        activeTabId.value = parsed.activeTabId || tabs.value[0].id
      }
    } catch (error) {
      console.error('Error loading saved tabs:', error)
    }
  }
})

// Watch tabs và lưu vào localStorage
watch(
  tabs,
  (newTabs) => {
    localStorage.setItem('api-tabs', JSON.stringify({
      tabs: newTabs,
      activeTabId: activeTabId.value
    }))
  },
  { deep: true }
)
</script>

<template>

  <div class="flex h-screen" style="width: 95%;">





    <!-- Sidebar -->
    <div class="w-80 border-r border-gray-200 flex-shrink-0">
      <List ref="listRef" @selectRequest="handleSelectRequest" @addNewTab="handleAddNewTab"
        @openExportImport="handleOpenExportImport" />
    </div>

    <!-- Main -->
    <div class="flex-1 flex flex-col min-w-0">
      <!-- Tab Bar -->
      <div class="border-b border-gray-200 bg-gray-50 flex items-center overflow-x-auto flex-shrink-0">
        <div v-for="tab in tabs" :key="tab.id"
          class="flex items-center gap-2 px-4 py-2.5 border-r border-gray-200 cursor-pointer hover:bg-white transition-colors min-w-0 max-w-xs group"
          :class="activeTabId === tab.id ? 'bg-white border-b-2 border-blue-600' : ''" @click="switchTab(tab.id)">


          <span class="text-sm truncate flex-1">{{ tab.title }}</span>



          <button v-if="tabs.length > 1" @click.stop="closeTab(tab.id)"
            class="opacity-0 group-hover:opacity-100 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded p-0.5 flex-shrink-0 transition-opacity"
            title="Close tab">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <button @click="handleAddNewTab(0)"
          class="px-3 py-2.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 transition-colors flex-shrink-0"
          title="New tab">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
        </button>
      </div>

      <!-- Card Component -->
      <div class="flex-1 overflow-hidden">
        <!-- Truyền dataBaseTest vào Card -->
        <Card ref="cardRef" :key="activeTab.id" :title="activeTab.title" :defaultUrl="activeTab.url"
          :defaultMethod="activeTab.method" :defaultBody="activeTab.body" :requestId="activeTab.requestId"
          :dataBaseTest="activeTab.dataBaseTest || null" @stateChange="handleStateChange"
          @requestSaved="handleRequestSaved" />
      </div>
    </div>

    <!-- Export/Import Modal -->
    <ExportImportModal v-if="showExportImportModal" @close="handleCloseExportImport" @imported="handleImported" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import List from '../components/home/List.vue'
import Card from '../components/home/Card.vue'
import ExportImportModal from '../components/home/popup/ExportImportModal.vue'

// ==================== INTERFACES ====================
interface Tab {
  id: string
  title: string
  method: string
  url: string
  body: string
  bodyId?: number
  bodies?: Array<any>
  collectionId?: number
  requestId?: number
  params?: Array<{ key: string; value: string; enabled: boolean }>
  headers?: Array<{ key: string; value: string; enabled: boolean }>
  auth?: any
  activeSubTab?: string
  dataBaseTest?: string | null
}

// ==================== CONSTANTS ====================
const DEFAULT_TAB: Tab = {
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
  activeSubTab: 'Body',
  dataBaseTest: null
}

// ==================== STATE ====================
const tabs = ref<Tab[]>([{ ...DEFAULT_TAB }])
const activeTabId = ref('default')
const cardRef = ref<any>(null)
const listRef = ref<any>(null)
const showExportImportModal = ref(false)

// ==================== COMPUTED ====================
const activeTab = computed(() =>
  tabs.value.find(t => t.id === activeTabId.value) || tabs.value[0]
)

const currentBodyId = computed(() => activeTab.value?.bodyId || 0)

// ==================== HELPERS ====================
function debounce<T extends (...args: any[]) => any>(
  fn: T,
  delay: number
): (...args: Parameters<T>) => void {
  let timeoutId: ReturnType<typeof setTimeout> | null = null
  return (...args: Parameters<T>) => {
    if (timeoutId) clearTimeout(timeoutId)
    timeoutId = setTimeout(() => fn(...args), delay)
  }
}

const saveTabsToLocalStorage = debounce((tabsData: Tab[], activeId: string) => {
  try {
    localStorage.setItem('api-tabs', JSON.stringify({
      tabs: tabsData,
      activeTabId: activeId
    }))
    console.log('💾 [index] Saved to localStorage')
  } catch (error) {
    console.error('❌ [index] Error saving tabs:', error)
  }
}, 500)

function parseBodyContent(content: string): string {
  try {
    const parsed = JSON.parse(content)
    return JSON.stringify(parsed, null, 2)
  } catch {
    return content
  }
}

// ==================== EVENT HANDLERS ====================

// ✅ Handle select request from List
function handleSelectRequest(payload: any) {
  console.log('🟢 [index] handleSelectRequest:', payload)

  const currentTab = tabs.value.find(t => t.id === activeTabId.value)
  if (!currentTab) return

  // Update tab data
  currentTab.title = payload.name
  currentTab.method = payload.method
  currentTab.url = payload.url
  currentTab.headers = payload.headers || []
  currentTab.params = payload.queryParams || []
  currentTab.requestId = payload.requestId
  currentTab.dataBaseTest = payload.dataBaseTest || null
  currentTab.collectionId = payload.collectionId
  currentTab.bodies = payload.bodies || []

  if (payload.body) {
    currentTab.bodyId = payload.body.id || 0
    currentTab.body = payload.body.value
      ? parseBodyContent(payload.body.value)
      : '{}'
  } else {
    currentTab.body = '{}'
    currentTab.bodyId = 0
  }

  // ✅ Update Card với params và headers
  nextTick(() => {
    setTimeout(() => {
      if (cardRef.value?.updateFromParent) {
        console.log('🟢 [index] Calling Card.updateFromParent')
        cardRef.value.updateFromParent({
          url: currentTab.url,
          method: currentTab.method,
          body: currentTab.body,
          bodyId: currentTab.bodyId,
          dataBaseTest: currentTab.dataBaseTest,
          requestId: currentTab.requestId,
          collectionId: currentTab.collectionId,
          params: currentTab.params,      // ✅ Thêm
          headers: currentTab.headers      // ✅ Thêm
        })
      }
    }, 100)

    saveTabsToLocalStorage(tabs.value, activeTabId.value)
  })
}

// ✅ Add new tab
function handleAddNewTab(collectionId: number) {
  const newId = `tab-${Date.now()}`

  tabs.value.push({
    ...DEFAULT_TAB,
    id: newId,
    collectionId
  })

  activeTabId.value = newId

  nextTick(() => {
    saveTabsToLocalStorage(tabs.value, activeTabId.value)
  })
}

// ✅ Close tab
function closeTab(tabId: string) {
  const index = tabs.value.findIndex(t => t.id === tabId)

  if (index > -1) {
    tabs.value.splice(index, 1)

    if (activeTabId.value === tabId && tabs.value.length > 0) {
      activeTabId.value = tabs.value[tabs.value.length - 1].id
    }
  }

  if (tabs.value.length === 0) {
    tabs.value.push({ ...DEFAULT_TAB })
    activeTabId.value = 'default'
  }

  nextTick(() => {
    saveTabsToLocalStorage(tabs.value, activeTabId.value)
  })
}

// ✅ Switch tab
function switchTab(tabId: string) {
  if (activeTabId.value === tabId) return
  activeTabId.value = tabId
  saveTabsToLocalStorage(tabs.value, activeTabId.value)
}

// ✅ Export/Import handlers
function handleOpenExportImport() {
  showExportImportModal.value = true
}

function handleCloseExportImport() {
  showExportImportModal.value = false
}

async function handleImported() {
  if (listRef.value?.refreshData) {
    await listRef.value.refreshData()
  }
}

// ✅ Request saved handler
async function handleRequestSaved(requestId: number) {
  console.log('🟢 [index] Request saved:', requestId)

  if (listRef.value?.refreshData) {
    await listRef.value.refreshData()
  }

  const currentTab = tabs.value.find(t => t.id === activeTabId.value)
  if (currentTab) {
    currentTab.requestId = requestId
  }

  nextTick(() => {
    saveTabsToLocalStorage(tabs.value, activeTabId.value)
  })
}

// ==================== LIFECYCLE ====================

// ✅ Load saved tabs from localStorage
function loadSavedTabs() {
  const savedTabs = localStorage.getItem('api-tabs')
  if (!savedTabs) return

  try {
    const parsed = JSON.parse(savedTabs)
    if (parsed.tabs && parsed.tabs.length > 0) {
      tabs.value = parsed.tabs
      activeTabId.value = parsed.activeTabId || tabs.value[0].id
      console.log('✅ [index] Loaded tabs from localStorage:', tabs.value.length)
    }
  } catch (error) {
    console.error('❌ [index] Error loading saved tabs:', error)
  }
}

onMounted(() => {
  loadSavedTabs()
})


watch(activeTabId, (newId, oldId) => {
  if (newId === oldId) return

  console.log('🟢 [index] Tab switched:', newId)

  nextTick(() => {
    const currentTab = activeTab.value

    setTimeout(() => {
      if (cardRef.value?.updateFromParent && currentTab) {
        console.log('🟢 [index] Updating Card after tab switch')
        cardRef.value.updateFromParent({
          url: currentTab.url,
          method: currentTab.method,
          body: currentTab.body,
          bodyId: currentTab.bodyId,
          dataBaseTest: currentTab.dataBaseTest,
          requestId: currentTab.requestId,
          collectionId: currentTab.collectionId,
          params: currentTab.params,      // ✅ Thêm
          headers: currentTab.headers      // ✅ Thêm
        })
      }
    }, 100)
  })
})

// ❌ BỎ WATCH TABS - Gây loop
// watch(() => tabs.value.map(...), ...)
</script>

<template>
  <div class="flex h-screen" style="width: 95%;">
    <!-- ==================== SIDEBAR ==================== -->
    <div class="w-80 border-r border-gray-200 flex-shrink-0">
      <List ref="listRef" @selectRequest="handleSelectRequest" @addNewTab="handleAddNewTab"
        @openExportImport="handleOpenExportImport" />
    </div>

    <!-- ==================== MAIN CONTENT ==================== -->
    <div class="flex-1 flex flex-col min-w-0">

      <!-- ==================== TAB BAR ==================== -->
      <div class="border-b border-gray-200 bg-gray-50 flex items-center overflow-x-auto flex-shrink-0">
        <!-- Tabs -->
        <div v-for="tab in tabs" :key="tab.id"
          class="flex items-center gap-2 px-4 py-2.5 border-r border-gray-200 cursor-pointer hover:bg-white transition-colors min-w-0 max-w-xs group"
          :class="activeTabId === tab.id ? 'bg-white border-b-2 border-blue-600' : ''" @click="switchTab(tab.id)">

          <span class="text-sm truncate flex-1">{{ tab.title }}</span>

          <!-- Close button -->
          <button v-if="tabs.length > 1" @click.stop="closeTab(tab.id)"
            class="opacity-0 group-hover:opacity-100 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded p-0.5 flex-shrink-0 transition-opacity"
            title="Close tab">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Add new tab button -->
        <button @click="handleAddNewTab(0)"
          class="px-3 py-2.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 transition-colors flex-shrink-0"
          title="New tab">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
        </button>
      </div>

      <!-- ==================== CARD COMPONENT ==================== -->
      <div class="flex-1 overflow-hidden">
        <Card ref="cardRef" :key="activeTab.id" :title="activeTab.title" :defaultUrl="activeTab.url"
          :defaultMethod="activeTab.method" :defaultBody="activeTab.body" :requestId="activeTab.requestId"
          :collectionId="activeTab.collectionId" :bodyId="currentBodyId" :bodies="activeTab.bodies || []"
          :dataBaseTest="activeTab.dataBaseTest || null" @requestSaved="handleRequestSaved" />
      </div>
    </div>

    <ExportImportModal v-if="showExportImportModal" @close="handleCloseExportImport" @imported="handleImported" />
  </div>
</template>
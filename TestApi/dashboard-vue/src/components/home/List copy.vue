<script setup lang="ts">
import { ref } from 'vue'

interface TestData {
  id: string
  name: string
  content: string
}

interface RequestItem {
  id: string
  name: string
  method: string
  testData: TestData[]
}

interface Collection {
  id: string
  name: string
  requests: RequestItem[]
}

// ==== GIẢ DỮ LIỆU MẪU ====
const collections = ref<Collection[]>([
  {
    id: '1',
    name: 'SFIN-INVOICE',
    requests: [
      {
        id: 'r1',
        name: 'Lấy mã truy cập',
        method: 'POST',
        testData: [
          { id: 't1', name: 'data_1.json', content: '{ "user": "test1" }' },
          { id: 't2', name: 'data_2.json', content: '{ "user": "test2" }' }
        ]
      },
      {
        id: 'r2',
        name: 'Danh sách hóa đơn',
        method: 'GET',
        testData: [
          { id: 't3', name: 'default.json', content: '{ "page": 1 }' },
          { id: 't4', name: 'filter.json', content: '{ "filter": true }' }
        ]
      }
    ]
  },
  {
    id: '2',
    name: 'USER-MANAGER',
    requests: [
      {
        id: 'r3',
        name: 'Đăng nhập',
        method: 'POST',
        testData: [
          { id: 't5', name: 'login_success.json', content: '{ "ok": true }' },
          { id: 't6', name: 'login_fail.json', content: '{ "ok": false }' }
        ]
      }
    ]
  }
])

// ==== STATE ====
const selectedCollection = ref<string | null>(null)
const selectedRequest = ref<string | null>(null)
const selectedTestData = ref<string | null>(null)

// ==== METHODS ====
const toggleCollection = (id: string) => {
  selectedCollection.value = selectedCollection.value === id ? null : id
  selectedRequest.value = null
  selectedTestData.value = null
}

const toggleRequest = (id: string) => {
  selectedRequest.value = selectedRequest.value === id ? null : id
  selectedTestData.value = null
}
</script>

<template>
  <div class="overflow-y-auto divide-y divide-default p-2 text-sm">
    <!-- CẤP 1: COLLECTION -->
    <div v-for="col in collections" :key="col.id" class="py-2">
      <div
        class="flex items-center justify-between cursor-pointer hover:bg-gray-50 px-3 py-2 rounded-md border-l-2 transition-colors"
        :class="selectedCollection === col.id ? 'border-primary bg-primary/10' : 'border-transparent'"
        @click="toggleCollection(col.id)"
      >
        <div class="font-semibold text-gray-800">
          📁 {{ col.name }}
        </div>
        <span class="text-gray-400 text-xs">{{ col.requests.length }} requests</span>
      </div>

      <!-- CẤP 2: REQUESTS -->
      <div
        v-if="selectedCollection === col.id"
        class="pl-6 border-l border-gray-200 ml-2 mt-2 space-y-1"
      >
        <div v-for="req in col.requests" :key="req.id">
          <div
            class="cursor-pointer px-2 py-2 hover:bg-gray-50 rounded-md flex items-center justify-between"
            :class="selectedRequest === req.id ? 'bg-blue-50' : ''"
            @click="toggleRequest(req.id)"
          >
            <div class="flex items-center gap-2">
              <span
                class="text-xs font-semibold"
                :class="req.method === 'POST' ? 'text-green-600' : 'text-blue-600'"
              >
                {{ req.method }}
              </span>
              <span class="text-gray-800">{{ req.name }}</span>
            </div>
            <span class="text-gray-400 text-xs">{{ req.testData.length }} data</span>
          </div>

          <!-- CẤP 3: TEST DATA (nằm NGAY DƯỚI request) -->
          <div
            v-if="selectedRequest === req.id"
            class="pl-6 mt-1"
          >
            <div
              v-for="data in req.testData"
              :key="data.id"
              class="text-gray-600 hover:text-primary cursor-pointer py-1"
              :class="selectedTestData === data.id ? 'font-semibold text-blue-600' : ''"
              @click.stop="selectedTestData = data.id"
            >
              📄 {{ data.name }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.border-primary {
  border-color: #3b82f6 !important;
}
.bg-primary\/10 {
  background-color: rgba(59, 130, 246, 0.1);
}
.text-primary {
  color: #3b82f6;
}
</style>

<script setup lang="ts">
import { ref, nextTick } from 'vue'
import List from '../components/home/List.vue'
import Card from '../components/home/Card.vue'

const selectedBody = ref('{}')
const selectedUrl = ref('')
const selectedMethod = ref('POST')
const cardRef = ref<any>(null)

const onSelectTestData = (payload: { content: string; name: string }) => {
  console.log('✅ Index: Received test data:', payload)
  selectedBody.value = payload.content
  
  // THÊM: Clear response khi chọn test data mới
  if (cardRef.value?.clearResponse) {
    cardRef.value.clearResponse()
  }
  
  nextTick(() => {
    cardRef.value?.setActiveTab?.('Body')
    cardRef.value?.focusBody?.()
  })
}

const onSelectRequest = (payload: { url: string; method: string; name: string }) => {
  console.log('🔗 Index: Received request:', payload)
  selectedUrl.value = payload.url
  selectedMethod.value = payload.method
  
  // THÊM: Clear response khi chọn request mới
  if (cardRef.value?.clearResponse) {
    cardRef.value.clearResponse()
  }
}
</script>

<template>
  <!-- LEFT PANEL - List -->
  <UDashboardPanel
    id="inbox-1"
    :default-size="25"
    :min-size="20"
    :max-size="30"
    resizable
  >
    <UDashboardNavbar>
      <template #leading>
        <UDashboardSidebarCollapse />
      </template>
    </UDashboardNavbar>

    <List 
      @selectTestData="onSelectTestData" 
      @selectRequest="onSelectRequest"
    />
  </UDashboardPanel>

  <!-- RIGHT PANEL - Card -->
  <UDashboardPanel
    id="inbox-2"
    :min-size="70"
    class="overflow-hidden"
  >
    <Card 
      ref="cardRef" 
      :defaultBody="selectedBody"
      :defaultUrl="selectedUrl"
      :defaultMethod="selectedMethod"
    />
  </UDashboardPanel>
</template>
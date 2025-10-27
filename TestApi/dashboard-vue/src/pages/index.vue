<script setup lang="ts">
import { ref } from 'vue'
import List from '../components/home/List.vue'
import Card from '../components/home/Card.vue'

const selectedUrl = ref('')
const selectedMethod = ref('POST')
const selectedBody = ref('{}')
const cardKey = ref(0) // Force re-render key

// Nhận event từ List khi click request
const handleSelectRequest = (payload: any) => {
  console.log('📨 Index received selectRequest:', payload)
  
  selectedUrl.value = payload.url
  selectedMethod.value = payload.method
  
  // ✅ Parse và format body content
  if (payload.body?.content) {
    try {
      const parsed = JSON.parse(payload.body.content)
      selectedBody.value = JSON.stringify(parsed, null, 2)
    } catch (error) {
      // Nếu không parse được, dùng raw content
      selectedBody.value = payload.body.content
    }
  } else {
    selectedBody.value = '{}'
  }
  
  // Force re-render Card (optional, nhưng đảm bảo update)
  cardKey.value++
  
  console.log('✅ Updated body:', selectedBody.value)
}
</script>

<template>
  <div class="flex h-screen">
    <!-- Sidebar: Collection List -->
    <div class="w-80 border-r border-gray-200">
      <List 
        @selectRequest="handleSelectRequest"
      />
    </div>
    
    <!-- Main: API Request Card -->
    <div class="flex-1">
      <Card 
        :key="cardKey"
        :defaultUrl="selectedUrl"
        :defaultMethod="selectedMethod"
        :defaultBody="selectedBody"
      />
    </div>
  </div>
</template>
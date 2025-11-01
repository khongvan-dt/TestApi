<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useToast } from "primevue/usetoast"
import {
  getMySQLConnections,
  saveSQLConnection,
  testSQLConnection,
  deleteSQLConnection,
  toggleSQLConnectionStatus,
  type SQLConnection,
  type CreateSQLConnectionDto
} from '../../composables/sqlConnectionService'

// ==================== STATE ====================
const toast = useToast()
const connections = ref<SQLConnection[]>([])
const loading = ref(false)
const showModal = ref(false)
const isEditing = ref(false)
const testingConnection = ref(false)
const savingConnection = ref(false)

const formData = ref<CreateSQLConnectionDto>({
  name: '',
  connectString: '',
  isActive: true
})

// ==================== API FUNCTIONS ====================
async function fetchConnections() {
  loading.value = true
  try {
    connections.value = await getMySQLConnections()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to fetch connections',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

async function saveConnection() {
  if (!formData.value.name || !formData.value.connectString) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Name and Connection String are required',
      life: 3000
    })
    return
  }

  if (formData.value.name.length > 200) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Name must not exceed 200 characters',
      life: 3000
    })
    return
  }

  if (formData.value.connectString.length > 1000) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Connection string must not exceed 1000 characters',
      life: 3000
    })
    return
  }

  savingConnection.value = true
  try {
    await saveSQLConnection(formData.value)
    
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: isEditing.value ? 'Connection updated successfully' : 'Connection created successfully',
      life: 3000
    })

    showModal.value = false
    resetForm()
    await fetchConnections()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to save connection',
      life: 3000
    })
  } finally {
    savingConnection.value = false
  }
}

async function handleTestConnection(connectionString?: string) {
  const testString = connectionString || formData.value.connectString

  if (!testString) {
    toast.add({
      severity: 'warn',
      summary: 'Validation Error',
      detail: 'Connection String is required',
      life: 3000
    })
    return
  }

  testingConnection.value = true
  try {
    const result = await testSQLConnection(testString)
    
    toast.add({
      severity: result.success ? 'success' : 'error',
      summary: result.success ? 'Success' : 'Connection Failed',
      detail: result.message,
      life: 3000
    })
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to test connection',
      life: 3000
    })
  } finally {
    testingConnection.value = false
  }
}

async function handleDeleteConnection(id: number) {
  if (!confirm('Are you sure you want to delete this connection?')) {
    return
  }

  loading.value = true
  try {
    await deleteSQLConnection(id)
    
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: 'Connection deleted successfully',
      life: 3000
    })

    await fetchConnections()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to delete connection',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

async function handleToggleActive(connection: SQLConnection) {
  loading.value = true
  try {
    await toggleSQLConnectionStatus(connection)
    
    toast.add({
      severity: 'success',
      summary: 'Success',
      detail: `Connection ${!connection.isActive ? 'activated' : 'deactivated'} successfully`,
      life: 3000
    })

    await fetchConnections()
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Error',
      detail: error.message || 'Failed to update connection',
      life: 3000
    })
  } finally {
    loading.value = false
  }
}

// ==================== HELPERS ====================
function openCreateModal() {
  isEditing.value = false
  resetForm()
  showModal.value = true
}

function openEditModal(connection: SQLConnection) {
  isEditing.value = true
  formData.value = {
    id: connection.id,
    name: connection.name,
    connectString: connection.connectString,
    isActive: connection.isActive
  }
  showModal.value = true
}

function resetForm() {
  formData.value = {
    name: '',
    connectString: '',
    isActive: true
  }
}

function closeModal() {
  showModal.value = false
  resetForm()
}

function formatDate(dateString?: string) {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// ==================== LIFECYCLE ====================
onMounted(() => {
  fetchConnections()
})
</script>

<template>
  <div class="max-w-7xl mx-auto p-6">
    <!-- ==================== HEADER ==================== -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">SQL Database Connections</h1>
        <p class="text-sm text-gray-600 mt-1">Manage your SQL Server database connections</p>
      </div>

      <UButton 
        icon="i-heroicons-plus" 
        size="lg" 
        @click.stop="openCreateModal">
        Add Connection
      </UButton>
    </div>

    <!-- ==================== LOADING STATE ==================== -->
    <UPageCard v-if="loading && connections.length === 0" variant="subtle">
      <div class="flex items-center justify-center py-12">
        <UIcon name="i-heroicons-arrow-path" class="w-8 h-8 animate-spin text-blue-600" />
        <span class="ml-3 text-gray-600">Loading connections...</span>
      </div>
    </UPageCard>

    <!-- ==================== EMPTY STATE ==================== -->
    <div v-else-if="connections.length === 0" class="text-center py-12">
      <UIcon name="i-heroicons-circle-stack" class="w-16 h-16 mx-auto text-gray-400 mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">No connections yet</h3>
      <p class="text-gray-600 mb-4">Get started by creating your first database connection</p>
      <UButton @click.stop="openCreateModal">
        Add Your First Connection
      </UButton>
    </div>

    <!-- ==================== CONNECTIONS LIST ==================== -->
    <div v-else class="space-y-4">
      <UPageCard 
        v-for="connection in connections" 
        :key="connection.id" 
        variant="subtle"
        class="hover:shadow-md transition-shadow">

        <div class="flex items-start justify-between">
          <!-- Left: Connection Info -->
          <div class="flex-1">
            <div class="flex items-center gap-3 mb-2">
              <UIcon name="i-heroicons-circle-stack" class="w-6 h-6 text-blue-600" />
              <h3 class="text-lg font-semibold text-gray-900">{{ connection.name }}</h3>
              <UBadge 
                 variant="subtle">
                {{ connection.isActive ? 'Active' : 'Inactive' }}
              </UBadge>
            </div>

            <div class="ml-9 space-y-1">
              <div class="flex items-center gap-2 text-sm text-gray-600">
                <UIcon name="i-heroicons-link" class="w-4 h-4" />
                <code class="bg-gray-100 px-2 py-1 rounded text-xs font-mono">
                  {{ connection.connectString.length > 60 
                    ? connection.connectString.substring(0, 60) + '...' 
                    : connection.connectString }}
                </code>
              </div>

              <div class="flex items-center gap-4 text-xs text-gray-500">
                <span>Created: {{ formatDate(connection.createdAt) }}</span>
                <span v-if="connection.updatedAt">
                  Updated: {{ formatDate(connection.updatedAt) }}
                </span>
              </div>
            </div>
          </div>

          <!-- Right: Actions -->
          <div class="flex items-center gap-2 ml-4">
            <UTooltip text="Test Connection">
              <UButton 
                icon="i-heroicons-bolt" 
                 variant="ghost" 
                size="sm" 
                :loading="testingConnection"
                @click.stop="handleTestConnection(connection.connectString)" />
            </UTooltip>

            <UTooltip text="Edit">
              <UButton 
                icon="i-heroicons-pencil-square" 
                 variant="ghost" 
                size="sm"
                @click.stop="openEditModal(connection)" />
            </UTooltip>

            <UTooltip :text="connection.isActive ? 'Deactivate' : 'Activate'">
              <UButton 
                :icon="connection.isActive ? 'i-heroicons-pause-circle' : 'i-heroicons-play-circle'"
                 variant="ghost" 
                size="sm"
                @click.stop="handleToggleActive(connection)" />
            </UTooltip>

            <UTooltip text="Delete">
              <UButton 
                icon="i-heroicons-trash" 
                 variant="ghost" 
                size="sm"
                @click.stop="handleDeleteConnection(connection.id!)" />
            </UTooltip>
          </div>
        </div>
      </UPageCard>
    </div>

    <!-- ==================== MODAL: CREATE/EDIT ==================== -->
    <UModal 
      v-model="showModal" 
      :prevent-close="savingConnection">
      <UCard @click.stop>
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900">
              {{ isEditing ? 'Edit Connection' : 'Add New Connection' }}
            </h3>
            <UButton 
              icon="i-heroicons-x-mark" 
               variant="ghost"
              :disabled="savingConnection"
              @click.stop="closeModal" />
          </div>
        </template>

        <div class="space-y-5" @click.stop>
          <!-- Connection Name -->
          <div class="space-y-2">
            <label class="block text-sm font-medium text-gray-900">
              Connection Name 
              <span class="text-red-500">*</span>
            </label>
            <UInput 
              v-model="formData.name" 
              placeholder="e.g. Production DB, Local Development" 
              icon="i-heroicons-identification"
              size="lg"
              :maxlength="200"
              @click.stop />
            <p class="text-xs text-gray-500">
              Maximum 200 characters
            </p>
          </div>

          <!-- Connection String -->
          <div class="space-y-2">
            <label class="block text-sm font-medium text-gray-900">
              Connection String 
              <span class="text-red-500">*</span>
            </label>
            <UTextarea 
              v-model="formData.connectString"
              placeholder="Server=localhost;Database=MyDB;User Id=sa;Password=****;TrustServerCertificate=True" 
              :rows="5"
              size="lg"
              class="font-mono text-sm"
              :maxlength="1000"
              @click.stop />
            <div class="text-xs text-gray-500 space-y-1">
              <p>Example: Server=localhost;Database=TestDB;User Id=sa;Password=123456;TrustServerCertificate=True</p>
              <p>Maximum 1000 characters</p>
            </div>
          </div>

          <!-- Active Status -->
          <div class="space-y-2">
            <label class="block text-sm font-medium text-gray-900">
              Status
            </label>
            <div class="flex items-center gap-3 p-3 bg-gray-50 rounded-lg" @click.stop>
              <USwitch 
                v-model="formData.isActive" 
                size="lg"
                @click.stop />
              <div>
                <p class="text-sm font-medium text-gray-900">
                  {{ formData.isActive ? 'Active' : 'Inactive' }}
                </p>
                <p class="text-xs text-gray-500">
                  {{ formData.isActive 
                    ? 'This connection is available for use' 
                    : 'This connection is disabled' }}
                </p>
              </div>
            </div>
          </div>

          <!-- Test Connection Button -->
          <div class="pt-2">
            <UButton 
              icon="i-heroicons-bolt" 
               variant="outline" 
              block
              size="lg"
              :loading="testingConnection"
              :disabled="!formData.connectString"
              @click.stop="handleTestConnection()">
              {{ testingConnection ? 'Testing...' : 'Test Connection' }}
            </UButton>
          </div>
        </div>

        <template #footer>
          <div class="flex items-center justify-end gap-3">
            <UButton 
              label="Cancel" 
               variant="ghost"
              :disabled="savingConnection"
              @click.stop="closeModal" />

            <UButton 
              :label="isEditing ? 'Update Connection' : 'Create Connection'" 
              icon="i-heroicons-check"
              :loading="savingConnection"
              :disabled="!formData.name || !formData.connectString"
              @click.stop="saveConnection" />
          </div>
        </template>
      </UCard>
    </UModal>
  </div>
</template>


<style scoped>
/* Custom scrollbar */
:deep(.overflow-y-auto) {
  scrollbar-width: thin;
  scrollbar-color: #cbd5e0 #f7fafc;
}

:deep(.overflow-y-auto::-webkit-scrollbar) {
  width: 6px;
}

:deep(.overflow-y-auto::-webkit-scrollbar-track) {
  background: #f7fafc;
}

:deep(.overflow-y-auto::-webkit-scrollbar-thumb) {
  background-color: #cbd5e0;
  border-radius: 3px;
}

:deep(.overflow-y-auto::-webkit-scrollbar-thumb:hover) {
  background-color: #a0aec0;
}
</style>
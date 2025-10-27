<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useAuth } from '../composables/useAuth'
import { useUserData } from '../composables/useUserData'

const { user } = useAuth()
const { loading, error, data, fetchUserData, loadCachedData } = useUserData()

// Load cached data first for instant display
loadCachedData()

// Then fetch fresh data
onMounted(async () => {
  if (!data.value) {
    await fetchUserData()
  }
})

const recentRequests = computed(() => {
  return data.value?.requests.slice(0, 5) || []
})

const activeEnvironment = computed(() => {
  return data.value?.environments.find(e => e.isActive)
})
</script>

<template>
  <div class="min-h-screen bg-gray-50 p-6">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900">
        Welcome back, {{ user?.username || 'User' }}! 👋
      </h1>
      <p class="text-gray-600 mt-2">{{ user?.email }}</p>
    </div>

    <!-- Loading State -->
    <div v-if="loading && !data" class="flex items-center justify-center py-12">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
        <p class="text-gray-600">Loading your data...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-6">
      <div class="flex items-start gap-3">
        <svg class="w-6 h-6 text-red-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div>
          <h3 class="text-lg font-semibold text-red-900">Error loading data</h3>
          <p class="text-red-700 mt-1">{{ error }}</p>
          <button
            @click="fetchUserData"
            class="mt-3 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
          >
            Try Again
          </button>
        </div>
      </div>
    </div>

    <!-- Data Display -->
    <div v-else-if="data" class="space-y-6">
      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-4">
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">Workspaces</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">{{ data.summary.totalWorkspaces }}</p>
            </div>
            <div class="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">Collections</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">{{ data.summary.totalCollections }}</p>
            </div>
            <div class="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">Requests</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">{{ data.summary.totalRequests }}</p>
            </div>
            <div class="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z" />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">Environments</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">{{ data.summary.totalEnvironments }}</p>
            </div>
            <div class="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center">
              <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-600">Executions</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">{{ data.summary.totalExecutions }}</p>
            </div>
            <div class="w-12 h-12 bg-red-100 rounded-lg flex items-center justify-center">
              <svg class="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      <!-- Active Environment -->
      <div v-if="activeEnvironment" class="bg-white rounded-lg shadow p-6">
        <h2 class="text-lg font-semibold text-gray-900 mb-3">Active Environment</h2>
        <div class="flex items-center gap-3">
          <div class="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
          <span class="font-medium text-gray-900">{{ activeEnvironment.name }}</span>
        </div>
      </div>

      <!-- Recent Requests -->
      <div class="bg-white rounded-lg shadow">
        <div class="px-6 py-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Recent Requests</h2>
        </div>
        <div class="p-6">
          <div v-if="recentRequests.length === 0" class="text-center text-gray-500 py-8">
            No requests yet. Create your first API request!
          </div>
          <div v-else class="space-y-3">
            <div
              v-for="request in recentRequests"
              :key="request.id"
              class="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:border-blue-500 hover:bg-blue-50 transition-all cursor-pointer"
            >
              <div class="flex items-center gap-4">
                <span 
                  class="px-2 py-1 text-xs font-bold rounded"
                  :class="{
                    'bg-green-100 text-green-700': request.method === 'GET',
                    'bg-blue-100 text-blue-700': request.method === 'POST',
                    'bg-yellow-100 text-yellow-700': request.method === 'PUT',
                    'bg-red-100 text-red-700': request.method === 'DELETE',
                    'bg-purple-100 text-purple-700': request.method === 'PATCH'
                  }"
                >
                  {{ request.method }}
                </span>
                <div>
                  <p class="font-medium text-gray-900">{{ request.name }}</p>
                  <p class="text-sm text-gray-500 mt-1">{{ request.url }}</p>
                </div>
              </div>
              <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      <!-- Workspaces List -->
      <div class="bg-white rounded-lg shadow">
        <div class="px-6 py-4 border-b border-gray-200">
          <h2 class="text-lg font-semibold text-gray-900">Your Workspaces</h2>
        </div>
        <div class="p-6">
          <div v-if="data.workspaces.length === 0" class="text-center text-gray-500 py-8">
            No workspaces yet. Create your first workspace!
          </div>
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div
              v-for="workspace in data.workspaces"
              :key="workspace.id"
              class="border border-gray-200 rounded-lg p-5 hover:border-blue-500 hover:shadow-md transition-all cursor-pointer"
            >
              <h3 class="font-semibold text-gray-900 text-lg">{{ workspace.name }}</h3>
              <p class="text-sm text-gray-600 mt-2">{{ workspace.description || 'No description' }}</p>
              <p class="text-xs text-gray-400 mt-3">
                Created: {{ new Date(workspace.createdAt).toLocaleDateString() }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Refresh Button -->
      <div class="flex justify-center">
        <button
          @click="fetchUserData"
          :disabled="loading"
          class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
        >
          <svg v-if="loading" class="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          {{ loading ? 'Refreshing...' : 'Refresh Data' }}
        </button>
      </div>
    </div>
  </div>
</template>
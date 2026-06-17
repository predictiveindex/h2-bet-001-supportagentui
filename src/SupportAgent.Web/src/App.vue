<template>
  <div class="app-layout">
    <!-- Nav -->
    <nav class="nav">
      <span class="brand">Support<span class="accent">Agent</span></span>
      <button class="btn-new-chat" @click="newChat" :disabled="loading">
        <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M7 1v12M1 7h12"/></svg>
        New Chat
      </button>
    </nav>

    <!-- Chat window -->
    <ChatWindow :messages="messages" :loading="loading" />

    <!-- Input -->
    <ChatInput @send="sendMessage" :disabled="loading" />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ChatWindow from './components/ChatWindow.vue'
import ChatInput from './components/ChatInput.vue'

const messages = ref([])
const loading = ref(false)

function newChat() {
  messages.value = []
}

async function sendMessage(text) {
  if (!text.trim() || loading.value) return

  messages.value.push({ role: 'user', content: text })
  loading.value = true

  try {
    const res = await fetch('/api/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ messages: messages.value }),
    })

    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    const data = await res.json()
    messages.value.push({ role: 'assistant', content: data.reply })
  } catch (err) {
    messages.value.push({
      role: 'assistant',
      content: 'Sorry, something went wrong. Please try again.',
    })
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.app-layout {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--bg);
}

.nav {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(6, 1, 15, 0.92);
  backdrop-filter: blur(14px);
  border-bottom: 1px solid var(--border);
  padding: 0 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 60px;
  flex-shrink: 0;
}

.brand {
  font-size: 20px;
  font-weight: 700;
  letter-spacing: -0.03em;
  color: var(--text);
}

.accent {
  color: var(--accent);
}

.btn-new-chat {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  background: transparent;
  border: 1px solid var(--border);
  border-radius: 8px;
  color: var(--muted);
  cursor: pointer;
  transition: border-color 0.18s, color 0.18s;
}

.btn-new-chat:hover:not(:disabled) {
  border-color: var(--accent);
  color: var(--text);
}

.btn-new-chat:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}
</style>

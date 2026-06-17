<template>
  <div class="chat-window" ref="windowEl">
    <!-- Empty state -->
    <div v-if="messages.length === 0 && !loading" class="empty-state">
      <div class="empty-glow"></div>
      <div class="empty-icon">
        <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
          <circle cx="20" cy="20" r="19" stroke="#2d1a4a" stroke-width="2"/>
          <path d="M12 20h16M20 12l8 8-8 8" stroke="#7C3AED" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </div>
      <p class="empty-title">How can I help you today?</p>
      <p class="empty-sub">Ask me anything and I'll do my best to assist.</p>
    </div>

    <!-- Messages -->
    <div v-else class="messages">
      <MessageBubble
        v-for="(msg, i) in messages"
        :key="i"
        :message="msg"
      />

      <!-- Typing indicator -->
      <div v-if="loading" class="bubble bubble--ai bubble--typing">
        <span></span><span></span><span></span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue'
import MessageBubble from './MessageBubble.vue'

const props = defineProps({
  messages: { type: Array, required: true },
  loading: { type: Boolean, default: false },
})

const windowEl = ref(null)

function scrollToBottom() {
  nextTick(() => {
    if (windowEl.value) {
      windowEl.value.scrollTop = windowEl.value.scrollHeight
    }
  })
}

watch(() => props.messages.length, scrollToBottom)
watch(() => props.loading, scrollToBottom)
</script>

<style scoped>
.chat-window {
  flex: 1;
  overflow-y: auto;
  padding: 24px 16px;
  scroll-behavior: smooth;
}

.chat-window::-webkit-scrollbar {
  width: 4px;
}
.chat-window::-webkit-scrollbar-track {
  background: transparent;
}
.chat-window::-webkit-scrollbar-thumb {
  background: var(--border);
  border-radius: 4px;
}

/* ── Empty state ─────────────────────── */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  text-align: center;
  position: relative;
  padding: 40px 24px;
}

.empty-glow {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 400px;
  height: 400px;
  background: radial-gradient(ellipse at center, rgba(124, 58, 237, 0.12) 0%, transparent 68%);
  pointer-events: none;
}

.empty-icon {
  margin-bottom: 20px;
  position: relative;
  z-index: 1;
}

.empty-title {
  font-family: 'Outfit', sans-serif;
  font-size: 22px;
  font-weight: 700;
  color: var(--text);
  margin-bottom: 8px;
  position: relative;
  z-index: 1;
}

.empty-sub {
  font-size: 14px;
  color: var(--muted);
  max-width: 320px;
  position: relative;
  z-index: 1;
}

/* ── Messages ────────────────────────── */
.messages {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-width: 760px;
  margin: 0 auto;
  width: 100%;
}

/* ── Typing indicator ────────────────── */
.bubble {
  padding: 12px 16px;
  border-radius: 16px;
  max-width: 72%;
  line-height: 1.6;
  font-size: 15px;
}

.bubble--ai {
  align-self: flex-start;
  background: var(--bg2);
  border: 1px solid var(--border);
  border-left: 3px solid var(--accent2);
}

.bubble--typing {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 14px 18px;
}

.bubble--typing span {
  display: inline-block;
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--muted);
  animation: bounce 1.2s infinite ease-in-out;
}

.bubble--typing span:nth-child(2) { animation-delay: 0.2s; }
.bubble--typing span:nth-child(3) { animation-delay: 0.4s; }

@keyframes bounce {
  0%, 60%, 100% { transform: translateY(0); opacity: 0.5; }
  30% { transform: translateY(-5px); opacity: 1; }
}
</style>

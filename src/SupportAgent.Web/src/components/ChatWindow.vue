<template>
  <div class="chat-window" ref="windowEl">
    <!-- Empty state -->
    <div v-if="exchanges.length === 0 && !loading" class="empty-state">
      <div class="empty-glow"></div>
      <div class="empty-icon">
        <svg width="40" height="40" viewBox="0 0 40 40" fill="none">
          <circle cx="20" cy="20" r="19" stroke="#2d1a4a" stroke-width="2"/>
          <path d="M12 20h16M20 12l8 8-8 8" stroke="#7C3AED" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </div>
      <p class="empty-title">Hi, I'm FinEdge Support. What can I help you with?</p>
      <div class="example-prompts">
        <button class="example-btn" @click="$emit('example', 'Why did my budgeting coach move to premium?')">Why did my budgeting coach move to premium?</button>
        <button class="example-btn" @click="$emit('example', 'How do I cancel my plan?')">How do I cancel my plan?</button>
        <button class="example-btn" @click="$emit('example', 'What\'s included in premium?')">What's included in premium?</button>
      </div>
    </div>

    <!-- Exchanges -->
    <div v-else class="messages">
      <template v-for="(exchange, i) in exchanges" :key="i">
        <!-- User message -->
        <MessageBubble :message="{ role: 'user', content: exchange.user }" />

        <!-- Rate limit message -->
        <div v-if="exchange.rateLimited" class="rate-limit-msg">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none"><circle cx="8" cy="8" r="7" stroke="#f59e0b" stroke-width="1.5"/><path d="M8 5v3.5M8 10.5v.5" stroke="#f59e0b" stroke-width="1.5" stroke-linecap="round"/></svg>
          Daily request limit reached. Please try again tomorrow.
        </div>

        <!-- A/B responses -->
        <ResponsePair
          v-else-if="exchange.replyA || exchange.replyB"
          :exchange="exchange"
          :index="i"
          @vote="(idx, choice) => $emit('vote', idx, choice)"
        />
      </template>

      <!-- Typing indicator -->
      <div v-if="loading" class="ab-typing">
        <div class="typing-card">
          <span class="label label-a">A</span>
          <div class="dots"><span></span><span></span><span></span></div>
        </div>
        <div class="typing-card">
          <span class="label label-b">B</span>
          <div class="dots"><span></span><span></span><span></span></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue'
import MessageBubble from './MessageBubble.vue'
import ResponsePair from './ResponsePair.vue'

const props = defineProps({
  exchanges: { type: Array, required: true },
  loading: { type: Boolean, default: false },
})

defineEmits(['vote', 'example'])

const windowEl = ref(null)

function scrollToBottom() {
  nextTick(() => {
    if (windowEl.value) windowEl.value.scrollTop = windowEl.value.scrollHeight
  })
}

watch(() => props.exchanges.length, scrollToBottom)
watch(() => props.loading, scrollToBottom)
</script>

<style scoped>
.chat-window {
  flex: 1;
  overflow-y: auto;
  padding: 24px 32px;
  scroll-behavior: smooth;
}

.chat-window::-webkit-scrollbar { width: 4px; }
.chat-window::-webkit-scrollbar-track { background: transparent; }
.chat-window::-webkit-scrollbar-thumb { background: var(--border); border-radius: 4px; }

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
  top: 50%; left: 50%;
  transform: translate(-50%, -50%);
  width: 400px; height: 400px;
  background: radial-gradient(ellipse at center, rgba(124,58,237,0.12) 0%, transparent 68%);
  pointer-events: none;
}

.empty-icon { margin-bottom: 20px; position: relative; z-index: 1; }

.empty-title {
  font-family: 'Outfit', sans-serif;
  font-size: 22px; font-weight: 700;
  color: var(--text); margin-bottom: 8px;
  position: relative; z-index: 1;
}

.example-prompts {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 20px;
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 400px;
}

.example-btn {
  background: var(--bg3);
  border: 1px solid var(--border);
  border-radius: 10px;
  color: var(--muted);
  font-family: inherit;
  font-size: 13px;
  padding: 10px 16px;
  cursor: pointer;
  text-align: left;
  transition: border-color 0.18s, color 0.18s;
}

.example-btn:hover {
  border-color: var(--accent);
  color: var(--text);
}

/* ── Messages ────────────────────────── */
.messages {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-width: 1400px;
  margin: 0 auto;
  width: 100%;
}

/* ── Rate limit notice ───────────────── */
.rate-limit-msg {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background: rgba(245, 158, 11, 0.08);
  border: 1px solid rgba(245, 158, 11, 0.3);
  border-radius: 12px;
  color: #f59e0b;
  font-size: 14px;
}

/* ── Typing indicator ────────────────── */
.ab-typing {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.typing-card {
  background: var(--bg2);
  border: 1px solid var(--border);
  border-radius: 14px;
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 12px;
}

.typing-card .label {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  font-size: 18px;
  font-weight: 800;
  font-family: 'Outfit', sans-serif;
  flex-shrink: 0;
}

.typing-card .label-a {
  background: rgba(124, 58, 237, 0.25);
  color: #c084fc;
  border: 2px solid rgba(124, 58, 237, 0.6);
}

.typing-card .label-b {
  background: rgba(128, 255, 219, 0.12);
  color: var(--accent2);
  border: 2px solid rgba(128, 255, 219, 0.4);
}

.dots {
  display: flex;
  gap: 5px;
}

.dots span {
  display: inline-block;
  width: 7px; height: 7px;
  border-radius: 50%;
  background: var(--muted);
  animation: bounce 1.2s infinite ease-in-out;
}

.dots span:nth-child(2) { animation-delay: 0.2s; }
.dots span:nth-child(3) { animation-delay: 0.4s; }

@keyframes bounce {
  0%, 60%, 100% { transform: translateY(0); opacity: 0.5; }
  30% { transform: translateY(-5px); opacity: 1; }
}
</style>

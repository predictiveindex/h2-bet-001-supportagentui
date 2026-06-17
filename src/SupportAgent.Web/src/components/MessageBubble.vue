<template>
  <div class="bubble-row" :class="isUser ? 'row--user' : 'row--ai'">
    <div v-if="!isUser" class="avatar avatar--ai">AI</div>
    <div class="bubble" :class="isUser ? 'bubble--user' : 'bubble--ai'">
      <p v-for="(line, i) in lines" :key="i" :class="{ 'para-gap': i > 0 }">{{ line }}</p>
    </div>
    <div v-if="isUser" class="avatar avatar--user">You</div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  message: { type: Object, required: true },
})

const isUser = computed(() => props.message.role === 'user')
const lines = computed(() => (props.message.content ?? '').split('\n').filter(Boolean))
</script>

<style scoped>
.bubble-row {
  display: flex;
  align-items: flex-end;
  gap: 10px;
}

.row--user {
  flex-direction: row-reverse;
}

.row--ai {
  flex-direction: row;
}

/* ── Avatar ──────────────────────────── */
.avatar {
  flex-shrink: 0;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  font-size: 10px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  letter-spacing: 0.02em;
}

.avatar--ai {
  background: rgba(124, 58, 237, 0.2);
  border: 1px solid var(--accent);
  color: var(--accent2);
}

.avatar--user {
  background: rgba(124, 58, 237, 0.3);
  border: 1px solid var(--accent);
  color: var(--text);
}

/* ── Bubble ──────────────────────────── */
.bubble {
  max-width: 70%;
  padding: 12px 16px;
  border-radius: 16px;
  font-size: 15px;
  line-height: 1.65;
  word-break: break-word;
}

.bubble--user {
  background: var(--accent);
  color: #fff;
  border-bottom-right-radius: 4px;
}

.bubble--ai {
  background: var(--bg2);
  border: 1px solid var(--border);
  border-left: 3px solid var(--accent2);
  color: var(--text);
  border-bottom-left-radius: 4px;
}

.para-gap {
  margin-top: 8px;
}
</style>

<template>
  <div class="input-bar">
    <div class="input-wrap">
      <textarea
        ref="textareaEl"
        v-model="text"
        class="input"
        placeholder="Tell us what's going on…"
        :disabled="disabled"
        rows="1"
        @keydown.enter.exact.prevent="submit"
        @keydown.enter.shift.exact="newLine"
        @input="autoResize"
      ></textarea>
      <button
        class="send-btn"
        :disabled="disabled || !text.trim()"
        @click="submit"
        aria-label="Send message"
      >
        <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
          <path d="M16 2L2 9l5 2 2 5 7-14z" fill="currentColor"/>
        </svg>
      </button>
    </div>
    <p class="hint">Enter to send · Shift+Enter for new line</p>
  </div>
</template>

<script setup>
import { ref, nextTick } from 'vue'

const props = defineProps({
  disabled: { type: Boolean, default: false },
})

const emit = defineEmits(['send'])

const text = ref('')
const textareaEl = ref(null)

function submit() {
  const msg = text.value.trim()
  if (!msg || props.disabled) return
  emit('send', msg)
  text.value = ''
  nextTick(() => {
    if (textareaEl.value) {
      textareaEl.value.style.height = 'auto'
    }
  })
}

function newLine() {
  text.value += '\n'
  nextTick(autoResize)
}

function autoResize() {
  const el = textareaEl.value
  if (!el) return
  el.style.height = 'auto'
  el.style.height = Math.min(el.scrollHeight, 160) + 'px'
}
</script>

<style scoped>
.input-bar {
  flex-shrink: 0;
  padding: 16px 16px 12px;
  background: rgba(6, 1, 15, 0.92);
  border-top: 1px solid var(--border);
  backdrop-filter: blur(14px);
}

.input-wrap {
  display: flex;
  align-items: flex-end;
  gap: 10px;
  max-width: 1400px;
  margin: 0 auto;
  background: var(--bg3);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 10px 12px;
  transition: border-color 0.18s, box-shadow 0.18s;
}

.input-wrap:focus-within {
  border-color: var(--accent);
  box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.18);
}

.input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  resize: none;
  font-family: inherit;
  font-size: 15px;
  color: var(--text);
  line-height: 1.5;
  min-height: 24px;
  max-height: 160px;
  overflow-y: auto;
}

.input::placeholder {
  color: var(--muted);
}

.input:disabled {
  opacity: 0.5;
}

.send-btn {
  flex-shrink: 0;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  border: none;
  background: linear-gradient(135deg, var(--accent) 0%, var(--accent2) 100%);
  color: #06010F;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.18s, transform 0.18s, box-shadow 0.18s;
}

.send-btn:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(124, 58, 237, 0.4);
}

.send-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.hint {
  text-align: center;
  font-size: 11px;
  color: var(--muted);
  margin-top: 8px;
  opacity: 0.6;
}
</style>

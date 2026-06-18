<template>
  <div class="response-pair">
    <!-- A/B response cards -->
    <div class="cards">
      <div class="card" :class="{ 'card--winner': exchange.voted === 'A', 'card--loser': exchange.voted === 'B' }">
        <div class="card-header">
          <span class="label label--a">A</span>
          <span v-if="exchange.voted === 'A'" class="voted-badge">
            <svg width="12" height="12" viewBox="0 0 12 12" fill="none"><path d="M2 6l3 3 5-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>
            Your pick
          </span>
        </div>
        <div class="card-body">
          <p v-for="(line, i) in linesA" :key="i" :class="{ 'para-gap': i > 0 }">{{ line }}</p>
        </div>
      </div>

      <div class="card" :class="{ 'card--winner': exchange.voted === 'B', 'card--loser': exchange.voted === 'A' }">
        <div class="card-header">
          <span class="label label--b">B</span>
          <span v-if="exchange.voted === 'B'" class="voted-badge">
            <svg width="12" height="12" viewBox="0 0 12 12" fill="none"><path d="M2 6l3 3 5-5" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>
            Your pick
          </span>
        </div>
        <div class="card-body">
          <p v-for="(line, i) in linesB" :key="i" :class="{ 'para-gap': i > 0 }">{{ line }}</p>
        </div>
      </div>
    </div>

    <!-- Vote buttons -->
    <div class="vote-row">
      <span class="vote-label">Which response was better?</span>
      <div class="vote-buttons">
        <button
          class="vote-btn vote-btn--a"
          :class="{ 'vote-btn--selected': exchange.voted === 'A', 'vote-btn--dim': exchange.voted === 'B' }"
          :disabled="!!exchange.voted"
          @click="$emit('vote', index, 'A')"
        >
          <svg width="14" height="14" viewBox="0 0 14 14" fill="none"><path d="M7 1l1.8 3.6 4 .6-2.9 2.8.7 4L7 10l-3.6 1.9.7-4L1.2 5.2l4-.6L7 1z" fill="currentColor"/></svg>
          A is better
        </button>
        <button
          class="vote-btn vote-btn--b"
          :class="{ 'vote-btn--selected': exchange.voted === 'B', 'vote-btn--dim': exchange.voted === 'A' }"
          :disabled="!!exchange.voted"
          @click="$emit('vote', index, 'B')"
        >
          <svg width="14" height="14" viewBox="0 0 14 14" fill="none"><path d="M7 1l1.8 3.6 4 .6-2.9 2.8.7 4L7 10l-3.6 1.9.7-4L1.2 5.2l4-.6L7 1z" fill="currentColor"/></svg>
          B is better
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  exchange: { type: Object, required: true },
  index: { type: Number, required: true },
})

defineEmits(['vote'])

const linesA = computed(() => (props.exchange.replyA ?? '').split('\n').filter(Boolean))
const linesB = computed(() => (props.exchange.replyB ?? '').split('\n').filter(Boolean))
</script>

<style scoped>
.response-pair {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

/* ── Cards ───────────────────────────── */
.cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.card {
  background: var(--bg2);
  border: 1px solid var(--border);
  border-radius: 14px;
  padding: 16px;
  transition: border-color 0.2s, opacity 0.2s;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.card--winner {
  border-color: var(--accent2);
  box-shadow: 0 0 0 1px rgba(128, 255, 219, 0.15);
}

.card--loser {
  opacity: 0.5;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.label {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  padding: 3px 8px;
  border-radius: 6px;
  flex-shrink: 0;
}

.label--a {
  background: rgba(124, 58, 237, 0.2);
  color: var(--accent2);
  border: 1px solid rgba(124, 58, 237, 0.4);
}

.label--b {
  background: rgba(128, 255, 219, 0.1);
  color: var(--accent2);
  border: 1px solid rgba(128, 255, 219, 0.25);
}

.voted-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  font-weight: 600;
  color: var(--accent2);
}

.card-body {
  font-size: 14px;
  line-height: 1.65;
  color: var(--text);
}

.para-gap {
  margin-top: 8px;
}

/* ── Vote row ────────────────────────── */
.vote-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.vote-label {
  font-size: 12px;
  color: var(--muted);
  flex-shrink: 0;
}

.vote-buttons {
  display: flex;
  gap: 8px;
}

.vote-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  font-size: 12px;
  font-weight: 600;
  font-family: inherit;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
  border: 1px solid transparent;
}

.vote-btn--a {
  background: rgba(124, 58, 237, 0.12);
  border-color: rgba(124, 58, 237, 0.3);
  color: var(--accent2);
}

.vote-btn--b {
  background: rgba(128, 255, 219, 0.07);
  border-color: rgba(128, 255, 219, 0.2);
  color: var(--accent2);
}

.vote-btn:hover:not(:disabled) {
  filter: brightness(1.2);
  transform: translateY(-1px);
}

.vote-btn--selected {
  border-color: var(--accent2);
  filter: brightness(1.3);
}

.vote-btn--dim {
  opacity: 0.35;
}

.vote-btn:disabled {
  cursor: default;
}

@media (max-width: 600px) {
  .cards {
    grid-template-columns: 1fr;
  }
}
</style>

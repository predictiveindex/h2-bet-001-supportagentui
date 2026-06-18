<template>
  <div class="response-pair">
    <!-- "Which is better?" header -->
    <div class="vote-header" v-if="!exchange.voted">
      <span class="vote-label">Which response was better?</span>
    </div>

    <!-- A/B response cards with vote buttons inside -->
    <div class="cards">
      <!-- Card A -->
      <div class="card" :class="{ 'card--winner': exchange.voted === 'A', 'card--loser': exchange.voted === 'B' }">
        <div class="card-header">
          <span class="badge badge--a">A</span>
          <span v-if="exchange.voted === 'A'" class="voted-badge">
            <svg width="13" height="13" viewBox="0 0 13 13" fill="none"><path d="M2 6.5l3.5 3.5 5.5-5.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
            Your pick
          </span>
        </div>
        <div class="card-body">
          <p v-for="(line, i) in linesA" :key="i" :class="{ 'para-gap': i > 0 }">{{ line }}</p>
        </div>
        <div class="card-vote">
          <button
            class="vote-btn vote-btn--a"
            :class="{ 'vote-btn--selected': exchange.voted === 'A', 'vote-btn--dim': exchange.voted === 'B' }"
            :disabled="!!exchange.voted"
            @click="$emit('vote', index, 'A')"
          >
            <svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M7 1l1.8 3.6 4 .6-2.9 2.8.7 4L7 10l-3.6 1.9.7-4L1.2 5.2l4-.6L7 1z" fill="currentColor"/></svg>
            A is better
          </button>
        </div>
      </div>

      <!-- Card B -->
      <div class="card" :class="{ 'card--winner': exchange.voted === 'B', 'card--loser': exchange.voted === 'A' }">
        <div class="card-header">
          <span class="badge badge--b">B</span>
          <span v-if="exchange.voted === 'B'" class="voted-badge">
            <svg width="13" height="13" viewBox="0 0 13 13" fill="none"><path d="M2 6.5l3.5 3.5 5.5-5.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
            Your pick
          </span>
        </div>
        <div class="card-body">
          <p v-for="(line, i) in linesB" :key="i" :class="{ 'para-gap': i > 0 }">{{ line }}</p>
        </div>
        <div class="card-vote">
          <button
            class="vote-btn vote-btn--b"
            :class="{ 'vote-btn--selected': exchange.voted === 'B', 'vote-btn--dim': exchange.voted === 'A' }"
            :disabled="!!exchange.voted"
            @click="$emit('vote', index, 'B')"
          >
            <svg width="13" height="13" viewBox="0 0 14 14" fill="none"><path d="M7 1l1.8 3.6 4 .6-2.9 2.8.7 4L7 10l-3.6 1.9.7-4L1.2 5.2l4-.6L7 1z" fill="currentColor"/></svg>
            B is better
          </button>
        </div>
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
  gap: 8px;
}

/* ── "Which is better?" header ───────── */
.vote-header {
  display: flex;
  align-items: center;
  justify-content: center;
}

.vote-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--muted);
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

/* ── Cards grid ──────────────────────── */
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
  gap: 12px;
}

.card--winner {
  border-color: var(--accent2);
  box-shadow: 0 0 0 1px rgba(128, 255, 219, 0.15);
}

.card--loser {
  opacity: 0.45;
}

/* ── Card header ─────────────────────── */
.card-header {
  display: flex;
  align-items: center;
  gap: 10px;
}

/* Big prominent badge */
.badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  font-size: 18px;
  font-weight: 800;
  font-family: 'Outfit', sans-serif;
  letter-spacing: -0.01em;
  flex-shrink: 0;
}

.badge--a {
  background: rgba(124, 58, 237, 0.25);
  color: #c084fc;
  border: 2px solid rgba(124, 58, 237, 0.6);
}

.badge--b {
  background: rgba(128, 255, 219, 0.12);
  color: var(--accent2);
  border: 2px solid rgba(128, 255, 219, 0.4);
}

.voted-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  font-weight: 600;
  color: var(--accent2);
}

/* ── Card body ───────────────────────── */
.card-body {
  font-size: 14px;
  line-height: 1.65;
  color: var(--text);
  flex: 1;
}

.para-gap {
  margin-top: 8px;
}

/* ── Vote button (inside card) ───────── */
.card-vote {
  margin-top: auto;
  padding-top: 4px;
  border-top: 1px solid var(--border);
}

.vote-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 16px;
  font-size: 12px;
  font-weight: 700;
  font-family: inherit;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
  border: 1px solid transparent;
  width: 100%;
  justify-content: center;
}

.vote-btn--a {
  background: rgba(124, 58, 237, 0.15);
  border-color: rgba(124, 58, 237, 0.4);
  color: #c084fc;
}

.vote-btn--b {
  background: rgba(128, 255, 219, 0.09);
  border-color: rgba(128, 255, 219, 0.3);
  color: var(--accent2);
}

.vote-btn:hover:not(:disabled) {
  filter: brightness(1.25);
  transform: translateY(-1px);
}

.vote-btn--selected {
  border-color: var(--accent2);
  filter: brightness(1.35);
}

.vote-btn--dim {
  opacity: 0;
  pointer-events: none;
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

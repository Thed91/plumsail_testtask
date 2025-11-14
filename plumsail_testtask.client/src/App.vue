<template>
  <nav class="main-nav">
    <router-link to="/" class="nav-link">Form</router-link>
    <router-link to="/list" class="nav-link">List</router-link>
  </nav>
  <router-view />
</template>

<script setup lang="ts">
</script>

<style scoped>
  .main-nav {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 2rem;
    padding: 1.5rem;
    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }

  .nav-link {
    padding: 0.75rem 1.5rem;
    font-size: 1rem;
    font-weight: 600;
    color: #ffffff;
    text-decoration: none;
    border-radius: 8px;
    transition: all 0.2s ease;
    background-color: rgba(255, 255, 255, 0.1);
  }

  .nav-link:hover {
    background-color: rgba(255, 255, 255, 0.2);
    transform: translateY(-2px);
  }

  .nav-link.router-link-active {
    background-color: #ffffff;
    color: #3b82f6;
  }
</style>

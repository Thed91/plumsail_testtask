<template>
  <div class="submissions-list">
    <div class="header-section">
      <h2 class="list-title">Submissions List</h2>
      <p class="results-count" v-if="!loading">
        {{ filteredData.length }} result{{ filteredData.length !== 1 ? 's' : '' }}
        <span v-if="searchQuery"> for "{{ searchQuery }}"</span>
      </p>
    </div>
    <div v-if="error" class="alert alert-error">
        {{ error }}
      <button @click="error = ''" class="close-btn">×</button>
    </div>
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Loading submissions...</p>
    </div>
    <template v-else>
      <div class="search-container">
      <input
        v-model="searchQuery"
        @input="onSearch"
        type="text"
        placeholder="Search by name, email, or any field..."
        class="search-input"
      />
      <button
        v-if="searchQuery"
        @click="clearSearch"
        class="clear-button"
      >
        Clear
      </button>
    </div>

    <div class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Selected</th>
            <th>Checked</th>
            <th>Picked</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredData.length === 0">
            <td colspan="5" class="no-results">
              No results found{{ searchQuery ? ` for "${searchQuery}"` : '' }}
            </td>
          </tr>
          <tr v-for="item in paginatedData" :key="item.id">
            <td>{{item.data.name}}</td>
            <td>{{item.data.selected}}</td>
            <td>
              <span :class="['status-badge', item.data.checked ? 'status-active' : 'status-inactive']">
                {{ item.data.checked ? 'Yes' : 'No' }}
              </span>
            </td>
            <td>{{item.data.picked}}</td>
            <td>{{formatDate(item.data.date)}}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="filteredData.length > itemsPerPage" class="pagination-wrapper">
      <vue-awesome-paginate
        :total-items="filteredData.length"
        :items-per-page="itemsPerPage"
        :max-pages-shown="5"
        v-model="currentPage"
        @click="onClickHandler"
      />
    </div>
    <div v-else-if="filteredData.length > 0" class="pagination-info">
      Showing all {{ filteredData.length }} result{{ filteredData.length !== 1 ? 's' : '' }}
    </div>
    </template>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';

  interface TableData {
    id: string;
    formType: string;
    submittedAt: string;
    data: {
      name: string;
      selected: string;
      checked: boolean;
      picked: string;
      date: string;
    }
  }

  const currentPage = ref(1);
  const itemsPerPage = ref(5);
  const loading = ref<boolean>(false);
  const error = ref<string>('');
  const searchQuery = ref('');

  const post = ref<TableData[]>([]);
  const filteredData = computed(() => {
    if (!searchQuery.value.trim()) {
      return post.value;
    }

    const query = searchQuery.value.toLowerCase();

    return post.value.filter(item => {
      const searchableText = [
        item.data.name,
        item.data.selected,
        item.data.picked,
        item.data.date,
        item.data.checked ? 'yes' : 'no', 
        item.formType,
        item.submittedAt
      ].join(' ').toLowerCase();

      return searchableText.includes(query);
    });
  });

  const paginatedData = computed(() => {
    const start = (currentPage.value - 1) * itemsPerPage.value;
    const end = start + itemsPerPage.value;
    return filteredData.value.slice(start, end);
  });

  const onClickHandler = (page: number) => {
    currentPage.value = page;
  };

  const onSearch = () => {
    currentPage.value = 1;
  };

  const clearSearch = () => {
    searchQuery.value = '';
    currentPage.value = 1;
  };

  const fetchData = async (retries = 3) => {
    try {
      loading.value = true;
      error.value = '';

      const response = await fetch('/api/submissions', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
      });

      if (!response.ok) {
        throw new Error(`HTTP Error:, ${response.status}`);
      }

      const result = await response.json();

      post.value = result.data || result;
    } catch (err: any) {
      console.error('Error fetching submissions:', err);
      error.value = err.message || 'Failed to load';

      if (retries > 0 && err.message.includes('500')) {
        console.log('retrying... ');
        setTimeout(() => fetchData(retries - 1), 1000);
      }
    } finally {
      loading.value = false;
    }
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toISOString().split('T')[0];
  };

  onMounted(() => {
    fetchData();
  });
</script>
<style scoped>
  .submissions-list {
    max-width: 1200px;
    margin: 0 auto;
    padding: 2rem;
  }

  .header-section {
    text-align: center;
    margin-bottom: 2rem;
  }

  .list-title {
    margin: 0 0 0.5rem 0;
    font-size: 1.75rem;
    font-weight: 600;
    color: #1a1a1a;
  }

  .results-count {
    margin: 0;
    font-size: 0.875rem;
    color: #6b7280;
  }

  .results-count span {
    font-weight: 500;
    color: #3b82f6;
  }

  .search-container {
    display: flex;
    gap: 0.75rem;
    margin-bottom: 1.5rem;
    align-items: center;
  }

  .search-input {
    flex: 1;
    padding: 0.75rem 1rem;
    font-size: 1rem;
    color: #1a1a1a;
    background: #ffffff;
    border: 2px solid #e5e7eb;
    border-radius: 8px;
    transition: all 0.2s ease;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  }

  .search-input:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .search-input::placeholder {
    color: #9ca3af;
  }

  .clear-button {
    padding: 0.75rem 1.5rem;
    font-size: 0.875rem;
    font-weight: 600;
    color: #ffffff;
    background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s ease;
    white-space: nowrap;
  }

  .clear-button:hover {
    background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
  }

  .clear-button:active {
    transform: translateY(0);
    box-shadow: 0 2px 4px rgba(239, 68, 68, 0.4);
  }

  .table-container {
    background: #ffffff;
    border-radius: 12px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1), 0 1px 3px rgba(0, 0, 0, 0.08);
    overflow: hidden;
  }

  .data-table {
    width: 100%;
    border-collapse: collapse;
  }

  .data-table thead {
    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  }

  .data-table th {
    padding: 1rem;
    text-align: left;
    font-size: 0.875rem;
    font-weight: 600;
    color: #ffffff;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .data-table tbody tr {
    border-bottom: 1px solid #e5e7eb;
    transition: background-color 0.2s ease;
  }

  .data-table tbody tr:last-child {
    border-bottom: none;
  }

  .data-table tbody tr:hover {
    background-color: #f9fafb;
  }

  .data-table td {
    padding: 1rem;
    font-size: 0.875rem;
    color: #374151;
  }

  .no-results {
    text-align: center;
    padding: 3rem 1rem !important;
    font-size: 1rem;
    color: #6b7280;
    font-style: italic;
  }

  .status-badge {
    display: inline-block;
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 600;
    text-align: center;
  }

  .status-active {
    background-color: #d1fae5;
    color: #065f46;
  }

  .status-inactive {
    background-color: #fee2e2;
    color: #991b1b;
  }

  .pagination-wrapper {
    margin-top: 2rem;
  }

  .pagination-info {
    text-align: center;
    margin-top: 1.5rem;
    padding: 1rem;
    font-size: 0.875rem;
    color: #6b7280;
    background-color: #f9fafb;
    border-radius: 8px;
  }

  .loading-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 4rem 2rem;
    text-align: center;
  }

  .spinner {
    width: 50px;
    height: 50px;
    border: 4px solid #e5e7eb;
    border-top-color: #3b82f6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  .loading-container p {
    margin-top: 1rem;
    color: #6b7280;
    font-size: 0.875rem;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }

  .alert {
    padding: 0.75rem 1rem;
    margin-bottom: 1.5rem;
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 500;
    position: relative;
    animation: slideIn 0.3s ease;
  }

  .alert-error {
    background-color: #fee2e2;
    color: #991b1b;
    border: 1px solid #fca5a5;
  }

  .close-btn {
    position: absolute;
    right: 0.5rem;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: #991b1b;
    font-size: 1.5rem;
    font-weight: bold;
    cursor: pointer;
    padding: 0 0.5rem;
    line-height: 1;
  }

  .close-btn:hover {
    color: #7f1d1d;
  }

  @keyframes slideIn {
    from {
      opacity: 0;
      transform: translateY(-10px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (max-width: 768px) {
    .submissions-list {
      padding: 1rem;
    }

    .header-section {
      margin-bottom: 1.5rem;
    }

    .list-title {
      font-size: 1.5rem;
      margin-bottom: 0.5rem;
    }

    .results-count {
      font-size: 0.8125rem;
    }

    .search-container {
      flex-direction: column;
      gap: 0.5rem;
    }

    .search-input {
      width: 100%;
    }

    .clear-button {
      width: 100%;
    }

    .table-container {
      overflow-x: auto;
    }

    .data-table {
      min-width: 600px;
    }

    .data-table th,
    .data-table td {
      padding: 0.75rem 0.5rem;
      font-size: 0.8125rem;
    }
  }
</style>

<style>
  .pagination-container {
    display: flex;
    justify-content: center;
    align-items: center;
    column-gap: 10px;
    margin-top: 2rem;
  }

  .paginate-buttons {
    height: 40px;
    width: 40px;
    border-radius: 8px;
    cursor: pointer;
    background-color: #ffffff;
    border: 2px solid #e5e7eb;
    color: #374151;
    font-weight: 500;
    transition: all 0.2s ease;
  }

  .paginate-buttons:hover {
    background-color: #f9fafb;
    border-color: #3b82f6;
    color: #3b82f6;
  }

  .active-page {
    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
    border: 2px solid #3b82f6;
    color: white;
  }

  .active-page:hover {
    background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
    border-color: #2563eb;
  }

  .back-button,
  .next-button {
    height: 40px;
    width: auto;
    padding: 0 1rem;
    border-radius: 8px;
    cursor: pointer;
    background-color: #ffffff;
    border: 2px solid #e5e7eb;
    color: #374151;
    font-weight: 500;
    transition: all 0.2s ease;
  }

  .back-button:hover,
  .next-button:hover {
    background-color: #f9fafb;
    border-color: #3b82f6;
    color: #3b82f6;
  }

  .back-button:disabled,
  .next-button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
</style>

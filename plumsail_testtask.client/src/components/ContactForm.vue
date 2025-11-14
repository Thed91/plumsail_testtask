<template>
  <div class="contact-form">
    <h2 class="form-title">Contact Form</h2>
    <div v-if="successMessage" class="alert alert-success">
       {{ successMessage }}
    </div>
    <div v-if="errorMessage" class="alert alert-error">
       {{ errorMessage }}
    </div>
    <form @submit.prevent="handleSubmit">
      <div class="form-group">
        <label for="name" class="form-label">Name or Email *</label>
        <input id="name"
               v-model="name"
               @blur="validator.name.$touch"
               placeholder="Enter your nickname or email"
               :class="['form-input', { 'input-error': validator.name.$error }]" />
        <div v-if="validator.name.$error" class="error-messages">
          <span v-for="error in validator.name.$errors" :key="error.$uid" class="error-message">
            {{ error.$message }}
          </span>
        </div>
      </div>
      <div class="form-group">
        <label for="select" class="form-label">Select an option *</label>
        <select id="select"
                v-model="selected"
                @blur="validator.selected.$touch"
                :class="['form-select', { 'input-error': validator.selected.$error }]">
          <option disabled value="">Please select one</option>
          <option>A</option>
          <option>B</option>
          <option>C</option>
        </select>
        <div v-if="validator.selected.$error" class="error-messages">
          <span v-for="error in validator.selected.$errors" :key="error.$uid" class="error-message">
            {{ error.$message }}
          </span>
        </div>
      </div>
      <div class="form-group">
        <label :class="['checkbox-label', { 'checkbox-error': validator.checked.$error }]">
          <input type="checkbox"
                 id="checkbox"
                 v-model="checked"
                 @blur="validator.checked.$touch"
                 class="form-checkbox" />
          <span class="checkbox-text">I agree to terms and conditions *</span>
        </label>
        <div v-if="validator.checked.$error" class="error-messages">
          <span v-for="error in validator.checked.$errors" :key="error.$uid" class="error-message">
            {{ error.$message }}
          </span>
        </div>
      </div>
      <div class="form-group">
        <label class="form-label">Choose one option *</label>
        <div class="radio-group">
          <label for="one" class="radio-label">
            <input type="radio"
                   id="one"
                   value="One"
                   v-model="picked"
                   @change="validator.picked.$touch"
                   class="form-radio" />
            <span>Option One</span>
          </label>
          <label for="two" class="radio-label">
            <input type="radio"
                   id="two"
                   value="Two"
                   v-model="picked"
                   @change="validator.picked.$touch"
                   class="form-radio" />
            <span>Option Two</span>
          </label>
        </div>
        <div v-if="validator.picked.$error" class="error-messages">
          <span v-for="error in validator.picked.$errors" :key="error.$uid" class="error-message">
            {{ error.$message }}
          </span>
        </div>
      </div>
      <div class="form-group">
        <label class="form-label">Select a date *</label>
        <VueDatePicker v-model="date"
                       @blur="validator.date.$touch"
                       :class="{ 'datepicker-error': validator.date.$error }"
                       class="form-datepicker" />
        <div v-if="validator.date.$error" class="error-messages">
          <span v-for="error in validator.date.$errors" :key="error.$uid" class="error-message">
            {{ error.$message }}
          </span>
        </div>
      </div>

      <button type="submit"
              class="submit-button"
              :disabled="isSubmitting || (validator.$invalid && validator.$dirty)">
        <span v-if="!isSubmitting">Submit</span>
        <span v-else>Submitting...</span>
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed } from 'vue';
  import { VueDatePicker } from '@vuepic/vue-datepicker';
  import '@vuepic/vue-datepicker/dist/main.css';
  import { useVuelidate } from '@vuelidate/core';
  import { required, helpers } from '@vuelidate/validators';

  const name = ref('');
  const selected = ref('');
  const checked = ref(false);
  const picked = ref('');
  const date = ref(null);

  const isSubmitting = ref(false);
  const successMessage = ref('');
  const errorMessage = ref('');

  const nameValidator = (value: string) => {
    if (!value) return false;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(value) || value.length >= 3;
  };

  const rules = computed(() => ({
    name: {
      required: helpers.withMessage('Name or Email is required', required),
       nameValidator: helpers.withMessage(
         'need 3 characters',
         nameValidator
       )
    },
    selected: {
      required: helpers.withMessage('Please select an option', required)
    },
    checked: {
      required: helpers.withMessage(
        'You must agree to terms and conditions',
        (value: boolean) => value === true
      )
    },
    picked: {
      required: helpers.withMessage('Please choose one option', required)
    },
    date: {
      required: helpers.withMessage('Please select a date', required)
    }
  }));

  const validator = useVuelidate(rules, { name, selected, checked, picked, date });

  const handleSubmit = async () => {
    successMessage.value = '';
    errorMessage.value = '';

    const isValid = await validator.value.$validate();

    if (!isValid) {
      errorMessage.value = 'Please fill in all fields';
      return;
    }

    if (isSubmitting.value) return;

    try {
      isSubmitting.value = true;

      const formData = {
        name: name.value,
        selected: selected.value,
        checked: checked.value,
        picked: picked.value,
        date: date.value
      };

      const response = await fetch('/api/submissions/contact', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
      });

      if (!response.ok) {
        throw new Error(`HTTP Error:, ${response.status}`);
      }

      const result = await response.json();
      console.log('success:', result);

      successMessage.value = 'submitted successfully!';

      resetForm();

      setTimeout(() => {
        successMessage.value = '';
      }, 5000);

    } catch (error) {
      errorMessage.value = (error as Error).message || 'Failed to submit';

      setTimeout(() => {
        errorMessage.value = '';
      }, 10000);
    } finally {
      isSubmitting.value = false;
    }
  };

  const resetForm = () => {
    name.value = '';
    selected.value = '';
    checked.value = false;
    picked.value = '';
    date.value = null;
    validator.value.$reset();
  };
</script>

<style scoped>
  .contact-form {
    max-width: 500px;
    margin: 0 auto;
    padding: 2rem;
    background: #ffffff;
    border-radius: 12px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1), 0 1px 3px rgba(0, 0, 0, 0.08);
  }

  .form-title {
    margin: 0 0 1.5rem 0;
    font-size: 1.75rem;
    font-weight: 600;
    color: #1a1a1a;
    text-align: center;
  }

  .form-group {
    margin-bottom: 1.5rem;
  }

  .form-label {
    display: block;
    margin-bottom: 0.5rem;
    font-size: 0.875rem;
    font-weight: 500;
    color: #374151;
  }

  .form-input,
  .form-select {
    width: 100%;
    padding: 0.75rem 1rem;
    font-size: 1rem;
    color: #1a1a1a;
    background: #ffffff;
    border: 2px solid #e5e7eb;
    border-radius: 8px;
    transition: all 0.2s ease;
    box-sizing: border-box;
  }

    .form-input:focus,
    .form-select:focus {
      outline: none;
      border-color: #3b82f6;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }

    .form-input::placeholder {
      color: #9ca3af;
    }

  .form-select {
    cursor: pointer;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23374151' d='M6 9L1 4h10z'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 1rem center;
    padding-right: 2.5rem;
  }

  .input-error {
    border-color: #ef4444 !important;
    box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1) !important;
  }

  .checkbox-error .checkbox-text {
    color: #ef4444;
  }

  .datepicker-error {
    border: 2px solid #ef4444;
    border-radius: 8px;
  }

  .error-messages {
    margin-top: 0.5rem;
  }

  .error-message {
    display: block;
    font-size: 0.75rem;
    color: #ef4444;
    margin-top: 0.25rem;
  }

  .checkbox-label {
    display: flex;
    align-items: center;
    cursor: pointer;
    user-select: none;
  }

  .form-checkbox {
    width: 1.25rem;
    height: 1.25rem;
    margin-right: 0.75rem;
    cursor: pointer;
    accent-color: #3b82f6;
  }

  .checkbox-text {
    font-size: 0.875rem;
    color: #374151;
  }

  .radio-group {
    display: flex;
    gap: 1.5rem;
  }

  .radio-label {
    display: flex;
    align-items: center;
    cursor: pointer;
    user-select: none;
  }

  .form-radio {
    width: 1.125rem;
    height: 1.125rem;
    margin-right: 0.5rem;
    cursor: pointer;
    accent-color: #3b82f6;
  }

  .radio-label span {
    font-size: 0.875rem;
    color: #374151;
  }

  .form-datepicker {
    width: 100%;
  }

  .submit-button {
    width: 100%;
    padding: 0.875rem 1.5rem;
    font-size: 1rem;
    font-weight: 600;
    color: #ffffff;
    background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s ease;
    margin-top: 0.5rem;
  }

    .submit-button:hover:not(:disabled) {
      background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
    }

    .submit-button:active:not(:disabled) {
      transform: translateY(0);
      box-shadow: 0 2px 4px rgba(59, 130, 246, 0.4);
    }

    .submit-button:disabled {
      background: #9ca3af;
      cursor: not-allowed;
      opacity: 0.6;
    }

  .alert {
    padding: 0.75rem 1rem;
    margin-bottom: 1.5rem;
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 500;
    animation: slideIn 0.3s ease;
  }

  .alert-success {
    background-color: #d1fae5;
    color: #065f46;
    border: 1px solid #6ee7b7;
  }

  .alert-error {
    background-color: #fee2e2;
    color: #991b1b;
    border: 1px solid #fca5a5;
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

  @media (max-width: 640px) {
    .contact-form {
      padding: 1.5rem;
    }

    .form-title {
      font-size: 1.5rem;
    }

    .radio-group {
      flex-direction: column;
      gap: 1rem;
    }
  }
</style>

import { createApp } from 'vue'
import App from './App.vue'

import { VueDatePicker } from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css'

import VueAwesomePaginate from "vue-awesome-paginate";
import "vue-awesome-paginate/dist/style.css";

import router from './router/index.ts';

const app = createApp(App);

app.component('VueDatePicker', VueDatePicker);
app.use(VueAwesomePaginate);
app.use(router);
app.mount('#app');

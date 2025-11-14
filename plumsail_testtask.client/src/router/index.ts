import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import ContactForm from '../components/ContactForm.vue';
import UserList from '../components/UserList.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'home',
    component: ContactForm,
  },
  {
    path: '/list',
    name: 'list',
    component: UserList,
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;

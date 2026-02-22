<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const isLogin = ref(true);

const form = ref({
  email: '',
  password: '',
  confirmPassword: ''
});

const toggleMode = () => {
  isLogin.value = !isLogin.value;
  form.value = { email: '', password: '', confirmPassword: '' };
};

const goBack = () => {
  router.push('/');
};

const handleSubmit = () => {
  router.push('/');
};
</script>

<template>
  <div class="min-vh-100 d-flex align-items-center justify-content-center bg-light">
    <div class="container">
      <div class="row justify-content-center">
        <div class="col-md-6 col-lg-5 col-xl-4">

          <div class="text-center mb-4">
            <div class="d-inline-flex align-items-center justify-content-center">
              <div class="bg-primary bg-gradient text-white rounded-3 p-2 me-2 d-flex align-items-center justify-content-center shadow-sm" style="width: 42px; height: 42px;">
                <i class="bi bi-layers-half fs-4"></i>
              </div>
              <div class="d-flex flex-column align-items-start lh-1">
                <span class="fw-bolder text-dark fs-4" style="letter-spacing: -0.5px;">AlgoTrace</span>
                <span class="text-primary fw-bold" style="font-size: 10px; letter-spacing: 2px;">PRO ANALYZER</span>
              </div>
            </div>
          </div>

          <div class="card border-0 shadow-sm rounded-4">
            <div class="card-body p-4 p-sm-5">

              <div class="text-center mb-4">
                <h4 class="fw-bold text-dark mb-1">
                  {{ isLogin ? 'Вхід до системи' : 'Реєстрація' }}
                </h4>
                <p class="text-muted small">
                  {{ isLogin ? 'Продовжіть роботу з аналізатором' : 'Створіть новий профіль' }}
                </p>
              </div>

              <form @submit.prevent="handleSubmit">
                <div class="form-floating mb-3">
                  <input
                    type="email"
                    class="form-control rounded-3"
                    id="email"
                    v-model="form.email"
                    required
                    placeholder="name@example.com"
                  />
                  <label for="email" class="text-muted">
                    <i class="bi bi-envelope me-2"></i> Email
                  </label>
                </div>

                <div class="form-floating mb-3">
                  <input
                    type="password"
                    class="form-control rounded-3"
                    id="password"
                    v-model="form.password"
                    required
                    placeholder="Password"
                  />
                  <label for="password" class="text-muted">
                    <i class="bi bi-lock me-2"></i> Пароль
                  </label>
                </div>

                <div class="form-floating mb-3" v-if="!isLogin">
                  <input
                    type="password"
                    class="form-control rounded-3"
                    id="confirmPassword"
                    v-model="form.confirmPassword"
                    required
                    placeholder="Confirm Password"
                  />
                  <label for="confirmPassword" class="text-muted">
                    <i class="bi bi-shield-lock me-2"></i> Підтвердження
                  </label>
                </div>

                <div class="d-grid gap-2 mt-4">
                  <button type="submit" class="btn btn-primary btn-lg rounded-3 fw-bold shadow-sm">
                    {{ isLogin ? 'Увійти' : 'Зареєструватися' }}
                  </button>
                </div>
              </form>

              <div class="text-center mt-4">
                <span class="text-muted small">
                  {{ isLogin ? 'Немає акаунту?' : 'Вже є акаунт?' }}
                </span>
                <button class="btn btn-link text-decoration-none fw-bold p-0 ms-1 align-baseline" @click="toggleMode">
                  {{ isLogin ? 'Зареєструватися' : 'Увійти' }}
                </button>
              </div>

              <hr class="my-4 text-muted opacity-10" />

              <div class="text-center">
                <button class="btn btn-sm btn-link text-secondary text-decoration-none" @click="goBack">
                  <i class="bi bi-arrow-left me-1"></i> На головну
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

// Состояние приложения
const level = ref(1);
const file1 = ref<File | null>(null);
const file2 = ref<File | null>(null);
const isDragging1 = ref(false);
const isDragging2 = ref(false);

// Обработчики файлов
const handleDrop = (event: DragEvent, fileNumber: 1 | 2) => {
  const files = event.dataTransfer?.files;
  if (files && files.length > 0) {
    validateAndSetFile(files[0], fileNumber);
  }
  if (fileNumber === 1) isDragging1.value = false;
  else isDragging2.value = false;
};

const handleFileSelect = (event: Event, fileNumber: 1 | 2) => {
  const input = event.target as HTMLInputElement;
  if (input.files && input.files.length > 0) {
    validateAndSetFile(input.files[0], fileNumber);
  }
};

const validateAndSetFile = (file: File, fileNumber: 1 | 2) => {
  // Простая проверка расширения (можно усилить)
  const allowedExtensions = ['.cs', '.py', '.js', '.ts', '.java', '.cpp', '.h', '.html', '.css', '.php', '.rb', '.go'];
  const fileName = file.name.toLowerCase();
  const isValid = allowedExtensions.some(ext => fileName.endsWith(ext));

  if (isValid) {
    if (fileNumber === 1) file1.value = file;
    else file2.value = file;
  } else {
    alert('Пожалуйста, выберите файл с кодом (например: .cs, .py, .js)');
  }
};

const removeFile = (fileNumber: 1 | 2) => {
  if (fileNumber === 1) file1.value = null;
  else file2.value = null;
};

const startComparison = () => {
  if (!file1.value || !file2.value) {
    alert('Пожалуйста, загрузите оба файла для сравнения.');
    return;
  }
  alert(`Запуск сравнения...\nУровень: ${level.value}\nФайл 1: ${file1.value.name}\nФайл 2: ${file2.value.name}`);
  // Здесь будет логика отправки на бэкенд
};
</script>

<template>
  <!-- Шапка сайта -->
  <nav class="navbar navbar-expand-lg navbar-dark bg-primary shadow-sm">
    <div class="container">
      <a class="navbar-brand fw-bold d-flex align-items-center" href="#">
        <i class="bi bi-code-square me-2 fs-4"></i>
        AlgoTrace
      </a>

      <div class="d-flex align-items-center text-white">
        <div class="dropdown">
          <a href="#" class="d-flex align-items-center text-white text-decoration-none dropdown-toggle" id="dropdownUser1" data-bs-toggle="dropdown" aria-expanded="false">
            <i class="bi bi-person-circle fs-4 me-2"></i>
            <span>Войти / Регистрация</span>
          </a>
        </div>
      </div>
    </div>
  </nav>

  <!-- Основной контент -->
  <div class="container py-5">
    <div class="text-center mb-5">
      <h1 class="display-5 fw-bold text-primary">Сравнение исходного кода</h1>
      <p class="lead text-muted">Загрузите файлы, выберите глубину проверки и найдите совпадения.</p>
    </div>

    <!-- Область загрузки файлов -->
    <div class="row g-4 justify-content-center">
      <!-- Файл 1 -->
      <div class="col-md-5">
        <div
          class="card h-100 shadow-sm border-2"
          :class="isDragging1 ? 'border-primary bg-light' : 'border-secondary border-opacity-25'"
          @dragover.prevent="isDragging1 = true"
          @dragleave.prevent="isDragging1 = false"
          @drop.prevent="(e) => handleDrop(e, 1)"
        >
          <div class="card-body d-flex flex-column align-items-center justify-content-center p-5 text-center" style="min-height: 300px;">
            <div v-if="!file1">
              <i class="bi bi-cloud-arrow-up text-primary" style="font-size: 4rem;"></i>
              <h5 class="mt-3">Файл 1</h5>
              <p class="text-muted small">Перетащите файл сюда (.cs, .py, .js...)</p>
              <label class="btn btn-outline-primary mt-2">
                Выбрать файл
                <input type="file" hidden @change="(e) => handleFileSelect(e, 1)" accept=".cs,.py,.js,.ts,.java,.cpp,.h,.html,.css,.php,.rb,.go" />
              </label>
            </div>
            <div v-else>
              <i class="bi bi-file-earmark-check-fill text-success" style="font-size: 4rem;"></i>
              <h5 class="mt-3 text-break">{{ file1.name }}</h5>
              <p class="text-muted small">{{ (file1.size / 1024).toFixed(2) }} KB</p>
              <button class="btn btn-sm btn-danger mt-2" @click="removeFile(1)">
                <i class="bi bi-trash"></i> Удалить
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Разделитель / VS -->
      <div class="col-md-1 d-flex align-items-center justify-content-center">
        <div class="bg-light rounded-circle p-3 shadow-sm text-muted fw-bold border">VS</div>
      </div>

      <!-- Файл 2 -->
      <div class="col-md-5">
        <div
          class="card h-100 shadow-sm border-2"
          :class="isDragging2 ? 'border-primary bg-light' : 'border-secondary border-opacity-25'"
          @dragover.prevent="isDragging2 = true"
          @dragleave.prevent="isDragging2 = false"
          @drop.prevent="(e) => handleDrop(e, 2)"
        >
          <div class="card-body d-flex flex-column align-items-center justify-content-center p-5 text-center" style="min-height: 300px;">
            <div v-if="!file2">
              <i class="bi bi-cloud-arrow-up text-primary" style="font-size: 4rem;"></i>
              <h5 class="mt-3">Файл 2</h5>
              <p class="text-muted small">Перетащите файл сюда (.cs, .py, .js...)</p>
              <label class="btn btn-outline-primary mt-2">
                Выбрать файл
                <input type="file" hidden @change="(e) => handleFileSelect(e, 2)" accept=".cs,.py,.js,.ts,.java,.cpp,.h,.html,.css,.php,.rb,.go" />
              </label>
            </div>
            <div v-else>
              <i class="bi bi-file-earmark-check-fill text-success" style="font-size: 4rem;"></i>
              <h5 class="mt-3 text-break">{{ file2.name }}</h5>
              <p class="text-muted small">{{ (file2.size / 1024).toFixed(2) }} KB</p>
              <button class="btn btn-sm btn-danger mt-2" @click="removeFile(2)">
                <i class="bi bi-trash"></i> Удалить
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Настройки и запуск -->
    <div class="row mt-5 justify-content-center">
      <div class="col-lg-8">
        <div class="card shadow-sm border-0 bg-light">
          <div class="card-body p-4">
            <label for="levelRange" class="form-label fw-bold d-flex justify-content-between">
              <span>Уровень проверки (Глубина анализа)</span>
              <span class="badge bg-primary rounded-pill">Уровень {{ level }}</span>
            </label>
            <input type="range" class="form-range" min="1" max="5" step="1" id="levelRange" v-model="level">
            <div class="d-flex justify-content-between text-muted small px-1">
              <span>1 (Быстро)</span>
              <span>2</span>
              <span>3 (Баланс)</span>
              <span>4</span>
              <span>5 (Точно)</span>
            </div>

            <div class="text-center mt-4">
              <button class="btn btn-success btn-lg px-5 shadow" @click="startComparison" :disabled="!file1 || !file2">
                <i class="bi bi-play-circle-fill me-2"></i> Запустить сравнение
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>


<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { authState } from '@/services/auth.service';
import api from '@/services/api';

// --- ИНТЕРФЕЙСЫ ---
interface FileEntry {
  fileId: string;
  name: string;
}

interface FolderEntry {
  folderId: string;
  name: string;
}

interface FolderContent {
  folderId: string | null;
  name: string;
  parentId: string | null;
  folders: FolderEntry[];
  files: FileEntry[];
}

const router = useRouter();

// --- СОСТОЯНИЕ ---
const currentFolderContent = ref<FolderContent | null>(null);
const selectedFile = ref<FileEntry | null>(null);
const fileContent = ref<string>('');
const isLoading = ref(false);
const isRoot = ref(true);

const newFolderName = ref('');
const fileInput = ref<HTMLInputElement | null>(null);
const editingId = ref<string | null>(null);
const renameValue = ref('');

// --- МЕТОДЫ ---
const openFolder = async (folderId: string | null = null) => {
  isLoading.value = true;
  isRoot.value = folderId === null;
  try {
    const url = folderId ? `/api/directory/folder/${folderId}` : '/api/directory/folder';
    const res = await api.get<FolderContent>(url);
    currentFolderContent.value = res.data;
    selectedFile.value = null;
  } catch (err) {
    console.error("Access error:", err);
    if (!isRoot.value) alert("Помилка доступу до папки");
  } finally {
    isLoading.value = false;
  }
};

const triggerUpload = () => fileInput.value?.click();

const handleFileUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (!target.files || target.files.length === 0) return;

  const file: File | undefined = target.files[0];
  if (!file) return;

  const formData = new FormData();
  formData.append('file', file);

  const currentId = currentFolderContent.value?.folderId || '';
  const query = currentId ? `?folderId=${currentId}` : '';

  try {
    isLoading.value = true;
    await api.post(`/api/directory/file/upload${query}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    await openFolder(currentFolderContent.value?.folderId || null);
  } catch (err) {
    console.error("Upload error:", err);
    alert("Помилка завантаження файлу");
  } finally {
    isLoading.value = false;
    target.value = '';
  }
};

const viewFile = async (file: FileEntry) => {
  selectedFile.value = file;
  fileContent.value = 'Завантаження...';
  try {
    // Используем blob и text() для избежания 'any' в responseType
    const res = await api.get<string>(`/api/directory/file/download/${file.fileId}`, {
      responseType: 'text'
    });
    fileContent.value = res.data;
  } catch (err) {
    console.error("View file error:", err);
    fileContent.value = 'Помилка завантаження вмісту.';
  }
};

const downloadFile = (fileId: string) => {
  window.open(`${api.defaults.baseURL}/api/directory/file/download/${fileId}`, '_blank');
};

const deleteFile = async (id: string) => {
  if (!confirm('Видалити цей файл?')) return;
  try {
    await api.delete(`/api/directory/file/${id}`);
    if (selectedFile.value?.fileId === id) selectedFile.value = null;
    await openFolder(currentFolderContent.value?.folderId || null);
  } catch (err) {
    console.error("Delete file error:", err);
    alert("Не вдалося видалити файл");
  }
};

const createNewFolder = async () => {
  if (!newFolderName.value.trim()) return;
  try {
    const parentId = currentFolderContent.value?.folderId || null;
    await api.post('/api/directory/folder', {
      name: newFolderName.value,
      parentId: parentId
    });
    newFolderName.value = '';
    await openFolder(parentId);
  } catch (err) {
    console.error("Create folder error:", err);
    alert("Не вдалося створити папку");
  }
};

const startRename = (id: string, currentName: string) => {
  editingId.value = id;
  renameValue.value = currentName;
};

const saveRename = async (id: string) => {
  if (!renameValue.value.trim()) return;
  try {
    await api.put(`/api/directory/folder/${id}/rename`, JSON.stringify(renameValue.value), {
        headers: { 'Content-Type': 'application/json' }
    });
    editingId.value = null;
    await openFolder(currentFolderContent.value?.folderId || null);
  } catch (err) {
    console.error("Rename error:", err);
    alert("Помилка перейменування");
  }
};

const deleteFolder = async (id: string) => {
  if (!confirm('Видалити папку та все вкладене?')) return;
  try {
    await api.delete(`/api/directory/folder/${id}`);
    await openFolder(currentFolderContent.value?.parentId || null);
  } catch (err) {
    console.error("Delete folder error:", err);
    alert("Не вдалося видалити папку");
  }
};

onMounted(() => {
  if (!authState.isAuthenticated) {
    router.push('/auth');
    return;
  }
  openFolder(null);
});
</script>

<template>
  <div class="min-vh-100 bg-body-tertiary d-flex flex-column font-sans">
    <nav class="navbar navbar-light bg-white border-bottom shadow-sm py-2">
      <div class="container-fluid px-4">
        <div class="d-flex align-items-center">
          <button @click="router.push('/')" class="btn btn-outline-primary btn-sm rounded-pill me-3 px-3 shadow-none">
            <i class="bi bi-house-door-fill"></i>
          </button>
          <span class="navbar-brand fw-bold text-primary mb-0 fs-5">
            AlgoTrace
          </span>
        </div>
        <div class="d-flex align-items-center gap-3">
            <div class="d-none d-md-block small text-muted border-end pe-3 text-end">
              <strong class="text-dark">{{ authState.user?.email }}</strong>
            </div>
            <i class="bi bi-person-circle fs-4 text-primary"></i>
        </div>
      </div>
    </nav>

    <div class="flex-grow-1 d-flex overflow-hidden">
      <div class="flex-grow-1 bg-white d-flex flex-column">

        <div class="p-3 border-bottom d-flex align-items-center justify-content-between bg-light bg-opacity-25">
          <div class="d-flex align-items-center gap-3">
            <button v-if="!isRoot" @click="openFolder(currentFolderContent?.parentId)"
                    class="btn btn-sm btn-white border shadow-sm px-3 rounded-pill">
              <i class="bi bi-arrow-left me-1"></i> Назад
            </button>

            <nav aria-label="breadcrumb">
              <ol class="breadcrumb mb-0 py-1 fw-bold fs-6">
                <li class="breadcrumb-item"><a href="#" @click.prevent="openFolder(null)" class="text-decoration-none">Моє сховище</a></li>
                <li v-if="!isRoot" class="breadcrumb-item active text-dark" aria-current="page">{{ currentFolderContent?.name }}</li>
              </ol>
            </nav>
          </div>

          <div class="d-flex gap-2">
            <div class="input-group input-group-sm" style="width: 250px;">
              <input v-model="newFolderName" @keyup.enter="createNewFolder" type="text" class="form-control" placeholder="Нова папка...">
              <button @click="createNewFolder" class="btn btn-primary px-3 shadow-none"><i class="bi bi-folder-plus"></i></button>
            </div>
            <div class="vr mx-2"></div>
            <button class="btn btn-sm btn-success px-3 shadow-sm rounded-pill fw-bold" @click="triggerUpload">
              <i class="bi bi-cloud-arrow-up-fill me-2"></i> Завантажити
            </button>
            <input type="file" ref="fileInput" class="d-none" @change="handleFileUpload">
          </div>
        </div>

        <div class="flex-grow-1 overflow-auto">
          <div v-if="isLoading" class="text-center py-5">
            <div class="spinner-border text-primary opacity-50"></div>
          </div>

          <table v-else class="table table-hover align-middle mb-0">
            <thead class="table-light small text-uppercase opacity-75 fw-bold">
              <tr>
                <th class="ps-4" style="width: 60%">Назва</th>
                <th class="text-end pe-4">Дії</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="folder in currentFolderContent?.folders" :key="folder.folderId" class="group-hover border-bottom">
                <td class="ps-4 cursor-pointer" @click="editingId !== folder.folderId && openFolder(folder.folderId)">
                  <div class="d-flex align-items-center py-1">
                    <i class="bi bi-folder-fill text-warning fs-3 me-3"></i>

                    <div v-if="editingId === folder.folderId" class="input-group input-group-sm w-75 shadow-sm" @click.stop>
                      <input
                        v-model="renameValue"
                        @keyup.enter="saveRename(folder.folderId)"
                        @keyup.esc="editingId = null"
                        class="form-control"
                        autoFocus
                      >
                      <button @click="saveRename(folder.folderId)" class="btn btn-success px-3">
                        <i class="bi bi-check-lg"></i>
                      </button>
                      <button @click="editingId = null" class="btn btn-outline-secondary px-2">
                        <i class="bi bi-x-lg"></i>
                      </button>
                    </div>

                    <span v-else class="fw-medium text-dark fs-6">{{ folder.name }}</span>
                  </div>
                </td>
                <td class="text-end pe-4">
                  <div class="d-flex justify-content-end gap-1 opacity-0-hover">
                    <button @click.stop="startRename(folder.folderId, folder.name)" class="btn btn-sm btn-outline-secondary border-0 rounded-circle" style="width: 32px; height:32px;"><i class="bi bi-pencil"></i></button>
                    <button @click.stop="deleteFolder(folder.folderId)" class="btn btn-sm btn-outline-danger border-0 rounded-circle" style="width: 32px; height:32px;"><i class="bi bi-trash"></i></button>
                  </div>
                </td>
              </tr>
              <tr v-for="file in currentFolderContent?.files" :key="file.fileId"
                  :class="{'table-primary bg-opacity-10': selectedFile?.fileId === file.fileId}" class="border-bottom">
                <td class="ps-4 cursor-pointer" @click="viewFile(file)">
                  <div class="d-flex align-items-center py-1">
                    <i class="bi bi-file-earmark-code-fill text-primary fs-3 me-3"></i>
                    <span class="text-dark fs-6">{{ file.name }}</span>
                  </div>
                </td>
                <td class="text-end pe-4">
                  <div class="d-flex justify-content-end gap-1">
                    <button @click.stop="downloadFile(file.fileId)" class="btn btn-sm btn-outline-primary border-0 rounded-circle" style="width: 32px; height:32px;"><i class="bi bi-download"></i></button>
                    <button @click.stop="deleteFile(file.fileId)" class="btn btn-sm btn-outline-danger border-0 rounded-circle" style="width: 32px; height:32px;"><i class="bi bi-trash"></i></button>
                  </div>
                </td>
              </tr>
              <tr v-if="!currentFolderContent?.folders?.length && !currentFolderContent?.files?.length">
                <td colspan="2" class="text-center py-5">
                  <div class="py-5 opacity-25">
                    <i class="bi bi-folder2-open display-1"></i>
                    <h5 class="mt-3 fw-bold">Ця папка порожня</h5>
                    <p class="small">Створіть папку або завантажте файл, щоб почати</p>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div v-if="selectedFile" class="bg-white border-start shadow-lg d-flex flex-column animate__animated animate__slideInRight" style="width: 450px;">
        <div class="p-3 border-bottom d-flex justify-content-between align-items-center bg-light">
          <div class="d-flex align-items-center overflow-hidden">
            <i class="bi bi-file-earmark-text text-primary me-2 fs-5"></i>
            <span class="fw-bold text-truncate small">{{ selectedFile.name }}</span>
          </div>
          <button @click="selectedFile = null" class="btn-close shadow-none" style="font-size: 0.8rem;"></button>
        </div>
        <div class="flex-grow-1 bg-dark">
          <textarea readonly v-model="fileContent" class="form-control border-0 h-100 p-3 code-view shadow-none"></textarea>
        </div>
        <div class="p-3 border-top bg-light d-grid">
           <button @click="downloadFile(selectedFile.fileId)" class="btn btn-primary fw-bold">
             <i class="bi bi-download me-2"></i> Скачати файл
           </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cursor-pointer { cursor: pointer; }

.code-view {
  background: #1e1e1e;
  color: #d4d4d4;
  font-family: 'Fira Code', 'Consolas', monospace;
  font-size: 0.85rem;
  resize: none;
  line-height: 1.5;
}

.group-hover:hover .opacity-0-hover { opacity: 1 !important; }
.opacity-0-hover {
  opacity: 0;
  transition: opacity 0.2s ease-in-out;
}

.breadcrumb-item + .breadcrumb-item::before {
  content: "›";
  font-size: 1.2rem;
  line-height: 1;
  vertical-align: middle;
}

.table-hover tbody tr:hover {
  background-color: rgba(0, 123, 255, 0.02);
}

::-webkit-scrollbar { width: 8px; }
::-webkit-scrollbar-track { background: #f1f1f1; }
::-webkit-scrollbar-thumb { background: #ccc; border-radius: 4px; }
::-webkit-scrollbar-thumb:hover { background: #bbb; }
</style>

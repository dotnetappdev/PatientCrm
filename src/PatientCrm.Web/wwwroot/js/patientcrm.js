// PatientCRM - Main JavaScript

// Theme management
const ThemeManager = {
    init() {
        const saved = localStorage.getItem('patientcrm-theme') || 'light';
        this.apply(saved);
    },

    apply(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('patientcrm-theme', theme);

        const btn = document.getElementById('themeToggle');
        if (btn) {
            btn.innerHTML = theme === 'dark'
                ? '<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg> Light Mode'
                : '<svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg> Dark Mode';
        }
    },

    toggle() {
        const current = document.documentElement.getAttribute('data-theme') || 'light';
        const next = current === 'dark' ? 'light' : 'dark';
        this.apply(next);

        // Persist preference to server
        fetch('/Account/UpdateTheme', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('[name=__RequestVerificationToken]')?.value || '' },
            body: JSON.stringify({ theme: next })
        }).catch(() => {});
    }
};

// Sidebar toggle (mobile)
const SidebarManager = {
    init() {
        const toggleBtn = document.getElementById('sidebarToggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', () => {
                document.querySelector('.sidebar')?.classList.toggle('open');
            });
        }

        // Close sidebar on outside click (mobile)
        document.addEventListener('click', (e) => {
            const sidebar = document.querySelector('.sidebar');
            const toggle = document.getElementById('sidebarToggle');
            if (sidebar?.classList.contains('open') && !sidebar.contains(e.target) && !toggle?.contains(e.target)) {
                sidebar.classList.remove('open');
            }
        });
    }
};

// Patient search
const PatientSearch = {
    debounceTimer: null,

    init() {
        const searchInput = document.getElementById('globalSearch');
        if (searchInput) {
            searchInput.addEventListener('input', (e) => {
                clearTimeout(this.debounceTimer);
                this.debounceTimer = setTimeout(() => this.search(e.target.value), 300);
            });

            searchInput.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    const query = e.target.value.trim();
                    if (query) window.location.href = `/Patients?search=${encodeURIComponent(query)}`;
                }
            });
        }
    },

    search(query) {
        if (query.length < 2) return;
        // Quick search results would go here via AJAX
    }
};

// Image upload handler
const ImageUpload = {
    init() {
        const dropzone = document.querySelector('.upload-dropzone');
        if (!dropzone) return;

        dropzone.addEventListener('dragover', (e) => {
            e.preventDefault();
            dropzone.classList.add('active');
        });

        dropzone.addEventListener('dragleave', () => dropzone.classList.remove('active'));

        dropzone.addEventListener('drop', (e) => {
            e.preventDefault();
            dropzone.classList.remove('active');
            const files = e.dataTransfer.files;
            this.handleFiles(files);
        });

        const fileInput = dropzone.querySelector('input[type="file"]');
        fileInput?.addEventListener('change', (e) => this.handleFiles(e.target.files));
    },

    handleFiles(files) {
        for (const file of files) {
            if (!file.type.startsWith('image/') && !file.name.endsWith('.dcm')) {
                alert(`File ${file.name} is not a supported image format`);
                continue;
            }
            this.previewFile(file);
        }
    },

    previewFile(file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            const preview = document.getElementById('imagePreviewContainer');
            if (preview) {
                const img = document.createElement('div');
                img.className = 'image-preview-item';
                img.innerHTML = `
                    <img src="${e.target.result}" alt="${file.name}" style="max-height: 120px; border-radius: 8px;" />
                    <p class="fs-sm text-muted mt-1">${file.name} (${(file.size / 1024).toFixed(1)}KB)</p>
                `;
                preview.appendChild(img);
            }
        };
        reader.readAsDataURL(file);
    }
};

// Dental chart
const DentalChart = {
    init() {
        const teeth = document.querySelectorAll('.tooth');
        teeth.forEach(tooth => {
            tooth.addEventListener('click', function () {
                const toothNum = this.dataset.tooth;
                document.getElementById('selectedTooth').textContent = `Tooth #${toothNum}`;
                // Open tooth detail modal
                const modal = document.getElementById('toothModal');
                if (modal) {
                    modal.classList.add('open');
                    document.getElementById('toothNumberInput').value = toothNum;
                }
            });
        });
    }
};

// Alert dismissal
const Alerts = {
    init() {
        document.querySelectorAll('.alert-dismiss').forEach(btn => {
            btn.addEventListener('click', function () {
                this.closest('.alert')?.remove();
            });
        });
    }
};

// Confirmation dialogs
function confirmAction(message, action) {
    if (confirm(message)) {
        action();
    }
}

// Auto-dismiss success messages
function autoDismissAlerts() {
    setTimeout(() => {
        document.querySelectorAll('.alert-success, .alert-info').forEach(el => {
            el.style.transition = 'opacity 0.5s ease';
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 500);
        });
    }, 4000);
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    ThemeManager.init();
    SidebarManager.init();
    PatientSearch.init();
    ImageUpload.init();
    DentalChart.init();
    Alerts.init();
    autoDismissAlerts();
});

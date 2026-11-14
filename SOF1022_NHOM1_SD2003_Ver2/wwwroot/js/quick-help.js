class QuickHelpPanel {
    constructor() {
        this.panel = document.getElementById('quickHelpPanel');
        this.toggleBtn = document.getElementById('quickHelpToggle');
        this.window = document.getElementById('quickHelpWindow');
        this.closeBtn = document.getElementById('quickHelpClose');
        this.searchInput = document.getElementById('quickHelpSearch');
        this.categories = Array.from(document.querySelectorAll('.quick-help-category'));
        this.items = Array.from(document.querySelectorAll('.quick-help-item'));
        this.contactBtn = document.getElementById('quickHelpContactStaff');
        this.noResults = document.getElementById('quickHelpNoResults');
        this.contactSection = document.getElementById('quickHelpContact');
        
        this.bindEvents();
    }

    bindEvents() {
        if (this.toggleBtn) {
            this.toggleBtn.addEventListener('click', () => this.toggle());
        }

        if (this.closeBtn) {
            this.closeBtn.addEventListener('click', () => this.close());
        }

        // Accordion functionality
        this.items.forEach(item => {
            const question = item.querySelector('.quick-help-question');
            if (question) {
                question.addEventListener('click', () => {
                    // Close other items in the same category
                    const category = item.closest('.quick-help-category');
                    const categoryItems = category.querySelectorAll('.quick-help-item');
                    categoryItems.forEach(otherItem => {
                        if (otherItem !== item) {
                            otherItem.classList.remove('expanded');
                        }
                    });
                    
                    // Toggle current item
                    item.classList.toggle('expanded');
                });
            }
        });

        // Search functionality
        if (this.searchInput) {
            this.searchInput.addEventListener('input', (e) => this.filter(e.target.value));
        }

        // Contact / AI Assistant button
        if (this.contactBtn) {
            this.contactBtn.addEventListener('click', () => {
                this.close();
                const aiToggle = document.getElementById('aiChatToggle');
                if (aiToggle) {
                    aiToggle.click();
                }
            });
        }

        // Close panel when clicking outside
        document.addEventListener('click', (e) => {
            if (this.window && this.window.classList.contains('active')) {
                if (!this.panel.contains(e.target) && !this.toggleBtn.contains(e.target)) {
                    this.close();
                }
            }
        });

        // Close panel on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.window && this.window.classList.contains('active')) {
                this.close();
            }
        });
    }

    toggle() {
        if (this.window) {
            this.window.classList.toggle('active');
        }
    }

    close() {
        if (this.window) {
            this.window.classList.remove('active');
        }
    }

    filter(query) {
        const q = query.trim().toLowerCase();
        
        // Show/hide items based on search
        this.items.forEach(item => {
            const text = item.textContent.toLowerCase();
            const match = q === '' || text.includes(q);
            item.style.display = match ? '' : 'none';
            
            // Reset expanded state when filtering
            if (q !== '') {
                item.classList.remove('expanded');
            }
        });

        // Hide empty categories
        this.categories.forEach(category => {
            const visibleItems = category.querySelectorAll('.quick-help-item:not([style*="display: none"])');
            const hasVisibleItems = visibleItems.length > 0;
            category.style.display = hasVisibleItems ? '' : 'none';
        });

        // Show/hide no results message
        if (this.noResults && this.contactSection) {
            const hasVisibleCategories = this.categories.some(cat => cat.style.display !== 'none');
            if (q !== '' && !hasVisibleCategories) {
                this.noResults.style.display = 'block';
                this.contactSection.style.display = 'block';
            } else {
                this.noResults.style.display = 'none';
                this.contactSection.style.display = 'block';
            }
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.quickHelpPanel = new QuickHelpPanel();
});

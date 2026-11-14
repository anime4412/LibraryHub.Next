// Notification System - Real-time SignalR Integration
// This file handles the notification UI and SignalR connection for both Reader and Admin

class NotificationManager {
    constructor() {
        this.connection = null;
        this.userId = null;
        this.userRole = null;
        this.notificationBadge = null;
        this.notificationDropdown = null;
        this.unreadCount = 0;
    }

    async initialize() {
        // Get user info from session
        const userIdElement = document.getElementById('currentUserId');
        const userRoleElement = document.getElementById('currentUserRole');
        
        if (userIdElement) {
            this.userId = parseInt(userIdElement.value);
        }
        if (userRoleElement) {
            this.userRole = userRoleElement.value;
        }

        if (!this.userId) {
            console.log('No user logged in, skipping notification initialization');
            return;
        }

        // Initialize UI elements
        this.notificationBadge = document.getElementById('notificationBadge');
        this.notificationDropdown = document.getElementById('notificationDropdown');

        // Load SignalR dynamically
        await this.loadSignalR();

        // Initialize SignalR connection
        await this.initializeSignalR();

        // Load initial notifications
        await this.loadNotifications();

        // Set up periodic refresh for unread count
        setInterval(() => this.refreshUnreadCount(), 30000); // Every 30 seconds
    }

    async loadSignalR() {
        return new Promise((resolve, reject) => {
            if (window.signalR) {
                resolve();
                return;
            }

            const script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.0/dist/browser/signalr.js';
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    async initializeSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/notification')
                .withAutomaticReconnect()
                .build();

            // Set up event handlers
            this.connection.on('ReceiveNotification', (notification) => {
                this.handleNewNotification(notification);
            });

            this.connection.onreconnecting(error => {
                console.log('SignalR reconnecting...', error);
            });

            this.connection.onreconnected(connectionId => {
                console.log('SignalR reconnected with ID:', connectionId);
                this.joinUserGroup();
            });

            this.connection.onclose(error => {
                console.log('SignalR connection closed:', error);
            });

            // Start connection
            await this.connection.start();
            console.log('SignalR connected successfully');

            // Join user-specific group
            await this.joinUserGroup();

            // Join role-specific groups
            if (this.userRole === 'Admin') {
                await this.connection.invoke('JoinAdminGroup');
            } else if (this.userRole === 'Librarian') {
                await this.connection.invoke('JoinLibrarianGroup');
            }

            if (this.userRole === 'Admin' || this.userRole === 'Librarian') {
                await this.connection.invoke('JoinStaffGroup');
            }

        } catch (error) {
            console.error('Error initializing SignalR:', error);
        }
    }

    async joinUserGroup() {
        try {
            if (this.connection && this.userId) {
                await this.connection.invoke('JoinUserGroup', this.userId);
                console.log(`Joined user group for user ${this.userId}`);
            }
        } catch (error) {
            console.error('Error joining user group:', error);
        }
    }

    async loadNotifications() {
        try {
            const response = await fetch('/Notifications/Recent');
            const data = await response.json();
            
            if (data.success) {
                this.unreadCount = data.unreadCount;
                this.updateBadge();
                this.renderNotifications(data.notifications);
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
        }
    }

    async refreshUnreadCount() {
        try {
            const response = await fetch('/Notifications/UnreadCount');
            const data = await response.json();
            
            if (data.success) {
                this.unreadCount = data.unreadCount;
                this.updateBadge();
            }
        } catch (error) {
            console.error('Error refreshing unread count:', error);
        }
    }

    handleNewNotification(notification) {
        console.log('New notification received:', notification);
        
        // Increment unread count
        this.unreadCount++;
        this.updateBadge();

        // Add notification to dropdown
        this.prependNotification(notification);

        // Show toast notification
        this.showToast(notification);

        // Play notification sound (optional)
        this.playNotificationSound();
    }

    updateBadge() {
        if (this.notificationBadge) {
            if (this.unreadCount > 0) {
                this.notificationBadge.textContent = this.unreadCount > 99 ? '99+' : this.unreadCount;
                this.notificationBadge.style.display = 'block';
            } else {
                this.notificationBadge.style.display = 'none';
            }
        }
    }

    renderNotifications(notifications) {
        if (!this.notificationDropdown) return;

        const container = this.notificationDropdown.querySelector('.notification-list');
        if (!container) return;

        container.innerHTML = '';

        if (notifications.length === 0) {
            container.innerHTML = '<div class="no-notifications">Không có thông báo nào</div>';
            return;
        }

        notifications.forEach(notification => {
            const element = this.createNotificationElement(notification);
            container.appendChild(element);
        });
    }

    prependNotification(notification) {
        if (!this.notificationDropdown) return;

        const container = this.notificationDropdown.querySelector('.notification-list');
        if (!container) return;

        // Remove "no notifications" message if exists
        const noNotifications = container.querySelector('.no-notifications');
        if (noNotifications) {
            noNotifications.remove();
        }

        const element = this.createNotificationElement(notification);
        container.insertBefore(element, container.firstChild);

        // Keep only last 10 notifications in dropdown
        while (container.children.length > 10) {
            container.removeChild(container.lastChild);
        }
    }

    createNotificationElement(notification) {
        const div = document.createElement('div');
        div.className = `notification-item ${notification.isRead ? 'read' : 'unread'}`;
        div.dataset.notificationId = notification.id;

        const timeAgo = this.formatTimeAgo(new Date(notification.createdDate));

        div.innerHTML = `
            <div class="notification-content">
                <div class="notification-title">${this.escapeHtml(notification.title)}</div>
                <div class="notification-message">${this.escapeHtml(notification.message)}</div>
                <div class="notification-time">${timeAgo}</div>
            </div>
            ${!notification.isRead ? '<div class="notification-indicator"></div>' : ''}
        `;

        // Add click handler for mark as read and redirect
        div.addEventListener('click', () => this.handleNotificationClick(notification));

        return div;
    }

    async handleNotificationClick(notification) {
        try {
            // Mark as read
            if (!notification.isRead) {
                const formData = new FormData();
                formData.append('__RequestVerificationToken', this.getAntiForgeryToken());
                
                const response = await fetch(`/Notifications/MarkAsRead/${notification.id}`, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    this.unreadCount--;
                    this.updateBadge();
                    
                    // Update UI to show as read
                    const element = document.querySelector(`[data-notification-id="${notification.id}"]`);
                    if (element) {
                        element.classList.remove('unread');
                        element.classList.add('read');
                        const indicator = element.querySelector('.notification-indicator');
                        if (indicator) indicator.remove();
                    }
                }
            }

            // Redirect to link if provided
            if (notification.link) {
                window.location.href = notification.link;
            }

            // Close dropdown
            this.closeDropdown();

        } catch (error) {
            console.error('Error handling notification click:', error);
        }
    }

    showToast(notification) {
        // Create toast element
        const toast = document.createElement('div');
        toast.className = 'notification-toast';
        toast.innerHTML = `
            <div class="toast-content">
                <div class="toast-title">${this.escapeHtml(notification.title)}</div>
                <div class="toast-message">${this.escapeHtml(notification.message)}</div>
            </div>
            <button class="toast-close">&times;</button>
        `;

        document.body.appendChild(toast);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            toast.classList.add('fade-out');
            setTimeout(() => toast.remove(), 300);
        }, 5000);

        // Close button handler
        toast.querySelector('.toast-close').addEventListener('click', () => {
            toast.remove();
        });
    }

    playNotificationSound() {
        // Optional: Play a notification sound
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.3;
            audio.play().catch(e => console.log('Audio play failed:', e));
        } catch (error) {
            console.log('Notification sound not available');
        }
    }

    closeDropdown() {
        if (this.notificationDropdown) {
            this.notificationDropdown.classList.remove('show');
        }
    }

    formatTimeAgo(date) {
        const seconds = Math.floor((new Date() - date) / 1000);
        
        if (seconds < 60) return 'Vừa xong';
        if (seconds < 3600) return `${Math.floor(seconds / 60)} phút trước`;
        if (seconds < 86400) return `${Math.floor(seconds / 3600)} giờ trước`;
        if (seconds < 604800) return `${Math.floor(seconds / 86400)} ngày trước`;
        
        return date.toLocaleDateString('vi-VN');
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }
}

// Initialize notification manager when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.notificationManager = new NotificationManager();
    window.notificationManager.initialize();
});

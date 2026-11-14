document.addEventListener('DOMContentLoaded', function() {
    const notificationBellBtn = document.getElementById('notificationBellBtn');
    const notificationDropdown = document.getElementById('notificationDropdown');
    const notificationList = document.getElementById('notificationList');
    const notificationBadge = document.getElementById('notificationBadge');
    const markAllReadBtn = document.getElementById('markAllReadBtn');
    
    let isDropdownOpen = false;
    let unreadCount = 0;

    // Poll unread count every 30 seconds
    setInterval(updateUnreadCount, 30000);

    // Initial load
    updateUnreadCount();

    // Toggle dropdown
    notificationBellBtn.addEventListener('click', function(e) {
        e.stopPropagation();
        isDropdownOpen = !isDropdownOpen;
        if (isDropdownOpen) {
            notificationDropdown.style.display = 'block';
            loadRecentNotifications();
        } else {
            notificationDropdown.style.display = 'none';
        }
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function(e) {
        if (isDropdownOpen && !notificationDropdown.contains(e.target) && !notificationBellBtn.contains(e.target)) {
            isDropdownOpen = false;
            notificationDropdown.style.display = 'none';
        }
    });

    // Mark all as read
    markAllReadBtn.addEventListener('click', function(e) {
        e.preventDefault();
        markAllAsRead();
    });

    function updateUnreadCount() {
        fetch('/Notifications/UnreadCount')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    unreadCount = data.unreadCount;
                    updateBadge();
                }
            })
            .catch(error => console.error('Error fetching unread count:', error));
    }

    function updateBadge() {
        if (unreadCount > 0) {
            notificationBadge.textContent = unreadCount > 99 ? '99+' : unreadCount;
            notificationBadge.classList.remove('d-none');
        } else {
            notificationBadge.classList.add('d-none');
        }
    }

    function loadRecentNotifications() {
        notificationList.innerHTML = `
            <div class="text-center py-3">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Đang tải...</span>
                </div>
            </div>
        `;

        fetch('/Notifications/Recent')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    unreadCount = data.unreadCount;
                    updateBadge();
                    renderNotifications(data.notifications);
                }
            })
            .catch(error => {
                console.error('Error fetching notifications:', error);
                notificationList.innerHTML = `
                    <div class="notification-empty">
                        <p>Không thể tải thông báo.</p>
                    </div>
                `;
            });
    }

    function renderNotifications(notifications) {
        if (!notifications || notifications.length === 0) {
            notificationList.innerHTML = `
                <div class="notification-empty">
                    <p>Không có thông báo nào.</p>
                </div>
            `;
            return;
        }

        let html = '';
        notifications.forEach(notification => {
            const timeAgo = formatTimeAgo(notification.createdDate);
            const unreadClass = notification.isRead ? '' : 'unread';
            const link = notification.link || '#';
            
            html += `
                <div class="notification-item position-relative ${unreadClass}" data-id="${notification.id}">
                    <button class="notification-delete-btn" onclick="event.stopPropagation(); deleteNotification(${notification.id})">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <line x1="18" y1="6" x2="6" y2="18"></line>
                            <line x1="6" y1="6" x2="18" y2="18"></line>
                        </svg>
                    </button>
                    <div class="notification-title">${escapeHtml(notification.title)}</div>
                    <div class="notification-message">${escapeHtml(notification.message)}</div>
                    <div class="notification-time">${timeAgo}</div>
                </div>
            `;
        });

        notificationList.innerHTML = html;

        // Add click handlers for notification items
        document.querySelectorAll('.notification-item').forEach(item => {
            item.addEventListener('click', function() {
                const notificationId = parseInt(this.dataset.id);
                const link = notifications.find(n => n.id === notificationId)?.link;
                markAsRead(notificationId);
                if (link && link !== '#') {
                    window.location.href = link;
                }
            });
        });
    }

    function markAsRead(notificationId) {
        const formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        fetch('/Notifications/MarkAsRead', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                unreadCount = Math.max(0, unreadCount - 1);
                updateBadge();
                const item = document.querySelector(`.notification-item[data-id="${notificationId}"]`);
                if (item) {
                    item.classList.remove('unread');
                }
            }
        })
        .catch(error => console.error('Error marking notification as read:', error));
    }

    function markAllAsRead() {
        const formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        fetch('/Notifications/MarkAllAsRead', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                unreadCount = 0;
                updateBadge();
                document.querySelectorAll('.notification-item').forEach(item => {
                    item.classList.remove('unread');
                });
            }
        })
        .catch(error => console.error('Error marking all as read:', error));
    }

    async function deleteNotification(notificationId) {
        const { isConfirmed } = await Swal.fire({
            title: 'Xác nhận',
            text: 'Bạn có chắc muốn xóa thông báo này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Huỷ'
        });
        if (!isConfirmed) {
            return;
        }

        const formData = new FormData();
        formData.append('__RequestVerificationToken', getAntiForgeryToken());

        fetch('/Notifications/Delete', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const item = document.querySelector(`.notification-item[data-id="${notificationId}"]`);
                if (item) {
                    item.remove();
                    if (!item.classList.contains('unread')) {
                        unreadCount = Math.max(0, unreadCount - 1);
                        updateBadge();
                    }
                    // Check if list is empty
                    if (notificationList.querySelectorAll('.notification-item').length === 0) {
                        notificationList.innerHTML = `
                            <div class="notification-empty">
                                <p>Không có thông báo nào.</p>
                            </div>
                        `;
                    }
                }
            }
        })
        .catch(error => console.error('Error deleting notification:', error));
    }

    function formatTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const seconds = Math.floor((now - date) / 1000);

        const intervals = {
            'năm': 31536000,
            'tháng': 2592000,
            'tuần': 604800,
            'ngày': 86400,
            'giờ': 3600,
            'phút': 60
        };

        for (const [unit, secondsInUnit] of Object.entries(intervals)) {
            const interval = Math.floor(seconds / secondsInUnit);
            if (interval >= 1) {
                return `${interval} ${unit} trước`;
            }
        }

        return 'Vừa xong';
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }
});

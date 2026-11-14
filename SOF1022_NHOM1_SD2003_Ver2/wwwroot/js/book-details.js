/**
 * Book Details Page - Interactive JavaScript
 * Handles tabs, animations, carousel, and real API interactions
 */

// Get book ID from page
const bookId = document.querySelector('[data-book-id]')?.dataset.bookId || 
               document.querySelector('.floating-btn[data-book-id]')?.dataset.bookId ||
               window.location.pathname.split('/').pop();

document.addEventListener('DOMContentLoaded', function() {
    // Initialize all components
    initTabs();
    initCarousel();
    initFloatingButtons();
    if (typeof initToastNotifications === 'function') {
        initToastNotifications();
    }
    initDescriptionToggle();
    initRatingDistribution();
    initStarRating();
    initCommentActions();
    initQuickView();
    initScrollAnimations();

    // Load real data (except favorite status)
    loadComments();
    loadRatingStats();
});

// ===== TABS FUNCTIONALITY =====
function initTabs() {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            const targetTab = this.dataset.tab;

            // Remove active class from all buttons and contents
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabContents.forEach(content => content.classList.remove('active'));

            // Add active class to clicked button and target content
            this.classList.add('active');
            const targetContent = document.querySelector(`.tab-content[data-tab="${targetTab}"]`);
            if (targetContent) {
                targetContent.classList.add('active');
            }
        });
    });
}

// ===== CAROUSEL FUNCTIONALITY =====
function initCarousel() {
    const carousels = document.querySelectorAll('.carousel-container');

    carousels.forEach(carousel => {
        const track = carousel.querySelector('.carousel-track');
        const prevBtn = carousel.querySelector('.carousel-nav.prev');
        const nextBtn = carousel.querySelector('.carousel-nav.next');

        if (!track || !prevBtn || !nextBtn) return;

        const items = track.querySelectorAll('.carousel-item');
        if (items.length === 0) return;

        // Ensure track is visible for proper width calculation
        track.style.width = 'max-content';
        track.style.display = 'flex';

        let currentIndex = 0;
        const itemWidth = 240; // Width of each carousel item
        const gap = 16; // Gap between items
        const visibleItems = 4; // Number of visible items
        const maxIndex = Math.max(0, items.length - visibleItems);

        // Initial state
        updateCarousel();

        prevBtn.addEventListener('click', () => {
            if (currentIndex > 0) {
                currentIndex--;
                updateCarousel();
            }
        });

        nextBtn.addEventListener('click', () => {
            if (currentIndex < maxIndex) {
                currentIndex++;
                updateCarousel();
            }
        });

        function updateCarousel() {
            const translateX = -(currentIndex * (itemWidth + gap));
            track.style.transform = `translateX(${translateX}px)`;
            track.style.transition = 'transform 0.3s ease';

            // Update button states
            prevBtn.disabled = currentIndex === 0;
            nextBtn.disabled = currentIndex === maxIndex;

            prevBtn.style.opacity = currentIndex === 0 ? '0.3' : '1';
            prevBtn.style.cursor = currentIndex === 0 ? 'not-allowed' : 'pointer';
            nextBtn.style.opacity = currentIndex === maxIndex ? '0.3' : '1';
            nextBtn.style.cursor = currentIndex === maxIndex ? 'not-allowed' : 'pointer';
        }
    });
}

// ===== FLOATING BUTTONS =====
function initFloatingButtons() {
    const scrollTopBtn = document.querySelector('.floating-btn[data-action="scrollTop"]');

    if (scrollTopBtn) {
        scrollTopBtn.addEventListener('click', function() {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });

        window.addEventListener('scroll', function() {
            if (window.scrollY > 500) {
                scrollTopBtn.style.opacity = '1';
                scrollTopBtn.style.pointerEvents = 'auto';
            } else {
                scrollTopBtn.style.opacity = '0';
                scrollTopBtn.style.pointerEvents = 'none';
            }
        });

        scrollTopBtn.style.opacity = '0';
        scrollTopBtn.style.pointerEvents = 'none';
        scrollTopBtn.style.transition = 'opacity 0.3s ease';
    }
}

// ===== LOAD FAVORITE STATUS =====
// Removed - favorite now only toggles color without API call

// ===== LOAD COMMENTS =====
async function loadComments(page = 1) {
    try {
        const response = await fetch(`/api/BookComment/list?bookId=${bookId}&page=${page}&pageSize=10`);
        const result = await response.json();

        if (result.success && result.comments) {
            const commentsContainer = document.querySelector('.comments-list');
            if (commentsContainer) {
                // Remove existing comments
                const existingComments = commentsContainer.querySelectorAll('.comment-item');
                existingComments.forEach(comment => comment.remove());

                // Add real comments
                result.comments.forEach(comment => {
                    const commentHtml = createCommentHtml(comment);
                    commentsContainer.appendChild(commentHtml);
                });

                // Update comment count
                const commentCountText = document.querySelector('#comment-count-text');
                if (commentCountText) {
                    commentCountText.textContent = result.totalCount;
                }
            }
        }
    } catch (error) {
        console.error('Error loading comments:', error);
    }
}

function formatTimeAgo(isoString) {
    const date = new Date(isoString);
    const now = new Date();
    const seconds = Math.floor((now - date) / 1000);

    const intervals = {
        năm: 31536000,
        tháng: 2592000,
        tuần: 604800,
        ngày: 86400,
        giờ: 3600,
        phút: 60
    };

    for (const [unit, secondsInUnit] of Object.entries(intervals)) {
        const interval = Math.floor(seconds / secondsInUnit);
        if (interval >= 1) {
            return `${interval} ${unit} trước`;
        }
    }

    return 'Vừa xong';
}

function createCommentHtml(comment, isReply = false) {
    const div = document.createElement('div');
    div.className = isReply ? 'comment-item reply' : 'comment-item';
    div.dataset.commentId = comment.id;

    const formattedDate = formatTimeAgo(comment.date);
    
    // Get current user ID
    const currentUserId = document.querySelector('input[name="UserId"]')?.value;
    
    // Generate avatar URL - use actual avatar if available, otherwise use UI Avatars API
    const avatarUrl = comment.avatar && comment.avatar.trim() !== '' 
        ? comment.avatar 
        : `https://ui-avatars.com/api/?name=${encodeURIComponent(comment.author || 'User')}&background=10b981&color=fff&size=100`;
    
    // Check if this is the user's own comment
    const isOwnComment = currentUserId && comment.readerId && parseInt(currentUserId) === comment.readerId;
    
    // Generate profile link based on whether it's the user's own profile or public profile
    const profileLink = comment.readerCode && comment.readerCode.trim() !== '' 
        ? (isOwnComment ? '/Account/Profile' : `/Account/Profile?id=${comment.readerCode}`) 
        : '#';

    div.innerHTML = `
        <div class="comment-header">
            ${comment.readerCode ? `
                <a href="${profileLink}" class="comment-avatar-link" style="text-decoration: none; cursor: pointer;">
                    <div class="comment-avatar" style="background-image: url('${avatarUrl}'); background-size: cover; background-position: center; width: 40px; height: 40px; border-radius: 50%; border: 2px solid #00ffcc;"></div>
                </a>
            ` : `
                <div class="comment-avatar" style="background-image: url('${avatarUrl}'); background-size: cover; background-position: center; width: 40px; height: 40px; border-radius: 50%; border: 2px solid #00ffcc;"></div>
            `}
            <div>
                ${comment.readerCode ? `
                    <a href="${profileLink}" class="comment-author-link" style="text-decoration: none; color: inherit; cursor: pointer;">
                        <div class="comment-author">${comment.author}</div>
                    </a>
                ` : `
                    <div class="comment-author">${comment.author}</div>
                `}
                <div class="comment-date">${formattedDate}</div>
            </div>
        </div>
        <div class="comment-text">${comment.content}</div>
        <div class="comment-actions">
            <button class="comment-action like-btn" data-comment-id="${comment.id}">
                <i class="bi bi-hand-thumbs-up"></i>
                <span class="like-count">${comment.likes}</span>
            </button>
            <button class="comment-action reply-btn" data-comment-id="${comment.id}">
                <i class="bi bi-reply"></i>
                Trả lời
            </button>
            ${!isReply && comment.replyCount > 0 ? `
                <button class="comment-action view-replies-btn" data-comment-id="${comment.id}">
                    <i class="bi bi-chat-dots"></i>
                    <span>Xem ${comment.replyCount} câu trả lời</span>
                </button>
            ` : ''}
            ${!isOwnComment ? `
                <button class="comment-action report-btn" data-comment-id="${comment.id}">
                    <i class="bi bi-flag"></i>
                    Báo cáo
                </button>
            ` : ''}
        </div>
        <div class="replies-container" id="replies-${comment.id}"></div>
    `;
    return div;
}

// ===== LOAD RATING STATS =====
async function loadRatingStats() {
    try {
        const response = await fetch(`/api/BookRating/stats?bookId=${bookId}`);
        const result = await response.json();
        
        if (result.success) {
            // Update rating display
            const ratingValue = document.querySelector('.rating-value');
            const ratingCount = document.querySelector('.rating-count');
            
            if (ratingValue) {
                ratingValue.textContent = result.averageRating;
            }
            if (ratingCount) {
                ratingCount.textContent = `(${result.totalRatings} đánh giá)`;
            }
            
            // Update rating distribution bars
            const total = result.totalRatings || 1;
            const distribution = result.distribution;
            
            const ratingRows = document.querySelectorAll('.rating-row');
            if (ratingRows.length >= 5) {
                ratingRows[0].querySelector('.rating-row-fill').style.width = `${(distribution.five / total) * 100}%`;
                ratingRows[0].querySelector('.rating-row-count').textContent = `${Math.round((distribution.five / total) * 100)}%`;
                
                ratingRows[1].querySelector('.rating-row-fill').style.width = `${(distribution.four / total) * 100}%`;
                ratingRows[1].querySelector('.rating-row-count').textContent = `${Math.round((distribution.four / total) * 100)}%`;
                
                ratingRows[2].querySelector('.rating-row-fill').style.width = `${(distribution.three / total) * 100}%`;
                ratingRows[2].querySelector('.rating-row-count').textContent = `${Math.round((distribution.three / total) * 100)}%`;
                
                ratingRows[3].querySelector('.rating-row-fill').style.width = `${(distribution.two / total) * 100}%`;
                ratingRows[3].querySelector('.rating-row-count').textContent = `${Math.round((distribution.two / total) * 100)}%`;
                
                ratingRows[4].querySelector('.rating-row-fill').style.width = `${(distribution.one / total) * 100}%`;
                ratingRows[4].querySelector('.rating-row-count').textContent = `${Math.round((distribution.one / total) * 100)}%`;
            }
        }
    } catch (error) {
        console.error('Error loading rating stats:', error);
    }
}

function showToast(message, type = 'success') {
    // Remove existing toast
    const existingToast = document.querySelector('.toast');
    if (existingToast) {
        existingToast.remove();
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.className = 'toast';
    
    const icon = type === 'success' ? 'bi-check-circle' : 
                  type === 'error' ? 'bi-x-circle' : 'bi-info-circle';
    
    toast.innerHTML = `
        <i class="bi ${icon} toast-icon"></i>
        <span class="toast-message">${message}</span>
    `;

    document.body.appendChild(toast);

    // Show toast
    setTimeout(() => toast.classList.add('show'), 10);

    // Hide toast after 3 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ===== DESCRIPTION TOGGLE =====
function initDescriptionToggle() {
    const expandBtn = document.getElementById('expandBtn');
    const descriptionText = document.getElementById('descriptionText');

    if (expandBtn && descriptionText) {
        expandBtn.addEventListener('click', function() {
            const isCollapsed = descriptionText.classList.contains('collapsed');

            if (isCollapsed) {
                descriptionText.classList.remove('collapsed');
                descriptionText.style.maxHeight = 'none';
                this.innerHTML = '<i class="bi bi-chevron-up"></i> Thu gọn';
            } else {
                descriptionText.classList.add('collapsed');
                descriptionText.style.maxHeight = '200px';
                this.innerHTML = '<i class="bi bi-chevron-down"></i> Xem thêm';
            }
        });

        // Initialize collapsed state if text is long (more than 4-5 lines)
        // Wait for DOM to fully render before checking scrollHeight
        setTimeout(() => {
            const lineHeight = 24; // Approximate line height in pixels
            const maxLines = 5;
            const maxHeight = lineHeight * maxLines;

            if (descriptionText.scrollHeight > maxHeight) {
                descriptionText.classList.add('collapsed');
                descriptionText.style.maxHeight = maxHeight + 'px';
                descriptionText.style.overflow = 'hidden';
                expandBtn.innerHTML = '<i class="bi bi-chevron-down"></i> Xem thêm';
                expandBtn.style.display = 'flex';
            } else {
                expandBtn.style.display = 'none';
            }
        }, 100);
    }
}

// ===== RATING DISTRIBUTION ANIMATION =====
function initRatingDistribution() {
    const ratingBars = document.querySelectorAll('.rating-row-fill');

    ratingBars.forEach((bar, index) => {
        const width = bar.style.width;
        bar.style.width = '0';

        setTimeout(() => {
            bar.style.width = width;
        }, 100 + (index * 100));
    });
}

// ===== STAR RATING INTERACTION =====
function initStarRating() {
    const starRating = document.getElementById('starRating');
    const ratingFeedback = document.getElementById('ratingFeedback');

    if (!starRating) return;

    const stars = starRating.querySelectorAll('.star');
    const bookId = starRating.dataset.bookId;

    // Handle star hover
    stars.forEach((star, index) => {
        star.addEventListener('mouseenter', function() {
            const value = parseInt(this.dataset.value);
            highlightStars(stars, value);
        });
    });

    // Handle star mouse leave (reset to selected rating)
    starRating.addEventListener('mouseleave', function() {
        const selectedValue = starRating.dataset.selectedValue || 0;
        highlightStars(stars, parseInt(selectedValue));
    });

    // Handle star click
    stars.forEach((star) => {
        star.addEventListener('click', function() {
            const value = parseInt(this.dataset.value);
            submitRating(bookId, value, ratingFeedback, stars);
        });
    });
}

function highlightStars(stars, value) {
    stars.forEach((star) => {
        const starValue = parseInt(star.dataset.value);
        const icon = star.querySelector('i');

        if (starValue <= value) {
            star.classList.add('active');
            icon.classList.remove('bi-star');
            icon.classList.add('bi-star-fill');
        } else {
            star.classList.remove('active');
            icon.classList.remove('bi-star-fill');
            icon.classList.add('bi-star');
        }
    });
}

async function submitRating(bookId, rating, feedbackElement, stars) {
    const starRating = document.getElementById('starRating');

    // Set selected value
    starRating.dataset.selectedValue = rating;
    highlightStars(stars, rating);

    // Show loading state
    feedbackElement.textContent = 'Đang gửi đánh giá...';
    feedbackElement.style.color = '#8b949e';

    try {
        const response = await fetch(`/Books/Rate?bookId=${bookId}&rating=${rating}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (result.success) {
            feedbackElement.textContent = result.message;
            feedbackElement.style.color = '#22c55e';

            // Update main rating display
            const mainRatingElements = document.querySelectorAll('.rating-value, #stat-ratings');
            mainRatingElements.forEach(el => {
                el.textContent = result.averageRating;
            });

            // Update review count
            const reviewCountElements = document.querySelectorAll('#stat-comments');
            reviewCountElements.forEach(el => {
                el.textContent = result.reviewCount;
            });

            showToast(result.message, 'success');
        } else {
            feedbackElement.textContent = result.message;
            feedbackElement.style.color = '#ef4444';
            showToast(result.message, 'error');
        }
    } catch (error) {
        console.error('Error submitting rating:', error);
        feedbackElement.textContent = 'Lỗi khi gửi đánh giá';
        feedbackElement.style.color = '#ef4444';
        showToast('Lỗi khi gửi đánh giá', 'error');
    }
}

// ===== COMMENT ACTIONS (POPUP MODAL & SWEETALERT2) =====
function initCommentActions() {
    const commentModalEl = document.getElementById('commentModal');
    let commentModalInstance = null;
    
    function getCommentModal() {
        if (!commentModalInstance && commentModalEl && typeof bootstrap !== 'undefined') {
            commentModalInstance = bootstrap.Modal.getOrCreateInstance(commentModalEl);
        }
        return commentModalInstance;
    }

    // Character counter for comment modal textarea
    const commentInput = document.getElementById('modalCommentText');
    const commentCharCount = document.getElementById('commentCharCount');
    if (commentInput && commentCharCount) {
        commentInput.addEventListener('input', function() {
            commentCharCount.textContent = `${this.value.length} / 1000`;
        });
    }

    // Auto focus textarea when modal opens
    if (commentModalEl) {
        commentModalEl.addEventListener('shown.bs.modal', function() {
            const textarea = document.getElementById('modalCommentText');
            if (textarea) textarea.focus();
        });
    }

    // Cancel reply context button inside modal
    const cancelReplyBtn = document.getElementById('cancelReplyContextBtn');
    if (cancelReplyBtn) {
        cancelReplyBtn.addEventListener('click', function() {
            const parentIdInput = document.getElementById('modalParentCommentId');
            if (parentIdInput) parentIdInput.value = '';
            
            const banner = document.getElementById('replyingToBanner');
            if (banner) {
                banner.classList.add('d-none');
                banner.classList.remove('d-flex');
            }

            const titleEl = document.getElementById('commentModalLabel');
            if (titleEl) titleEl.textContent = 'Viết bình luận';

            const submitBtnText = document.getElementById('commentSubmitText');
            if (submitBtnText) submitBtnText.textContent = 'Đăng bình luận';
        });
    }

    // Main "Viết bình luận" trigger button
    const writeCommentBtn = document.querySelector('.write-comment-btn');
    if (writeCommentBtn) {
        writeCommentBtn.addEventListener('click', function(e) {
            e.preventDefault();
            const userIdString = document.querySelector('input[name="UserId"]')?.value;
            if (!userIdString) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'info',
                        title: 'Đăng nhập để bình luận',
                        text: 'Vui lòng đăng nhập để cùng chia sẻ suy nghĩ và cảm nhận về tác phẩm này.',
                        showCancelButton: true,
                        confirmButtonText: 'Đăng nhập ngay',
                        cancelButtonText: 'Để sau',
                        confirmButtonColor: '#f59e0b',
                        cancelButtonColor: '#64748b',
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
                        }
                    });
                } else {
                    showToast('Vui lòng đăng nhập để bình luận', 'error');
                }
                return;
            }

            // Reset modal for new root comment
            const parentIdInput = document.getElementById('modalParentCommentId');
            if (parentIdInput) parentIdInput.value = '';

            const banner = document.getElementById('replyingToBanner');
            if (banner) {
                banner.classList.add('d-none');
                banner.classList.remove('d-flex');
            }

            const titleEl = document.getElementById('commentModalLabel');
            if (titleEl) titleEl.textContent = 'Viết bình luận';

            const submitBtnText = document.getElementById('commentSubmitText');
            if (submitBtnText) submitBtnText.textContent = 'Đăng bình luận';

            const textarea = document.getElementById('modalCommentText');
            if (textarea) textarea.value = '';

            if (commentCharCount) commentCharCount.textContent = '0 / 1000';

            const modal = getCommentModal();
            if (modal) modal.show();
        });
    }

    // Submit button in comment modal
    const submitModalBtn = document.getElementById('modalSubmitCommentBtn');
    if (submitModalBtn) {
        submitModalBtn.addEventListener('click', async function(e) {
            e.preventDefault();
            const textarea = document.getElementById('modalCommentText');
            const content = textarea ? textarea.value.trim() : '';
            if (!content) {
                showToast('Nội dung bình luận không được để trống', 'error');
                textarea?.focus();
                return;
            }

            const parentId = document.getElementById('modalParentCommentId')?.value;
            const spinner = document.getElementById('commentSubmitSpinner');
            const icon = document.getElementById('commentSubmitIcon');
            const btnText = document.getElementById('commentSubmitText');

            submitModalBtn.disabled = true;
            if (spinner) spinner.classList.remove('d-none');
            if (icon) icon.classList.add('d-none');

            try {
                const payload = {
                    bookId: parseInt(bookId),
                    content: content
                };
                if (parentId && parseInt(parentId) > 0) {
                    payload.parentCommentId = parseInt(parentId);
                }

                const response = await fetch('/api/BookComment/add', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(payload)
                });

                const result = await response.json();

                if (result.success) {
                    showToast(result.message || 'Bình luận thành công!', 'success');
                    const modal = getCommentModal();
                    if (modal) modal.hide();
                    if (textarea) textarea.value = '';
                    if (commentCharCount) commentCharCount.textContent = '0 / 1000';

                    // Refresh comments
                    await loadComments();
                    if (parentId && parseInt(parentId) > 0) {
                        await loadReplies(parseInt(parentId));
                    }
                } else {
                    showToast(result.message || 'Không thể đăng bình luận', 'error');
                }
            } catch (error) {
                console.error('Error submitting comment:', error);
                showToast('Lỗi kết nối khi gửi bình luận', 'error');
            } finally {
                submitModalBtn.disabled = false;
                if (spinner) spinner.classList.add('d-none');
                if (icon) icon.classList.remove('d-none');
            }
        });
    }

    // Event delegation for reply, view replies, like, and report buttons
    document.addEventListener('click', async function(e) {
        // 1. Reply button click -> Open comment modal in reply mode
        const replyBtn = e.target.closest('.reply-btn');
        if (replyBtn) {
            e.preventDefault();
            const commentId = replyBtn.dataset.commentId;
            const userIdString = document.querySelector('input[name="UserId"]')?.value;

            if (!userIdString) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'info',
                        title: 'Đăng nhập để trả lời',
                        text: 'Vui lòng đăng nhập để phản hồi lại bình luận này.',
                        showCancelButton: true,
                        confirmButtonText: 'Đăng nhập ngay',
                        cancelButtonText: 'Để sau',
                        confirmButtonColor: '#f59e0b',
                        cancelButtonColor: '#64748b',
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.href = `/Account/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
                        }
                    });
                } else {
                    showToast('Vui lòng đăng nhập để trả lời', 'error');
                }
                return;
            }

            const commentItem = replyBtn.closest('.comment-item');
            const authorEl = commentItem?.querySelector('.comment-author');
            const authorName = authorEl ? authorEl.textContent.trim() : 'Độc giả';

            // Setup modal for reply mode
            const parentIdInput = document.getElementById('modalParentCommentId');
            if (parentIdInput) parentIdInput.value = commentId;

            const banner = document.getElementById('replyingToBanner');
            const authorSpan = document.getElementById('replyingToAuthor');
            if (banner && authorSpan) {
                authorSpan.textContent = authorName;
                banner.classList.remove('d-none');
                banner.classList.add('d-flex');
            }

            const titleEl = document.getElementById('commentModalLabel');
            if (titleEl) titleEl.textContent = `Trả lời bình luận của ${authorName}`;

            const submitBtnText = document.getElementById('commentSubmitText');
            if (submitBtnText) submitBtnText.textContent = 'Gửi câu trả lời';

            const textarea = document.getElementById('modalCommentText');
            if (textarea) textarea.value = '';
            if (commentCharCount) commentCharCount.textContent = '0 / 1000';

            const modal = getCommentModal();
            if (modal) modal.show();
            return;
        }

        // 2. View replies button click -> Toggle or fetch replies
        const viewRepliesBtn = e.target.closest('.view-replies-btn');
        if (viewRepliesBtn) {
            e.preventDefault();
            const commentId = viewRepliesBtn.dataset.commentId;
            const repliesContainer = document.querySelector(`#replies-${commentId}`);
            if (repliesContainer) {
                if (repliesContainer.children.length > 0 && repliesContainer.style.display !== 'none') {
                    repliesContainer.style.display = 'none';
                    const icon = viewRepliesBtn.querySelector('i');
                    if (icon) icon.className = 'bi bi-chevron-down';
                } else {
                    repliesContainer.style.display = 'block';
                    await loadReplies(commentId);
                    const icon = viewRepliesBtn.querySelector('i');
                    if (icon) icon.className = 'bi bi-chevron-up';
                }
            }
            return;
        }

        // 3. Like button click
        const likeBtn = e.target.closest('.like-btn');
        if (likeBtn) {
            e.preventDefault();
            const commentId = likeBtn.dataset.commentId;
            const userIdString = document.querySelector('input[name="UserId"]')?.value;

            if (!userIdString) {
                showToast('Vui lòng đăng nhập để thích bình luận', 'error');
                return;
            }

            try {
                const response = await fetch('/api/BookComment/like', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        commentId: parseInt(commentId)
                    })
                });

                const result = await response.json();

                if (result.success) {
                    const likeCountSpan = likeBtn.querySelector('.like-count');
                    if (likeCountSpan) likeCountSpan.textContent = result.likes;

                    const icon = likeBtn.querySelector('i');
                    if (icon) {
                        icon.classList.toggle('bi-hand-thumbs-up-fill');
                        icon.classList.toggle('bi-hand-thumbs-up');
                    }
                    likeBtn.classList.toggle('liked');
                } else {
                    showToast(result.message || 'Lỗi khi thích bình luận', 'error');
                }
            } catch (error) {
                console.error('Error liking comment:', error);
                showToast('Lỗi khi thích bình luận', 'error');
            }
            return;
        }

        // 4. Report button click -> Modern SweetAlert2 prompt popup
        const reportBtn = e.target.closest('.report-btn');
        if (reportBtn) {
            e.preventDefault();
            const commentId = reportBtn.dataset.commentId;
            const userIdString = document.querySelector('input[name="UserId"]')?.value;

            if (!userIdString) {
                showToast('Vui lòng đăng nhập để báo cáo vi phạm', 'error');
                return;
            }

            if (typeof Swal !== 'undefined') {
                const { value: reason } = await Swal.fire({
                    title: 'Báo cáo bình luận vi phạm',
                    input: 'textarea',
                    inputLabel: 'Lý do báo cáo',
                    inputPlaceholder: 'Nêu rõ lý do (nội dung kích động, quảng cáo spam, xúc phạm, thô tục...)',
                    inputAttributes: {
                        'aria-label': 'Lý do báo cáo',
                        'maxlength': '500'
                    },
                    showCancelButton: true,
                    confirmButtonText: 'Gửi báo cáo',
                    cancelButtonText: 'Hủy',
                    confirmButtonColor: '#ef4444',
                    cancelButtonColor: '#64748b',
                    reverseButtons: true,
                    inputValidator: (value) => {
                        if (!value || !value.trim()) {
                            return 'Vui lòng nhập lý do báo cáo!';
                        }
                    }
                });

                if (reason) {
                    try {
                        const response = await fetch('/api/BookComment/report', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                                commentId: parseInt(commentId),
                                reason: reason.trim()
                            })
                        });

                        const result = await response.json();

                        if (result.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Đã gửi báo cáo',
                                text: result.message || 'Cảm ơn bạn đã hỗ trợ giữ gìn môi trường đọc sách văn minh.',
                                confirmButtonColor: '#10b981'
                            });
                        } else {
                            showToast(result.message || 'Không thể gửi báo cáo', 'error');
                        }
                    } catch (error) {
                        console.error('Error submitting report:', error);
                        showToast('Lỗi khi gửi báo cáo', 'error');
                    }
                }
            } else {
                const reason = prompt('Nhập lý do báo cáo bình luận:');
                if (reason && reason.trim()) {
                    try {
                        const response = await fetch('/api/BookComment/report', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify({
                                commentId: parseInt(commentId),
                                reason: reason.trim()
                            })
                        });
                        const result = await response.json();
                        showToast(result.message, result.success ? 'success' : 'error');
                    } catch (err) {
                        showToast('Lỗi gửi báo cáo', 'error');
                    }
                }
            }
            return;
        }
    });
}

async function loadReplies(parentCommentId) {
    try {
        const response = await fetch(`/api/BookComment/replies?parentCommentId=${parentCommentId}`);
        const result = await response.json();

        if (result.success && result.replies) {
            const repliesContainer = document.querySelector(`#replies-${parentCommentId}`);
            if (repliesContainer) {
                // Remove existing reply form if any
                const existingForm = repliesContainer.querySelector('.reply-form');
                if (existingForm) {
                    existingForm.remove();
                }

                // Clear existing replies
                repliesContainer.innerHTML = '';

                // Add replies
                result.replies.forEach(reply => {
                    const replyHtml = createCommentHtml(reply, true);
                    repliesContainer.appendChild(replyHtml);
                });
            }
        }
    } catch (error) {
        console.error('Error loading replies:', error);
    }
}

// ===== QUICK VIEW MODAL =====
function initQuickView() {
    const quickViewButtons = document.querySelectorAll('.quick-view-btn');
    
    quickViewButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const bookCard = this.closest('.book-card');
            const bookId = bookCard.dataset.bookId;
            
            // You can implement a modal here to show quick book details
            // For now, just navigate to the book details page
            const link = bookCard.querySelector('a');
            if (link) {
                window.location.href = link.href;
            }
        });
    });
}

// ===== SCROLL ANIMATIONS =====
function initScrollAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    const animatedElements = document.querySelectorAll(
        '.stat-card, .info-item, .comment-item, .book-card'
    );

    animatedElements.forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });

    // Add CSS class for animation
    const style = document.createElement('style');
    style.textContent = `
        .animate-in {
            opacity: 1 !important;
            transform: translateY(0) !important;
        }
    `;
    document.head.appendChild(style);
}

// ===== ACTION BUTTONS =====
document.addEventListener('click', async function(e) {
    // Skip if clicked on anchor link (let it navigate naturally)
    const clickedLink = e.target.closest('a.action-link');
    if (clickedLink) {
        return; // Allow default link behavior
    }

    // Pre-order/Reserve button
    if (e.target.closest('button.action-btn[data-action="reserve"]')) {
        e.preventDefault();
        const btn = e.target.closest('button.action-btn');
        const bookId = btn.dataset.bookId;

        // Check if user is logged in
        const userId = document.querySelector('input[name="UserId"]')?.value;
        if (!userId) {
            showToast('Vui lòng đăng nhập để đặt trước sách', 'error');
            window.location.href = '/Account/Login';
            return;
        }

        // Show confirmation dialog
        const { isConfirmed } = await Swal.fire({
            title: 'Xác nhận',
            text: 'Sách đã được mượn hết, bạn có muốn đăng ký đặt trước không? Bạn sẽ được thông báo khi có sách trả lại.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Huỷ'
        });
        if (!isConfirmed) {
            return;
        }

        // Call API to create reservation
        fetch(`/Books/CreateReservation?bookId=${bookId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                showToast(result.message, 'success');
                // Redirect to reservations page after short delay
                setTimeout(() => {
                    window.location.href = '/Reservations/MyReservation';
                }, 1500);
            } else {
                showToast(result.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error creating reservation:', error);
            showToast('Lỗi khi đặt trước sách', 'error');
        });
    }

    // Favorite button (in action buttons)
    if (e.target.closest('button.action-btn[data-action="favorite"]')) {
        e.preventDefault();
        const btn = e.target.closest('button.action-btn');
        const bookId = btn.dataset.bookId;
        const icon = btn.querySelector('i');
        const textSpan = btn.querySelector('.favorite-text');

        // Check if user is logged in
        const userId = document.querySelector('input[name="UserId"]')?.value;
        if (!userId) {
            showToast('Vui lòng đăng nhập để sử dụng tính năng yêu thích', 'error');
            return;
        }

        // Prevent multiple simultaneous clicks
        if (btn.disabled || btn.classList.contains('processing')) {
            return;
        }

        // Set processing state
        btn.classList.add('processing');
        btn.disabled = true;

        // Call API to toggle favorite
        fetch(`/Books/ToggleFavorite?bookId=${bookId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                // Update button state based on server response
                const isNowFavorited = result.isFavorite;
                if (isNowFavorited) {
                    btn.classList.add('active');
                    icon.classList.remove('bi-heart');
                    icon.classList.add('bi-heart-fill');
                    if (textSpan) textSpan.textContent = 'Đã yêu thích';
                } else {
                    btn.classList.remove('active');
                    icon.classList.remove('bi-heart-fill');
                    icon.classList.add('bi-heart');
                    if (textSpan) textSpan.textContent = 'Yêu thích';
                }

                // Update favorite count display based on server response
                const favoriteCountElements = document.querySelectorAll('#badge-favorite-count, #stat-favorites');
                if (result.favoriteCount !== undefined) {
                    // Use server-provided count if available
                    favoriteCountElements.forEach(el => {
                        el.textContent = `${result.favoriteCount.toLocaleString()} yêu thích`;
                    });
                } else {
                    // Fallback to local calculation
                    const currentCount = parseInt(favoriteCountElements[0]?.textContent.replace(/[^0-9]/g, '') || '0');
                    const newCount = isNowFavorited ? currentCount + 1 : currentCount - 1;
                    favoriteCountElements.forEach(el => {
                        el.textContent = `${newCount.toLocaleString()} yêu thích`;
                    });
                }

                showToast(result.message, 'success');
            } else {
                showToast(result.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error toggling favorite:', error);
            showToast('Lỗi khi cập nhật yêu thích', 'error');
        })
        .finally(() => {
            // Always remove processing state
            btn.classList.remove('processing');
            btn.disabled = false;
        });
    }

});

// ===== KEYBOARD NAVIGATION =====
document.addEventListener('keydown', function(e) {
    // Escape key to close modals
    if (e.key === 'Escape') {
        const replyForms = document.querySelectorAll('.reply-form');
        replyForms.forEach(form => form.remove());
    }
});

// ===== LAZY LOADING IMAGES =====
function initLazyLoading() {
    const lazyImages = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.removeAttribute('data-src');
                observer.unobserve(img);
            }
        });
    });

    lazyImages.forEach(img => imageObserver.observe(img));
}

// ===== RIPPLE EFFECT =====
// Pure CSS ripple via .action-btn::before is used instead of DOM injection to maintain stable button sizing


// ===== STOCK STATUS CHECK =====
function checkStockStatus() {
    const stockBadge = document.querySelector('.stock-badge');
    const borrowBtn = document.querySelector('.action-btn[data-action="borrow"]');
    
    if (stockBadge && borrowBtn) {
        const isOutOfStock = stockBadge.classList.contains('out-of-stock');
        
        if (isOutOfStock) {
            borrowBtn.disabled = true;
            borrowBtn.innerHTML = '<i class="bi bi-x-circle"></i> Hết sách';
            borrowBtn.style.opacity = '0.5';
        }
    }
}

// Initialize stock check
checkStockStatus();

// ===== RATING INTERACTION =====
function initRatingInteraction() {
    const ratingStars = document.querySelectorAll('.rating-display .star');
    
    ratingStars.forEach((star, index) => {
        star.addEventListener('mouseenter', function() {
            // Highlight stars up to current
            ratingStars.forEach((s, i) => {
                if (i <= index) {
                    s.classList.remove('empty');
                } else {
                    s.classList.add('empty');
                }
            });
        });

        star.addEventListener('mouseleave', function() {
            // Reset to original rating
            const ratingValue = parseFloat(document.querySelector('.rating-value').textContent);
            ratingStars.forEach((s, i) => {
                if (i < ratingValue) {
                    s.classList.remove('empty');
                } else {
                    s.classList.add('empty');
                }
            });
        });

        star.addEventListener('click', async function() {
            const newRating = index + 1;
            
            try {
                const response = await fetch('/api/BookRating/add', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        bookId: parseInt(bookId),
                        rating: newRating
                    })
                });
                
                const result = await response.json();
                
                if (result.success) {
                    document.querySelector('.rating-value').textContent = result.averageRating;
                    document.querySelector('.rating-count').textContent = `(${result.totalRatings} đánh giá)`;
                    showToast(`Đánh giá ${newRating} sao`, 'success');
                    loadRatingStats(); // Reload rating distribution
                } else {
                    showToast(result.message, 'error');
                }
            } catch (error) {
                console.error('Error adding rating:', error);
                showToast('Lỗi khi đánh giá', 'error');
            }
        });
    });
}

// Initialize rating interaction
initRatingInteraction();

window.BookDetails = {
    showToast,
    initTabs,
    initCarousel
};

console.log('Book Details page initialized successfully');

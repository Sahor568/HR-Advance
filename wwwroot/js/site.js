// HRMS Nepal - Main JavaScript

document.addEventListener('DOMContentLoaded', function () {
    loadNotificationCount();
    initSidebarToggle();
});

function loadNotificationCount() {
    fetch('/api/notifications')
        .then(r => r.json())
        .then(data => {
            var unread = Array.isArray(data) ? data.filter(n => !n.isRead).length : 0;
            var badge = document.getElementById('notifBadge');
            if (badge) {
                if (unread > 0) {
                    badge.style.display = 'flex';
                    badge.textContent = unread > 9 ? '9+' : unread;
                } else {
                    badge.style.display = 'none';
                }
            }
        })
        .catch(() => { });
}

function initSidebarToggle() {
    document.addEventListener('click', function (e) {
        var sidebar = document.getElementById('sidebar');
        if (sidebar && sidebar.classList.contains('open')) {
            if (!sidebar.contains(e.target) && !e.target.closest('.header-btn')) {
                sidebar.classList.remove('open');
            }
        }
    });
}

function closeModal(id) {
    var modal = document.getElementById(id);
    if (modal) modal.classList.remove('active');
}

function switchTab(btn, tabId) {
    btn.parentElement.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    var container = btn.closest('.card, .modal-box, main, .main-content');
    if (container) {
        container.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
        var tab = container.querySelector('#tab-' + tabId);
        if (tab) tab.classList.add('active');
    }
}

// Team Management Functions
function initTeamMemberSelection() {
    const memberSelect = document.getElementById('teamMembersSelect');
    const selectedMembersContainer = document.getElementById('selectedMembersContainer');
    
    if (memberSelect && selectedMembersContainer) {
        memberSelect.addEventListener('change', function() {
            const selectedOption = this.options[this.selectedIndex];
            if (selectedOption.value) {
                addSelectedMember(selectedOption.value, selectedOption.text);
                this.selectedIndex = 0; // Reset to placeholder
            }
        });
    }
}

function addSelectedMember(value, text) {
    const container = document.getElementById('selectedMembersContainer');
    if (!container) return;
    
    const memberId = `selected-member-${value}`;
    if (document.getElementById(memberId)) return; // Already added
    
    const memberDiv = document.createElement('div');
    memberDiv.className = 'selected-member-item';
    memberDiv.id = memberId;
    memberDiv.innerHTML = `
        <span>${text}</span>
        <input type="hidden" name="SelectedMemberIds" value="${value}" />
        <button type="button" class="btn-remove-member" onclick="removeSelectedMember('${value}')">
            <i class="fas fa-times"></i>
        </button>
    `;
    container.appendChild(memberDiv);
}

function removeSelectedMember(value) {
    const memberDiv = document.getElementById(`selected-member-${value}`);
    if (memberDiv) {
        memberDiv.remove();
    }
}

// Task Deadline Warning
function initTaskDeadlineWarning() {
    const deadlineInput = document.getElementById('Deadline');
    if (deadlineInput) {
        deadlineInput.addEventListener('change', function() {
            const deadline = new Date(this.value);
            const today = new Date();
            const diffDays = Math.ceil((deadline - today) / (1000 * 60 * 60 * 24));
            
            const warningDiv = document.getElementById('deadlineWarning');
            if (!warningDiv) return;
            
            if (diffDays < 0) {
                warningDiv.innerHTML = '<div class="alert alert-danger mt-2"><i class="fas fa-exclamation-triangle"></i> Deadline is in the past!</div>';
                warningDiv.style.display = 'block';
            } else if (diffDays < 3) {
                warningDiv.innerHTML = `<div class="alert alert-warning mt-2"><i class="fas fa-exclamation-circle"></i> Deadline is very soon (${diffDays} day${diffDays !== 1 ? 's' : ''} from now)</div>`;
                warningDiv.style.display = 'block';
            } else {
                warningDiv.style.display = 'none';
            }
        });
    }
}

// Initialize team management features
document.addEventListener('DOMContentLoaded', function() {
    initTeamMemberSelection();
    initTaskDeadlineWarning();
    
    // Confirm team deactivation/activation
    document.querySelectorAll('form[onsubmit*="confirm"]').forEach(form => {
        form.onsubmit = function() {
            const message = this.getAttribute('data-confirm-message') || 'Are you sure?';
            return confirm(message);
        };
    });
});
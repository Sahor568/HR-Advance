// HR Management System - Global JavaScript Functions

// API Base Configuration
const API_BASE = '/api';

// Global notification function
function showNotification(title, message, type = 'info') {
    const alertClass = {
        'success': 'alert-success',
        'danger': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    }[type] || 'alert-info';

    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <strong>${title}</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    const container = document.querySelector('main') || document.body;
    const alertElement = document.createElement('div');
    alertElement.innerHTML = alertHtml;
    container.insertBefore(alertElement.firstElementChild, container.firstChild);

    setTimeout(() => {
        document.querySelector('.alert')?.remove();
    }, 5000);
}

// Employee Management Functions
const employees = {
    list: [],
    filteredList: [],

    async loadEmployees() {
        try {
            const response = await fetch(`${API_BASE}/Employee`);
            if (!response.ok) throw new Error('Failed to load employees');
            this.list = await response.json();
            this.filteredList = [...this.list];
            this.render();
            this.loadDepartments();
        } catch (error) {
            console.error('Error loading employees:', error);
            showNotification('Error', 'Failed to load employees', 'danger');
        }
    },

    filter(searchTerm = '', deptFilter = '') {
        this.filteredList = this.list.filter(emp => {
            const matchesSearch = !searchTerm || 
                emp.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                emp.user?.email.toLowerCase().includes(searchTerm.toLowerCase());
            
            const matchesDept = !deptFilter || emp.departmentId == deptFilter;
            
            return matchesSearch && matchesDept;
        });
        this.render();
    },

    render() {
        const tbody = document.getElementById('employees-tbody');
        if (!tbody) return;

        if (this.filteredList.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No employees found</td></tr>';
            return;
        }

        tbody.innerHTML = this.filteredList.map(emp => `
            <tr>
                <td>${emp.fullName || '--'}</td>
                <td>${emp.department?.name || '--'}</td>
                <td>${emp.position || '--'}</td>
                <td>Nu. ${emp.salary?.toLocaleString() || '--'}</td>
                <td>${emp.supervisor?.fullName || '--'}</td>
                <td>${emp.user?.email || '--'}</td>
                <td>
                    <button class="btn btn-sm btn-info" onclick="viewEmployee(${emp.id})"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-sm btn-warning" onclick="editEmployee(${emp.id})"><i class="fas fa-edit"></i></button>
                </td>
            </tr>
        `).join('');
    },

    loadDepartments() {
        fetch(`${API_BASE}/Departments`)
            .then(r => r.json())
            .then(depts => {
                const select = document.getElementById('empDepartment');
                const filterSelect = document.getElementById('deptFilter');
                if (select) {
                    select.innerHTML = '<option value="">Select Department</option>' +
                        depts.map(d => `<option value="${d.id}">${d.name}</option>`).join('');
                }
                if (filterSelect) {
                    filterSelect.innerHTML = '<option value="">All Departments</option>' +
                        depts.map(d => `<option value="${d.id}">${d.name}</option>`).join('');
                }
            })
            .catch(e => console.error('Error loading departments:', e));
    }
};

// Employee Management UI Functions
function openAddModal() {
    document.getElementById('empId').value = '';
    document.getElementById('modalTitle').textContent = 'Add New Employee';
    document.getElementById('employeeForm').reset();
    document.getElementById('employeeModal').style.display = 'flex';
}

function closeModal() {
    document.getElementById('employeeModal').style.display = 'none';
    document.getElementById('viewModal').style.display = 'none';
}

function filterEmployees() {
    const searchTerm = document.getElementById('searchInput')?.value || '';
    const deptFilter = document.getElementById('deptFilter')?.value || '';
    employees.filter(searchTerm, deptFilter);
}

function viewEmployee(id) {
    const emp = employees.list.find(e => e.id === id);
    if (!emp) return;

    const content = `
        <div class="row">
            <div class="col-md-6"><label class="form-label"><strong>Full Name</strong></label><p>${emp.fullName}</p></div>
            <div class="col-md-6"><label class="form-label"><strong>Email</strong></label><p>${emp.user?.email}</p></div>
        </div>
        <div class="row">
            <div class="col-md-6"><label class="form-label"><strong>Phone</strong></label><p>${emp.phone || '--'}</p></div>
            <div class="col-md-6"><label class="form-label"><strong>Department</strong></label><p>${emp.department?.name}</p></div>
        </div>
        <div class="row">
            <div class="col-md-6"><label class="form-label"><strong>Position</strong></label><p>${emp.position}</p></div>
            <div class="col-md-6"><label class="form-label"><strong>Salary</strong></label><p>Nu. ${emp.salary?.toLocaleString()}</p></div>
        </div>
        <div class="row">
            <div class="col-md-6"><label class="form-label"><strong>Hire Date</strong></label><p>${new Date(emp.hireDate).toLocaleDateString()}</p></div>
            <div class="col-md-6"><label class="form-label"><strong>Status</strong></label><p>${emp.isActive ? 'Active' : 'Inactive'}</p></div>
        </div>
    `;
    document.getElementById('viewContent').innerHTML = content;
    document.getElementById('viewModal').style.display = 'flex';
}

function editEmployee(id) {
    const emp = employees.list.find(e => e.id === id);
    if (!emp) return;

    document.getElementById('empId').value = emp.id;
    document.getElementById('modalTitle').textContent = 'Edit Employee';
    document.getElementById('empFullName').value = emp.fullName;
    document.getElementById('empEmail').value = emp.user?.email || '';
    document.getElementById('empPhone').value = emp.phone || '';
    document.getElementById('empAge').value = emp.age || '';
    document.getElementById('empDepartment').value = emp.departmentId || '';
    document.getElementById('empPosition').value = emp.position || '';
    document.getElementById('empSalary').value = emp.salary || '';
    document.getElementById('empHireDate').value = emp.hireDate?.split('T')[0] || '';
    document.getElementById('empAddress').value = emp.address || '';
    
    document.getElementById('employeeModal').style.display = 'flex';
}

function deleteEmployee(id) {
    if (!confirm('Are you sure you want to delete this employee? This action cannot be undone.')) {
        return;
    }

    fetch(`${API_BASE}/Employee/${id}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) throw new Error('Failed to delete employee');
        showNotification('Success', 'Employee deleted successfully', 'success');
        employees.loadEmployees();
    })
    .catch(error => {
        console.error('Error deleting employee:', error);
        showNotification('Error', 'Failed to delete employee', 'danger');
    });
}

function submitEmployeeForm() {
    const form = document.getElementById('employeeForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return false;
    }

    const empId = document.getElementById('empId').value;
    const employeeData = {
        id: empId ? parseInt(empId) : 0,
        fullName: document.getElementById('empFullName').value,
        email: document.getElementById('empEmail').value,
        phone: document.getElementById('empPhone').value,
        age: document.getElementById('empAge').value ? parseInt(document.getElementById('empAge').value) : 0,
        departmentId: parseInt(document.getElementById('empDepartment').value),
        position: document.getElementById('empPosition').value,
        salary: parseFloat(document.getElementById('empSalary').value),
        hireDate: document.getElementById('empHireDate').value,
        address: document.getElementById('empAddress').value,
        isActive: true
    };

    const method = empId ? 'PUT' : 'POST';
    const url = empId ? `${API_BASE}/Employee/${empId}` : `${API_BASE}/Employee`;

    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(employeeData)
    })
    .then(response => {
        if (!response.ok) throw new Error('Failed to save employee');
        showNotification('Success', empId ? 'Employee updated successfully' : 'Employee added successfully', 'success');
        closeModal();
        employees.loadEmployees();
    })
    .catch(error => {
        console.error('Error saving employee:', error);
        showNotification('Error', 'Failed to save employee. Please check the form and try again.', 'danger');
    });
}

// Leave Management Functions
const leaves = {
    list: [],
    filteredList: [],

    async loadLeaves() {
        try {
            const response = await fetch(`${API_BASE}/Leave`);
            if (!response.ok) throw new Error('Failed to load leaves');
            this.list = await response.json();
            this.filteredList = [...this.list];
            this.updateStats();
            this.render();
        } catch (error) {
            console.error('Error loading leaves:', error);
        }
    },

    updateStats() {
        const approved = this.list.filter(l => l.status === 'Approved').length;
        const pending = this.list.filter(l => l.status === 'Pending').length;
        const rejected = this.list.filter(l => l.status === 'Rejected').length;

        document.getElementById('stat-approved').textContent = approved;
        document.getElementById('stat-pending').textContent = pending;
        document.getElementById('stat-rejected').textContent = rejected;
        document.getElementById('stat-total').textContent = this.list.length;
        document.getElementById('tab-pending').textContent = pending;
        document.getElementById('tab-approved').textContent = approved;
        document.getElementById('tab-rejected').textContent = rejected;
    },

    filterByStatus(status) {
        if (status === 'all') {
            this.filteredList = [...this.list];
        } else {
            this.filteredList = this.list.filter(l => l.status === status);
        }
        this.render();
    },

    render() {
        const container = document.getElementById('leaves-container');
        if (!container) return;

        if (this.filteredList.length === 0) {
            container.innerHTML = '<div class="text-center text-muted py-4">No leave requests found</div>';
            return;
        }

        const statusBadges = {
            'Approved': '<span style="background: #d1fae5; color: #065f46; padding: 0.25rem 0.75rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600;">Approved</span>',
            'Pending': '<span style="background: #fef3c7; color: #92400e; padding: 0.25rem 0.75rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600;">Pending</span>',
            'Rejected': '<span style="background: #fee2e2; color: #991b1b; padding: 0.25rem 0.75rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600;">Rejected</span>'
        };

        container.innerHTML = this.filteredList.map(leave => `
            <div class="card" style="margin-bottom: 1rem;">
                <div class="card-body">
                    <div style="display: flex; justify-content: space-between; align-items: start;">
                        <div>
                            <h6 style="margin: 0 0 0.5rem 0;">${leave.employee?.fullName}</h6>
                            <p style="margin: 0; color: #6b7280; font-size: 0.85rem;">
                                <i class="fas fa-calendar-alt"></i> ${new Date(leave.fromDate).toLocaleDateString()} to ${new Date(leave.toDate).toLocaleDateString()}
                            </p>
                            <p style="margin: 0.5rem 0 0 0; color: #6b7280; font-size: 0.85rem;">
                                <i class="fas fa-comment"></i> ${leave.reason}
                            </p>
                        </div>
                        <div style="text-align: right;">
                            ${statusBadges[leave.status]}
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    }
};

function filterLeaves(status) {
    leaves.filterByStatus(status);
    document.querySelectorAll('.nav-link').forEach(el => el.classList.remove('active'));
    event.target.classList.add('active');
}

function openAddLeaveModal() {
    document.getElementById('leaveModal').style.display = 'flex';
}

function closeLeaveModal() {
    document.getElementById('leaveModal').style.display = 'none';
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('employees-tbody')) {
        employees.loadEmployees();
    }
    if (document.getElementById('leaves-container')) {
        leaves.loadLeaves();
    }
});

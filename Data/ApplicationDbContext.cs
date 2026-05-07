using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using HR_Management_System.Models;
using HR_Management_System.Models.Identity;
using HR_Management_System.Models.Recruitment;
using HR_Management_System.Models.Performance;

namespace HR_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Suppress the warning about pending model changes
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        // Core HRMS Models
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<EmploymentContract> EmploymentContracts { get; set; }
        public DbSet<DisciplinaryRecord> DisciplinaryRecords { get; set; }
        public DbSet<AccidentLog> AccidentLogs { get; set; }
        public DbSet<MedicalInsuranceClaim> MedicalInsuranceClaims { get; set; }
        public DbSet<SSFRecord> SSFRecords { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<LaborAuditReport> LaborAuditReports { get; set; }

        // Enhanced HRIS Models
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<TrainingAttendance> TrainingAttendances { get; set; }
        public DbSet<TrainingEvaluation> TrainingEvaluations { get; set; }
        public DbSet<TravelRequest> TravelRequests { get; set; }
        public DbSet<TravelExpense> TravelExpenses { get; set; }
        public DbSet<TravelApproval> TravelApprovals { get; set; }
        public DbSet<Reimbursement> Reimbursements { get; set; }
        public DbSet<ReimbursementItem> ReimbursementItems { get; set; }
        public DbSet<ReimbursementApproval> ReimbursementApprovals { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Dynamic Payroll Models
        public DbSet<PayrollConfiguration> PayrollConfigurations { get; set; }
        public DbSet<PayrollComponent> PayrollComponents { get; set; }
        public DbSet<TaxSlab> TaxSlabs { get; set; }
        public DbSet<SSFConfiguration> SSFConfigurations { get; set; }

        // Employee Exit Models
        public DbSet<EmployeeExit> EmployeeExits { get; set; }
        public DbSet<ExitClearance> ExitClearances { get; set; }
        public DbSet<ExitSurvey> ExitSurveys { get; set; }
        public DbSet<ExitDocument> ExitDocuments { get; set; }

        // Recruitment Models
        public DbSet<JobPosition> JobPositions { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<OfferLetter> OfferLetters { get; set; }
        public DbSet<WorkforcePlanning> WorkforcePlannings { get; set; }
        public DbSet<EmployeeRequisition> EmployeeRequisitions { get; set; }

        // Performance Appraisal Models
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<PerformanceGoal> PerformanceGoals { get; set; }
        public DbSet<PerformanceFeedback> PerformanceFeedbacks { get; set; }
        public DbSet<FeedbackCriteria> FeedbackCriterias { get; set; }
        public DbSet<OnlineExam> OnlineExams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<ExamAnswer> ExamAnswers { get; set; }

        // Team and Project Management Models
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<TaskExtensionRequest> TaskExtensionRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.Emp_ID).IsUnique();
                entity.HasIndex(e => e.PAN_No);
                entity.HasIndex(e => e.CitizenshipNumber);
                entity.HasIndex(e => e.Email);
                entity.Property(e => e.BaseSalary).HasDefaultValue(0);
                entity.Property(e => e.GradeAmount).HasDefaultValue(0);
                
                // Department relationship
                entity.HasOne(e => e.DepartmentEntity)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Supervisor relationship (self-referencing)
                entity.HasOne(e => e.Supervisor)
                      .WithMany(s => s.Subordinates)
                      .HasForeignKey(e => e.SupervisorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Department configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasIndex(d => d.Name).IsUnique();
                entity.HasIndex(d => d.Code).IsUnique();
                entity.Property(d => d.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Attendance configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(e => new { e.EmployeeId, e.Date }).IsUnique();
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.Attendances)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LeaveBalance configuration
            modelBuilder.Entity<LeaveBalance>(entity =>
            {
                entity.HasIndex(e => new { e.EmployeeId, e.Year }).IsUnique();
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.LeaveBalances)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LeaveRequest configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.LeaveRequests)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Payroll configuration
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasIndex(e => new { e.EmployeeId, e.Month, e.Year }).IsUnique();
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.Payrolls)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // EmploymentContract configuration
            modelBuilder.Entity<EmploymentContract>(entity =>
            {
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.Contracts)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // DisciplinaryRecord configuration
            modelBuilder.Entity<DisciplinaryRecord>(entity =>
            {
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.DisciplinaryRecords)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AccidentLog configuration
            modelBuilder.Entity<AccidentLog>(entity =>
            {
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.AccidentLogs)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // MedicalInsuranceClaim configuration
            modelBuilder.Entity<MedicalInsuranceClaim>(entity =>
            {
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.MedicalInsuranceClaims)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SSFRecord configuration
            modelBuilder.Entity<SSFRecord>(entity =>
            {
                entity.HasIndex(e => new { e.EmployeeId, e.Month, e.Year }).IsUnique();
                entity.HasOne(e => e.Employee)
                      .WithMany(emp => emp.SSFRecords)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PerformanceReview configuration - fix cascade cycle
            modelBuilder.Entity<PerformanceReview>(entity =>
            {
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Reviewer)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OnlineExam configuration - fix cascade cycle
            modelBuilder.Entity<OnlineExam>(entity =>
            {
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.PerformanceReview)
                      .WithMany()
                      .HasForeignKey(e => e.PerformanceReviewId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ExamAttempt configuration - fix cascade cycle
            modelBuilder.Entity<ExamAttempt>(entity =>
            {
                entity.HasOne(e => e.OnlineExam)
                      .WithMany()
                      .HasForeignKey(e => e.OnlineExamId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => e.TimeStamp);
                entity.HasIndex(e => e.Level);
            });

            // LaborAuditReport configuration
            modelBuilder.Entity<LaborAuditReport>(entity =>
            {
                entity.HasIndex(e => e.ReportNumber).IsUnique();
            });

            // Team configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Department relationship
                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // CreatedBy relationship (Employee)
                entity.HasOne(e => e.Creator)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // TeamMember configuration
            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.HasIndex(e => new { e.TeamId, e.EmployeeId }).IsUnique();
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Team relationship
                entity.HasOne(e => e.Team)
                      .WithMany(t => t.TeamMembers)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Employee relationship
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ProjectTask configuration
            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity.HasIndex(e => new { e.TeamId, e.Title });
                entity.Property(e => e.StartDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.Priority).HasDefaultValue(1);
                
                // Team relationship
                entity.HasOne(e => e.Team)
                      .WithMany(t => t.Tasks)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // AssignedTo relationship (Employee)
                entity.HasOne(e => e.AssignedEmployee)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedTo)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Recruitment models configuration to fix cascade cycles
            modelBuilder.Entity<JobPosition>(entity =>
            {
                entity.HasIndex(e => e.PositionCode).IsUnique();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<Candidate>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone);
                
                // JobPosition relationship
                entity.HasOne(e => e.JobPosition)
                      .WithMany()
                      .HasForeignKey(e => e.JobPositionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Interview>(entity =>
            {
                entity.HasIndex(e => new { e.CandidateId, e.ScheduledDate });
                
                // Candidate relationship
                entity.HasOne(e => e.Candidate)
                      .WithMany()
                      .HasForeignKey(e => e.CandidateId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // JobPosition relationship
                entity.HasOne(e => e.JobPosition)
                      .WithMany()
                      .HasForeignKey(e => e.JobPositionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OfferLetter>(entity =>
            {
                // Candidate relationship
                entity.HasOne(e => e.Candidate)
                      .WithMany()
                      .HasForeignKey(e => e.CandidateId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // JobPosition relationship
                entity.HasOne(e => e.JobPosition)
                      .WithMany()
                      .HasForeignKey(e => e.JobPositionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed roles
            SeedRoles(modelBuilder);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            var adminRole = new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN"
            };
            var hrRole = new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = "2",
                Name = "HRManager",
                NormalizedName = "HRMANAGER"
            };
            var essRole = new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = "3",
                Name = "Employee",
                NormalizedName = "EMPLOYEE"
            };

            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(adminRole, hrRole, essRole);
        }

        public static async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Ensure roles exist
            string[] roleNames = { "Admin", "HRManager", "Employee" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            
            // Create admin user if it doesn't exist
            var adminEmail = "admin@hrms.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                var createResult = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "HRManager"); // Give both roles for full access
                }
            }
        }
    }
}

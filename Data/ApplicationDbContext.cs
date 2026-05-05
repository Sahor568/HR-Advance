using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        // Core HRMS Models
        public DbSet<Employee> Employees { get; set; }
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

            // Create default admin user if it doesn't exist
            var adminEmail = "admin@hrms.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    PhoneNumber = "9800000000",
                    CreatedAt = DateTime.Now
                };

                var createUser = await userManager.CreateAsync(user, "Admin@123");
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}

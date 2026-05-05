using HR_Management_System.Data;
using HR_Management_System.Models;
using HR_Management_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HR_Management_System.Services.Implementations
{
    public class DynamicPayrollService : IDynamicPayrollService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DynamicPayrollService> _logger;

        public DynamicPayrollService(ApplicationDbContext context, ILogger<DynamicPayrollService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PayrollConfiguration> GetPayrollConfigurationAsync(int employeeId)
        {
            _logger.LogInformation("Fetching payroll configuration for employee ID: {EmployeeId}", employeeId);

            var config = await _context.PayrollConfigurations
                .Include(pc => pc.Components)
                .FirstOrDefaultAsync(pc => pc.EmployeeId == employeeId && pc.IsActive);

            if (config == null)
            {
                // Create default configuration if none exists
                config = await CreateDefaultPayrollConfigurationAsync(employeeId);
            }

            return config;
        }

        public async Task<PayrollConfiguration> UpdatePayrollConfigurationAsync(PayrollConfiguration configuration)
        {
            _logger.LogInformation("Updating payroll configuration ID: {ConfigurationId}", configuration.Id);

            var existingConfig = await _context.PayrollConfigurations
                .Include(pc => pc.Components)
                .FirstOrDefaultAsync(pc => pc.Id == configuration.Id);

            if (existingConfig == null)
                throw new KeyNotFoundException("Payroll configuration not found");

            // Update configuration properties
            existingConfig.ConfigurationName = configuration.ConfigurationName;
            existingConfig.IsActive = configuration.IsActive;
            existingConfig.EffectiveFrom = configuration.EffectiveFrom;
            existingConfig.EffectiveTo = configuration.EffectiveTo;
            existingConfig.UpdatedAt = DateTime.UtcNow;

            // Update components
            foreach (var component in configuration.Components)
            {
                var existingComponent = existingConfig.Components.FirstOrDefault(c => c.Id == component.Id);
                if (existingComponent != null)
                {
                    existingComponent.ComponentName = component.ComponentName;
                    existingComponent.ComponentType = component.ComponentType;
                    existingComponent.CalculationType = component.CalculationType;
                    existingComponent.Value = component.Value;
                    existingComponent.IsActive = component.IsActive;
                    existingComponent.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    component.CreatedAt = DateTime.UtcNow;
                    existingConfig.Components.Add(component);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Payroll configuration updated successfully. Configuration ID: {ConfigurationId}", existingConfig.Id);

            return existingConfig;
        }

        public async Task<IEnumerable<PayrollComponent>> GetPayrollComponentsAsync(int configurationId)
        {
            return await _context.PayrollComponents
                .Where(pc => pc.PayrollConfigurationId == configurationId && pc.IsActive)
                .OrderBy(pc => pc.ComponentType)
                .ToListAsync();
        }

        public async Task<PayrollComponent> AddPayrollComponentAsync(PayrollComponent component)
        {
            _logger.LogInformation("Adding payroll component to configuration ID: {ConfigurationId}", component.PayrollConfigurationId);

            component.CreatedAt = DateTime.UtcNow;
            _context.PayrollComponents.Add(component);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payroll component added successfully. Component ID: {ComponentId}", component.Id);
            return component;
        }

        public async Task<bool> RemovePayrollComponentAsync(int componentId)
        {
            _logger.LogInformation("Removing payroll component ID: {ComponentId}", componentId);

            var component = await _context.PayrollComponents.FindAsync(componentId);
            if (component == null) return false;

            component.IsActive = false;
            component.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Payroll component removed successfully. Component ID: {ComponentId}", componentId);
            return true;
        }

        public async Task<IEnumerable<TaxSlab>> GetTaxSlabsAsync(int year)
        {
            return await _context.TaxSlabs
                .Where(ts => ts.Year == year)
                .OrderBy(ts => ts.MinimumIncome)
                .ToListAsync();
        }

        public async Task<TaxSlab> AddTaxSlabAsync(TaxSlab taxSlab)
        {
            _logger.LogInformation("Adding tax slab for year: {Year}", taxSlab.Year);

            // Validate no overlap
            var overlappingSlab = await _context.TaxSlabs
                .Where(ts => ts.Year == taxSlab.Year &&
                            ((taxSlab.MinimumIncome >= ts.MinimumIncome && taxSlab.MinimumIncome <= ts.MaximumIncome) ||
                             (taxSlab.MaximumIncome >= ts.MinimumIncome && taxSlab.MaximumIncome <= ts.MaximumIncome) ||
                             (taxSlab.MinimumIncome <= ts.MinimumIncome && taxSlab.MaximumIncome >= ts.MaximumIncome)))
                .FirstOrDefaultAsync();

            if (overlappingSlab != null)
                throw new InvalidOperationException("Tax slab overlaps with existing slab");

            taxSlab.CreatedAt = DateTime.UtcNow;
            _context.TaxSlabs.Add(taxSlab);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tax slab added successfully. Slab ID: {SlabId}", taxSlab.Id);
            return taxSlab;
        }

        public async Task<SSFConfiguration> GetSSFConfigurationAsync()
        {
            var config = await _context.SSFConfigurations
                .OrderByDescending(sc => sc.EffectiveFrom)
                .FirstOrDefaultAsync();

            if (config == null)
            {
                // Create default SSF configuration
                config = new SSFConfiguration
                {
                    EmployeeRate = 0.10m, // 10%
                    EmployerRate = 0.21m, // 21%
                    GratuityRate = 0.0833m, // 8.33%
                    EffectiveFrom = new DateTime(2024, 1, 1),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SSFConfigurations.Add(config);
                await _context.SaveChangesAsync();
            }

            return config;
        }

        public async Task<SSFConfiguration> UpdateSSFConfigurationAsync(SSFConfiguration configuration)
        {
            _logger.LogInformation("Updating SSF configuration ID: {ConfigurationId}", configuration.Id);

            var existingConfig = await _context.SSFConfigurations.FindAsync(configuration.Id);
            if (existingConfig == null)
                throw new KeyNotFoundException("SSF configuration not found");

            // Deactivate old configuration
            existingConfig.IsActive = false;
            existingConfig.UpdatedAt = DateTime.UtcNow;

            // Create new configuration
            var newConfig = new SSFConfiguration
            {
                EmployeeRate = configuration.EmployeeRate,
                EmployerRate = configuration.EmployerRate,
                GratuityRate = configuration.GratuityRate,
                EffectiveFrom = configuration.EffectiveFrom,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SSFConfigurations.Add(newConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("SSF configuration updated successfully. New configuration ID: {ConfigurationId}", newConfig.Id);
            return newConfig;
        }

        public async Task<decimal> CalculateComplexPayrollAsync(int employeeId, int month, int year, Dictionary<string, decimal> customComponents)
        {
            _logger.LogInformation("Calculating complex payroll for employee ID: {EmployeeId}, {Month}/{Year}", employeeId, month, year);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            var config = await GetPayrollConfigurationAsync(employeeId);
            var baseSalary = employee.BaseSalary;

            decimal totalEarnings = baseSalary;
            decimal totalDeductions = 0;

            // Calculate earnings components
            var earningComponents = config.Components.Where(c => c.ComponentType == ComponentType.Earning && c.IsActive);
            foreach (var component in earningComponents)
            {
                decimal componentValue = CalculateComponentValue(component, baseSalary);
                totalEarnings += componentValue;
            }

            // Add custom earnings
            foreach (var custom in customComponents.Where(kv => kv.Value > 0))
            {
                totalEarnings += custom.Value;
            }

            // Calculate deduction components
            var deductionComponents = config.Components.Where(c => c.ComponentType == ComponentType.Deduction && c.IsActive);
            foreach (var component in deductionComponents)
            {
                decimal componentValue = CalculateComponentValue(component, baseSalary);
                totalDeductions += componentValue;
            }

            // Calculate tax
            var annualIncome = totalEarnings * 12;
            var tax = await CalculateTaxAsync(annualIncome, year);

            // Calculate SSF
            var ssfConfig = await GetSSFConfigurationAsync();
            var ssfEmployee = totalEarnings * ssfConfig.EmployeeRate;
            var ssfEmployer = totalEarnings * ssfConfig.EmployerRate;

            totalDeductions += tax + ssfEmployee;

            decimal netPay = totalEarnings - totalDeductions;

            _logger.LogInformation("Complex payroll calculation completed. Net Pay: {NetPay}", netPay);
            return netPay;
        }

        public async Task<Dictionary<string, object>> GeneratePayrollReportAsync(int employeeId, int month, int year)
        {
            _logger.LogInformation("Generating payroll report for employee ID: {EmployeeId}, {Month}/{Year}", employeeId, month, year);

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new KeyNotFoundException("Employee not found");

            var config = await GetPayrollConfigurationAsync(employeeId);
            var baseSalary = employee.BaseSalary;

            var report = new Dictionary<string, object>
            {
                ["EmployeeId"] = employeeId,
                ["EmployeeName"] = employee.FullName,
                ["Month"] = month,
                ["Year"] = year,
                ["BaseSalary"] = baseSalary,
                ["Components"] = new List<Dictionary<string, object>>(),
                ["Summary"] = new Dictionary<string, decimal>()
            };

            decimal totalEarnings = baseSalary;
            decimal totalDeductions = 0;

            // Process all components
            foreach (var component in config.Components.Where(c => c.IsActive))
            {
                decimal componentValue = CalculateComponentValue(component, baseSalary);

                var componentData = new Dictionary<string, object>
                {
                    ["ComponentName"] = component.ComponentName,
                    ["ComponentType"] = component.ComponentType.ToString(),
                    ["CalculationType"] = component.CalculationType.ToString(),
                    ["Value"] = component.Value,
                    ["CalculatedValue"] = componentValue
                };

                ((List<Dictionary<string, object>>)report["Components"]).Add(componentData);

                if (component.ComponentType == ComponentType.Earning)
                    totalEarnings += componentValue;
                else
                    totalDeductions += componentValue;
            }

            // Calculate tax
            var annualIncome = totalEarnings * 12;
            var tax = await CalculateTaxAsync(annualIncome, year);
            totalDeductions += tax;

            // Calculate SSF
            var ssfConfig = await GetSSFConfigurationAsync();
            var ssfEmployee = totalEarnings * ssfConfig.EmployeeRate;
            var ssfEmployer = totalEarnings * ssfConfig.EmployerRate;
            totalDeductions += ssfEmployee;

            decimal netPay = totalEarnings - totalDeductions;

            var summary = (Dictionary<string, decimal>)report["Summary"];
            summary["TotalEarnings"] = totalEarnings;
            summary["TotalDeductions"] = totalDeductions;
            summary["Tax"] = tax;
            summary["SSFEmployee"] = ssfEmployee;
            summary["SSFEmployer"] = ssfEmployer;
            summary["NetPay"] = netPay;

            _logger.LogInformation("Payroll report generated successfully for employee ID: {EmployeeId}", employeeId);
            return report;
        }

        private async Task<PayrollConfiguration> CreateDefaultPayrollConfigurationAsync(int employeeId)
        {
            _logger.LogInformation("Creating default payroll configuration for employee ID: {EmployeeId}", employeeId);

            var defaultConfig = new PayrollConfiguration
            {
                EmployeeId = employeeId,
                ConfigurationName = "Default Configuration",
                IsActive = true,
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Components = new List<PayrollComponent>
                {
                    new PayrollComponent
                    {
                        ComponentName = "House Rent Allowance",
                        ComponentType = ComponentType.Earning,
                        CalculationType = CalculationType.Percentage,
                        Value = 0.40m, // 40% of basic
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new PayrollComponent
                    {
                        ComponentName = "Medical Allowance",
                        ComponentType = ComponentType.Earning,
                        CalculationType = CalculationType.Fixed,
                        Value = 2000, // Fixed 2000
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new PayrollComponent
                    {
                        ComponentName = "Provident Fund",
                        ComponentType = ComponentType.Deduction,
                        CalculationType = CalculationType.Percentage,
                        Value = 0.10m, // 10% of basic
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            _context.PayrollConfigurations.Add(defaultConfig);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Default payroll configuration created. Configuration ID: {ConfigurationId}", defaultConfig.Id);
            return defaultConfig;
        }

        private decimal CalculateComponentValue(PayrollComponent component, decimal baseSalary)
        {
            return component.CalculationType switch
            {
                CalculationType.Fixed => component.Value,
                CalculationType.Percentage => baseSalary * component.Value,
                CalculationType.Formula => CalculateFormula(component.Value, baseSalary),
                _ => 0
            };
        }

        private decimal CalculateFormula(decimal formulaValue, decimal baseSalary)
        {
            // Simple formula calculation - can be extended
            return baseSalary * formulaValue;
        }

        private async Task<decimal> CalculateTaxAsync(decimal annualIncome, int year)
        {
            var taxSlabs = await GetTaxSlabsAsync(year);
            if (!taxSlabs.Any())
            {
                // Default Nepal tax slabs for 2081
                taxSlabs = GetDefaultTaxSlabs(year);
            }

            decimal tax = 0;
            decimal remainingIncome = annualIncome;

            foreach (var slab in taxSlabs.OrderBy(ts => ts.MinimumIncome))
            {
                if (remainingIncome <= 0) break;

                decimal slabRange = slab.MaximumIncome - slab.MinimumIncome;
                decimal taxableInThisSlab = Math.Min(remainingIncome, slabRange);

                if (taxableInThisSlab > 0)
                {
                    tax += taxableInThisSlab * slab.TaxRate / 100;
                    remainingIncome -= taxableInThisSlab;
                }
            }

            return tax / 12; // Monthly tax
        }

        private IEnumerable<TaxSlab> GetDefaultTaxSlabs(int year)
        {
            return new List<TaxSlab>
            {
                new TaxSlab { Year = year, MinimumIncome = 0, MaximumIncome = 500000, TaxRate = 1, CreatedAt = DateTime.UtcNow },
                new TaxSlab { Year = year, MinimumIncome = 500001, MaximumIncome = 700000, TaxRate = 10, CreatedAt = DateTime.UtcNow },
                new TaxSlab { Year = year, MinimumIncome = 700001, MaximumIncome = 1000000, TaxRate = 20, CreatedAt = DateTime.UtcNow },
                new TaxSlab { Year = year, MinimumIncome = 1000001, MaximumIncome = 2000000, TaxRate = 30, CreatedAt = DateTime.UtcNow },
                new TaxSlab { Year = year, MinimumIncome = 2000001, MaximumIncome = decimal.MaxValue, TaxRate = 36, CreatedAt = DateTime.UtcNow }
            };
        }
    }
}
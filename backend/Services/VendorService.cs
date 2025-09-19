using backend.Config;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public class VendorService
    {
        private readonly ApplicationDbContext _context;

        public VendorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetEmployeesAsync(int vendorId)
        {
            return await _context.Employees
                                 .Where(e => e.VendorId == vendorId)
                                 .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId, int vendorId)
        {
            return await _context.Employees
                                 .FirstOrDefaultAsync(e => e.Id == employeeId && e.VendorId == vendorId);
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee, int vendorId, int userId)
        {
            // The duplicate check on EmployeeCode is removed since it doesn't exist in the model.
            employee.VendorId = vendorId;
            employee.CreatedByUserId = userId;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee?> UpdateEmployeeAsync(int employeeId, int vendorId, Employee employeeUpdate)
        {
            var employee = await _context.Employees
                                         .FirstOrDefaultAsync(e => e.Id == employeeId && e.VendorId == vendorId);
            if (employee == null)
            {
                return null; // Employee not found or doesn't belong to the vendor
            }

            // Update properties
            employee.FirstName = employeeUpdate.FirstName;
            employee.LastName = employeeUpdate.LastName;
            employee.JobTitle = employeeUpdate.JobTitle;
            employee.UpdatedAt = DateTime.UtcNow;
            // Note: EmployeeCode should generally not be updated as it's a unique identifier

            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(int employeeId, int vendorId)
        {
            var employee = await _context.Employees
                                         .FirstOrDefaultAsync(e => e.Id == employeeId && e.VendorId == vendorId);
            if (employee == null)
            {
                return false; // Employee not found or doesn't belong to the vendor
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
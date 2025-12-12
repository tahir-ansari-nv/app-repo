using TimesheetApi.Models;

namespace TimesheetApi.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id);
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee> CreateAsync(Employee employee);
    Task<bool> ExistsAsync(Guid id);
}

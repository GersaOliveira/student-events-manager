using StudentEvent.Domain.Entities;

namespace StudentEvent.Domain.Interfaces;

public interface IStudentRepository
{
    Task<bool> ExistsAsync(string microsoftId);
    Task<Dictionary<string, Guid>> GetExistingStudentsMapAsync();
    Task<IEnumerable<Student>> GetAllAsync();
    Task<bool> EventExistsAsync(string microsoftEventId);
    Task AddAsync(Student student);
    Task AddEventAsync(Event calendarEvent);
    Task SaveChangesAsync();
}
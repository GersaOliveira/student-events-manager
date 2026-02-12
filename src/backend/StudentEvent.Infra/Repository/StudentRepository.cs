using Microsoft.EntityFrameworkCore;
using StudentEvent.Domain.Entities;
using StudentEvent.Domain.Interfaces;
using StudentEvent.Infra.Context;

namespace StudentEvent.Infra.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string microsoftId)
    {
        return await _context.Students.AnyAsync(s => s.MicrosoftId == microsoftId);
    }

    public async Task<Dictionary<string, Guid>> GetExistingStudentsMapAsync()
    {        
        return await _context.Students.ToDictionaryAsync(s => s.MicrosoftId, s => s.Id);
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students.Include(s => s.Events).ToListAsync();
    }

    public async Task<bool> EventExistsAsync(string microsoftEventId)
    {
        return await _context.Events.AnyAsync(e => e.MicrosoftEventId == microsoftEventId);
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
    }

    public async Task AddEventAsync(Event calendarEvent)
    {
        await _context.Events.AddAsync(calendarEvent);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
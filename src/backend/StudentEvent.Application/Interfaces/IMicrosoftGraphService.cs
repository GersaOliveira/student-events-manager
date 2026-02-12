using StudentEvent.Domain.Entities;

namespace StudentEvent.Application.Interfaces;

public interface IMicrosoftGraphService
{
    Task<IEnumerable<Student>> GetExternalStudentsAsync();

    Task<IEnumerable<Event>> GetExternalEventsAsync(string microsoftId);
}
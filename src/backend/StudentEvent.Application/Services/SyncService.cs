using StudentEvent.Application.DTOs;
using StudentEvent.Application.Interfaces;
using StudentEvent.Domain.Interfaces;
using StudentEvent.Domain.Entities; // Necessário para a classe Student
using System;
using System.Threading.Tasks;

namespace StudentEvent.Application.Services
{
    public class SyncService : ISyncService
    {
        private readonly IMicrosoftGraphService _graphService;
        private readonly IStudentRepository _studentRepository;

        public SyncService(IMicrosoftGraphService graphService, IStudentRepository studentRepository)
        {
            _graphService = graphService;
            _studentRepository = studentRepository;
        }

        public async Task SyncStudentsAndEventsAsync()
        {
            var externalStudents = await _graphService.GetExternalStudentsAsync();

            if (externalStudents == null) return;

            var existingStudentsMap = await _studentRepository.GetExistingStudentsMapAsync();

            foreach (var studentFromGraph in externalStudents)
            {
                Guid studentInternalId;

                // 1. Sincroniza o Estudante
                if (existingStudentsMap.TryGetValue(studentFromGraph.MicrosoftId, out var existingId))
                {
                    studentInternalId = existingId;
                }
                else
                {
                    studentInternalId = studentFromGraph.Id;
                    await _studentRepository.AddAsync(studentFromGraph);
                }

                // 2. Sincroniza os Eventos deste Estudante
                var events = await _graphService.GetExternalEventsAsync(studentFromGraph.MicrosoftId);
                
                foreach (var evt in events)
                {
                    // Verifica se o evento já foi importado para evitar duplicatas
                    if (await _studentRepository.EventExistsAsync(evt.MicrosoftEventId)) continue;

                    evt.StudentId = studentInternalId;
                    await _studentRepository.AddEventAsync(evt);
                }
            }

            // Salva as alterações no banco de dados
            await _studentRepository.SaveChangesAsync();
        }
    }
}
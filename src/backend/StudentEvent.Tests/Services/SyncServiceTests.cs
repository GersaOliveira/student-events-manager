using Moq;
using StudentEvent.Application.Services;
using StudentEvent.Application.Interfaces;
using StudentEvent.Domain.Interfaces;
using StudentEvent.Domain.Entities;
using Xunit;

namespace StudentEvent.Tests.Services;

// Testes unitários para o serviço de sincronização (SyncService)
public class SyncServiceTests
{
    private readonly Mock<IMicrosoftGraphService> _mockGraphService;
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly SyncService _syncService;

    public SyncServiceTests()
    {
        _mockGraphService = new Mock<IMicrosoftGraphService>();
        _mockStudentRepository = new Mock<IStudentRepository>();
        _syncService = new SyncService(_mockGraphService.Object, _mockStudentRepository.Object);
    }

    [Fact]
    public async Task SyncStudentsAndEventsAsync_ShouldAddNewStudent_WhenStudentDoesNotExist()
    {
        // Arrange
        var newStudent = new Student { Id = Guid.NewGuid(), MicrosoftId = "ms-id-1", Name = "Teste", Email = "teste@email.com" };
        var externalStudents = new List<Student> { newStudent };

        // Mock: Graph retorna 1 estudante
        _mockGraphService.Setup(s => s.GetExternalStudentsAsync())
            .ReturnsAsync(externalStudents);

        // Mock: Banco diz que não tem ninguém cadastrado (Dicionário vazio)
        _mockStudentRepository.Setup(r => r.GetExistingStudentsMapAsync())
            .ReturnsAsync(new Dictionary<string, Guid>());

        // Mock: Graph retorna lista vazia de eventos para simplificar este teste
        _mockGraphService.Setup(s => s.GetExternalEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Event>());

        // Act
        await _syncService.SyncStudentsAndEventsAsync();

        // Assert
        // Verifica se o método AddAsync foi chamado 1 vez com o estudante correto
        _mockStudentRepository.Verify(r => r.AddAsync(It.Is<Student>(s => s.MicrosoftId == "ms-id-1")), Times.Once);
        // Verifica se salvou no final
        _mockStudentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SyncStudentsAndEventsAsync_ShouldAddEvent_WhenEventDoesNotExist()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var student = new Student { Id = studentId, MicrosoftId = "ms-id-1" };
        
        var newEvent = new Event { MicrosoftEventId = "evt-1", Subject = "Aula" };

        // Mock: Graph retorna o estudante
        _mockGraphService.Setup(s => s.GetExternalStudentsAsync())
            .ReturnsAsync(new List<Student> { student });

        // Mock: Banco diz que o estudante JÁ existe
        var existingMap = new Dictionary<string, Guid> { { "ms-id-1", studentId } };
        _mockStudentRepository.Setup(r => r.GetExistingStudentsMapAsync())
            .ReturnsAsync(existingMap);

        // Mock: Graph retorna 1 evento para esse estudante
        _mockGraphService.Setup(s => s.GetExternalEventsAsync("ms-id-1"))
            .ReturnsAsync(new List<Event> { newEvent });

        // Mock: Banco diz que o evento NÃO existe
        _mockStudentRepository.Setup(r => r.EventExistsAsync("evt-1"))
            .ReturnsAsync(false);

        // Act
        await _syncService.SyncStudentsAndEventsAsync();

        // Assert
        // 1. Não deve adicionar o estudante novamente
        _mockStudentRepository.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Never);

        // 2. Deve adicionar o evento
        _mockStudentRepository.Verify(r => r.AddEventAsync(It.Is<Event>(e => 
            e.MicrosoftEventId == "evt-1" && 
            e.StudentId == studentId // Garante que vinculou o ID interno correto
        )), Times.Once);

        // 3. Deve salvar
        _mockStudentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
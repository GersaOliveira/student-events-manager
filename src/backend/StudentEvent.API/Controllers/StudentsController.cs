using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentEvent.Application.Interfaces;
using StudentEvent.Domain.Interfaces;
using StudentEvent.Domain.Entities;

namespace StudentEvent.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentRepository _studentRepository;

    public StudentsController(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    [HttpGet("external")]
    public async Task<IActionResult> GetExternal()
    {
        // Busca os estudantes do banco de dados local
        var students = await _studentRepository.GetAllAsync();
        return Ok(students);
    }
}
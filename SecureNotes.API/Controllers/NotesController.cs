using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNotes.Api.Models;
using SecureNotes.Api.Services;

namespace SecureNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class NotesController(DbService db) : ControllerBase
{
    private int CurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Invalid token.");
        return userId;
    }

    [HttpGet]
    public ActionResult<List<NoteEntity>> GetMine()
    {
        var userId = CurrentUserId();
        return Ok(db.GetNotesForUser(userId));
    }

    [HttpPost]
    public ActionResult<object> Create([FromBody] UpsertNoteRequest req)
    {
        var userId = CurrentUserId();
        var id = db.AddNote(userId, req);
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update([FromRoute] int id, [FromBody] UpsertNoteRequest req)
    {
        var userId = CurrentUserId();
        var ok = db.UpdateNote(id, userId, req);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var userId = CurrentUserId();
        var ok = db.DeleteNote(id, userId);
        return ok ? NoContent() : NotFound();
    }
}
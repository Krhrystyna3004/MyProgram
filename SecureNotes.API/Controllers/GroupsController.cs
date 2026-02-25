using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNotes.Api.Models;
using SecureNotes.Api.Services;

namespace SecureNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class GroupsController(DbService db) : ControllerBase
{
    private int CurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Invalid token.");
        return userId;
    }

    [HttpGet]
    public ActionResult<List<GroupResponse>> GetMine()
    {
        var userId = CurrentUserId();
        var groups = db.GetGroupsForUser(userId)
            .Select(g => new GroupResponse
            {
                Id = g.Id,
                Name = g.Name,
                InviteCode = g.InviteCode,
                OwnerId = g.OwnerId,
                CreatedAt = g.CreatedAt
            })
            .ToList();

        return Ok(groups);
    }

    [HttpPost]
    public ActionResult<GroupResponse> Create([FromBody] CreateGroupRequest req)
    {
        var userId = CurrentUserId();
        var grp = db.CreateGroup(userId, string.IsNullOrWhiteSpace(req.Name) ? "Моя група" : req.Name.Trim());

        return Ok(new GroupResponse
        {
            Id = grp.Id,
            Name = grp.Name,
            InviteCode = grp.InviteCode,
            OwnerId = grp.OwnerId,
            CreatedAt = grp.CreatedAt
        });
    }

    [HttpPost("join")]
    public ActionResult<GroupResponse> Join([FromBody] JoinGroupRequest req)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(req.InviteCode))
            return BadRequest("InviteCode is required.");

        var grp = db.GetGroupByInvite(req.InviteCode.Trim().ToUpper());
        if (grp is null)
            return NotFound("Group not found.");

        db.AddMember(grp.Id, userId, "edit");

        return Ok(new GroupResponse
        {
            Id = grp.Id,
            Name = grp.Name,
            InviteCode = grp.InviteCode,
            OwnerId = grp.OwnerId,
            CreatedAt = grp.CreatedAt
        });
    }

    [HttpPost("{id:int}/leave")]
    public IActionResult Leave([FromRoute] int id)
    {
        var userId = CurrentUserId();
        db.LeaveGroup(id, userId);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var userId = CurrentUserId();
        db.DeleteGroup(id, userId);
        return NoContent();
    }
}

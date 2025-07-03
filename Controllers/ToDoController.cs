using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ToDoController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ToDoController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private string? GetUserId()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.FailResponse("User not found"));

        var tasks = await _dbContext.Tasks.Where(t => t.UserId == userId).ToListAsync();
        return Ok(ApiResponse<IEnumerable<MyTask>>.SuccessResponse(tasks));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.FailResponse("User not found"));

        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null || task.UserId != userId)
            return NotFound(ApiResponse<string>.FailResponse("Task not found or not owned by the current user"));

        return Ok(ApiResponse<MyTask>.SuccessResponse(task));
    }

    [HttpPost]
    public async Task<IActionResult> AddTask([FromBody] MyTask task)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.FailResponse("User not found"));

        task.UserId = userId;
        await _dbContext.Tasks.AddAsync(task);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id },
            ApiResponse<MyTask>.SuccessResponse(task, "Task created successfully"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] MyTask updatedTask)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.FailResponse("User not found"));

        var existingTask = await _dbContext.Tasks.FindAsync(id);
        if (existingTask == null || existingTask.UserId != userId)
            return NotFound(ApiResponse<string>.FailResponse("Task not found or not owned by the current user"));

        existingTask.Title = updatedTask.Title;
        existingTask.Description = updatedTask.Description;
        existingTask.Done = updatedTask.Done;

        _dbContext.Tasks.Update(existingTask);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<MyTask>.SuccessResponse(existingTask, "Task updated successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveTask(int id)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.FailResponse("User not found"));

        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null || task.UserId != userId)
            return NotFound(ApiResponse<string>.FailResponse("Task not found or not owned by the current user"));

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("Task deleted successfully"));
    }
}

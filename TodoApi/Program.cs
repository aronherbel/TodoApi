using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);


var dbConnectionString = builder.Configuration["ConnectionStrings:myServerConnection"];

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddDbContext<TodoDb>(options =>
    options.UseSqlite(dbConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:3000", "http://localhost:5500")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

var app = builder.Build();
app.UseCors(MyAllowSpecificOrigins);
app.UseDefaultFiles();
app.UseStaticFiles();

RouteGroupBuilder todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/done", GetDoneTodos);
todoItems.MapGet("/priority", GetPriorityTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.TodosTable.Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<IResult> GetDoneTodos(TodoDb db)
{
    return TypedResults.Ok(await db.TodosTable.Where(t => t.IsDone).Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<IResult> GetPriorityTodos(TodoDb db)
{
    return TypedResults.Ok(await db.TodosTable.Where(t => t.IsPriority).Select(x => new TodoItemDTO(x)).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.TodosTable.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsDone = todoItemDTO.IsDone,
        IsPriority = todoItemDTO.IsPriority,
        Name = todoItemDTO.Name,
        Information = todoItemDTO.Information,
        Date = todoItemDTO.Date,
    };

    db.TodosTable.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.TodosTable.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsDone = todoItemDTO.IsDone;
    todo.IsPriority = todoItemDTO.IsPriority;
    todo.Information = todoItemDTO.Information;
    todo.Date = todoItemDTO.Date;   

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.TodosTable.FindAsync(id) is Todo todo)
    {
        db.TodosTable.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}
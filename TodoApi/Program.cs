using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Drawing.Text;

var builder = WebApplication.CreateBuilder(args);


var dbConnectionString = builder.Configuration["ConnectionStrings:myServerConnection"];

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddDbContext<TodoDb>(options =>
    options.UseSqlite(dbConnectionString));


builder.Services.AddDbContext<UserDb>(options =>
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

RouteGroupBuilder user = app.MapGroup("/users");
RouteGroupBuilder todoItems = app.MapGroup("/todoitems");


user.MapGet("/", GetAllUsers);
user.MapGet("/{userId}", GetUser);
user.MapPost("/", CreateUser);
user.MapPut("/{userId}", UpdateUser);
user.MapDelete("/{userId}", DeleteUser);
user.MapPost("/authenticate", async (HttpContext context, UserDb db) =>
{
    try
    {
       
        var form = await context.Request.ReadFormAsync();
        var userName = form["userName"];
        var hashedPassword = form["hashedPassword"];

        var result = await AuthenticateUser(userName, hashedPassword, db);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/done", GetDoneTodos);
todoItems.MapGet("/priority", GetPriorityTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();


static async Task<IResult> GetAllUsers(UserDb db)
{
    return TypedResults.Ok(await db.UserTable.Select(x => new UserDTO(x)).ToListAsync());
}


static async Task<IResult> GetUser(int userId, UserDb db)
{
    return await db.UserTable.FindAsync(userId)
        is User user
            ? TypedResults.Ok(new UserDTO(user))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateUser(UserDTO userDTO, UserDb db)
{
    var user = new User
    {
        UserName = userDTO.UserName,
        Password = userDTO.Password,

    };

    db.UserTable.Add(user);
    await db.SaveChangesAsync();

    userDTO = new UserDTO(user);

    return TypedResults.Created($"/users/{user.UserId}",userDTO);
}


static async Task<IResult> UpdateUser(int userId, UserDTO userDTO, UserDb db)
{
    var user = await db.UserTable.FindAsync(userId);

    if (user is null) return TypedResults.NotFound();


    user.UserName = userDTO.UserName;
    user.Password = userDTO.Password;
    

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteUser(int userId, UserDb db)
{
    if (await db.UserTable.FindAsync(userId) is User user)
    {
        db.UserTable.Remove(user);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

 static bool CheckPassword(string  hashedPassword, string password )
{
    return hashedPassword == password;
}

static async Task<IResult> AuthenticateUser(string userName, string hashedPassword, UserDb db)
{
    User user = await db.UserTable.FirstOrDefaultAsync(u => u.UserName == userName);


    if (user == null) return TypedResults.NotFound();

    if (CheckPassword(hashedPassword, user.Password) == true)
    {
        return TypedResults.Ok("Authentifizierung war erfolgreich");
    }
    else
    {
        return TypedResults.NotFound("falsches Passwort");
    }
}


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
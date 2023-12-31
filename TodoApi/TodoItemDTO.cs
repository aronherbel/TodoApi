﻿namespace TodoApi
{
    public class TodoItemDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsDone { get; set; }
        public bool IsPriority { get; set; }
        public string Information { get; set; }
        public string Date { get; set; }

        public TodoItemDTO() { }
        public TodoItemDTO(Todo todoItem) =>
        (Id, Name, IsDone, IsPriority, Information, Date) = (todoItem.Id, todoItem.Name, todoItem.IsDone, todoItem.IsPriority, todoItem.Information, todoItem.Date);
    }
}

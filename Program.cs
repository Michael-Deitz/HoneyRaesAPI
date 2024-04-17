using HoneyRaesAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

List<CustomerDTO> customers = new List<CustomerDTO> ()
{ 
    new CustomerDTO() { Id = 1, Name = "John", Address = "1234 Left lane"},
    new CustomerDTO() { Id = 2, Name = "Jane", Address = "4321 RIght Road"},
    new CustomerDTO() { Id = 3, Name = "Adam", Address = "5678 Uptown Circle"}
};
List<EmployeeDTO> employees = new List<EmployeeDTO> ()
{ 
    new EmployeeDTO() { Id = 1, Name = "Bob", Specialty = "Windows Products"},
    new EmployeeDTO() { Id = 2, Name = "Frances", Specialty = "Apple Products"}
};
List<ServiceTicketDTO> serviceTickets = new List<ServiceTicketDTO> ()
{ 
    new ServiceTicketDTO() { Id = 1, CustomerId = 2, EmployeeId = 2, Description = "My Mac book is cracked and needs a new screen", Emergency = true, DateCompleted = new DateTime(2024, 01, 02)},
    new ServiceTicketDTO() { Id = 2, CustomerId = 3, EmployeeId = 1, Description = "My Desktop keeps crashing and i don't know why", Emergency = false, DateCompleted = new DateTime(2024, 02, 20)},
    new ServiceTicketDTO() { Id = 3, CustomerId = 1, EmployeeId = 2, Description = "I dropped my phone in the toilet", Emergency = true, DateCompleted = new DateTime(2024 , 02, 12)},
    new ServiceTicketDTO() { Id = 4, CustomerId = 3, EmployeeId = 2, Description = "I need my screen replaced on my Iphone", Emergency = false, DateCompleted = new DateTime(2024, 04, 08)},
    new ServiceTicketDTO() { Id = 5, CustomerId = 1, EmployeeId = 1, Description = "I want to upgrade my desktop parts", Emergency = false, DateCompleted = new DateTime(2024, 03, 25)},
    new ServiceTicketDTO() { Id = 6, CustomerId = 2,  Description = "Mac book keeps crashing", Emergency = true, },
};




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicketDTO serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    EmployeeDTO employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    CustomerDTO customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok (new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});

app.MapGet("/customers", () =>
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address
    });
});

app.MapGet("/customers/{id}", (int id) => 
{
    CustomerDTO customer = customers.FirstOrDefault(ci => ci.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    });
});

app.MapGet("/employees", () =>
{
    return employees.Select(e => new EmployeeDTO
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty
    });
});

app.MapGet("/employees/{id}", (int id) =>
{
    EmployeeDTO employee = employees.FirstOrDefault(ei => ei.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
        List<ServiceTicketDTO> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
        return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.MapPost("/servicetickets", (ServiceTicketDTO serviceTicket) =>
{
    CustomerDTO customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    if (customer == null)
    {
        return Results.BadRequest();
    }

    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO 
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });
});

app.MapDelete("/servicetickets/{id}", (int id) => 
{
    ServiceTicketDTO serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    serviceTickets.Remove(serviceTicket);

    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, [FromBody] ServiceTicketDTO serviceTicket) => 
{
    ServiceTicketDTO ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});

app.MapPost("/servicetickets/{id}/complete", (int id) => 
{
    ServiceTicketDTO ticketToComplete = serviceTickets. FirstOrDefault(st => st.Id == id);

    ticketToComplete.DateCompleted = DateTime.Today;
});


app.Run();




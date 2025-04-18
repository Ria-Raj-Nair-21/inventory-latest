using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

// Simple in-memory storage for demo purposes
var inventory = new List<InventoryItem>();
var nextId = 1;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventory API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API V1");
});

// API Endpoints
app.MapGet("/", () => "Inventory System is running!");

// View all inventory items
app.MapGet("/inventory", () => Results.Ok(inventory));

// View specific item by ID
app.MapGet("/inventory/{id}", (int id) =>
{
    var item = inventory.FirstOrDefault(i => i.Id == id);
    return item != null ? Results.Ok(item) : Results.NotFound();
});

// Add new inventory item
app.MapPost("/inventory", (InventoryItem item) =>
{
    item.Id = nextId++;
    inventory.Add(item);
    return Results.Created($"/inventory/{item.Id}", item);
});

// Update existing item
app.MapPut("/inventory/{id}", (int id, InventoryItem updatedItem) =>
{
    var existingItem = inventory.FirstOrDefault(i => i.Id == id);
    if (existingItem == null)
        return Results.NotFound();

    existingItem.Name = updatedItem.Name;
    existingItem.Quantity = updatedItem.Quantity;
    existingItem.Price = updatedItem.Price;
    existingItem.Category = updatedItem.Category;

    return Results.Ok(existingItem);
});

// Delete an item
app.MapDelete("/inventory/{id}", (int id) =>
{
    var item = inventory.FirstOrDefault(i => i.Id == id);
    if (item == null)
        return Results.NotFound();

    inventory.Remove(item);
    return Results.NoContent();
});

// Search by category
app.MapGet("/inventory/category/{category}", (string category) =>
{
    var items = inventory.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(items);
});

// Get low stock items (quantity < 5)
app.MapGet("/inventory/lowstock", () =>
{
    var lowStockItems = inventory.Where(i => i.Quantity < 5);
    return Results.Ok(lowStockItems);
});

app.Run("http://0.0.0.0:5000");

// Inventory Item Model
public class InventoryItem
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public decimal Price { get; set; } = 0.0m;
    public string Description { get; set; } = string.Empty;
}

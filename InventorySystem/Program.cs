using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http; // For Results
using System; // For StringComparison
using System.Collections.Generic;
using System.Linq;

var inventory = new List<InventoryItem>();
var nextId = 1;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "Inventory System is running!");

// View all inventory items
app.MapGet("/inventory", () => inventory);

// View specific item by ID
app.MapGet("/inventory/{id}", (int id) => 
{
    var item = inventory.FirstOrDefault(i => i.Id == id);
    return item != null ? Results.Ok(item) : Results.NotFound();
});

// Add new inventory item
app.MapPost("/inventory", (InventoryItem item) =>
{
    // Convert string values to proper types
    item.Id = nextId++;
    item.Quantity = int.TryParse(item.Quantity.ToString(), out var qty) ? qty : 0;
    item.Price = decimal.TryParse(item.Price.ToString(), out var price) ? price : 0m;
    
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
    existingItem.Quantity = int.TryParse(updatedItem.Quantity.ToString(), out var qty) ? qty : existingItem.Quantity;
    existingItem.Price = decimal.TryParse(updatedItem.Price.ToString(), out var price) ? price : existingItem.Price;
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

public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public object Quantity { get; set; } // Changed to object to handle both string and int
    public object Price { get; set; } // Changed to object to handle both string and decimal
    public string Description { get; set; }
}

using ReadCity.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession();

var app = builder.Build();

Database.InitializeDatabase();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapGet("", () => Results.Redirect("/Login"));
app.MapRazorPages();
app.Run();

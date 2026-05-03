using Microsoft.EntityFrameworkCore;
using SmartClinic.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session for simple authentication simulation
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    if (!context.Users.Any())
    {
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("password");
        context.Users.Add(new SmartClinic.Models.User { Name = "Admin User", Code = "admin123", Role = "Admin", Password = defaultPasswordHash });
        context.Users.Add(new SmartClinic.Models.User { Name = "Dr. Smith", Code = "doc123", Role = "Doctor", Password = defaultPasswordHash });
        context.Users.Add(new SmartClinic.Models.User { Name = "Front Desk", Code = "rec123", Role = "Reception", Password = defaultPasswordHash });
        context.Users.Add(new SmartClinic.Models.User { Name = "John Doe", Code = "pat123", Role = "Patient", Password = defaultPasswordHash });
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();

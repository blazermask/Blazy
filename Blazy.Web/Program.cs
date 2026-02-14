using Blazy.Data;
using Blazy.Repository.Interfaces;
using Blazy.Repository.Repositories;
using Blazy.Services.Interfaces;
using Blazy.Services.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Database configuration
builder.Services.AddDbContext<BlazyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddIdentity<Blazy.Core.Entities.User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<BlazyDbContext>()
.AddDefaultTokenProviders();

// Configure Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();

// Register UserManager and RoleManager
builder.Services.AddScoped<UserManager<Blazy.Core.Entities.User>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BlazyDbContext>();
    var userManager = services.GetRequiredService<UserManager<Blazy.Core.Entities.User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await DataInitializer.SeedDataAsync(context, userManager, roleManager);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "blog",
    pattern: "{username}",
    defaults: new { controller = "Blog", action = "Index" });

app.MapControllerRoute(
    name: "post",
    pattern: "post/{id}",
    defaults: new { controller = "Post", action = "Index" });

app.Run();
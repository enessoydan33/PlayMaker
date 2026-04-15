using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayMaker.Api;
using PlayMaker.Data;
using PlayMaker.Data.Concrete.EfCore;
using PlayMaker.Entity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<FootballService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IUserVoteRepository, UserVoteRepository>();
builder.Services.AddScoped<LigIsimler>();
builder.Services.AddScoped<LiveScoreServices>();
builder.Services.AddScoped<PlayerSearchServices>();
builder.Services.AddScoped<PlayerServices>();
builder.Services.AddScoped<FootballNewsServices>();
builder.Services.AddScoped<Top10Players>();
builder.Services.AddScoped<GoalServices>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddHostedService<PollBackgroundService>();
builder.Services.AddScoped<PollService>();
builder.Services.AddMemoryCache();







builder.Services.AddDbContext<PlaymakerContext>(
    options => {

        var a = builder.Configuration;
        var b = a.GetConnectionString("DefaultConnection");
        options.UseNpgsql(b);
        
        });

builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<PlaymakerContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredUniqueChars = 0;
  
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.Run();

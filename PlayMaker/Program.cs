using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlayMaker.Api;
using PlayMaker.Data;
using PlayMaker.Data.Concrete.EfCore;
using PlayMaker.Entity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<SofaScoreService>();
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
builder.Services.AddScoped<PollService>();

// Hosted service kept registered but does not call external APIs (see PollBackgroundService).
builder.Services.AddHostedService<PollBackgroundService>();

builder.Services.AddDbContext<PlaymakerContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connection);
});

builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<PlaymakerContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredUniqueChars = 0;
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
});

var app = builder.Build();

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

app.Run();

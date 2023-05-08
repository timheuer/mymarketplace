using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LiteDbOptions>(builder.Configuration.GetSection("LiteDbOptions"));
builder.Services.Configure<FormOptions>(p =>
{
    p.ValueLengthLimit = int.MaxValue;
    p.MultipartBodyLengthLimit = int.MaxValue;
    p.MemoryBufferThreshold = int.MaxValue;
});

builder.Services.AddSingleton<ILiteDbContext, LiteDbContext>();
builder.Services.AddSingleton<IPackageReader, PackageReader>();
builder.Services.AddSingleton<ExtensionClient>();
builder.Services.AddTransient<IDatabaseService, DatabaseService>();
builder.Services.AddSingleton<AppInfo>();
builder.Services.AddAntDesign();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("{*path}","/_Host");

app.Run();

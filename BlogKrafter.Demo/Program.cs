using BlogKrafter;
using BlogKrafter.Data.Sqlite;
using BlogKrafter.Demo.Components;
using BlogKrafter.Demo.Components.Layout;
using BlogKrafter.Demo.Infrastructure;
using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Authentication;
using Pickstack;
using Pickstack.FileManager.WebFilez;
using Pickstack.RichTextEditor.TinyMce8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(DemoAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(DemoAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();

builder.Services.AddNetForgeComponents(ui =>
{
    ui.ThemeStorageKey = "blogkrafter-demo.theme";
    ui.DefaultColorScheme = "system";
    ui.AccentColor = "#0f766e";
    ui.AccentColorDark = "#2dd4bf";
});

builder.Services.ConfigureNetForgeToastSounds(sounds => sounds.EnableSounds = true);

builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "BlogKrafter demo";
    shell.BrandHref = "/";
    shell.UseSharedMenu = true;
});

builder.Services.BuildMenu(menu => menu
    .AsSidebar()
    .AddMenu("Content")
    .AddBlogKrafter());

// The WebFilez file browser is an MVC page shown in the picker's frame.
builder.Services.AddControllersWithViews();
builder.Services.AddPickstack(builder.Configuration)
    .AddWebFilez(files => files.UseHostBlazor())
    .AddTinyMce8(editor => editor
        .WithHeight(520)
        .WithToolbar("undo redo | blocks | bold italic | bullist numlist blockquote | link image media | code"));

builder.Services.AddBlogKrafter(options =>
{
    builder.Configuration.GetSection(BlogKrafterOptions.SectionName).Bind(options);
    options.PublicLayout = typeof(SiteLayout);
});
builder.Services.AddBlogKrafterSqlite(builder.Configuration.GetConnectionString("Blog"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseStaticFiles();
app.MapStaticAssets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UsePickstack();
app.MapControllers();
app.MapBlogKrafter();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(BlogKrafterComponents.Assembly);

app.Run();

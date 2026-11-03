# BlogKrafter

A blog for any ASP.NET Core site, built with Blazor on .NET 10. Add a package, call two methods, and the site gets public blog pages, RSS and Atom feeds, a sitemap, and editor screens for posts, categories and tags.

- Posts are written in Markdown or, when the site has one, a rich text editor. Authors can switch a post between the two.
- Scheduled publishing, drafts with private previews, and old addresses that redirect when a post's address changes.
- Tags, categories, search, an archive by year, related posts, previous and next links, and a table of contents for longer posts.
- Open Graph, canonical links and structured data on every post.
- Stored HTML is sanitized on save. Embedded frames are kept only for hosts you allow.
- SQL Server, PostgreSQL, MySQL or SQLite.

## Packages

| Package | What it is |
| --- | --- |
| `BlogKrafter` | The pages, editor screens and registration. This is the one a site references. |
| `BlogKrafter.Services` | Services and options without any UI, for hosts that only need the data. |
| `BlogKrafter.Data` | Entities and the shared EF Core model. |
| `BlogKrafter.Data.Sql`, `.Npgsql`, `.MySql`, `.Sqlite` | One storage provider each, with its migrations. Reference one. |

## Getting started

Reference `BlogKrafter` and one storage provider, then register both:

```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddBlogKrafter(options =>
{
    builder.Configuration.GetSection(BlogKrafterOptions.SectionName).Bind(options);
    options.PublicLayout = typeof(MainLayout);
});
builder.Services.AddBlogKrafterSqlServer(builder.Configuration.GetConnectionString("Blog")!);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlogKrafter();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(BlogKrafterComponents.Assembly);
```

Add the same assembly to the router so the blog's pages are found:

```razor
<Router AppAssembly="typeof(Program).Assembly" AdditionalAssemblies="[BlogKrafterComponents.Assembly]">
    ...
</Router>
```

The blog is now at `/blog` and the editor at `/admin/blog`. `MapBlogKrafter` serves the feeds at `/blog/feeds/rss.xml`, `/blog/feeds/atom.xml` and `/blog/feeds/sitemap.xml`.

The storage providers are `AddBlogKrafterSqlServer`, `AddBlogKrafterNpgsql`, `AddBlogKrafterMySql` and `AddBlogKrafterSqlite`. The SQLite connection string is optional. Without one, the database goes in the user's local application data folder. Migrations run at startup unless `AutoMigrate` is off.

### Layouts

`PublicLayout` is usually the site's own public layout, so the blog looks like the rest of the site. `AdminLayout` does the same for the editor screens. When either is left null, the NetForge shell layout is used.

A custom admin layout must include `<ForgeDialogHost />`, which shows the editor's confirmation dialogs. The editor screens run in Interactive Server mode. They work whether the rest of the site is static or interactive.

## Moving the blog

The pages that ship with the library answer at `/blog` and `/admin/blog`, because Blazor fixes routes when a library is compiled. To put the blog somewhere else, set the paths and add two small pages of your own:

```csharp
builder.Services.AddBlogKrafter(options =>
{
    options.BasePath = "/posts";
    options.AdminPath = "/admin/posts";
});
```

```razor
@* Components/Pages/Posts.razor *@
@page "/posts"
@page "/posts/{Slug}"
@page "/posts/tag/{Tag}"
@page "/posts/category/{Category}"

<BlogView Slug="@Slug" Tag="@Tag" Category="@Category" />

@code {
    [Parameter] public string? Slug { get; set; }
    [Parameter] public string? Tag { get; set; }
    [Parameter] public string? Category { get; set; }
}
```

```razor
@* Components/Pages/PostsAdmin.razor *@
@page "/admin/posts"
@page "/admin/posts/{Section}"
@page "/admin/posts/{Section}/{Id}"
@attribute [Authorize(Policy = BlogKrafterPolicies.Manage)]

<BlogAdminView Section="@Section" Id="@Id" />

@code {
    [Parameter] public string? Section { get; set; }
    [Parameter] public string? Id { get; set; }
}
```

The built-in pages stop answering once the paths change, so `/blog` returns 404 rather than showing the blog twice. Links, feeds, redirects and the sitemap all follow the new paths. An empty `BasePath` puts the blog at the root of the site.

## Who can edit

The editor screens require the `BlogKrafter.Manage` policy. By default it allows signed-in users in any of `ManagerRoles`, which is `Administrator` unless you change it. An empty list lets any signed-in user in. To decide some other way, register your own policy with that name; BlogKrafter then leaves it alone.

The author's name and id come from the signed-in user's claims. To supply them another way, register your own `IUserContextService` before calling `AddBlogKrafter`.

BlogKrafter brings no sign-in of its own. The site's authentication applies.

## Rich text and images

Markdown works with nothing else installed. For a rich text editor and a file picker, register them with [Pickstack](https://www.nuget.org/packages/Pickstack). BlogKrafter finds whatever is registered:

```csharp
builder.Services.AddControllersWithViews();
builder.Services.AddPickstack(builder.Configuration)
    .AddWebFilez(files => files.UseHostBlazor())
    .AddTinyMce7();

// ...

app.UsePickstack();
app.MapControllers();
```

- With a rich text editor, authors choose Markdown or rich text for each post. `EditorMode` can limit them to one. Switching a post from Markdown to rich text converts it. Switching back keeps the formatted text as HTML inside the Markdown, so the post looks the same. Either way, the editor asks first.
- With a file manager, the cover image gets a Browse button and the Markdown toolbar an Insert image button. The picker opens wherever the host configured that file manager. BlogKrafter does not choose a folder of its own. A site that keeps each tenant's or author's uploads in separate folders registers an `IBlogFileRootProvider` (scoped is fine) whose `GetFileRoot()` returns that folder under the upload directory, such as `site-3`. Register it before calling `AddBlogKrafter`.
- The WebFilez file browser is an MVC page shown in a frame. It needs `AddControllersWithViews()`, `MapControllers()`, and `<RequiresAspNetWebAssets>true</RequiresAspNetWebAssets>` in the site's project file.

## Showing posts elsewhere

`RecentPosts` lists the latest posts on any page, such as a home page:

```razor
<RecentPosts Count="3" Title="From the blog" />
<RecentPosts Count="5" Compact="true" TagSlug="releases" Title="Release notes" />
```

It takes `Count`, `TagSlug`, `CategorySlug`, `Title`, `Compact` (titles and dates instead of cards) and `ShowMoreLink`.

## Comments

BlogKrafter doesn't store comments. To show a comment system, such as Giscus or Disqus, under each post, set `CommentsComponent` to a component of your own. It receives the post as a `Post` parameter of type `PostDto`, and is shown only on posts that allow comments.

```csharp
options.CommentsComponent = typeof(GiscusComments);
```

## Several sites, one database

To keep each site's posts apart, implement `IBlogTenantProvider` and register it before calling `AddBlogKrafter`. `GetTenantId()` returns the current site's id, or an empty string for a single site. Every query and save is scoped to that id, and addresses only need to be unique within a site.

## Options

Bind from the `BlogKrafter` configuration section or set them in code.

| Option | Default | Purpose |
| --- | --- | --- |
| `BasePath` | `/blog` | Where the public blog lives. See [Moving the blog](#moving-the-blog). |
| `AdminPath` | `/admin/blog` | Where the editor screens live. |
| `Title` | `Blog` | The heading on the post list and the title of the feeds. |
| `Description` | | Shown under the title and in the feeds. |
| `SiteUrl` | | The site's public address, such as `https://example.com`. When empty, it is taken from each request. |
| `TimeZone` | UTC | The time zone public dates are shown in, such as `Europe/London`. Editors always see their own browser's time. |
| `DateFormat` | `MMMM d, yyyy` | A .NET date format for dates on public pages. |
| `PageSize` | `10` | Posts per page. |
| `FeedItemCount` | `20` | Posts in each feed. |
| `RelatedPostCount` | `3` | Related posts under each post. `0` hides them. |
| `TableOfContentsMinHeadings` | `3` | Headings a post needs before it gets a table of contents. |
| `WordsPerMinute` | `220` | Used for reading time. |
| `EditorMode` | `Both` | `Markdown`, `RichText` or `Both`. |
| `DefaultContentFormat` | `Markdown` | The format a new post starts in when both are allowed. |
| `ManagerRoles` | `Administrator` | Roles allowed into the editor. |
| `AllowedFrameHosts` | YouTube, Vimeo | Hosts allowed in embedded frames. |
| `AutoMigrate` | `true` | Apply migrations at startup. |
| `PublicLayout`, `AdminLayout` | NetForge shell | Layouts for the public pages and the editor. |
| `CommentsComponent` | | See [Comments](#comments). |

```json
{
  "BlogKrafter": {
    "Title": "Field Notes",
    "Description": "Short essays on building small, durable software.",
    "SiteUrl": "https://example.com",
    "TimeZone": "Europe/London"
  }
}
```

## Menu

On a site using the NetForge shell, `AddBlogKrafter()` puts the editor in a menu:

```csharp
builder.Services.BuildMenu(menu => menu
    .AddMenu("Content")
    .AddBlogKrafter());
```

The menu items are shown only to `ManagerRoles`. If the site has no NetForge authentication, BlogKrafter tells the shell so, and the shell shows no sign-in menu of its own.

## Demo

`BlogKrafter.Demo` is a small site with its own home page that hosts the blog on SQLite, with TinyMCE and WebFilez. Requests from the local machine are signed in as an editor.

```
dotnet run --project BlogKrafter.Demo
```

## License

MIT

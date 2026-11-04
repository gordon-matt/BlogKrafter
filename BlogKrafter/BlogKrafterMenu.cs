using Heroicsoft.NetForge.Components.Navigation;

namespace BlogKrafter;

/// <summary>Puts the blog editor on the host menu. The host places it with <c>AddBlogKrafter()</c>.</summary>
public sealed class BlogKrafterMenuContributor(BlogUrls urls, IOptions<BlogKrafterOptions> options) : INetForgeMenuContributor
{
    public string Name => "BlogKrafter";

    public string DefaultMenu => "Content";

    public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure) => group.AddCustom(item =>
    {
        item.Title = "Blog";
        item.Icon = "book";
        item.Url = urls.Admin();
        item.Roles = options.Value.ManagerRoles.Count == 0 ? null : string.Join(',', options.Value.ManagerRoles);
        item.AddChild(child =>
        {
            child.Title = "Posts";
            child.Icon = "edit";
            child.Url = urls.Admin();
        });
        item.AddChild(child =>
        {
            child.Title = "Categories";
            child.Icon = "bookmark";
            child.Url = urls.AdminCategories();
        });
        item.AddChild(child =>
        {
            child.Title = "Tags";
            child.Icon = "link";
            child.Url = urls.AdminTags();
        });
        configure?.Invoke(item);
    });
}

/// <summary>Places the BlogKrafter contributor. The link keeps the library defaults unless <paramref name="configure"/> replaces them.</summary>
public static class BlogKrafterMenuExtensions
{
    extension(INetForgeMenuGroupBuilder menu)
    {
        public INetForgeMenuGroupBuilder AddBlogKrafter(Action<NetForgeMenuItemOptions>? configure = null) =>
            menu.AddContributor("BlogKrafter", configure);
    }
}
using Heroicsoft.NetForge.Components.Navigation;

namespace BlogKrafter;

/// <summary>Puts the blog editor on the host menu. The host places it with <c>AddBlogKrafter()</c>.</summary>
public sealed class BlogKrafterMenuContributor(BlogUrls urls, IOptions<BlogKrafterOptions> options) : INetForgeMenuContributor
{
    /// <summary>The contributor name. The value is <c>BlogKrafter</c>.</summary>
    public string Name => "BlogKrafter";

    /// <summary>The menu the item joins when the host does not place it. The value is <c>Content</c>.</summary>
    public string DefaultMenu => "Content";

    /// <summary>
    /// Adds Blog, with Posts, Categories, and Tags underneath. The roles are the manager roles, or none when that list is empty, which shows the item to everyone the shell allows.
    /// </summary>
    /// <param name="group">The menu group the item is added to.</param>
    /// <param name="configure">Adjusts the Blog item after the defaults are set. <see langword="null"/> keeps them.</param>
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

/// <summary>Places the BlogKrafter contributor on a menu group.</summary>
public static class BlogKrafterMenuExtensions
{
    extension(INetForgeMenuGroupBuilder menu)
    {
        /// <summary>Adds the Blog item. The link keeps the library defaults unless <paramref name="configure"/> replaces them.</summary>
        /// <param name="configure">Adjusts the item. <see langword="null"/> keeps the defaults.</param>
        /// <returns>The same menu group.</returns>
        public INetForgeMenuGroupBuilder AddBlogKrafter(Action<NetForgeMenuItemOptions>? configure = null) =>
            menu.AddContributor("BlogKrafter", configure);
    }
}
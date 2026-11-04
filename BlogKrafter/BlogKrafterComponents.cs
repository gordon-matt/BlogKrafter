using System.Reflection;

namespace BlogKrafter;

public static class BlogKrafterComponents
{
    /// <summary>
    /// The assembly holding the ready-made <c>/blog</c> and <c>/admin/blog</c> pages. Add it to the router's
    /// <c>AdditionalAssemblies</c> and to <c>MapRazorComponents().AddAdditionalAssemblies</c>. A host that
    /// serves the blog at another path leaves it out and adds its own pages instead.
    /// </summary>
    public static Assembly Assembly => typeof(BlogKrafterComponents).Assembly;
}
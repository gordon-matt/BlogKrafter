using Microsoft.AspNetCore.Components.Web;

namespace BlogKrafter.Infrastructure;

internal static class BlogRenderModes
{
    /// <summary>
    /// The editor needs a live circuit even when the host renders its public pages statically. Not
    /// prerendered: the screens load their data once the circuit connects.
    /// </summary>
    public static IComponentRenderMode Editor { get; } = new InteractiveServerRenderMode(prerender: false);
}
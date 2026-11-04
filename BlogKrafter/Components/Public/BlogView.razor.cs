using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlogKrafter.Components.Public;

/// <summary>
/// Everything under the blog's address, chosen from the route values. Render it from a page whose
/// routes match <see cref="BlogKrafterOptions.BasePath"/> to serve the blog somewhere other than <c>/blog</c>:
/// <c>{base}</c>, <c>{base}/{Slug}</c>, <c>{base}/tag/{Tag}</c> and <c>{base}/category/{Category}</c>.
/// </summary>
public partial class BlogView
{
    private bool preview;

    /// <summary>A post's slug, or <c>archive</c> for the list of every post by year.</summary>
    [Parameter]
    public string? Slug { get; set; }

    [Parameter]
    public string? Tag { get; set; }

    [Parameter]
    public string? Category { get; set; }

    [SupplyParameterFromQuery(Name = "page")]
    private int? PageNumber { get; set; }

    [SupplyParameterFromQuery(Name = "q")]
    private string? Search { get; set; }

    [SupplyParameterFromQuery(Name = "preview")]
    private bool? PreviewRequested { get; set; }

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    [Inject]
    private IAuthorizationService Authorization { get; set; } = default!;

    private bool IsArchive => string.Equals(Slug, "archive", StringComparison.OrdinalIgnoreCase);

    protected override async Task OnParametersSetAsync() =>
        // Drafts and scheduled posts are for the people who can edit them; anyone else gets the live post or a 404.
        preview = PreviewRequested == true && await CanManageAsync();

    private async Task<bool> CanManageAsync()
    {
        if (AuthenticationState is null)
        {
            return false;
        }

        var user = (await AuthenticationState).User;
        return (await Authorization.AuthorizeAsync(user, BlogKrafterPolicies.Manage)).Succeeded;
    }
}
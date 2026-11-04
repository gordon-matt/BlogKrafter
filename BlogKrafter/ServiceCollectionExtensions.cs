using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogKrafter;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the blog: its pages, editor screens, services and the <c>BlogKrafter.Manage</c>
        /// policy. Pair with a storage provider (<c>AddBlogKrafterSqlite</c>, <c>AddBlogKrafterSqlServer</c>,
        /// <c>AddBlogKrafterNpgsql</c> or <c>AddBlogKrafterMySql</c>) and <c>app.MapBlogKrafter()</c>.
        /// For a rich text editor and file picker, also register them with Pickstack.
        /// </summary>
        public IServiceCollection AddBlogKrafter(Action<BlogKrafterOptions>? configure = null)
        {
            services.AddBlogKrafterServices(configure);

            // Safe alongside a host that is itself a Blazor app: these calls are idempotent.
            services.AddRazorComponents().AddInteractiveServerComponents();
            services.AddCascadingAuthenticationState();
            services.AddHttpContextAccessor();

            // The host's own ForgeUiOptions win; this only fills them in when the host has none.
            services.AddNetForgeComponents();
            services.AddNetForgeMenuContributor<BlogKrafterMenuContributor>();

            // The NetForge shell's top bar needs to know the sign-in mode. A host without NetForge
            // authentication manages sign-in itself, so the shell shows no sign-in menu of its own.
            services.TryAddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.None, null));
            services.TryAddScoped<IUserContextService, ClaimsUserContextService>();
            services.TryAddScoped<IBlogFileRootProvider, DefaultFileRootProvider>();
            services.TryAddSingleton<EditorCapabilities>();
            services.TryAddScoped<BlogScript>();

            services.AddAuthorization();
            services.AddOptions<AuthorizationOptions>()
                .Configure<IOptions<BlogKrafterOptions>>((authorization, blog) =>
                {
                    if (authorization.GetPolicy(BlogKrafterPolicies.Manage) is not null)
                    {
                        return;
                    }

                    authorization.AddPolicy(BlogKrafterPolicies.Manage, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        if (blog.Value.ManagerRoles.Count > 0)
                        {
                            policy.RequireRole(blog.Value.ManagerRoles);
                        }
                    });
                });

            return services;
        }
    }
}
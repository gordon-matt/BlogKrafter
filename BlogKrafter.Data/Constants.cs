namespace BlogKrafter;

public static class Constants
{
    /// <summary>
    /// BlogKrafter usually shares a database with the host, so it keeps its own migration history.
    /// With EF's default table, each side would think the other's migrations are missing.
    /// </summary>
    public const string MigrationsHistoryTable = "__BlogKrafterMigrationsHistory";

    /// <summary>
    /// Prefixed rather than schema-qualified, because SQLite and MySQL have no schemas and one naming
    /// scheme across every provider is easier to find in a shared database.
    /// </summary>
    public static class Tables
    {
        public const string Categories = "BlogKrafterCategories";
        public const string Posts = "BlogKrafterPosts";
        public const string PostTags = "BlogKrafterPostTags";
        public const string SlugRedirects = "BlogKrafterSlugRedirects";
        public const string Tags = "BlogKrafterTags";
    }

    public static class MaxLengths
    {
        public const int AuthorName = 128;
        public const int CategoryDescription = 1000;
        public const int CategoryName = 128;
        public const int ImageAlt = 256;
        public const int MetaDescription = 320;
        public const int Slug = 200;
        public const int Summary = 1000;
        public const int TagName = 64;

        /// <summary>Wide enough for a GUID or a host name, and short enough to lead every composite index.</summary>
        public const int TenantId = 64;

        public const int Title = 256;
        public const int Url = 1024;
        public const int UserId = 128;
    }

    public static class DatabaseProviders
    {
        public const string MySql = "MySql";
        public const string Npgsql = "Npgsql";
        public const string Sqlite = "Sqlite";
        public const string SqlServer = "SqlServer";
    }
}
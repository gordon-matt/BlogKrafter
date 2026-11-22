namespace BlogKrafter;

/// <summary>Table names, column lengths, and the storage-provider names shared by every database package.</summary>
public static class Constants
{
    /// <summary>
    /// BlogKrafter usually shares a database with the host, so it keeps its own migration history.
    /// With EF's default table, each side would think the other's migrations are missing.
    /// </summary>
    public const string MigrationsHistoryTable = "__BlogKrafterMigrationsHistory";

    /// <summary>Database schema for providers that support one (SQL Server and PostgreSQL).</summary>
    public const string Schema = "bkraft";

    /// <summary>
    /// Prepended to every table name on providers without schemas. In MySQL a schema is a database,
    /// and SQLite has none, so the tables stay unqualified and are prefixed instead.
    /// </summary>
    public const string TablePrefix = "BlogKrafter_";

    /// <summary>
    /// Unqualified table names. The schema or <see cref="TablePrefix"/> is applied by the context.
    /// </summary>
    public static class Tables
    {
        /// <summary>The categories table. The value is <c>Categories</c>.</summary>
        public const string Categories = "Categories";

        /// <summary>The posts table. The value is <c>Posts</c>.</summary>
        public const string Posts = "Posts";

        /// <summary>The post-to-tag table. The value is <c>PostTags</c>.</summary>
        public const string PostTags = "PostTags";

        /// <summary>The old-address table. The value is <c>SlugRedirects</c>.</summary>
        public const string SlugRedirects = "SlugRedirects";

        /// <summary>The tags table. The value is <c>Tags</c>.</summary>
        public const string Tags = "Tags";
    }

    /// <summary>Column lengths, in characters. Saves that exceed them are rejected.</summary>
    public static class MaxLengths
    {
        /// <summary>The longest author name copied onto a post. The value is 128.</summary>
        public const int AuthorName = 128;

        /// <summary>The longest category description. The value is 1000.</summary>
        public const int CategoryDescription = 1000;

        /// <summary>The longest category name. The value is 128.</summary>
        public const int CategoryName = 128;

        /// <summary>The longest cover-image description. The value is 256.</summary>
        public const int ImageAlt = 256;

        /// <summary>The longest search description. The value is 320.</summary>
        public const int MetaDescription = 320;
        /// <summary>The longest slug. The value is 200.</summary>
        public const int Slug = 200;

        /// <summary>The longest summary or excerpt. The value is 1000.</summary>
        public const int Summary = 1000;
        /// <summary>The longest tag name. The value is 64.</summary>
        public const int TagName = 64;

        /// <summary>Wide enough for a GUID or a host name, and short enough to lead every composite index.</summary>
        public const int TenantId = 64;

        /// <summary>The longest title or search title. The value is 256.</summary>
        public const int Title = 256;

        /// <summary>The longest cover or canonical address. The value is 1024.</summary>
        public const int Url = 1024;
        /// <summary>The longest author id. The value is 128.</summary>
        public const int UserId = 128;
    }

    /// <summary>Values of <see cref="Data.IBlogKrafterDbContextFactory.ProviderName"/>.</summary>
    public static class DatabaseProviders
    {
        /// <summary>MySQL. The value is <c>MySql</c>.</summary>
        public const string MySql = "MySql";

        /// <summary>PostgreSQL. The value is <c>Npgsql</c>.</summary>
        public const string Npgsql = "Npgsql";

        /// <summary>SQLite. The value is <c>Sqlite</c>.</summary>
        public const string Sqlite = "Sqlite";

        /// <summary>SQL Server. The value is <c>SqlServer</c>.</summary>
        public const string SqlServer = "SqlServer";
    }
}
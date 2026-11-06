namespace BlogKrafter;

/// <summary>
/// Provides constants.
/// </summary>
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
        /// <summary>
        /// The categories constant.
        /// </summary>
        public const string Categories = "Categories";
        /// <summary>
        /// The posts constant.
        /// </summary>
        public const string Posts = "Posts";
        /// <summary>
        /// The post tags constant.
        /// </summary>
        public const string PostTags = "PostTags";
        /// <summary>
        /// The slug redirects constant.
        /// </summary>
        public const string SlugRedirects = "SlugRedirects";
        /// <summary>
        /// The tags constant.
        /// </summary>
        public const string Tags = "Tags";
    }

    /// <summary>
    /// Provides max lengths.
    /// </summary>
    public static class MaxLengths
    {
        /// <summary>
        /// The author name constant.
        /// </summary>
        public const int AuthorName = 128;
        /// <summary>
        /// The category description constant.
        /// </summary>
        public const int CategoryDescription = 1000;
        /// <summary>
        /// The category name constant.
        /// </summary>
        public const int CategoryName = 128;
        /// <summary>
        /// The image alt constant.
        /// </summary>
        public const int ImageAlt = 256;
        /// <summary>
        /// The meta description constant.
        /// </summary>
        public const int MetaDescription = 320;
        /// <summary>
        /// The slug constant.
        /// </summary>
        public const int Slug = 200;
        /// <summary>
        /// The summary constant.
        /// </summary>
        public const int Summary = 1000;
        /// <summary>
        /// The tag name constant.
        /// </summary>
        public const int TagName = 64;

        /// <summary>Wide enough for a GUID or a host name, and short enough to lead every composite index.</summary>
        public const int TenantId = 64;

        /// <summary>
        /// The title constant.
        /// </summary>
        public const int Title = 256;
        /// <summary>
        /// The URL constant.
        /// </summary>
        public const int Url = 1024;
        /// <summary>
        /// The user identifier constant.
        /// </summary>
        public const int UserId = 128;
    }

    /// <summary>
    /// Provides database providers.
    /// </summary>
    public static class DatabaseProviders
    {
        /// <summary>
        /// The my SQL constant.
        /// </summary>
        public const string MySql = "MySql";
        /// <summary>
        /// The npgsql constant.
        /// </summary>
        public const string Npgsql = "Npgsql";
        /// <summary>
        /// The sqlite constant.
        /// </summary>
        public const string Sqlite = "Sqlite";
        /// <summary>
        /// The SQL server constant.
        /// </summary>
        public const string SqlServer = "SqlServer";
    }
}
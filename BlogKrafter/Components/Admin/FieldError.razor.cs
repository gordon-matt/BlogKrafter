namespace BlogKrafter.Components.Admin;

/// <summary>The message a failed save carried for one field.</summary>
public partial class FieldError
{
    /// <summary>The message. <see langword="null"/> shows nothing.</summary>
    [Parameter]
    public string? Message { get; set; }
}
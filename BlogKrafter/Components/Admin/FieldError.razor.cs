namespace BlogKrafter.Components.Admin;

/// <summary>The message a failed save carried for one field.</summary>
public partial class FieldError
{
    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    [Parameter]
    public string? Message { get; set; }
}
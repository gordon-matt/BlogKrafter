namespace BlogKrafter.Components.Admin;

/// <summary>The message a failed save carried for one field.</summary>
public partial class FieldError
{
    [Parameter]
    public string? Message { get; set; }
}
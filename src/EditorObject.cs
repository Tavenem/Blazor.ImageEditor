namespace Tavenem.Blazor.ImageEditor;

/// <summary>
/// The data entities saved by <see cref="ImageEditorService.GetDataAsync"/>.
/// </summary>
public class EditorObject
{
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// The height of the object.
    /// </summary>
    public double height { get; set; }

    /// <summary>
    /// The left x-coordinate of the object.
    /// </summary>
    public double left { get; set; }

    /// <summary>
    /// The top y-coordinate of the object.
    /// </summary>
    public double top { get; set; }

    /// <summary>
    /// The width of the object.
    /// </summary>
    public double width { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}

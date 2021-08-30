namespace Tavenem.Blazor.ImageEditor;

/// <summary>
/// The data saved by <see cref="ImageEditorService.GetDataAsync"/>.
/// </summary>
public class EditorData
{
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    /// A collection of <see cref="EditorObject"/> entities.
    /// </summary>
    public EditorObject[]? objects { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}

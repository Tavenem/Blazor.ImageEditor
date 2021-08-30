namespace Tavenem.Blazor.ImageEditor;

/// <summary>
/// Extensions related to Tavenem.Blazor.ImageEditor.
/// </summary>
internal static class ImageEditorExtensions
{
    /// <summary>
    /// Converts a <see cref="DrawingMode"/> to a character for display.
    /// </summary>
    /// <param name="mode">The <see cref="DrawingMode"/> to convert.</param>
    /// <returns>A string representation of the <see cref="DrawingMode"/>.</returns>
    public static string ToCharacter(this DrawingMode mode) => mode switch
    {
        DrawingMode.Ellipse => "⬭",
        DrawingMode.Line => "/",
        DrawingMode.Rectangle => "▭",
        DrawingMode.Path => "∫",
        DrawingMode.Text => "A",
        DrawingMode.Triangle => "△",
        DrawingMode.Free => "ᘓ",
        _ => "?",
    };

    /// <summary>
    /// Converts a <see cref="StrokeLineCap"/> to a character for display.
    /// </summary>
    /// <param name="cap">The <see cref="StrokeLineCap"/> to convert.</param>
    /// <returns>A string representation of the <see cref="StrokeLineCap"/>.</returns>
    public static string ToCharacter(this StrokeLineCap cap) => cap switch
    {
        StrokeLineCap.butt => "|",
        StrokeLineCap.round => "⊃",
        StrokeLineCap.square => "⊐",
        _ => "?",
    };
}

namespace Tavenem.Blazor.ImageEditor;

/// <summary>
/// A drawing mode supported by the image editor.
/// </summary>
public enum DrawingMode
{
    /// <summary>
    /// Ellipses, including circles.
    /// </summary>
    Ellipse = 0,

    /// <summary>
    /// Straight lines.
    /// </summary>
    Line = 1,

    /// <summary>
    /// Rectangles, including squares.
    /// </summary>
    Rectangle = 2,

    /// <summary>
    /// Curved paths (quadratic curves).
    /// </summary>
    Path = 3,

    /// <summary>
    /// Text.
    /// </summary>
    Text = 4,

    /// <summary>
    /// Triangles.
    /// </summary>
    Triangle = 5,

    /// <summary>
    /// Free drawing mode.
    /// </summary>
    Free = 6,
}

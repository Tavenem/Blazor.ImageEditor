namespace Tavenem.Blazor.ImageEditor
{
    /// <summary>
    /// A drawing mode supported by the image editor.
    /// </summary>
    public enum DrawingMode
    {
        /// <summary>
        /// Ellipses, including circles.
        /// </summary>
        Ellipse,

        /// <summary>
        /// Straight lines.
        /// </summary>
        Line,

        /// <summary>
        /// Rectangles, including squares.
        /// </summary>
        Rectangle,

        /// <summary>
        /// Curved paths (quadratic curves).
        /// </summary>
        Path,

        /// <summary>
        /// Text.
        /// </summary>
        Text,

        /// <summary>
        /// Triangles.
        /// </summary>
        Triangle,

        /// <summary>
        /// Free drawing mode.
        /// </summary>
        Free,
    }
}

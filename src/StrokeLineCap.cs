namespace Tavenem.Blazor.ImageEditor
{
    /// <summary>
    /// A type of line cap supported by the image editor.
    /// </summary>
    public enum StrokeLineCap
    {
        /// <summary>
        /// No line cap at all.
        /// </summary>
        butt,

        /// <summary>
        /// A round end cap.
        /// </summary>
        round,

        /// <summary>
        /// A square end cap (differs from <see cref="butt"/> in that it adds to the length of the
        /// stroke).
        /// </summary>
        square,
    }
}

using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Tavenem.Blazor.ImageEditor
{
    /// <summary>
    /// An image editor component.
    /// </summary>
    public partial class ImageEditor
    {
        /// <summary>
        /// Gets the current border color, as a hex string.
        /// </summary>
        public string? BorderColor { get; private set; }

        /// <summary>
        /// Gets the current border size, as a percentage of the full image size.
        /// </summary>
        public double BorderPercent { get; private set; }

        /// <summary>
        /// Optional content shown beside the image.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets the current <see cref="Blazor.ImageEditor.DrawingMode"/> of the editor.
        /// </summary>
        public DrawingMode DrawingMode { get; private set; } = DrawingMode.Free;

        /// <summary>
        /// Optional content shown beside the image when in edit mode.
        /// </summary>
        [Parameter] public RenderFragment? EditContent { get; set; }

        /// <summary>
        /// Gets or sets whether the editor is in edit mode (vs. preview mode).
        /// </summary>
        public bool Editing { get; set; }

        /// <summary>
        /// A callback invoked when the editor switches from preview to edit mode, or vice versa.
        /// </summary>
        [Parameter] public EventCallback<bool> EditingChanged { get; set; }

        /// <summary>
        /// Gets the current fill color, as a hex string.
        /// </summary>
        public string? FillColor { get; private set; }

        /// <summary>
        /// Optional content shown beside the image when in preview mode.
        /// </summary>
        [Parameter] public RenderFragment? PreviewContent { get; set; }

        /// <summary>
        /// An optional callback invoked when generating an object URL representing the current
        /// (edited) image.
        /// </summary>
        [Parameter] public Func<string?, Task>? GetObjUrlCallback { get; set; }

        /// <summary>
        /// An optional callback invoked when generating a JSON string representing the current
        /// (edited) image.
        /// </summary>
        [Parameter] public Func<string?, Task>? SaveJsonCallback { get; set; }

        /// <summary>
        /// Determines whether the standard image edit UI is displayed.
        /// </summary>
        [Parameter] public bool ShowEditControls { get; set; } = true;

        /// <summary>
        /// Determines whether the toggle to enter edit mode is displayed.
        /// </summary>
        [Parameter] public bool ShowEditButton { get; set; } = true;

        /// <summary>
        /// Gets the current stroke color, as a hex string.
        /// </summary>
        public string? StrokeColor { get; private set; } = "#000000";

        /// <summary>
        /// <para>
        /// Gets the first value for the stroke "dash".
        /// </para>
        /// <para>
        /// Combined with <see cref="StrokeDash2"/> determines the pattern used for a non-solid
        /// stroke.
        /// </para>
        /// </summary>
        public double StrokeDash1 { get; private set; }

        /// <summary>
        /// <para>
        /// Gets the second value for the stroke "dash".
        /// </para>
        /// <para>
        /// Combined with <see cref="StrokeDash1"/> determines the pattern used for a non-solid
        /// stroke.
        /// </para>
        /// </summary>
        public double StrokeDash2 { get; private set; }

        /// <summary>
        /// Gets the current <see cref="Blazor.ImageEditor.StrokeLineCap"/>.
        /// </summary>
        public StrokeLineCap StrokeLineCap { get; private set; }

        /// <summary>
        /// Gets the current stroke width.
        /// </summary>
        public double StrokeWidth { get; private set; } = 1;

        private string DisplayBorderColor { get; set; } = "#000000";

        private double DisplayBorderPercent { get; set; }

        private string DisplayFillColor { get; set; } = "#000000";

        private string DisplayStrokeColor { get; set; } = "#000000";

        private double DisplayStrokeDash1 { get; set; }

        private double DisplayStrokeDash2 { get; set; }

        private double DisplayStrokeWidth { get; set; }

        private ElementReference EditorCanvas { get; set; }

        private string? Font { get; set; }

        private bool IsLoading { get; set; }

        private string? JSON { get; set; }

        private string LoadText { get; set; } = "Loading";

        private string? PreviewUrl { get; set; }

        private bool ShowAdvanced { get; set; }

        /// <summary>
        /// Method invoked after each time the component has been rendered. Note that the component
        /// does not automatically re-render after the completion of any returned <see cref="Task"
        /// />, because that would cause an infinite render loop.
        /// </summary>
        /// <param name="firstRender">
        /// Set to <c>true</c> if this is the first time <see
        /// cref="ComponentBase.OnAfterRender(bool)" /> has been invoked on this component instance;
        /// otherwise <c>false</c>.
        /// </param>
        /// <returns>A <see cref="Task" /> representing any asynchronous operation.</returns>
        /// <remarks>
        /// The <see cref="ComponentBase.OnAfterRender(bool)" /> and <see
        /// cref="ComponentBase.OnAfterRenderAsync(bool)" /> lifecycle methods are useful for
        /// performing interop, or interacting with values received from <c>@ref</c>. Use the
        /// <paramref name="firstRender" /> parameter to ensure that initialization work is only
        /// performed once.
        /// </remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await SetLoadingTextAsync().ConfigureAwait(false);
                await Service.LoadEditorAsync(EditorCanvas).ConfigureAwait(false);
                await SetDrawingModeAsync(DrawingMode).ConfigureAwait(false);
                IsLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Causes the control to enter edit mode.
        /// </summary>
        public async Task BeginEditAsync()
        {
            Editing = true;
            await Service.ResizeAsync().ConfigureAwait(false);
            if (EditingChanged.HasDelegate)
            {
                await EditingChanged.InvokeAsync(Editing).ConfigureAwait(false);
            }
            await SetLoadingTextAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(JSON))
            {
                await Service.SetBackgroundImageAsync(PreviewUrl).ConfigureAwait(false);
            }
            else
            {
                await Service.LoadJSONAsync(JSON).ConfigureAwait(false);
            }
            IsLoading = false;
            StateHasChanged();
        }

        /// <summary>
        /// Causes the control to enter preview mode without saving.
        /// </summary>
        public async Task CancelEditAsync()
        {
            Editing = false;
            if (EditingChanged.HasDelegate)
            {
                await EditingChanged.InvokeAsync(Editing).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Clears the image editor control.
        /// </summary>
        public async Task ClearAsync() => await Service.ClearAsync().ConfigureAwait(false);

        /// <summary>
        /// Controls whether objects in the editor may be rotated.
        /// </summary>
        /// <param name="value">Whether objects in the editor may be rotated.</param>
        public async Task EnableRotationAsync(bool value = true)
            => await Service.EnableRotationAsync(value).ConfigureAwait(false);

        /// <summary>
        /// Get the current edit information.
        /// </summary>
        public async Task<EditorData?> GetDataAsync()
        {
            if (Service is null)
            {
                return null;
            }
            return await Service.GetDataAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get a serialized vesion of the current edit information.
        /// </summary>
        /// <returns>A JSON string.</returns>
        public async Task<string?> GetJSONAsync()
        {
            if (Service is null)
            {
                return null;
            }
            return await Service.GetJSONAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Loads an image in the editor from a URL.
        /// </summary>
        /// <param name="imageUrl">The URL to load. Should resolve as an image file.</param>
        public async Task LoadImageAsync(string? imageUrl = null)
        {
            PreviewUrl = imageUrl;
            if (Editing)
            {
                await SetLoadingTextAsync().ConfigureAwait(false);
                await Service.SetBackgroundImageAsync(imageUrl).ConfigureAwait(false);
                IsLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Restores a serialized edit state, as obtained from <see cref="GetJSONAsync"/>.
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <remarks>
        /// If <see langword="null"/> or an empty <see cref="string"/> is given the editor will be
        /// cleared. Note, however, that the <see cref="ClearAsync"/> method is a more efficient way
        /// to achieve this when that effect is deliberately intended.
        /// </remarks>
        public async Task LoadJSONAsync(string? json = null)
        {
            JSON = json;
            if (Editing)
            {
                await SetLoadingTextAsync().ConfigureAwait(false);
                await Service.LoadJSONAsync(json).ConfigureAwait(false);
                IsLoading = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Sets the current border color to the given hex string.
        /// </summary>
        /// <param name="color">A hex string representing a color.</param>
        public async Task SetBorderColorAsync(string? color = null)
        {
            BorderColor = ColorString(color);
            DisplayBorderColor = BorderColor ?? "#000000";
            await Service.SetBorderColorAsync(BorderColor).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current border size as a fraction of the image size.
        /// </summary>
        /// <param name="value">
        /// <para>
        /// A value representing a fraction of the image size.
        /// </para>
        /// <para>
        /// Values less than zero are treated as zero.
        /// </para>
        /// <para>
        /// When a value greater than 1 is given, it is first divided by 100 under the assumption
        /// that it represents a percentage.
        /// </para>
        /// </param>
        public async Task SetBorderPercentAsync(double value = 0)
        {
            if (value < 0)
            {
                BorderPercent = 0;
                DisplayBorderPercent = 0;
            }
            else
            {
                BorderPercent = value > 1 ? value / 100.0 : value;
                DisplayBorderPercent = BorderPercent;
            }
            await Service.SetBorderPercentAsync(BorderPercent).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current border size as a number of pixels.
        /// </summary>
        /// <param name="value">A size, in pixels.</param>
        public async Task SetBorderWidthAsync(double value = 0)
            => await Service.SetBorderWidthAsync(value).ConfigureAwait(false);

        /// <summary>
        /// Sets the current <see cref="Blazor.ImageEditor.DrawingMode"/> of the editor.
        /// </summary>
        /// <param name="mode">A <see cref="Blazor.ImageEditor.DrawingMode"/>.</param>
        public async Task SetDrawingModeAsync(DrawingMode mode)
        {
            DrawingMode = mode;
            await Service.SetDrawingModeAsync(mode).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current fill color to the given hex string.
        /// </summary>
        /// <param name="color">
        /// A hex string representing a color. Or <see langword="null"/> or an empty string to set a
        /// transparent fill.
        /// </param>
        public async Task SetFillColorAsync(string? color = null)
        {
            FillColor = ColorString(color);
            DisplayFillColor = FillColor ?? "#000000";
            await Service.SetFillColorAsync(FillColor).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current font for the text tool. Must be the name of a Google Font.
        /// </summary>
        /// <param name="font">The name of a Google Font.</param>
        public async Task SetFontAsync(string? font = null)
        {
            Font = font;
            await Service.SetFontAsync(font).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current <see cref="Blazor.ImageEditor.StrokeLineCap"/>.
        /// </summary>
        /// <param name="cap">A <see cref="Blazor.ImageEditor.StrokeLineCap"/> value.</param>
        public async Task SetLineCapAsync(StrokeLineCap cap)
        {
            StrokeLineCap = cap;
            await Service.SetLineCapAsync(cap).ConfigureAwait(false);
        }

        /// <summary>
        /// Controls whether the editor should be cleared each time a new mark is made.
        /// </summary>
        /// <param name="value">Whether the editor should be cleared each time a new mark is
        /// made.</param>
        public async Task SetRedrawAsync(bool value = true)
            => await Service.SetRedrawAsync(value).ConfigureAwait(false);

        /// <summary>
        /// Sets the current stroke color to the given hex string.
        /// </summary>
        /// <param name="color">
        /// A hex string representing a color. Or <see langword="null"/> or an empty string to set a
        /// transparent stroke.
        /// </param>
        public async Task SetStrokeColorAsync(string? color = null)
        {
            StrokeColor = ColorString(color);
            DisplayStrokeColor = StrokeColor ?? "#000000";
            await Service.SetStrokeColorAsync(StrokeColor).ConfigureAwait(false);
        }

        /// <summary>
        /// <para>
        /// Sets the first value for the stroke "dash".
        /// </para>
        /// <para>
        /// Combined with <see cref="StrokeDash2"/> determines the pattern used for a non-solid
        /// stroke.
        /// </para>
        /// </summary>
        /// <param name="value">The first value for the stroke "dash".</param>
        public async Task SetStrokeDash1Async(double value = 0)
        {
            StrokeDash1 = value < 0 ? 0 : value;
            DisplayStrokeDash1 = StrokeDash1;
            await Service.SetStrokeDash1Async(StrokeDash1).ConfigureAwait(false);
        }

        /// <summary>
        /// <para>
        /// Sets the second value for the stroke "dash".
        /// </para>
        /// <para>
        /// Combined with <see cref="StrokeDash1"/> determines the pattern used for a non-solid
        /// stroke.
        /// </para>
        /// </summary>
        /// <param name="value">The second value for the stroke "dash".</param>
        public async Task SetStrokeDash2Async(double value = 0)
        {
            StrokeDash2 = value < 0 ? 0 : value;
            DisplayStrokeDash2 = StrokeDash2;
            await Service.SetStrokeDash2Async(StrokeDash2).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the stroke width.
        /// </summary>
        /// <param name="value">A number of pixels.</param>
        public async Task SetStrokeWidthAsync(double value = 1)
        {
            StrokeWidth = value < 1 ? 1 : value;
            DisplayStrokeWidth = StrokeWidth;
            await Service.SetStrokeWidthAsync(StrokeWidth).ConfigureAwait(false);
        }

        private static string? ColorString(string? color = null)
        {
            if (string.IsNullOrEmpty(color)
                || color[0] != '#')
            {
                return null;
            }
            if (color.Length == 7)
            {
                return color;
            }
            if (color.Length == 4)
            {
                return new string(new char[] { color[0], color[1], color[1], color[2], color[2], color[3], color[3] });
            }
            return null;
        }

        private async Task BringForwardAsync() => await Service.BringForwardAsync().ConfigureAwait(false);

        private async Task BringToFrontAsync() => await Service.BringToFrontAsync().ConfigureAwait(false);

        private Task ClearBorderColorAsync() => SetBorderColorAsync((string?)null);

        private Task ClearFillColorAsync() => SetFillColorAsync((string?)null);

        private Task ClearStrokeColorAsync() => SetStrokeColorAsync((string?)null);

        private async Task CopyAsync() => await Service.CopyAsync().ConfigureAwait(false);

        private async Task CutAsync() => await Service.CutAsync().ConfigureAwait(false);

        private async Task DeleteAsync() => await Service.DeleteAsync().ConfigureAwait(false);

        private async Task DownloadImageAsync() => await Service.SaveImageAsync().ConfigureAwait(false);

        private async Task PasteAsync() => await Service.PasteAsync().ConfigureAwait(false);

        private async Task RedoAsync() => await Service.RedoAsync().ConfigureAwait(false);

        private async Task SaveImageAsync()
        {
            JSON = await Service.GetJSONAsync().ConfigureAwait(false);
            if (SaveJsonCallback is not null)
            {
                await SaveJsonCallback.Invoke(JSON).ConfigureAwait(false);
            }
            if (GetObjUrlCallback is not null)
            {
                var url = await Service.GetObjUrlAsync().ConfigureAwait(false);
                await GetObjUrlCallback.Invoke(url).ConfigureAwait(false);
            }
            Editing = false;
            if (EditingChanged.HasDelegate)
            {
                await EditingChanged.InvokeAsync(Editing).ConfigureAwait(false);
            }
        }

        private async Task SendBackwardsAsync() => await Service.SendBackwardsAsync().ConfigureAwait(false);

        private async Task SendToBackAsync() => await Service.SendToBackAsync().ConfigureAwait(false);

        private Task SetBorderColorAsync(ChangeEventArgs e) => SetBorderColorAsync(e.Value as string);

        private async Task SetBorderPercentAsync(ChangeEventArgs e)
        {
            if (e.Value is string s
                && double.TryParse(s, out var value))
            {
                await SetBorderPercentAsync(value).ConfigureAwait(false);
            }
            else
            {
                DisplayBorderPercent = BorderPercent;
            }
        }

        private async Task SetDrawingModeAsync(ChangeEventArgs e)
        {
            if (e.Value is not null)
            {
                await SetDrawingModeAsync((DrawingMode)e.Value).ConfigureAwait(false);
            }
        }

        private Task SetFillColorAsync(ChangeEventArgs e) => SetFillColorAsync(e.Value as string);

        private Task SetFontAsync(ChangeEventArgs e) => SetFontAsync(e.Value as string);

        private async Task SetLineCapAsync(ChangeEventArgs e)
        {
            if (e.Value is not null)
            {
                await SetLineCapAsync((StrokeLineCap)e.Value).ConfigureAwait(false);
            }
        }

        private void SetLoadingText(string? value = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                LoadText = "Loading";
            }
            else
            {
                LoadText = value;
            }
            IsLoading = true;
            StateHasChanged();
        }

        private async Task SetLoadingTextAsync(string? value = null)
        {
            SetLoadingText(value);
            await Task.Delay(1).ConfigureAwait(false);
        }

        private Task SetStrokeColorAsync(ChangeEventArgs e) => SetStrokeColorAsync(e.Value as string);

        private async Task SetStrokeDash1Async(ChangeEventArgs e)
        {
            if (e.Value is string s
                && double.TryParse(s, out var value))
            {
                await SetStrokeDash1Async(value).ConfigureAwait(false);
            }
            else
            {
                DisplayStrokeDash2 = StrokeDash2;
            }
        }

        private async Task SetStrokeDash2Async(ChangeEventArgs e)
        {
            if (e.Value is string s
                && double.TryParse(s, out var value))
            {
                await SetStrokeDash2Async(value).ConfigureAwait(false);
            }
            else
            {
                DisplayStrokeDash1 = StrokeDash1;
            }
        }

        private async Task SetStrokeWidthAsync(ChangeEventArgs e)
        {
            if (e.Value is string s
                && double.TryParse(s, out var value))
            {
                await SetStrokeWidthAsync(value).ConfigureAwait(false);
            }
            else
            {
                DisplayStrokeWidth = StrokeWidth;
            }
        }

        private async Task UndoAsync() => await Service.UndoAsync().ConfigureAwait(false);
    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tavenem.Blazor.ImageEditor
{
    /// <summary>
    /// Provides JS Interop for the image editor component.
    /// </summary>
    public class ImageEditorService : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
        private readonly Lazy<List<string>> _objectURLs;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of <see cref="ImageEditorService"/>
        /// </summary>
        /// <param name="jsRuntime">An <see cref="IJSRuntime"/> instance.</param>
        public ImageEditorService(IJSRuntime jsRuntime)
        {
            _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
           "import", "./_content/Tavenem.Blazor.ImageEditor/editor.js").AsTask());
            _objectURLs = new(() => new List<string>());
        }

        /// <summary>
        /// Moves the currently selected object(s) in the editor above the next-higher object.
        /// </summary>
        public async ValueTask BringForwardAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("bringForward")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Moves the currently selected object(s) in the editor to the top of the stack.
        /// </summary>
        public async ValueTask BringToFrontAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("bringToFront")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Clears the image editor.
        /// </summary>
        public async ValueTask ClearAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("clear")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Copies the currently selected object(s) in the editor.
        /// </summary>
        public async ValueTask CopyAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("copy")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Copies the currently selected object(s) in the editor and deletes them.
        /// </summary>
        public async ValueTask CutAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("cut")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes the currently selected object(s) in the editor
        /// </summary>
        public async ValueTask DeleteAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("deleteObjects")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Disposes of the editor and its resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                if (_objectURLs.IsValueCreated)
                {
                    foreach (var url in _objectURLs.Value)
                    {
                        await module.InvokeVoidAsync("revokeObjUrl", url)
                            .ConfigureAwait(false);
                    }
                }
                await module.InvokeVoidAsync("dispose").ConfigureAwait(false);
                await module.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Controls whether objects in the editor may be rotated.
        /// </summary>
        /// <param name="value">Whether objects in the editor may be rotated.</param>
        public async ValueTask EnableRotationAsync(bool value = true)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && value)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("enableRotation", value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get the current edit information.
        /// </summary>
        public async ValueTask<EditorData?> GetDataAsync()
        {
            var json = await GetJSONAsync()
                .ConfigureAwait(false);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return System.Text.Json.JsonSerializer.Deserialize<EditorData>(json);
        }

        /// <summary>
        /// Get an object URL representing the current state of the image.
        /// </summary>
        public async ValueTask<string?> GetObjUrlAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                var url = await module.InvokeAsync<string?>("getObjUrl")
                    .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(url))
                {
                    _objectURLs.Value.Add(url);
                }
                return url;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get a serialized vesion of the current edit information.
        /// </summary>
        /// <returns>A JSON string.</returns>
        public async ValueTask<string?> GetJSONAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                return await module.InvokeAsync<string?>("getJSON")
                    .ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads the editor.
        /// </summary>
        /// <param name="editorCanvas">
        /// The HTML canvas element to use.
        /// </param>
        /// <param name="imageUrl">
        /// An optional image URL to load. Should resolve as an image file.
        /// </param>
        public async ValueTask LoadEditorAsync(ElementReference editorCanvas, string? imageUrl = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("loadEditor", editorCanvas, imageUrl)
                .ConfigureAwait(false);
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
        public async ValueTask LoadJSONAsync(string? json = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("loadJSON", json)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Paste any object(s) copied via <see cref="CopyAsync"/> or <see cref="CutAsync"/>.
        /// </summary>
        public async ValueTask PasteAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("paste")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Re-does any operations undone via <see cref="UndoAsync"/>.
        /// </summary>
        public async ValueTask RedoAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("redo")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Forces the editor to recalculate its dimensions.
        /// </summary>
        public async ValueTask ResizeAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("resize")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Initiates a browser file download operation for the current state of the edited image.
        /// </summary>
        public async ValueTask SaveImageAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("saveImage")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Moves the currently selected object(s) in the editor below the next-lower object.
        /// </summary>
        public async ValueTask SendBackwardsAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("sendBackwards")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Moves the currently selected object(s) in the editor to the bottom of the stack.
        /// </summary>
        public async ValueTask SendToBackAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("sendToBack")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sets the background image.
        /// </summary>
        /// <param name="imageUrl">
        /// The image URL to load. Should resolve as an image file. Or <see langword="null"/> to
        /// clear the background image.
        /// </param>
        public async ValueTask SetBackgroundImageAsync(string? imageUrl = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setBackgroundImage", imageUrl)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current border color to the given hex string.
        /// </summary>
        /// <param name="color">A hex string representing a color.</param>
        public async ValueTask SetBorderColorAsync(string? color = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && string.IsNullOrEmpty(color))
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setBorderColor", color)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current border size as a fraction of the image size.
        /// </summary>
        /// <param name="value">
        /// A value representing a fraction of the image size.
        /// </param>
        public async ValueTask SetBorderPercentAsync(double value = 0)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && value == 0)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setBorderPercent", value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current border size as a number of pixels.
        /// </summary>
        /// <param name="value">A size, in pixels.</param>
        public async ValueTask SetBorderWidthAsync(double value = 0)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && value == 0)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setBorderWidth", value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current <see cref="DrawingMode"/> of the editor.
        /// </summary>
        /// <param name="mode">A <see cref="DrawingMode"/>.</param>
        public async ValueTask SetDrawingModeAsync(DrawingMode mode)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setDrawingMode", (int)mode)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current fill color to the given hex string.
        /// </summary>
        /// <param name="color">
        /// A hex string representing a color. Or <see langword="null"/> or an empty string to set a
        /// transparent fill.
        /// </param>
        public async ValueTask SetFillColorAsync(string? color = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && string.IsNullOrEmpty(color))
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setFillColor", color)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current font for the text tool. Must be the name of a Google Font.
        /// </summary>
        /// <param name="font">The name of a Google Font.</param>
        public async ValueTask SetFontAsync(string? font = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (string.IsNullOrEmpty(font))
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setFont", font)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current <see cref="StrokeLineCap"/>.
        /// </summary>
        /// <param name="cap">A <see cref="StrokeLineCap"/> value.</param>
        public async ValueTask SetLineCapAsync(StrokeLineCap cap)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && cap == StrokeLineCap.butt)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setLineCap", cap.ToString())
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Controls whether the editor should be cleared each time a new mark is made.
        /// </summary>
        /// <param name="value">Whether the editor should be cleared each time a new mark is
        /// made.</param>
        public async ValueTask SetRedrawAsync(bool value = false)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && !value)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setRedraw", value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the current stroke color to the given hex string.
        /// </summary>
        /// <param name="color">
        /// A hex string representing a color. Or <see langword="null"/> or an empty string to set a
        /// transparent stroke.
        /// </param>
        public async ValueTask SetStrokeColorAsync(string? color = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && string.IsNullOrEmpty(color))
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setStrokeColor", color)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// <para>
        /// Sets the first value for the stroke "dash".
        /// </para>
        /// <para>
        /// Combined with <see cref="SetStrokeDash2Async(double)"/> determines the pattern used for
        /// a non-solid stroke.
        /// </para>
        /// </summary>
        /// <param name="value">The first value for the stroke "dash".</param>
        public async ValueTask SetStrokeDash1Async(double value = 0)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && value == 0)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setStrokeDash1", value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// <para>
        /// Sets the second value for the stroke "dash".
        /// </para>
        /// <para>
        /// Combined with <see cref="SetStrokeDash1Async(double)"/> determines the pattern used for
        /// a non-solid stroke.
        /// </para>
        /// </summary>
        /// <param name="value">The second value for the stroke "dash".</param>
        public async ValueTask SetStrokeDash2Async(double value = 0)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && value == 0)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setStrokeDash2", value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the stroke width.
        /// </summary>
        /// <param name="value">A number of pixels.</param>
        public async ValueTask SetStrokeWidthAsync(double value = 1)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (!_moduleTask.IsValueCreated && value == 1)
            {
                return;
            }
            var module = await _moduleTask.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("setStrokeWidth", value)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Un-does the most recent operation in the editor. Note that not all activities count as
        /// "operations" for this purpose.
        /// </summary>
        public async ValueTask UndoAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ImageEditorService));
            }
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.InvokeVoidAsync("undo")
                    .ConfigureAwait(false);
            }
        }
    }
}

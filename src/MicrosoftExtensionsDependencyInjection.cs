using Tavenem.Blazor.ImageEditor;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension for Tavenem.Blazor.ImageEditor.
    /// </summary>
    public static class MicrosoftExtensionsDependencyInjection
    {
        /// <summary>
        /// Add support for the Tavenem.Blazor.ImageEditor.
        /// </summary>
        /// <param name="services">An <see cref="IServiceCollection"/> instance.</param>
        public static void AddImageEditor(this IServiceCollection services)
            => services.AddScoped<ImageEditorService>();
    }
}

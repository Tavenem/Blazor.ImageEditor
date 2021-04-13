![build](https://img.shields.io/github/workflow/status/Tavenem/Blazor.ImageEditor/publish/main) [![NuGet downloads](https://img.shields.io/nuget/dt/Tavenem.Blazor.ImageEditor)](https://www.nuget.org/packages/Tavenem.Blazor.ImageEditor/)

Tavenem.Blazor.ImageEditor
==

Tavenem.Blazor.ImageEditor is a [Razor class
library](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class) (RCL) containing a
[Razor component](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/class-libraries).
It allows displaying an image with basic image edit controls, and has mechanisms for saving and
restoring the edited file.

It wraps the [Fabric.js](http://fabricjs.com/) HTML5 canvas library.

## Installation

Tavenem.Blazor.ImageEditor is available as a [NuGet package](https://www.nuget.org/packages/Tavenem.Blazor.ImageEditor/).

## Use

1. Call the `AddImageEditor()` extension method on your `IServiceCollection` (probably in
`Program.Main` for a Blazor WebAssembly project, or `Startup.ConfigureServices` for a Blazor server
project).

1. Add the following css reference to the head section of your index.html (for Blazor WebAssembly)
   or _Host.cshtml (for Blazor server):

   ````html
   <link rel="stylesheet" href="_content/Tavenem.Blazor.ImageEditor/style.css" />
   ````

   Note that you must [enable static
   files](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files) to serve content
   from a RCL.

   Why is this stylesheet not included via [Blazor CSS
   isolation](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation)? Because
   it must apply outside the scope of the component. It refers to a dynamically-generated DOM
   element managed by the [Fabric.js](http://fabricjs.com/) library which is appended to the body.

1. Add the following script references to the bottom fo the body section of your index.html (for
   Blazor WebAssembly) or _Host.cshtml (for Blazor server):

   ````html
    <script src="https://cdnjs.cloudflare.com/ajax/libs/fabric.js/4.3.1/fabric.min.js" integrity="sha512-ACqMrfAtm537AWzgx/xQ57JnFxXeq8RylQMGg4y/e6M2ew4Z8NycE8aId/Bt2ZE+w1gNsox3MgwxKl7SGMRdtA==" crossorigin="anonymous"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/webfont/1.6.26/webfont.js"></script>
   ````

   Why use external resources instead of packaging the libraries with the RCL? Because it keeps the
   package (and any downstream packages you build with it) small; and because it can improve browser
   caching, especially if the libraries and this package have different update cycles.

1. Include an `ImageEditor` component on a page.

1. To set an initial image, call the `LoadImageAsync(string?)` method with the URL of an image you
   wish to display.
   
   Alternatively, you can omit this step and the editor will start as a blank canvas onto which a
   user may draw.

1. The user may toggle the control from preview mode (displays the original image) to edit mode with
   a button. Edit mode displays a selection of common image editing controls which allow drawing
   onto the image.
   
   Alternatively, you may set the `ShowControls` parameter to false to disable the standard user
   interface. If you do so you will need to supply a custom UI (see below), or control the component
   programmatically.

1. By default users will have a "download" button that will invoke the browser's file save option,
   to save a copy of their edited image. You may also choose to provide values for the
   `GetObjUrlCallback` and/or `SaveJsonCallback` parameters. If you do the user will also see an
   additional "save" button which will invoke both of these callbacks.
   
   `GetObjUrlCallback` will be passed an object URL referencing the current, edited image. You may
   use this to work with the edited image. For example, to get the image as a byte array for
   persisting to storage:

   ```csharp
   var bytes = await HttpClient.GetByteArrayAsync(url);
   ```

   The object URL will remain valid as long as the `ImageEditor` component remains active. When the
   component is disposed, all object URLs it generated will be revoked.
   
   The `SaveJsonCallback` callback will receive a JSON string containing a serialized representation
   of the editor's state. This can be passed to the `LoadJSONAsync(string?)` method to allow a user
   to resume editing where they left off.

The `ImageEditor` component supports two options for custom UI:

  - You can supply `ChildContent` (additional markup nested inside the component, with or without
    a wrapping ChildContent element) which will be displayed alongside the existing controls in
    preview mode, and whenever the `ShowControls` parameter has been set to false.

  - **OR** you can supply `EditContent` and/or `PreviewContent` child components, which will be
    displayed *only* when the `ShowControls` parameter has been set to false. `EditContent` will
    then be displayed when the control is in edit mode, and `PreviewContent` will be shown in
    preview mode.
      
    Note that there will be no default UI for toggling between modes when `ShowControls` is false.
    You must either toggle modes programmatically (with the `BeginEditAsync` and `CancelEditAsync`
    methods), or provide your own UI in the `EditContent` and `PreviewContent` content that hooks
    into those methods.

## Roadmap

Tavenem.Blazor.ImageEditor is currently in a prerelease state. Development is ongoing, and breaking
changes are possible before the first production release.

No release date is currently set for v1.0 of Tavenem.Blazor.ImageEditor.

## Contributing

Contributions are always welcome. Please carefully read the [contributing](docs/CONTRIBUTING.md) document to learn more before submitting issues or pull requests.

## Code of conduct

Please read the [code of conduct](docs/CODE_OF_CONDUCT.md) before engaging with our community, including but not limited to submitting or replying to an issue or pull request.
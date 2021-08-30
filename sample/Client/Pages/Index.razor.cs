using Microsoft.AspNetCore.Components;
using System.Web;

namespace Tavenem.Blazor.ImageEditor.Sample.Client.Pages;

public partial class Index
{
    [Inject] private HttpClient? HttpClient { get; set; }

    private ImageEditor? ImageEditor { get; set; }

    private protected bool IsLoading { get; set; }

    private protected string LoadText { get; set; } = "Loading";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || HttpClient is null)
        {
            return;
        }

        LoadText = "Loading";
        IsLoading = true;
        StateHasChanged();
        await Task.Delay(1).ConfigureAwait(false);

        var host = await HttpClient.GetStringAsync("/upload").ConfigureAwait(false);
        if (string.IsNullOrEmpty(host))
        {
            IsLoading = false;
            StateHasChanged();
            return;
        }

        if (ImageEditor is not null)
        {
            if (await FileExistsAsync("editorimage.png").ConfigureAwait(false))
            {
                await ImageEditor.LoadImageAsync(Path.Combine(host, "editorimage.png")).ConfigureAwait(false);
            }
            if (await FileExistsAsync("editorjson.json").ConfigureAwait(false))
            {
                var json = await HttpClient.GetStringAsync(Path.Combine(host, "editorjson.json")).ConfigureAwait(false);
                await ImageEditor.LoadJSONAsync(json).ConfigureAwait(false);
            }
        }

        IsLoading = false;
        StateHasChanged();
    }

    private protected async Task<bool> FileExistsAsync(string name)
    {
        if (HttpClient is null)
        {
            return false;
        }
        try
        {
            var response = await HttpClient.GetAsync(
                $"/upload/exists?name={HttpUtility.UrlEncode(name)}")
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task GetObjUrlAsync(string? url)
    {
        if (HttpClient is null
            || string.IsNullOrEmpty(url))
        {
            return;
        }

        try
        {
            var bytes = await HttpClient
                .GetByteArrayAsync(url)
                .ConfigureAwait(false);

            _ = await HttpClient.PostAsync(
                "/upload?name=editorimage.png",
                new ByteArrayContent(bytes))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading image: {ex.Message}");
        }
    }

    private async Task SaveJsonAsync(string? json)
    {
        if (HttpClient is null
            || string.IsNullOrEmpty(json))
        {
            return;
        }

        try
        {
            _ = await HttpClient.PostAsync(
                "/upload?name=editorjson.json",
                new StringContent(json))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading json: {ex.Message}");
        }
    }
}

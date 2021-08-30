using Microsoft.AspNetCore.Mvc;

namespace Tavenem.Blazor.ImageEditor.Sample.Server.Controllers;

[ApiController]
[Route("upload")]
public class FileUploadController : ControllerBase
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly string? _rootPath;

    public FileUploadController(
        IHttpContextAccessor contextAccessor,
        IWebHostEnvironment env)
    {
        _contextAccessor = contextAccessor;
        _rootPath = env.WebRootPath;
    }

    [HttpGet("exists")]
    public IActionResult GetExists([FromQuery] string? name = null)
    {
        if (string.IsNullOrEmpty(_rootPath))
        {
            return new StatusCodeResult(500);
        }

        if (string.IsNullOrEmpty(name))
        {
            return BadRequest();
        }

        return System.IO.File.Exists(Path.Combine(_rootPath, name))
            ? Ok()
            : NotFound();
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (_contextAccessor.HttpContext is null)
        {
            return new StatusCodeResult(500);
        }

        return Ok($"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host.Value}{_contextAccessor.HttpContext.Request.PathBase}");
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromQuery] string? name = null)
    {
        if (string.IsNullOrEmpty(_rootPath)
            || _contextAccessor.HttpContext is null)
        {
            return new StatusCodeResult(500);
        }

        if (string.IsNullOrEmpty(name))
        {
            name = Path.GetRandomFileName();
        }

        var path = Path.Combine(_rootPath, name);
        using (var writer = System.IO.File.OpenWrite(path))
        {
            await Request.Body.CopyToAsync(writer).ConfigureAwait(false);
        }
        return Ok(UriCombine(
            $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host.Value}{_contextAccessor.HttpContext.Request.PathBase}",
            name));
    }

    private static string InternalCombine(string? source, string? dest)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return dest?.TrimStart('/', '\\').Replace('\\', '/')
                ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(dest))
        {
            return source.TrimEnd('/', '\\').Replace('\\', '/');
        }

        return $"{source.TrimEnd('/', '\\').Replace('\\', '/')}/{dest.TrimStart('/', '\\').Replace('\\', '/')}";
    }

    public static string UriCombine(string source, params string?[] args)
        => args.Aggregate(source, InternalCombine);
}

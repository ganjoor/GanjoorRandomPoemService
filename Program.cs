using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddSingleton(
                   HtmlEncoder.Create(allowedRanges: [ UnicodeRanges.BasicLatin,
                    UnicodeRanges.Arabic ]));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Redirect .html requests to extensionless URLs
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.HasValue && path.Value.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
    {
        // Remove .html extension
        var newPath = path.Value[..^5]; // Remove last 5 characters (".html")

        // Special case: root index.html should redirect to home
        if (newPath.Equals("/index", StringComparison.OrdinalIgnoreCase))
        {
            newPath = "/";
        }

        // Preserve query string
        var query = context.Request.QueryString;
        var newUrl = $"{newPath}{query}";

        context.Response.Redirect(newUrl, permanent: true);
        return;
    }

    await next();
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.UseFileServer();
app.UseStaticFiles();


app.Run();

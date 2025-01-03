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

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.UseFileServer();
app.UseStaticFiles();
app.UseDefaultFiles("/index.html");

app.Run();

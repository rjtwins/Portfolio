namespace BCT.Blazor.Services;

using Microsoft.Playwright;
using System.Threading.Tasks;

public class Printer : IPrinter
{
    public async Task<byte[]> PrintDashboard(string baseUrl, string token)
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
        });
        var page = await browser.NewPageAsync(new BrowserNewPageOptions()
        {
             ViewportSize = new ViewportSize
             {
                 Height=1080,
                 Width=1920,
             }
        });
        var fullUrl = $"{baseUrl}project/dashboard/{token}";
        await page.GotoAsync(fullUrl, new() { Timeout = 0 });
        await page.EmulateMediaAsync(new PageEmulateMediaOptions()
        {
            Media = Media.Screen,
            ForcedColors = ForcedColors.Active,
        });

        //await page.WaitForConsoleMessageAsync(new PageWaitForConsoleMessageOptions()
        //{
        //    Predicate = message => message.Text.Contains("ready to print"),
        //});

        Thread.Sleep(5 * 1000);

        var pdfBytes = await page.PdfAsync(new PagePdfOptions()
        {
            Width = "1920px",
            Height = "1080px",
        });

        return pdfBytes;
    }
}

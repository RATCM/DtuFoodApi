using SixLabors.ImageSharp;

namespace DtuFoodAPI.Extensions;

public static class ImageExtensions
{
    public static byte[] ToPng(this Image img)
    {
        using var msOut = new MemoryStream();
        img.SaveAsPng(msOut);
        return msOut.ToArray();
    }
    
    public static async Task<byte[]> ToPngAsync(this Image img, CancellationToken cancellationToken = default)
    {
        using var msOut = new MemoryStream();
        await img.SaveAsPngAsync(msOut, cancellationToken);
        return msOut.ToArray();
    }
}
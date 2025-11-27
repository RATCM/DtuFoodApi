using SixLabors.ImageSharp;

namespace DtuFoodAPI.Extensions;

public static class ByteArrayExtensions
{
    public static async Task<Image> ToImageAsync(this byte[] blob, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream(blob);
        return await Image.LoadAsync(ms, cancellationToken);
    }
    
    public static Image ToImage(this byte[] blob)
    {
        using var ms = new MemoryStream(blob);
        return Image.Load(ms);
    }
}
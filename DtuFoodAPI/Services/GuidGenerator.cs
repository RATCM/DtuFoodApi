namespace DtuFoodAPI.Services;

public class GuidGenerator : IGuidGenerator
{
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }
}

/// <summary>
/// This is useful for testing so we can mock the Guid generator
/// instead of getting a random one every time
/// </summary>
public interface IGuidGenerator
{
    Guid NewGuid();
}
namespace Cherris;

public sealed class ResourceLoader
{
    private static ResourceLoader? _instance;
    public static ResourceLoader Instance => _instance ??= new();

    public static T Load<T>(string path) => Instance.LoadInternal<T>(path);

    public T LoadInternal<T>(string path)
    {
        return typeof(T) switch
        {
            var t when t == typeof(Audio) => (T)(object)new Audio(path),
            var t when t == typeof(Texture) => (T)(object)TextureManager.Instance.Get(path),
            var t when t == typeof(Font) => (T)(object)FontManager.Instance.Get(path),
            _ => throw new InvalidOperationException($"Unsupported resource type: {typeof(T)}")
        };
    }
}
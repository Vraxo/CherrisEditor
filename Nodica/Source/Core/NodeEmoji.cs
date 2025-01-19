namespace Cherris;

public static class NodeEmoji
{
    public static string GetEmojiForNodeType(Node node)
    {
        return node switch
        {
            RayCast => "⚡",
            Timer => "⏰",
            NavigationRegion =>"🗺",
            NavigationAgent => "🧭",
            AudioPlayer => "🔉",
            Collider => "📦",
            Sprite => "🖼",
            Button => "🔘",
            ColorRectangle => "🟥",
            Node2D => "🟩",
            _ => "⭕",
        };
    }
}
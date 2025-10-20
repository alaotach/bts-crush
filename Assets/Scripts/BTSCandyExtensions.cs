using UnityEngine;

/// <summary>
/// Helper extension methods for BTS Candy system
/// Makes it easier to work with the new candy types
/// </summary>
public static class BTSCandyExtensions
{
    /// <summary>
    /// Check if this candy type is a regular member (not a special candy)
    /// </summary>
    public static bool IsRegularMember(this BTSCandyType candyType)
    {
        return candyType == BTSCandyType.RM ||
               candyType == BTSCandyType.Jin ||
               candyType == BTSCandyType.Suga ||
               candyType == BTSCandyType.JHope ||
               candyType == BTSCandyType.Jimin ||
               candyType == BTSCandyType.V ||
               candyType == BTSCandyType.Jungkook;
    }
    
    /// <summary>
    /// Check if this candy type is a special candy (created from matches)
    /// </summary>
    public static bool IsSpecialCandy(this BTSCandyType candyType)
    {
        return !IsRegularMember(candyType);
    }
    
    /// <summary>
    /// Get the member color theme (for visual effects)
    /// </summary>
    public static Color GetMemberColor(this BTSCandyType candyType)
    {
        switch (candyType)
        {
            case BTSCandyType.RM:
                return new Color(0.5f, 0.2f, 0.8f); // Purple
            case BTSCandyType.Jin:
                return new Color(1f, 0.4f, 0.7f); // Pink
            case BTSCandyType.Suga:
                return new Color(0.2f, 0.2f, 0.2f); // Black/Dark
            case BTSCandyType.JHope:
                return new Color(1f, 0.5f, 0.2f); // Orange/Red
            case BTSCandyType.Jimin:
                return new Color(1f, 0.9f, 0.3f); // Yellow
            case BTSCandyType.V:
                return new Color(0.3f, 0.7f, 0.5f); // Green/Blue
            case BTSCandyType.Jungkook:
                return new Color(0.4f, 0.5f, 1f); // Blue/Purple
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Get the member's emoji/animal
    /// </summary>
    public static string GetMemberEmoji(this BTSCandyType candyType)
    {
        switch (candyType)
        {
            case BTSCandyType.RM: return "🐨"; // Koala
            case BTSCandyType.Jin: return "🐹"; // Hamster
            case BTSCandyType.Suga: return "🐱"; // Cat
            case BTSCandyType.JHope: return "🐿️"; // Squirrel
            case BTSCandyType.Jimin: return "🐥"; // Chick
            case BTSCandyType.V: return "🐯"; // Tiger
            case BTSCandyType.Jungkook: return "🐰"; // Bunny
            default: return "⭐";
        }
    }
}

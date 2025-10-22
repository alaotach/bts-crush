using UnityEngine;


public static class BTSCandyExtensions
{

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

    public static bool IsSpecialCandy(this BTSCandyType candyType)
    {
        return !IsRegularMember(candyType);
    }

    public static Color GetMemberColor(this BTSCandyType candyType)
    {
        switch (candyType)
        {
            case BTSCandyType.RM:
                return new Color(0.5f, 0.2f, 0.8f);
            case BTSCandyType.Jin:
                return new Color(1f, 0.4f, 0.7f);
            case BTSCandyType.Suga:
                return new Color(0.2f, 0.2f, 0.2f);
            case BTSCandyType.JHope:
                return new Color(1f, 0.5f, 0.2f);
            case BTSCandyType.Jimin:
                return new Color(1f, 0.9f, 0.3f);
            case BTSCandyType.V:
                return new Color(0.3f, 0.7f, 0.5f); 
            case BTSCandyType.Jungkook:
                return new Color(0.4f, 0.5f, 1f);
            default:
                return Color.white;
        }
    }
    

    public static string GetMemberEmoji(this BTSCandyType candyType)
    {
        switch (candyType)
        {
            case BTSCandyType.RM: return "💜"; 
            case BTSCandyType.Jin: return "💗"; 
            case BTSCandyType.Suga: return "🖤";
            case BTSCandyType.JHope: return "🧡";
            case BTSCandyType.Jimin: return "💛";
            case BTSCandyType.V: return "💚";
            case BTSCandyType.Jungkook: return "💙";
            default: return "⭐";
        }
    }
}

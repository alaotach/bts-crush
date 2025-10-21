using UnityEngine;

/// <summary>
/// BTS-themed candy types for the match-3 game.
/// Main candies are chibi BTS members, special candies from combos.
/// Simplified system matching original Candy Crush mechanics.
/// </summary>
public enum BTSCandyType
{
    // === REGULAR CANDIES (7 BTS Members - Chibi versions) ===
    RM,         // 김남준 - Leader, Purple chibi
    Jin,        // 김석진 - Pink chibi
    Suga,       // 민윤기 - Black/White chibi
    JHope,      // 정호석 - Red/Orange chibi
    Jimin,      // 박지민 - Yellow chibi
    V,          // 김태형 - Green/Blue chibi
    Jungkook,   // 전정국 - Blue/Purple chibi
    
    // === SPECIAL CANDIES (Created from matches) ===
    // These keep the character's identity but have special effects
    
    StripedHorizontal,  // Match-4 vertical → clears entire row (character with horizontal stripes)
    StripedVertical,    // Match-4 horizontal → clears entire column (character with vertical stripes)
    
    Balloon,            // T or L shape → 3x3 explosion (character face on balloon)
    
    Rainbow             // Match-5+ → universal candy (galaxy/rainbow character)
}

/// <summary>
/// Special candy creation conditions
/// </summary>
public enum MatchType
{
    Normal3,            // Regular 3-match → no special candy
    Match4Horizontal,   // 4 in a row (horizontal) → StripedVertical
    Match4Vertical,     // 4 in a column (vertical) → StripedHorizontal
    TShape,             // T shape → Balloon
    LShape,             // L shape → Balloon
    Match5Plus          // 5 or more → Rainbow
}

/// <summary>
/// Data class for BTS candy properties
/// </summary>
[System.Serializable]
public class BTSCandyData
{
    public BTSCandyType candyType;
    public Sprite sprite;
    public string displayName;
    public string description;
    
    [Header("Special Candy Properties")]
    public bool isSpecial = false;
    public MatchType createdFrom;
    public int scoreValue = 10;
    public GameObject explosionEffect;
    public AudioClip activationSound;
    
    [Header("Directional Sprites (for striped candies)")]
    [Tooltip("Sprite with horizontal stripes (for StripedHorizontal)")]
    public Sprite horizontalStripedSprite;
    
    [Tooltip("Sprite with vertical stripes (for StripedVertical)")]
    public Sprite verticalStripedSprite;
    
    [Tooltip("Balloon version sprite (for Balloon special)")]
    public Sprite balloonSprite;
    
    [Tooltip("Rainbow/Galaxy version sprite (for Rainbow special)")]
    public Sprite rainbowSprite;
    
    [Header("Clear Pattern")]
    public ClearPattern clearPattern;
    public int clearRadius = 1;
    public bool clearsAllOfColor = false;
    
    [Header("Animation")]
    public float animationDuration = 0.5f;
    public AnimationCurve scaleCurve;
}

/// <summary>
/// How the candy clears other candies when activated
/// </summary>
public enum ClearPattern
{
    None,               // Regular candy
    Cross,              // + shape (row and column)
    Row,                // Horizontal line
    Column,             // Vertical line
    Area3x3,            // 3x3 square
    Area5x5,            // 5x5 square
    XShape,             // Diagonal X
    AllOfColor,         // All candies of target color
    Wave,               // Expanding wave outward
    Random,             // Random candies
    Path                // Along a path (like butter slide)
}

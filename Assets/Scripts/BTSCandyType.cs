using UnityEngine;

/// <summary>
/// BTS-themed candy types for the match-3 game.
/// Main candies are chibi BTS members, special candies from combos.
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
    
    MicCandy,           // Shoots music notes in a line
    Lightstick,         // ARMY Bomb beam across row/column
    
    AlbumBomb,          // Clears all candies of one member (color bomb)
    StageBomb,          // Clears X-shaped pattern with spotlights
    
    FanHeartBomb,       // ARMY hearts explode outward 3x3
    
    DynamiteCandy,      // "Dy-na-na-na!" - Clears row + column
    ButterSlide,        // "Smooth like butter" - Glides across board
    
    // === EASTER EGG / BONUS CANDIES ===
    FanChant,           // ARMY hands wave, clears nearby
    SugaRap,            // Rhythmic rap wave pattern
    RMInspire           // RM wisdom - turns candies into specials
}

/// <summary>
/// Special candy creation conditions
/// </summary>
public enum MatchType
{
    Normal3,            // Regular 3-match
    Row4,               // 4 in a row (horizontal)
    Column4,            // 4 in a column (vertical)
    Row5Plus,           // 5+ in a row
    Column5Plus,        // 5+ in a column
    LShape,             // L or T shape
    Square2x2,          // 2x2 square
    Match6Plus,         // 6 or more in any combination
    SpecialShape        // Other special patterns
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

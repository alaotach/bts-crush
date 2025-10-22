using UnityEngine;


public enum BTSCandyType
{
    RM, 
    Jin, 
    Suga, 
    JHope,
    Jimin,  
    V, 
    Jungkook, 
    
    
    StripedHorizontal, 
    StripedVertical,
    
    Balloon,
    
    ColorBomb, 
    SuperBomb
}


public enum MatchType
{
    Normal3, 
    Match4Horizontal, 
    Match4Vertical, 
    TShape, 
    LShape, 
    Match5, 
    Match6Plus
}


[System.Serializable]
public class BTSCandyData
{
    public BTSCandyType candyType;
    public Sprite sprite;
    public string displayName;
    
    public bool isSpecial = false;
    public int scoreValue = 10;
    
    public Sprite horizontalStripedSprite;
    
    public Sprite verticalStripedSprite;
    
    public Sprite balloonSprite;
    
    public Sprite colorBombSprite;
    
    public Sprite superBombSprite;
}

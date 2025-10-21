using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically creates visual effects for special candies
/// No need for separate sprite files - this generates them on the fly!
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpecialCandyVisualizer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GameObject stripesOverlay;
    private GameObject balloonBase;
    private GameObject rainbowEffects;
    
    [Header("Stripe Settings")]
    public Color stripeColor = new Color(1f, 1f, 1f, 0.7f); // White, 70% opacity
    public int numberOfStripes = 4;
    public float stripeThickness = 0.15f; // Relative to sprite size
    
    [Header("Balloon Settings")]
    public Sprite balloonSprite; // Assign a generic balloon sprite
    public Color balloonTint = Color.white;
    
    [Header("Rainbow Settings")]
    public bool animateRainbow = true;
    public float rainbowSpeed = 1f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    /// <summary>
    /// Apply horizontal stripes to the current sprite
    /// </summary>
    public void ApplyHorizontalStripes()
    {
        ClearEffects();
        CreateStripeOverlay(true);
    }
    
    /// <summary>
    /// Apply vertical stripes to the current sprite
    /// </summary>
    public void ApplyVerticalStripes()
    {
        ClearEffects();
        CreateStripeOverlay(false);
    }
    
    /// <summary>
    /// Apply balloon effect (puts character on a balloon)
    /// </summary>
    public void ApplyBalloon(Color memberColor)
    {
        ClearEffects();
        CreateBalloonEffect(memberColor);
    }
    
    /// <summary>
    /// Apply rainbow effect (colorful aura)
    /// </summary>
    public void ApplyRainbow()
    {
        ClearEffects();
        CreateRainbowEffect();
    }
    
    /// <summary>
    /// Remove all special effects, return to normal
    /// </summary>
    public void ClearEffects()
    {
        if (stripesOverlay != null)
        {
            Destroy(stripesOverlay);
            stripesOverlay = null;
        }
        
        if (balloonBase != null)
        {
            Destroy(balloonBase);
            balloonBase = null;
        }
        
        if (rainbowEffects != null)
        {
            Destroy(rainbowEffects);
            rainbowEffects = null;
        }
    }
    
    /// <summary>
    /// Create stripe overlay using Unity UI or additional SpriteRenderer
    /// </summary>
    private void CreateStripeOverlay(bool horizontal)
    {
        stripesOverlay = new GameObject("Stripes");
        stripesOverlay.transform.SetParent(transform);
        stripesOverlay.transform.localPosition = new Vector3(0, 0, -0.01f); // Slightly in front
        stripesOverlay.transform.localRotation = Quaternion.identity;
        
        // Get the base sprite's dimensions
        if (spriteRenderer.sprite != null)
        {
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            float spriteWidth = spriteBounds.size.x;
            float spriteHeight = spriteBounds.size.y;
            
            // Create a texture with stripes matching the sprite size
            Texture2D stripesTexture = CreateStripesTexture(horizontal, 256, 256);
            Sprite stripesSprite = Sprite.Create(
                stripesTexture,
                new Rect(0, 0, stripesTexture.width, stripesTexture.height),
                new Vector2(0.5f, 0.5f),
                spriteRenderer.sprite.pixelsPerUnit // Match the base sprite's PPU
            );
            
            SpriteRenderer stripesRenderer = stripesOverlay.AddComponent<SpriteRenderer>();
            stripesRenderer.sprite = stripesSprite;
            stripesRenderer.color = stripeColor;
            stripesRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
            stripesRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            
            // Scale the overlay to match the base sprite's size exactly
            stripesOverlay.transform.localScale = new Vector3(
                spriteWidth / stripesSprite.bounds.size.x,
                spriteHeight / stripesSprite.bounds.size.y,
                1f
            );
        }
        else
        {
            // Fallback if no sprite
            stripesOverlay.transform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// Create a texture with horizontal or vertical stripes
    /// </summary>
    private Texture2D CreateStripesTexture(bool horizontal, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;
        
        int stripeWidth = horizontal ? (height / numberOfStripes) : (width / numberOfStripes);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Determine if this pixel is in a stripe
                bool isStripe = false;
                
                if (horizontal)
                {
                    // Horizontal stripes (check y position)
                    int stripeIndex = y / stripeWidth;
                    isStripe = (stripeIndex % 2 == 0);
                }
                else
                {
                    // Vertical stripes (check x position)
                    int stripeIndex = x / stripeWidth;
                    isStripe = (stripeIndex % 2 == 0);
                }
                
                // Set pixel color
                if (isStripe)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return texture;
    }
    
    /// <summary>
    /// Create balloon effect by adding a balloon sprite behind the character
    /// </summary>
    private void CreateBalloonEffect(Color memberColor)
    {
        if (balloonSprite == null)
        {
            Debug.LogWarning("No balloon sprite assigned! Creating simple circle instead.");
            CreateSimpleBalloon(memberColor);
            return;
        }
        
        balloonBase = new GameObject("Balloon");
        balloonBase.transform.SetParent(transform);
        balloonBase.transform.localPosition = new Vector3(0, 0, 0.01f); // Behind character
        balloonBase.transform.localRotation = Quaternion.identity;
        balloonBase.transform.localScale = Vector3.one * 1.2f; // Slightly larger
        
        SpriteRenderer balloonRenderer = balloonBase.AddComponent<SpriteRenderer>();
        balloonRenderer.sprite = balloonSprite;
        balloonRenderer.color = memberColor;
        balloonRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        balloonRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        
        // Make character sprite slightly smaller to fit on balloon
        transform.localScale = transform.localScale * 0.7f;
    }
    
    /// <summary>
    /// Create a simple circle balloon if no sprite is assigned
    /// </summary>
    private void CreateSimpleBalloon(Color memberColor)
    {
        balloonBase = new GameObject("SimpleBalloon");
        balloonBase.transform.SetParent(transform);
        balloonBase.transform.localPosition = new Vector3(0, 0, 0.01f);
        balloonBase.transform.localRotation = Quaternion.identity;
        balloonBase.transform.localScale = Vector3.one * 1.2f;
        
        // Create a simple circular texture
        Texture2D circleTexture = CreateCircleTexture(256, memberColor);
        Sprite circleSprite = Sprite.Create(
            circleTexture,
            new Rect(0, 0, circleTexture.width, circleTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        SpriteRenderer balloonRenderer = balloonBase.AddComponent<SpriteRenderer>();
        balloonRenderer.sprite = circleSprite;
        balloonRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        balloonRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        
        transform.localScale = transform.localScale * 0.7f;
    }
    
    /// <summary>
    /// Create a circular texture for simple balloon
    /// </summary>
    private Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance < radius)
                {
                    // Inside circle - create gradient
                    float gradient = 1f - (distance / radius) * 0.3f; // Lighter at center
                    Color pixelColor = color * gradient;
                    pixelColor.a = 1f;
                    texture.SetPixel(x, y, pixelColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return texture;
    }
    
    /// <summary>
    /// Create rainbow effect with particle system or glow
    /// </summary>
    private void CreateRainbowEffect()
    {
        rainbowEffects = new GameObject("RainbowGlow");
        rainbowEffects.transform.SetParent(transform);
        rainbowEffects.transform.localPosition = Vector3.zero;
        rainbowEffects.transform.localRotation = Quaternion.identity;
        rainbowEffects.transform.localScale = Vector3.one * 1.3f;
        
        // Create a sprite with rainbow glow
        Texture2D glowTexture = CreateRainbowGlowTexture(256);
        Sprite glowSprite = Sprite.Create(
            glowTexture,
            new Rect(0, 0, glowTexture.width, glowTexture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        SpriteRenderer glowRenderer = rainbowEffects.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = glowSprite;
        glowRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        
        // Add animation component if desired
        if (animateRainbow)
        {
            RainbowAnimator animator = rainbowEffects.AddComponent<RainbowAnimator>();
            animator.speed = rainbowSpeed;
        }
    }
    
    /// <summary>
    /// Create a rainbow glow texture
    /// </summary>
    private Texture2D CreateRainbowGlowTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxRadius = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance < maxRadius)
                {
                    // Calculate rainbow color based on angle
                    float angle = Mathf.Atan2(y - center.y, x - center.x);
                    float hue = (angle / (Mathf.PI * 2f)) + 0.5f; // 0 to 1
                    
                    Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
                    
                    // Fade out at edges
                    float alpha = 1f - (distance / maxRadius);
                    rainbowColor.a = alpha * 0.6f; // 60% opacity
                    
                    texture.SetPixel(x, y, rainbowColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return texture;
    }
    
    private void OnDestroy()
    {
        ClearEffects();
    }
}

/// <summary>
/// Simple component to animate rainbow glow by rotating hue
/// </summary>
public class RainbowAnimator : MonoBehaviour
{
    public float speed = 1f;
    private SpriteRenderer spriteRenderer;
    private float hueOffset = 0f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        hueOffset += Time.deltaTime * speed;
        if (hueOffset > 1f) hueOffset -= 1f;
        
        // Shift the color over time
        Color.RGBToHSV(spriteRenderer.color, out float h, out float s, out float v);
        h = (h + Time.deltaTime * speed * 0.1f) % 1f;
        spriteRenderer.color = Color.HSVToRGB(h, s, v);
    }
}

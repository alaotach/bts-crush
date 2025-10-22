using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(SpriteRenderer))]
public class SpecialCandyVisualizer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GameObject stripesOverlay;
    private GameObject balloonBase;
    private GameObject rainbowEffects;
    
    public Color stripeColor = new Color(1f, 1f, 1f, 0.7f); 
    public int numberOfStripes = 4;
    public float stripeThickness = 0.15f;
    
    public Sprite balloonSprite;
    public Color balloonTint = Color.white;
    
    public bool animateRainbow = true;
    public float rainbowSpeed = 1f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void ApplyHorizontalStripes()
    {
        ClearEffects();
        CreateStripeOverlay(true);
    }
    
    public void ApplyVerticalStripes()
    {
        ClearEffects();
        CreateStripeOverlay(false);
    }
 
    public void ApplyBalloon(Color memberColor)
    {
        ClearEffects();
        CreateBalloonEffect(memberColor);
    }

    public void ApplyRainbow()
    {
        ClearEffects();
        CreateRainbowEffect();
    }
    
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

    private void CreateStripeOverlay(bool horizontal)
    {
        stripesOverlay = new GameObject("Stripes");
        stripesOverlay.transform.SetParent(transform);
        stripesOverlay.transform.localPosition = new Vector3(0, 0, -0.01f); // Slightly in front
        stripesOverlay.transform.localRotation = Quaternion.identity;
        if (spriteRenderer.sprite != null)
        {
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            float spriteWidth = spriteBounds.size.x;
            float spriteHeight = spriteBounds.size.y;
            Texture2D stripesTexture = CreateStripesTexture(horizontal, 256, 256);
            Sprite stripesSprite = Sprite.Create(
                stripesTexture,
                new Rect(0, 0, stripesTexture.width, stripesTexture.height),
                new Vector2(0.5f, 0.5f),
                spriteRenderer.sprite.pixelsPerUnit
            );
            
            SpriteRenderer stripesRenderer = stripesOverlay.AddComponent<SpriteRenderer>();
            stripesRenderer.sprite = stripesSprite;
            stripesRenderer.color = stripeColor;
            stripesRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
            stripesRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            stripesOverlay.transform.localScale = new Vector3(
                spriteWidth / stripesSprite.bounds.size.x,
                spriteHeight / stripesSprite.bounds.size.y,
                1f
            );
        }
        else
        {
            stripesOverlay.transform.localScale = Vector3.one;
        }
    }

    private Texture2D CreateStripesTexture(bool horizontal, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Bilinear;
        
        int stripeWidth = horizontal ? (height / numberOfStripes) : (width / numberOfStripes);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isStripe = false;
                
                if (horizontal)
                {
                    int stripeIndex = y / stripeWidth;
                    isStripe = (stripeIndex % 2 == 0);
                }
                else
                {
                    int stripeIndex = x / stripeWidth;
                    isStripe = (stripeIndex % 2 == 0);
                }
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
    

    private void CreateBalloonEffect(Color memberColor)
    {
        if (balloonSprite == null)
        {
            CreateSimpleBalloon(memberColor);
            return;
        }
        
        balloonBase = new GameObject("Balloon");
        balloonBase.transform.SetParent(transform);
        balloonBase.transform.localPosition = new Vector3(0, 0, 0.01f); 
        balloonBase.transform.localRotation = Quaternion.identity;
        balloonBase.transform.localScale = Vector3.one * 1.2f; 
        
        SpriteRenderer balloonRenderer = balloonBase.AddComponent<SpriteRenderer>();
        balloonRenderer.sprite = balloonSprite;
        balloonRenderer.color = memberColor;
        balloonRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        balloonRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        transform.localScale = transform.localScale * 0.7f;
    }
    
    private void CreateSimpleBalloon(Color memberColor)
    {
        balloonBase = new GameObject("SimpleBalloon");
        balloonBase.transform.SetParent(transform);
        balloonBase.transform.localPosition = new Vector3(0, 0, 0.01f);
        balloonBase.transform.localRotation = Quaternion.identity;
        balloonBase.transform.localScale = Vector3.one * 1.2f;
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
                    float gradient = 1f - (distance / radius) * 0.3f;
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
    
    private void CreateRainbowEffect()
    {
        rainbowEffects = new GameObject("RainbowGlow");
        rainbowEffects.transform.SetParent(transform);
        rainbowEffects.transform.localPosition = Vector3.zero;
        rainbowEffects.transform.localRotation = Quaternion.identity;
        rainbowEffects.transform.localScale = Vector3.one * 1.3f;
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
        if (animateRainbow)
        {
            RainbowAnimator animator = rainbowEffects.AddComponent<RainbowAnimator>();
            animator.speed = rainbowSpeed;
        }
    }

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
                    float angle = Mathf.Atan2(y - center.y, x - center.x);
                    float hue = (angle / (Mathf.PI * 2f)) + 0.5f; 
                    
                    Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
                    float alpha = 1f - (distance / maxRadius);
                    rainbowColor.a = alpha * 0.6f; 
                    
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
        
        Color.RGBToHSV(spriteRenderer.color, out float h, out float s, out float v);
        h = (h + Time.deltaTime * speed * 0.1f) % 1f;
        spriteRenderer.color = Color.HSVToRGB(h, s, v);
    }
}

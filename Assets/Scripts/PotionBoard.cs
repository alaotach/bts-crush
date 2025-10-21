using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PotionBoard : MonoBehaviour
{
    //define the size of the board
    public int width = 6;
    public int height = 8;
    //define the distance between potions
    public float potionSpacingX = 1.5f;
    public float potionSpacingY = 1.5f;
    
    // Automatic board fitting
    [Header("Auto-Fit Settings")]
    [Tooltip("Enable automatic board scaling to fit any screen size")]
    [SerializeField] public bool autoFitToScreen = true;
    [Tooltip("Are spacing values in pixels? (Values > 10 suggest pixels). Auto-converts to world units.")]
    public bool spacingInPixels = false;
    [Tooltip("Size of each potion in the same units as spacing. Used to calculate total space needed.")]
    public float potionSize = 200f; // Approximate diameter of potion sprites in pixels
    [Tooltip("Padding around the board as percentage of screen (0.1 = 10%)")]
    public float screenPaddingPercent = 0.1f; // 10% padding on each side
    public Vector2 manualBoardOffset = new Vector2(0, -50f); // Used when autoFit is disabled
    public float manualPotionParentScale = 1.2f; // Used when autoFit is disabled
    
    // Calculated values (visible in inspector for debugging)
    [Header("Calculated Values (Read-Only)")]
    [SerializeField] private Vector2 boardOffset; // Offset from center
    [SerializeField] private float potionParentScale; // Scale multiplier
    
    // Auto-scale based on screen orientation
    public bool autoAdjustForOrientation = true;
    public float portraitScale = 0.8f; // Scale multiplier for portrait mode (legacy, not used if autoFit is on)
    public Vector2 portraitOffset = new Vector2(0, 100f); // Different offset for portrait (legacy)
    
    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;
    
    [Header("Board Initialization")]
    [Tooltip("If true, allows initial matches and clears them (FASTER, prevents freeze)")]
    public bool allowInitialMatches = true;
    [Tooltip("BTS Candy Database with sprites and properties")]
    public BTSCandyDatabase candyDatabase;
    [Tooltip("Special Candy Manager for activating special effects")]
    public BTSSpecialCandyManager specialCandyManager;
    
    //get a reference to the collection nodes potionBoard + GO
    public Node[,] potionBoard;
    public GameObject potionBoardGO;
    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;

    [SerializeField]
    private Potion selectedPotion;

    [SerializeField]
    private bool isProcessingMove;

    //layoutArray
    public ArrayLayout arrayLayout;
    //public static of potionboard
    public static PotionBoard Instance;
    
    private int recursionCount = 0;
    private const int MAX_RECURSION = 10;
    
    private bool isBoardInitialized = false;

    [SerializeField]
    List<Potion> potionsToRemove = new();
    
    private struct SpecialCandySpawn
    {
        public int x;
        public int y;
        public MatchType matchType;
        public BTSCandyType originalType;
    }
    private List<SpecialCandySpawn> specialCandiesToSpawn = new();
    
    public TMPro.TMP_Text shuffleText; // Reference to UI text for shuffle notification
    private bool isShuffling = false;
    
    public GameObject selectionIndicator; // A square sprite to show selected potion
    public float blinkSpeed = 0.5f; // Time between blinks
    private Coroutine blinkCoroutine;
    private Vector3 indicatorOriginalScale; // Store original scale
    
    private Vector2 dragStartPos;
    private Potion dragStartPotion;
    private bool isDragging = false;
    private float dragThreshold = 0.5f; // Minimum distance to trigger swap

    private void Awake()
    {
        Instance = this;

        // 🐲 ROBUST-FIX: Find the SpecialCandyManager
        if (specialCandyManager == null)
        {
            Debug.LogWarning("SpecialCandyManager not assigned in Inspector. Searching...");
            
            // 1. Try to get it from this GameObject first.
            specialCandyManager = GetComponent<BTSSpecialCandyManager>();
            if (specialCandyManager != null)
            {
                Debug.Log("✓ Found BTSSpecialCandyManager on the same GameObject!");
            }
            else
            {
                // 2. If not on this GameObject, search the whole scene.
                Debug.Log("Component not found on this object. Searching entire scene...");
                specialCandyManager = FindObjectOfType<BTSSpecialCandyManager>();
                if (specialCandyManager != null)
                {
                    Debug.Log($"✓ Found BTSSpecialCandyManager on a different object: {specialCandyManager.gameObject.name}");
                }
                else
                {
                    Debug.LogError("❌ CRITICAL: Could not find an active BTSSpecialCandyManager component anywhere in the scene!");
                }
            }
        }
        else
        {
            Debug.Log("✓ SpecialCandyManager was already assigned in the Inspector.");
        }
        
        if (selectionIndicator != null)
        {
            indicatorOriginalScale = selectionIndicator.transform.localScale;
            Debug.Log($"Selection indicator original scale saved: {indicatorOriginalScale}");
        }
        
        if (autoFitToScreen)
        {
            Debug.Log("[Auto-Fit] Enabled - calculating optimal board transform...");
            CalculateOptimalBoardTransform();
        }
        else
        {
            Debug.Log("[Manual Mode] Using manual board settings...");
            boardOffset = manualBoardOffset;
            potionParentScale = manualPotionParentScale;
            
            // If manual scale is 0 or very small, use a default
            if (potionParentScale < 0.01f)
            {
                Debug.LogWarning("Manual scale too small, using default 0.65");
                potionParentScale = 0.65f;
            }
            
            if (autoAdjustForOrientation)
            {
                AdjustForScreenOrientation();
            }
        }
        
        ApplyBoardTransform();
    }
    
    /// <summary>
    /// Automatically calculates the optimal scale and offset for the board
    /// to fit perfectly on screen regardless of board size or device
    /// </summary>
    private void CalculateOptimalBoardTransform()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No main camera found! Using default values.");
            boardOffset = manualBoardOffset;
            potionParentScale = manualPotionParentScale;
            return;
        }
        
        // Auto-detect if spacing is in pixels (values > 10 are likely pixels)
        bool usingPixels = spacingInPixels || potionSpacingX > 10f || potionSpacingY > 10f;
        
        if (usingPixels && !spacingInPixels)
        {
            Debug.LogWarning($"⚠️ Auto-detected pixel-based spacing: X={potionSpacingX}, Y={potionSpacingY}");
            Debug.LogWarning($"⚠️ Enable 'Spacing In Pixels' checkbox or reduce values to 0.5-2.0 for world units");
        }
        
        float effectiveSpacingX = potionSpacingX;
        float effectiveSpacingY = potionSpacingY;
        float effectivePotionSize = potionSize;
        
        if (usingPixels)
        {
            // For pixel-based layouts, we need a different conversion factor
            // Typical sprite PPU is 100, but for UI/board layouts, we might need adjustment
            float pixelsPerUnit = 100f;
            
            // If values are VERY large (>100), they're probably meant for a different scale
            if (potionSpacingX > 100f || potionSize > 100f)
            {
                // These look like they're meant for a 1:1 or 10:1 pixel scale
                // Let's use a more aggressive conversion
                pixelsPerUnit = 200f; // More aggressive scaling
                Debug.Log($"[Large Pixel Values] Using adjusted conversion: {pixelsPerUnit} PPU");
            }
            
            effectiveSpacingX = potionSpacingX / pixelsPerUnit;
            effectiveSpacingY = potionSpacingY / pixelsPerUnit;
            effectivePotionSize = potionSize / pixelsPerUnit;
            Debug.Log($"[Pixel Conversion] Spacing: {potionSpacingX}px → {effectiveSpacingX} units, Size: {potionSize}px → {effectivePotionSize} units");
        }
        
        // The spacing should be the gap BETWEEN potions, so total cell size is potion + spacing
        float cellSizeX = effectivePotionSize + effectiveSpacingX;
        float cellSizeY = effectivePotionSize + effectiveSpacingY;
        
        float boardWidthLocal = (width - 1) * cellSizeX + effectivePotionSize; // Add one potion size for the last column
        float boardHeightLocal = (height - 1) * cellSizeY + effectivePotionSize; // Add one potion size for the last row
        
        Debug.Log($"[Board Calculation] Cell size: {cellSizeX}x{cellSizeY}, Board size: {boardWidthLocal}x{boardHeightLocal}");
        
        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;
        
        float availableWidth = screenWidth * (1f - screenPaddingPercent * 2f);
        float availableHeight = screenHeight * (1f - screenPaddingPercent * 2f);
        
        float scaleForWidth = availableWidth / boardWidthLocal;
        float scaleForHeight = availableHeight / boardHeightLocal;
        
        potionParentScale = Mathf.Min(scaleForWidth, scaleForHeight);
        
        // Safety check: if scale is too small, warn and clamp
        if (potionParentScale < 0.01f)
        {
            Debug.LogError($"⚠️ CRITICAL: Calculated scale is too small ({potionParentScale:F4})!");
            Debug.LogError($"⚠️ Board dimensions are too large for screen: {boardWidthLocal:F2}x{boardHeightLocal:F2}");
            Debug.LogError($"⚠️ Screen available: {availableWidth:F2}x{availableHeight:F2}");
            Debug.LogError($"⚠️ Check your spacing and potion size values!");
            potionParentScale = 0.1f; // Minimum sensible scale
        }
        
        float scaledBoardWidth = boardWidthLocal * potionParentScale;
        float scaledBoardHeight = boardHeightLocal * potionParentScale;
        
        // You can adjust this if you want the board positioned differently
        boardOffset = Vector2.zero; // Centered by default
        
        // Optional: Shift down a bit to leave room for UI at top
        float topUISpace = screenHeight * 0.15f; // Reserve 15% at top for score/moves
        boardOffset.y = -topUISpace / 2f;
        
        Debug.Log($"[Auto-Fit] Screen: {screenWidth:F2}x{screenHeight:F2}, " +
                  $"Board: {boardWidthLocal:F2}x{boardHeightLocal:F2}, " +
                  $"Scale: {potionParentScale:F2}, Offset: {boardOffset}");
    }
    
    /// <summary>
    /// Applies the calculated transform to the potionParent
    /// </summary>
    private void ApplyBoardTransform()
    {
        if (potionParent != null)
        {
            potionParent.transform.localPosition = new Vector3(boardOffset.x, boardOffset.y, 0);
            potionParent.transform.localScale = Vector3.one * potionParentScale;
            Debug.Log($"[Board Transform Applied]");
            Debug.Log($"  Position: {potionParent.transform.localPosition}");
            Debug.Log($"  Scale: {potionParent.transform.localScale}");
            Debug.Log($"  World Position: {potionParent.transform.position}");
            
            // CRITICAL CHECK: If scale is near zero, potions will be invisible/stacked
            if (potionParentScale < 0.01f)
            {
                Debug.LogError($"⚠️ SCALE TOO SMALL! Potions will appear stacked at origin!");
                Debug.LogError($"⚠️ Calculated scale: {potionParentScale}");
                Debug.LogError($"⚠️ Try: Disable 'Auto Fit To Screen' or reduce spacing values");
            }
        }
        else
        {
            Debug.LogError("[Board Transform] potionParent is NULL!");
        }
    }
    
    /// <summary>
    /// Public method to recalculate and apply board transform.
    /// Call this when changing board dimensions or loading new levels.
    /// </summary>
    public void RecalculateBoardTransform()
    {
        if (autoFitToScreen)
        {
            CalculateOptimalBoardTransform();
        }
        else
        {
            boardOffset = manualBoardOffset;
            potionParentScale = manualPotionParentScale;
        }
        ApplyBoardTransform();
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Context menu button to test recalculation in the Inspector
    /// Right-click on the component → Recalculate Board Transform
    /// </summary>
    [ContextMenu("Recalculate Board Transform")]
    private void RecalculateFromInspector()
    {
        RecalculateBoardTransform();
        Debug.Log("Board transform recalculated from Inspector!");
    }
    
    /// <summary>
    /// Context menu to test different board sizes
    /// </summary>
    [ContextMenu("Test: Small Board (6x8)")]
    private void TestSmallBoard()
    {
        width = 6;
        height = 8;
        RecalculateBoardTransform();
    }
    
    [ContextMenu("Test: Medium Board (8x10)")]
    private void TestMediumBoard()
    {
        width = 8;
        height = 10;
        RecalculateBoardTransform();
    }
    
    [ContextMenu("Test: Large Board (10x12)")]
    private void TestLargeBoard()
    {
        width = 10;
        height = 12;
        RecalculateBoardTransform();
    }
    
    [ContextMenu("Quick Fix: Use Sensible Defaults")]
    private void QuickFixUseSensibleDefaults()
    {
        Debug.Log("Applying sensible default values...");
        
        spacingInPixels = false;
        
        potionSpacingX = 1.5f;
        potionSpacingY = 1.5f;
        potionSize = 1.2f;
        
        manualBoardOffset = new Vector2(0, -1);
        manualPotionParentScale = 0.65f;
        
        // Recalculate based on current auto-fit setting
        if (Application.isPlaying)
        {
            RecalculateBoardTransform();
        }
        
        Debug.Log("✓ Applied defaults:");
        Debug.Log($"  - Auto-Fit: {autoFitToScreen}");
        Debug.Log($"  - Spacing: {potionSpacingX} x {potionSpacingY}");
        Debug.Log("✓ Board should now be visible!");
        Debug.Log("✓ You may need to restart Play mode for the board to regenerate");
    }
    
    [ContextMenu("Fix: Keep Current Spacing, Adjust Scale")]
    private void FixKeepSpacingAdjustScale()
    {
        Debug.Log("Keeping your spacing values, adjusting scale...");
        
        spacingInPixels = false;
        
        // With spacing of 230, we need a very small scale
        manualBoardOffset = new Vector2(0, -20);
        manualPotionParentScale = 0.02f; // Very small for large spacing values
        
        // Recalculate based on current auto-fit setting
        if (Application.isPlaying)
        {
            RecalculateBoardTransform();
        }
        
        Debug.Log($"✓ Auto-Fit: {autoFitToScreen}");
        Debug.Log($"✓ Using spacing: {potionSpacingX} x {potionSpacingY}");
        Debug.Log("✓ You may need to restart Play mode for the board to regenerate");
    }
    
    [ContextMenu("Toggle: Enable Auto-Fit")]
    private void EnableAutoFit()
    {
        autoFitToScreen = true;
        Debug.Log("✓ Auto-Fit ENABLED. Board will automatically scale to fit screen.");
        
        if (Application.isPlaying)
        {
            RecalculateBoardTransform();
        }
    }
    
    [ContextMenu("Toggle: Disable Auto-Fit (Use Manual)")]
    private void DisableAutoFit()
    {
        autoFitToScreen = false;
        Debug.Log("✓ Auto-Fit DISABLED. Using manual scale and offset values.");
        
        if (Application.isPlaying)
        {
            RecalculateBoardTransform();
        }
    }
#endif
    
    private void AdjustForScreenOrientation()
    {
        bool isPortrait = Screen.height > Screen.width;
        
        if (isPortrait)
        {
            potionParentScale = portraitScale;
            boardOffset = portraitOffset;
            Debug.Log($"[Orientation] Portrait mode detected. Scale: {potionParentScale}, Offset: {boardOffset}");
        }
        else
        {
            Debug.Log($"[Orientation] Landscape mode detected.");
        }
    }
    
    /// <summary>
    /// Helper method to get the cell size (potion + spacing) in world units
    /// </summary>
    private Vector2 GetCellSize()
    {
        // SIMPLIFIED: Just use the spacing values directly as-is
        // The potionParent scale will handle any necessary scaling
        return new Vector2(potionSpacingX, potionSpacingY);
    }
    
    /// <summary>
    /// Helper method to calculate world position for a grid position
    /// </summary>
    public Vector3 GetWorldPositionForCell(int x, int y)
    {
        if (potionParent == null)
        {
            Debug.LogError("[GetWorldPositionForCell] potionParent is NULL!");
            return Vector3.zero;
        }
        
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        Vector3 localPos = new Vector3(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY, 0);
        return potionParent.transform.TransformPoint(localPos);
    }
    
    /// <summary>
    /// Helper method to calculate local position for a grid position
    /// </summary>
    private Vector2 GetLocalPositionForCell(int x, int y)
    {
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        return new Vector2(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY);
    }
    
    /// <summary>
    /// Apply sprite from BTS Candy Database to a candy GameObject
    /// </summary>
    private void ApplyCandySprite(GameObject candyObject, BTSCandyType candyType)
    {
        if (candyDatabase == null)
        {
            Debug.LogWarning("BTSCandyDatabase not assigned! Sprites won't be updated.");
            return;
        }
        
        BTSCandyData candyData = candyDatabase.GetCandyData(candyType);
        if (candyData == null || candyData.sprite == null)
        {
            Debug.LogWarning($"No sprite found for candy type: {candyType}");
            return;
        }
        
        SpriteRenderer spriteRenderer = candyObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = candyData.sprite;
            Debug.Log($"✓ Applied sprite for {candyType}: {candyData.sprite.name}");
        }
        else
        {
            Debug.LogWarning($"No SpriteRenderer found on candy object for {candyType}");
        }
    }
    
    /// <summary>
    /// Apply a color tint to special candies to show their base color
    /// This helps players see which member the special candy can match with
    /// </summary>
    private void ApplyColorTintToSpecialCandy(GameObject candyObject, BTSCandyType baseColor)
    {
        SpriteRenderer spriteRenderer = candyObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        
        Color tintColor = GetMemberTintColor(baseColor);
        
        spriteRenderer.color = tintColor;
        
        Debug.Log($"Applied {baseColor} tint to special candy: {tintColor}");
    }
    
    /// <summary>
    /// Get a subtle tint color for each BTS member
    /// These colors will overlay on the special candy sprite
    /// </summary>
    private Color GetMemberTintColor(BTSCandyType member)
    {
        switch (member)
        {
            case BTSCandyType.RM:
                return new Color(1f, 0.5f, 1f, 1f); // Purple tint
                
            case BTSCandyType.Jin:
                return new Color(1f, 0.7f, 0.85f, 1f); // Pink tint
                
            case BTSCandyType.Suga:
                return new Color(0.8f, 0.8f, 0.8f, 1f); // Gray tint
                
            case BTSCandyType.JHope:
                return new Color(1f, 0.8f, 0.5f, 1f); // Orange tint
                
            case BTSCandyType.Jimin:
                return new Color(1f, 1f, 0.6f, 1f); // Yellow tint
                
            case BTSCandyType.V:
                return new Color(0.6f, 1f, 0.7f, 1f); // Green tint
                
            case BTSCandyType.Jungkook:
                return new Color(0.7f, 0.85f, 1f, 1f); // Blue tint
                
            default:
                return Color.white; // No tint
        }
    }

    void Start()
    {
        if (potionParent == null)
        {
            Debug.LogError("potionParent is NULL! Please assign the 'Potions' GameObject in the Inspector.");
        }
        
        if (potionBoardGO == null)
        {
            Debug.LogError("potionBoardGO is NULL! Please assign the 'PotionBoard' GameObject in the Inspector.");
        }
        
        // Don't initialize GameManager here - let LevelManager or Inspector values handle it
        // GameManager should already be initialized before PotionBoard starts
        
        StartCoroutine(InitializeBoardCoroutine());
    }

    private void Update()
    {
        HandleInput();
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// In the editor, recalculate when board dimensions change
    /// </summary>
    private void OnValidate()
    {
        // Only recalculate in Play mode if autoFit is enabled
        if (Application.isPlaying && autoFitToScreen && potionParent != null)
        {
            CalculateOptimalBoardTransform();
            ApplyBoardTransform();
        }
    }
#endif
    
    private void HandleInput()
    {
        if (Mouse.current == null) return;
        
        // Don't allow input until board is fully initialized
        if (!isBoardInitialized) return;
        
        // Don't allow input if game has ended
        if (GameManager.instance != null && GameManager.instance.isGameEnded)
        {
            return;
        }
        
        if (isProcessingMove)
        {
            Debug.Log("⛔ Input blocked: isProcessingMove = true");
            return;
        }
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        // Mouse button pressed - start drag or select
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hit.collider != null)
            {
                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                if (potion != null && !potion.isMatched) // Also check if not already matched
                {
                    dragStartPotion = potion;
                    dragStartPos = mousePosition;
                    isDragging = true;
                    SelectPotion(potion);
                }
            }
        }
        
        // Mouse button held - check for drag
        if (Mouse.current.leftButton.isPressed && isDragging && dragStartPotion != null)
        {
            Vector2 currentPos = mousePosition;
            float dragDistance = Vector2.Distance(dragStartPos, currentPos);
            
            if (dragDistance > dragThreshold * 100f) // Scale threshold for screen space
            {
                // Determine drag direction
                Vector2 dragDir = (currentPos - dragStartPos).normalized;
                
                int targetX = dragStartPotion.xIndex;
                int targetY = dragStartPotion.yIndex;
                
                if (Mathf.Abs(dragDir.x) > Mathf.Abs(dragDir.y))
                {
                    // Horizontal drag
                    targetX += dragDir.x > 0 ? 1 : -1;
                }
                else
                {
                    // Vertical drag
                    targetY += dragDir.y > 0 ? 1 : -1;
                }
                
                if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
                {
                    if (potionBoard[targetX, targetY] != null && potionBoard[targetX, targetY].isUsable && potionBoard[targetX, targetY].potion != null)
                    {
                        Potion targetPotion = potionBoard[targetX, targetY].potion.GetComponent<Potion>();
                        
                        // 🐲 CRITICAL FIX: Added adjacency check for drag-and-drop
                        if (targetPotion != null && dragStartPotion != null && isAdjacent(dragStartPotion, targetPotion))
                        {
                            Debug.Log($"🐲 Drag swap approved: {dragStartPotion.candyType} <=> {targetPotion.candyType}");
                            SwapPotion(dragStartPotion, targetPotion);
                        }
                        else
                        {
                            Debug.Log($"❌ Drag swap blocked: Not adjacent or invalid target.");
                        }
                        
                        // Stop processing this drag regardless of success
                        isDragging = false;
                        dragStartPotion = null;
                    }
                }
            }
        }
        
        // Mouse button released - handle click selection if no drag occurred
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDragging && dragStartPotion != null)
            {
                Vector2 currentPos = mousePosition;
                float dragDistance = Vector2.Distance(dragStartPos, currentPos);
                
                // If drag distance was small, treat as click
                if (dragDistance <= dragThreshold * 100f)
                {
                    // This was a click, not a drag - selection already handled
                }
            }
            
            isDragging = false;
            dragStartPotion = null;
        }
    }

    private void DestroyPotions()
    {
        if (potionsToDestroy != null)
        {
            foreach (GameObject potion in potionsToDestroy)
            {
                Destroy(potion);
            }
            potionsToDestroy.Clear();
        }
    }

    public void ClearAllPotions()
    {
        if (potionBoard != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
                    {
                        Destroy(potionBoard[x, y].potion);
                        potionBoard[x, y].potion = null;
                    }
                }
            }
        }
    }

    IEnumerator InitializeBoardCoroutine()
    {
        DestroyPotions();
        recursionCount++;
        Debug.Log($"=== InitializeBoard Called (Attempt #{recursionCount}) ===");
        
        // SAFETY CHECK: Ensure we have BTS candy database
        if (candyDatabase == null)
        {
            Debug.LogError("⚠️ BTSCandyDatabase not assigned! Please assign it in PotionBoard Inspector.");
            Debug.LogError("⚠️ Cannot create board without database!");
            yield break;
        }
        
        if (candyDatabase.candies == null || candyDatabase.candies.Length < 7)
        {
            Debug.LogError($"⚠️ Database has insufficient candy types! Found {(candyDatabase.candies != null ? candyDatabase.candies.Length : 0)}, need 7 BTS members.");
            Debug.LogError($"⚠️ Please configure all 7 BTS members in the BTSCandyDatabase asset (RM, Jin, Suga, J-Hope, Jimin, V, Jungkook)");
            yield break;
        }
        
        if (potionPrefabs.Length < 1)
        {
            Debug.LogError($"⚠️ NO PREFAB! You need at least 1 potion prefab (used as template).");
            Debug.LogError($"⚠️ Assign a basic potion prefab in the 'Potion Prefabs' array.");
            yield break;
        }
        
        candyDatabase.InitializeActiveMembersForGame();
        
        Debug.Log($"✓ Database loaded with {candyDatabase.candies.Length} candy types, using {potionPrefabs.Length} prefab(s) as template");
        
        if (recursionCount > MAX_RECURSION)
        {
            Debug.LogError("Too many recursions! Board might have an impossible configuration.");
            recursionCount = 0;
            yield break;
        }
        
        if (potionBoard != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
                    {
                        Destroy(potionBoard[x, y].potion);
                    }
                }
            }
            yield return null;
        }

        potionBoard = new Node[width, height];

        if (arrayLayout == null || arrayLayout.rows == null)
        {
            Debug.LogError("arrayLayout is not properly configured!");
            yield break;
        }
        
        if (potionPrefabs.Length == 0)
        {
            Debug.LogError("potionPrefabs is EMPTY! Cannot create potions!");
            yield break;
        }

        Debug.Log($"Creating board with dimensions {width}x{height}, Mode: {(allowInitialMatches ? "Fast (allows matches)" : "Strict (no matches)")}");
        int potionsPlaced = 0;
        List<Vector2> potionPositions = new List<Vector2>();
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBlocked = arrayLayout.rows[y].row[x];
                
                if (isBlocked)
                {
                    potionBoard[x, y] = new Node(false, null);
                }
                else
                {
                    if (allowInitialMatches)
                    {
                        // Fast mode: Just place random candies, we'll clear matches after
                        PlaceRandomCandyAt(x, y);
                    }
                    else
                    {
                        // Strict mode: Try to avoid matches (can be slow)
                        PlaceNonMatchingAt(x, y);
                    }
                    potionsPlaced++;
                    
                    if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
                    {
                        Vector2 pos = potionBoard[x, y].potion.transform.localPosition;
                        potionPositions.Add(pos);
                    }
                }
            }
        }
        
        Debug.Log($"Placed {potionsPlaced} potions on the board");
        
        if (potionPositions.Count > 1)
        {
            bool allSame = true;
            Vector2 firstPos = potionPositions[0];
            for (int i = 1; i < potionPositions.Count; i++)
            {
                if (Vector2.Distance(firstPos, potionPositions[i]) > 0.01f)
                {
                    allSame = false;
                    break;
                }
            }
            
            if (allSame)
            {
                Debug.LogError($"⚠️ BUG DETECTED: All {potionsPlaced} potions are at the same position: {firstPos}!");
                Debug.LogError($"⚠️ This means position calculation is broken!");
            }
            else
            {
                Debug.Log($"✓ Potions are at different positions. First: {potionPositions[0]}, Last: {potionPositions[potionPositions.Count - 1]}");
            }
        }

        // NEW APPROACH: Clear any initial matches by replacing them
        Debug.Log("🔧 Checking for initial matches...");
        
        int maxPasses = 10;
        int passCount = 0;
        bool hasMatches;
        
        do
        {
            passCount++;
            hasMatches = CheckBoard();
            
            if (hasMatches)
            {
                Debug.Log($"🔧 Pass {passCount}: Found {potionsToRemove.Count} matches, replacing...");
                
                foreach (Potion potion in potionsToRemove)
                {
                    int x = potion.xIndex;
                    int y = potion.yIndex;
                    
                    if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
                    {
                        Destroy(potionBoard[x, y].potion);
                    }
                    
                    PlaceRandomCandyAt(x, y);
                }
                
                potionsToRemove.Clear();
            }
            
            if (passCount >= maxPasses)
            {
                Debug.LogWarning("⚠️ Max passes reached, accepting current board state");
                break;
            }
        }
        while (hasMatches);
        
        Debug.Log($"✓ Board clean after {passCount} passes");
        
        PrintBoardState();
        
        Debug.Log("✓ Board created successfully!");
        isBoardInitialized = true; // ✅ Mark board as ready for input
        recursionCount = 0; // Reset counter when successful
    }

    void PrintBoardState()
    {
        Debug.Log("=== BOARD STATE ===");
        for (int y = height - 1; y >= 0; y--)
        {
            string row = $"Row {y}: ";
            for (int x = 0; x < width; x++)
            {
                if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable)
                {
                    row += "- ";
                }
                else
                {
                    Potion p = potionBoard[x, y].potion.GetComponent<Potion>();
                    row += p.candyType.ToString()[0] + " ";
                }
            }
            Debug.Log(row);
        }
        Debug.Log("==================");
    }

    void InitializeBoard()
    {
        // This method is now replaced by InitializeBoardCoroutine
        // Kept for compatibility if called from other scripts
        StartCoroutine(InitializeBoardCoroutine());
    }
    
    /// <summary>
    /// Restart the game with new random members
    /// </summary>
    public void RestartWithNewMembers()
    {
        Debug.Log("🔄 Restarting game with new random BTS members...");
        
        ClearAllPotions();
        
        if (candyDatabase != null)
        {
            candyDatabase.InitializeActiveMembersForGame();
        }
        
        // Reinitialize board
        StartCoroutine(InitializeBoardCoroutine());
        
        // Don't reinitialize GameManager - preserve current game state
        // Only board is being shuffled, not the game itself
    }

    /// <summary>
    /// Fast candy placement - just place random candy without match checking
    /// Used when allowInitialMatches = true
    /// </summary>
    void PlaceRandomCandyAt(int x, int y)
    {
        if (potionPrefabs == null || potionPrefabs.Length == 0)
        {
            Debug.LogError($"Cannot place candy at ({x},{y}): potionPrefabs is null or empty!");
            return;
        }
        
        Vector2 position = GetLocalPositionForCell(x, y);
        
        // Just use the first prefab as a template (all should be identical structurally)
        GameObject potion = Instantiate(potionPrefabs[0], Vector3.zero, Quaternion.identity);
        potion.transform.SetParent(potionParent.transform);
        potion.transform.localPosition = position;
        potionsToDestroy.Add(potion);
        
        Potion potionComponent = potion.GetComponent<Potion>();
        if (potionComponent == null)
        {
            Debug.LogError($"Potion prefab has no Potion component!");
            Destroy(potion);
            return;
        }
        
        potionComponent.SetIndices(x, y);
        
        // Randomly assign candy type from database
        if (candyDatabase != null)
        {
            potionComponent.candyType = candyDatabase.GetRandomRegularCandy();
            Debug.Log($"PlaceRandomCandyAt({x},{y}): Assigned type {potionComponent.candyType}");
        }
        else
        {
            // Fallback: use random prefab's type
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            BTSCandyType randomType = potionPrefabs[randomIndex].GetComponent<Potion>().candyType;
            potionComponent.candyType = randomType;
            Debug.LogWarning($"Database NULL at ({x},{y}), using fallback: {randomType}");
        }
        
        ApplyCandySprite(potion, potionComponent.candyType);
        Debug.Log($"After ApplyCandySprite at ({x},{y}): Final type is {potionComponent.candyType}");
        potionBoard[x, y] = new Node(true, potion);
    }

    void PlaceNonMatchingAt(int x, int y)
    {
        if (potionPrefabs == null || potionPrefabs.Length == 0)
        {
            Debug.LogError($"Cannot place potion at ({x},{y}): potionPrefabs is null or empty!");
            return;
        }
        
        Vector2 position = GetLocalPositionForCell(x, y);
        
        if (x == 0 && y == 0)
        {
            Vector2 cellSize = GetCellSize();
            Debug.Log($"[PlacePotion] First potion at ({x},{y}), localPos: {position}, cellSize: {cellSize}");
        }
        const int maxAttempts = 100; // Reduced from 1000 to prevent freeze
        GameObject potion = null;
        Potion potionComponent = null;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (potion != null) Destroy(potion);
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            potion = Instantiate(potionPrefabs[randomIndex], Vector3.zero, Quaternion.identity);
            potion.transform.SetParent(potionParent.transform);
            potion.transform.localPosition = position; // Set LOCAL position after parenting
            potionsToDestroy.Add(potion);
            potionComponent = potion.GetComponent<Potion>();
            
            if (potionComponent == null)
            {
                Debug.LogError($"Potion prefab at index {randomIndex} has no Potion component!");
                continue;
            }
            
            potionComponent.SetIndices(x, y);
            
            ApplyCandySprite(potion, potionComponent.candyType);
            
            // CRITICAL: Set the board slot temporarily to check
            potionBoard[x, y] = new Node(true, potion);
            
            bool formsMatch = WouldFormMatchAt(x, y, potionComponent.candyType);
            
            if (!formsMatch)
            {
                // Success! Keep this potion
                if (x <= 1 && y <= 1)
                {
                    Debug.Log($"✓ Placed {potionComponent.candyType} at ({x},{y}) on attempt {attempt+1}");
                }
                return;
            }
            else
            {
                // Forms a match, remove it and try again
                potionBoard[x, y] = new Node(false, null);
            }
        }
        
        // If we exhausted attempts, try each potion type systematically
        Debug.LogWarning($"⚠️ Exhausted {maxAttempts} random attempts at ({x},{y}), trying systematically...");
        for (int prefabIndex = 0; prefabIndex < potionPrefabs.Length; prefabIndex++)
        {
            if (potion != null) Destroy(potion);
            potion = Instantiate(potionPrefabs[prefabIndex], Vector3.zero, Quaternion.identity);
            potion.transform.SetParent(potionParent.transform);
            potion.transform.localPosition = position; // Set LOCAL position after parenting
            potionsToDestroy.Add(potion);
            potionComponent = potion.GetComponent<Potion>();
            potionComponent.SetIndices(x, y);
            
            // CRITICAL: Set the board slot temporarily
            potionBoard[x, y] = new Node(true, potion);
            
            bool formsMatch = WouldFormMatchAt(x, y, potionComponent.candyType);
            
            if (!formsMatch)
            {
                Debug.Log($"Systematic placement worked with prefab {prefabIndex} ({potionComponent.candyType}) at ({x},{y})");
                return;
            }
            else
            {
                potionBoard[x, y] = new Node(false, null);
            }
        }
        
        // This should never happen with 5 potion types
        Debug.LogError($"CRITICAL: Could not place any non-matching potion at ({x},{y})! This suggests insufficient potion variety.");
        potionBoard[x, y] = new Node(true, potion); // Keep last one as emergency fallback
    }

    // After initial fill, reroll any cells that still complete a 3+ line
    int ResolveInitialMatches()
    {
        int changes = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable) continue;
                Potion p = potionBoard[x, y].potion.GetComponent<Potion>();
                
                if (WouldFormMatchAt(x, y, p.candyType))
                {
                    Debug.Log($"🔧 Resolving match at ({x},{y}): {p.candyType} (isSpecial: {p.isSpecialCandy})");
                    // Yes, it forms a match, destroy it and place a different one
                    Destroy(potionBoard[x, y].potion);
                    potionBoard[x, y] = new Node(false, null); // Mark as empty
                    PlaceNonMatchingAt(x, y);
                    changes++;
                }
            }
        }
        Debug.Log($"🔧 ResolveInitialMatches: Fixed {changes} cells this pass");
        return changes;
    }
    
    // Now checks with the potion already placed in the board
    bool WouldFormMatchAt(int x, int y, BTSCandyType type)
    {
        if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable)
            return false;
            
        Potion currentPotion = potionBoard[x, y].potion.GetComponent<Potion>();
            
        // Horizontal: count neighbors left and right using CanMatch()
        int hCount = 1; // Count the current position
        for (int i = x - 1; i >= 0; i--)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            hCount++;
        }
        for (int i = x + 1; i < width; i++)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            hCount++;
        }
        if (hCount >= 3)
        {
            if (x == 1 && y >= 1 && y <= 5) Debug.Log($"🔍 H-Match detected: {hCount}x {type} at ({x},{y})");
            return true;
        }

        // Vertical: count neighbors up and down using CanMatch()
        int vCount = 1; // Count the current position
        for (int j = y - 1; j >= 0; j--)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            vCount++;
        }
        for (int j = y + 1; j < height; j++)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            vCount++;
        }
        if (vCount >= 3)
        {
            if (x == 1 && y >= 1 && y <= 5) Debug.Log($"🔍 V-Match detected: {vCount}x {type} at ({x},{y})");
            return true;
        }
        
        return false;
    }

    void VerifyNoMatches()
    {
        bool foundMatch = false;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable) continue;
                
                Potion p = potionBoard[x, y].potion.GetComponent<Potion>();
                
                int hCount = 1;
                for (int i = x - 1; i >= 0; i--)
                {
                    if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
                    Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
                    if (!CanMatch(p, neighbor)) break; // ✅ Use CanMatch() instead of direct comparison
                    hCount++;
                }
                for (int i = x + 1; i < width; i++)
                {
                    if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
                    Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
                    if (!CanMatch(p, neighbor)) break; // ✅ Use CanMatch() instead of direct comparison
                    hCount++;
                }
                if (hCount >= 3)
                {
                    Debug.LogError($"MATCH FOUND! Horizontal {hCount}x {p.candyType} at ({x},{y})");
                    foundMatch = true;
                }
                
                int vCount = 1;
                for (int j = y - 1; j >= 0; j--)
                {
                    if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
                    Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
                    if (!CanMatch(p, neighbor)) break; // ✅ Use CanMatch() instead of direct comparison
                    vCount++;
                }
                for (int j = y + 1; j < height; j++)
                {
                    if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
                    Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
                    if (!CanMatch(p, neighbor)) break; // ✅ Use CanMatch() instead of direct comparison
                    vCount++;
                }
                if (vCount >= 3)
                {
                    Debug.LogError($"MATCH FOUND! Vertical {vCount}x {p.candyType} at ({x},{y})");
                    foundMatch = true;
                }
            }
        }
        
        if (!foundMatch)
        {
            Debug.Log("✓ Verification complete: No 3+ matches found on board");
        }
        else
        {
            Debug.LogError("✗ VERIFICATION FAILED: Matches still exist on board!");
        }
    }

    // Generic check that inspects immediate patterns to see if (x,y) is part of a 3+ line with given type
    bool FormsLineOfThreeAt(int x, int y, BTSCandyType type)
    {
        // Early out if cell blocked
        if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable)
            return false;

        Potion currentPotion = potionBoard[x, y].potion.GetComponent<Potion>();

        // Horizontal: count contiguous matches using CanMatch()
        int count = 1;
        for (int i = x - 1; i >= 0; i--)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            count++;
        }
        for (int i = x + 1; i < width; i++)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            count++;
        }
        if (count >= 3) return true;

        // Vertical: count contiguous matches using CanMatch()
        count = 1;
        for (int j = y - 1; j >= 0; j--)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            count++;
        }
        for (int j = y + 1; j < height; j++)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break; // ✅ Use CanMatch() for consistency
            count++;
        }
        return count >= 3;
    }

    public bool CheckBoard()
    {
        Debug.Log("=== Checking Board ===");
        bool hasMatched = false;

        potionsToRemove.Clear();
        specialCandiesToSpawn.Clear();

        foreach(Node nodePotion in potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x,y] != null && potionBoard[x,y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    potion.isMatched = false;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //checking if potion node is usable
                if (potionBoard[x,y] != null && potionBoard[x,y].isUsable)
                {
                    //then proceed to get potion class in node.
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    //ensure its not matched
                    if(!potion.isMatched)
                    {
                        //run some matching logic

                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            Debug.Log($"Found match at ({x},{y}): {matchedPotions.connectedPotions.Count} {potion.candyType} potions, Direction: {matchedPotions.direction}");
                            MatchResult superMatchedPotions = SuperMatch(matchedPotions);
                            
                            if (superMatchedPotions.createSpecialCandy)
                            {
                                Debug.Log($"⭐ Creating special candy at ({x},{y}) for match type: {superMatchedPotions.matchType}");
                                specialCandiesToSpawn.Add(new SpecialCandySpawn
                                {
                                    x = x,
                                    y = y,
                                    matchType = superMatchedPotions.matchType,
                                    originalType = potion.candyType
                                });
                            }
                            
                            //complex matching...
                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                                pot.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        Debug.Log($"=== CheckBoard Complete: hasMatched={hasMatched}, Special Candies to Spawn: {specialCandiesToSpawn.Count} ===");
        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        List<Potion> matchedSpecials = new();
        List<Potion> regularMatches = new();

        foreach (Potion potion in potionsToRemove)
        {
            if (potion == null) continue;
            potion.isMatched = true;
            if (potion.isSpecialCandy)
            {
                matchedSpecials.Add(potion);
            }
            else
            {
                regularMatches.Add(potion);
            }
        }

        potionsToRemove.Clear();

        foreach (Potion special in matchedSpecials)
        {
            if (special == null) continue;
            Debug.Log($"🎬 Activating matched special candy {special.candyType} before removal");
            yield return StartCoroutine(ActivateSpecialCandySequence(special));
        }

        if (regularMatches.Count > 0)
        {
            foreach (Potion regular in regularMatches)
            {
                if (regular == null) continue;
                potionsToRemove.Add(regular);
            }

            if (potionsToRemove.Count > 0)
            {
                RemoveAndRefill(potionsToRemove, triggerSpecialChain: false);
                potionsToRemove.Clear();
            }
        }

        int totalRemoved = matchedSpecials.Count + regularMatches.Count;
        if (GameManager.instance != null && totalRemoved > 0)
        {
            GameManager.instance.ProcessTurn(totalRemoved, _subtractMoves);
        }

        yield return new WaitForSeconds(0.4f);

        if (GameManager.instance != null && GameManager.instance.isGameEnded)
        {
            isProcessingMove = false;
            yield break;
        }

        if (CheckBoard())
        {
            // Cascade matches should NOT subtract moves - only the initial move counts
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
        else
        {
            // No more matches, snap all potions to ensure alignment
            yield return new WaitForSeconds(0.2f);
            SnapAllPotionsToGrid();

            // Then check if there are valid moves
            CheckForValidMoves();

            // All processing complete, allow new moves
            Debug.Log("✅ ProcessTurnOnMatchedBoard complete, setting isProcessingMove = false");
            isProcessingMove = false;
        }
    }

    private void RemoveAndRefill(List<Potion> potionsToRemove, bool triggerSpecialChain = true)
    {
        // First, spawn special candies at match locations (before clearing)
        foreach (var specialSpawn in specialCandiesToSpawn)
        {
            bool shouldSpawn = false;
            foreach (Potion pot in potionsToRemove)
            {
                if (pot.xIndex == specialSpawn.x && pot.yIndex == specialSpawn.y)
                {
                    shouldSpawn = true;
                    break;
                }
            }
            
            if (shouldSpawn)
            {
                Debug.Log($"✨ Spawning special candy at ({specialSpawn.x},{specialSpawn.y})");
                SpawnSpecialCandyAt(specialSpawn.x, specialSpawn.y, specialSpawn.matchType, specialSpawn.originalType);
            }
        }
        
        specialCandiesToSpawn.Clear();
        
        List<Potion> chainSpecials = new();
        
        foreach (Potion potion in potionsToRemove)
        {
            if (potion == null) continue;
            
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            if (triggerSpecialChain && potion.isSpecialCandy)
            {
                chainSpecials.Add(potion);
                continue;
            }

            Destroy(potion.gameObject);
            
            // Only mark as empty if this candy is still in the board at this position
            if (_xIndex >= 0 && _xIndex < width && _yIndex >= 0 && _yIndex < height)
            {
                // Only set to null if this potion is still the one in the board
                // (A special candy might have already replaced it)
                if (potionBoard[_xIndex, _yIndex].potion == potion.gameObject)
                {
                    potionBoard[_xIndex, _yIndex] = new Node(true, null);
                }
            }
        }

        if (triggerSpecialChain)
        {
            foreach (Potion special in chainSpecials)
            {
                if (special == null) continue;
                Debug.Log($"🔁 Chain activating special candy {special.candyType}");
                StartCoroutine(ActivateSpecialCandySequence(special));
            }
        }

        for (int x = 0; x < width; x++)
        {
            RefillColumn(x);
        }
    }

    private void RefillColumn(int x)
    {
        float animationDelay = 0f;
        const float delayIncrement = 0.05f; // Small delay between each potion movement
        
        // First, drop all existing potions down
        for (int y = 0; y < height; y++)
        {
            if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion == null)
            {
                // Found empty spot, look for potion above to drop down
                for (int yAbove = y + 1; yAbove < height; yAbove++)
                {
                    if (potionBoard[x, yAbove] != null && potionBoard[x, yAbove].isUsable && potionBoard[x, yAbove].potion != null)
                    {
                        Potion potionAbove = potionBoard[x, yAbove].potion.GetComponent<Potion>();
                        
                        Vector3 worldTargetPos = GetWorldPositionForCell(x, y);
                        
                        potionAbove.MoveToTarget(worldTargetPos, animationDelay);
                        potionAbove.SetIndices(x, y);
                        potionBoard[x, y] = potionBoard[x, yAbove];
                        potionBoard[x, yAbove] = new Node(true, null);
                        
                        animationDelay += delayIncrement;
                        Debug.Log($"Dropped potion from ({x},{yAbove}) to ({x},{y}) with delay {animationDelay}");
                        break; // Found one potion to drop, move to next empty spot
                    }
                }
            }
        }
        
        // Then, spawn new potions for any remaining empty spots at the top
        for (int y = 0; y < height; y++)
        {
            if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion == null)
            {
                SpawnPotionAtPosition(x, y, animationDelay);
                animationDelay += delayIncrement;
            }
        }
    }

    private void SpawnPotionAtPosition(int x, int y, float delay = 0f)
    {
        Vector2 localTargetPos = GetLocalPositionForCell(x, y);
        
        // Spawn above the board - calculate spawn position above the top
        float offsetX = (width - 1) / 2f * potionSpacingX;
        Vector2 spawnPos = new Vector2(x * potionSpacingX - offsetX, height * potionSpacingY);
        
        GameObject newPotion = Instantiate(potionPrefabs[0], Vector3.zero, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.transform.localPosition = spawnPos; // Set in local space
        
        Potion potionComponent = newPotion.GetComponent<Potion>();
        potionComponent.SetIndices(x, y);
        
        // Randomly assign candy type
        if (candyDatabase != null)
        {
            potionComponent.candyType = candyDatabase.GetRandomRegularCandy();
        }
        else
        {
            // Fallback: pick random from prefabs
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            potionComponent.candyType = potionPrefabs[randomIndex].GetComponent<Potion>().candyType;
        }
        
        ApplyCandySprite(newPotion, potionComponent.candyType);
        
        potionBoard[x, y] = new Node(true, newPotion);
        
        Vector3 worldTargetPos = potionParent.transform.TransformPoint(localTargetPos);
        Debug.Log($"Spawning new potion at column {x}, row {y}, target: {worldTargetPos}, delay: {delay}");
        
        if (delay > 0f)
        {
            newPotion.GetComponent<Potion>().MoveToTarget(worldTargetPos, delay);
        }
        else
        {
            newPotion.GetComponent<Potion>().MoveToTarget(worldTargetPos);
        }
    }
    
    /// <summary>
    /// Spawn a special candy at the specified position based on match type
    /// </summary>
    private void SpawnSpecialCandyAt(int x, int y, MatchType matchType, BTSCandyType originalType)
    {
        if (candyDatabase == null)
        {
            Debug.LogWarning("Cannot spawn special candy: database is null");
            return;
        }
        
        BTSCandyType specialType = candyDatabase.GetSpecialCandyForMatch(matchType);
        
        Vector2 localPos = GetLocalPositionForCell(x, y);
        
        if (potionBoard[x, y] != null && potionBoard[x, y].potion != null)
        {
            Destroy(potionBoard[x, y].potion);
        }
        
        GameObject specialCandy = Instantiate(potionPrefabs[0], Vector3.zero, Quaternion.identity);
        specialCandy.transform.SetParent(potionParent.transform);
        specialCandy.transform.localPosition = localPos;
        
        Potion potionComponent = specialCandy.GetComponent<Potion>();
        potionComponent.SetIndices(x, y);
        potionComponent.candyType = specialType;
        potionComponent.isSpecialCandy = true; // Mark as special
        potionComponent.baseColor = originalType; // ⭐ PRESERVE THE MATCHED COLOR!
        
        Debug.Log($"✨✨✨ SPAWNED SPECIAL CANDY: Type={specialType}, isSpecial={potionComponent.isSpecialCandy}, BaseColor={originalType}, Position=({x},{y})");
        
        ApplyCandySprite(specialCandy, originalType); // Use the ORIGINAL member's sprite, not special type
        
        // ✨ AUTOMATICALLY APPLY VISUAL EFFECTS (stripes, balloon, rainbow)
        potionComponent.UpdateVisualEffects();
        
        potionBoard[x, y] = new Node(true, specialCandy);
        
        Debug.Log($"✨ Created {specialType} with base color {originalType} at ({x},{y}) from {matchType} match");
        
        // Optional: Play creation animation/effect
        // You could add a scale-up animation or particle effect here
        StartCoroutine(SpecialCandyCreationEffect(specialCandy));
    }
    
    /// <summary>
    /// Visual effect when a special candy is created
    /// </summary>
    private IEnumerator SpecialCandyCreationEffect(GameObject candy)
    {
        if (candy == null) yield break; // Early exit if candy is null
        
        Vector3 originalScale = candy.transform.localScale;
        float duration = 0.3f;
        float elapsed = 0f;
        
        candy.transform.localScale = Vector3.zero;
        
        while (elapsed < duration)
        {
            if (candy == null) yield break; // Check if destroyed during animation
            
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(0f, 1.2f, progress);
            candy.transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Bounce back to normal
        elapsed = 0f;
        duration = 0.1f;
        while (elapsed < duration)
        {
            if (candy == null) yield break; // Check if destroyed during animation
            
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(1.2f, 1f, progress);
            candy.transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        candy.transform.localScale = originalScale;
    }

    private void RefillPotion(int x, int y)
    {
        int yOffset = 1;

        while (y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
        {
            Debug.Log("The potion above me is null, but im not at the top of the board yet, so add to my yOffset and try again");
            yOffset++;
        }

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            Vector3 worldTargetPos = GetWorldPositionForCell(x, y);
            Debug.Log("I have found a potion above me and I need to move it to my target position of: " + worldTargetPos);
            potionAbove.MoveToTarget(worldTargetPos);
            potionAbove.SetIndices(x, y);
            potionBoard[x, y] = potionBoard[x, y + yOffset];
            potionBoard[x, y + yOffset] = new Node(true, null);
        }
        if (y + yOffset == height)
        {
            Debug.Log("I am at the top of the board, so I need to spawn a new potion");
            SpawnPotionAtTop(x);
        }
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7; y >= 0; y--)
        {
            if (potionBoard[x, y].potion == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        Debug.Log("about to spawn a potion, ideally i'd like to put it in the index of: " + index);
        
        Vector2 localTargetPos = GetLocalPositionForCell(x, index);
        
        // Spawn above the board
        float offsetX = (width - 1) / 2f * potionSpacingX;
        Vector2 spawnPos = new Vector2(x * potionSpacingX - offsetX, height * potionSpacingY);
        
        GameObject newPotion = Instantiate(potionPrefabs[0], Vector3.zero, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.transform.localPosition = spawnPos; // Set in local space
        
        Potion potionComponent = newPotion.GetComponent<Potion>();
        potionComponent.SetIndices(x, index);
        
        // Randomly assign candy type
        if (candyDatabase != null)
        {
            potionComponent.candyType = candyDatabase.GetRandomRegularCandy();
        }
        else
        {
            // Fallback: pick random from prefabs
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            potionComponent.candyType = potionPrefabs[randomIndex].GetComponent<Potion>().candyType;
        }
        
        ApplyCandySprite(newPotion, potionComponent.candyType);
        
        potionBoard[x, index] = new Node(true, newPotion);
        
        Vector3 worldTargetPos = potionParent.transform.TransformPoint(localTargetPos);
        Debug.Log($"Spawning potion at column {x}, target index {index}, world: {worldTargetPos}");
        newPotion.GetComponent<Potion>().MoveToTarget(worldTargetPos);
    }
    #region Cascading Potions

    #endregion

    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(0,1), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(0,-1), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("🎈 L-SHAPE or T-SHAPE DETECTED! Creates Balloon");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    
                    // Check total count: 5+ creates Rainbow, otherwise creates Balloon
                    int totalCount = extraConnectedPotions.Count;
                    if (totalCount >= 5)
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.Match5Plus,
                            createSpecialCandy = true
                        };
                    }
                    else
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.TShape, // Could be T or L, both create Balloon
                            createSpecialCandy = true
                        };
                    }
                }
            }
            return _matchedResults;
        }
        else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(1,0), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(-1,0), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    Debug.Log("🎈 L-SHAPE or T-SHAPE DETECTED! Creates Balloon");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    
                    int totalCount = extraConnectedPotions.Count;
                    if (totalCount >= 5)
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.Match5Plus,
                            createSpecialCandy = true
                        };
                    }
                    else
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.LShape, // Could be T or L, both create Balloon
                            createSpecialCandy = true
                        };
                    }
                }
            }
            return _matchedResults;
        }
        return null;
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        BTSCandyType potionType = potion.candyType;

        connectedPotions.Add(potion);

        //check right
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        //check left
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        
        int horizontalCount = connectedPotions.Count;
        
        //have we made a 3+ match? (Horizontal Match)
        if (horizontalCount >= 3)
        {
            if (horizontalCount >= 5)
            {
                Debug.Log($"� HORIZONTAL 5+ MATCH! {horizontalCount}x {potionType} - Creates Rainbow");
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongHorizontal,
                    matchType = MatchType.Match5Plus,
                    createSpecialCandy = true
                };
            }
            else if (horizontalCount == 4)
            {
                Debug.Log($"📏 HORIZONTAL 4-MATCH! {potionType} - Creates StripedVertical (clears column)");
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Horizontal,
                    matchType = MatchType.Match4Horizontal,
                    createSpecialCandy = true
                };
            }
            else
            {
                Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedPotions[0].candyType);
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Horizontal,
                    matchType = MatchType.Normal3,
                    createSpecialCandy = false
                };
            }
        }
        
        //clear out the connectedpotions
        connectedPotions.Clear();
        //readd our initial potion
        connectedPotions.Add(potion);

        //check up
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        //check down
        CheckDirection(potion, new Vector2Int(0,-1), connectedPotions);

        int verticalCount = connectedPotions.Count;
        
        //have we made a 3+ match? (Vertical Match)
        if (verticalCount >= 3)
        {
            if (verticalCount >= 5)
            {
                Debug.Log($"� VERTICAL 5+ MATCH! {verticalCount}x {potionType} - Creates Rainbow");
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongVertical,
                    matchType = MatchType.Match5Plus,
                    createSpecialCandy = true
                };
            }
            else if (verticalCount == 4)
            {
                Debug.Log($"📏 VERTICAL 4-MATCH! {potionType} - Creates StripedHorizontal (clears row)");
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Vertical,
                    matchType = MatchType.Match4Vertical,
                    createSpecialCandy = true
                };
            }
            else
            {
                Debug.Log("I have a normal vertical match, the color of my match is: " + connectedPotions[0].candyType);
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Vertical,
                    matchType = MatchType.Normal3,
                    createSpecialCandy = false
                };
            }
        }
        
        return new MatchResult
        {
            connectedPotions = connectedPotions,
            direction = MatchDirection.None,
            matchType = MatchType.Normal3,
            createSpecialCandy = false
        };
    }

    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        BTSCandyType potionType = pot.candyType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //check that we're within the boundaries of the board
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x,y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                //does our potionType Match? it must also not be matched
                if(!neighbourPotion.isMatched && CanMatch(pot, neighbourPotion))
                {
                    connectedPotions.Add(neighbourPotion);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
                
            }
            else
            {
                break;
            }
        }
    }
    
    /// <summary>
    /// Check if two candies can match (considering special candies with base colors)
    /// </summary>
    private bool CanMatch(Potion candy1, Potion candy2)
    {
        if (candy1.isSpecialCandy && candy2.isSpecialCandy)
        {
            // Two special candies always create a combo (handled separately)
            return false; // Don't match in regular check
        }
        else if (candy1.isSpecialCandy)
        {
            return candy1.baseColor == candy2.candyType;
        }
        else if (candy2.isSpecialCandy)
        {
            return candy2.baseColor == candy1.candyType;
        }
        else
        {
            // Regular candy matching
            return candy1.candyType == candy2.candyType;
        }
    }



    #region Swapping potions

    public void SelectPotion(Potion _potion)
    {
        if (selectedPotion == null)
        {
            if (_potion.isSpecialCandy)
            {
                Debug.Log($"✨ Special candy selected: {_potion.candyType} (isSpecial={_potion.isSpecialCandy})");
                selectedPotion = _potion;
                ShowSelectionIndicator(_potion);
            }
            else
            {
                Debug.Log($"Regular candy selected: {_potion.candyType}");
                selectedPotion = _potion;
                ShowSelectionIndicator(_potion);
            }
        }
        else if (selectedPotion == _potion)
        {
            // Second click on same candy just toggles selection off. Activation now requires a valid swap/match.
            Debug.Log($"Deselecting candy: {_potion.candyType}");
            selectedPotion = null;
            HideSelectionIndicator();
        }
        else if (selectedPotion != _potion)
        {
            // Check if candies are adjacent - REQUIRED even for special candies!
            bool adjacent = isAdjacent(selectedPotion, _potion);
            Debug.Log($"🎯 Second click: A=({selectedPotion.xIndex},{selectedPotion.yIndex}) B=({_potion.xIndex},{_potion.yIndex}) adjacent={adjacent}");
            if (!adjacent)
            {
                Debug.Log($"❌ Cannot swap: candies are not adjacent");
                // Switch selection to the clicked candy instead
                selectedPotion = _potion;
                ShowSelectionIndicator(_potion);
                return;
            }

            if (selectedPotion.isSpecialCandy && _potion.isSpecialCandy)
            {
                Debug.Log($"💥 SPECIAL COMBO! {selectedPotion.candyType} + {_potion.candyType}");
                StartCoroutine(HandleSpecialCombo(selectedPotion, _potion));
                selectedPotion = null;
                HideSelectionIndicator();
            }
            else if (selectedPotion.isSpecialCandy || _potion.isSpecialCandy)
            {
                // One special candy + regular candy = swap and activate
                SwapPotion(selectedPotion, _potion);
                selectedPotion = null;
                HideSelectionIndicator();
            }
            else
            {
                // Regular swap
                SwapPotion(selectedPotion, _potion);
                selectedPotion = null;
                HideSelectionIndicator();
            }
        }
    }
    
    private void ShowSelectionIndicator(Potion potion)
    {
        if (selectionIndicator != null)
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            
            if (indicatorOriginalScale == Vector3.zero)
            {
                indicatorOriginalScale = selectionIndicator.transform.localScale;
            }
            
            selectionIndicator.SetActive(true);
            selectionIndicator.transform.position = potion.transform.position;
            selectionIndicator.transform.SetParent(potion.transform);
            
            selectionIndicator.transform.localScale = indicatorOriginalScale;
            
            Vector3 pos = selectionIndicator.transform.localPosition;
            pos.z = 0.1f; // Slightly behind candy
            selectionIndicator.transform.localPosition = pos;
            
            SpriteRenderer indicatorSprite = selectionIndicator.GetComponent<SpriteRenderer>();
            SpriteRenderer candySprite = potion.GetComponent<SpriteRenderer>();
            
            if (indicatorSprite != null && candySprite != null)
            {
                indicatorSprite.sortingLayerName = candySprite.sortingLayerName;
                indicatorSprite.sortingOrder = candySprite.sortingOrder - 1;
            }
            
            blinkCoroutine = StartCoroutine(BlinkIndicator());
        }
    }
    
    private void HideSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            
            if (indicatorOriginalScale != Vector3.zero)
            {
                selectionIndicator.transform.localScale = indicatorOriginalScale;
            }
            
            selectionIndicator.SetActive(false);
            selectionIndicator.transform.SetParent(potionParent.transform);
        }
    }
    
    private IEnumerator BlinkIndicator()
    {
        if (selectionIndicator == null)
            yield break;
            
        SpriteRenderer spriteRenderer = selectionIndicator.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image imageComponent = selectionIndicator.GetComponent<UnityEngine.UI.Image>();
        
        while (true)
        {
            float elapsed = 0f;
            while (elapsed < blinkSpeed)
            {
                float alpha = Mathf.Lerp(1f, 0.3f, elapsed / blinkSpeed);
                
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                }
                else if (imageComponent != null)
                {
                    Color color = imageComponent.color;
                    color.a = alpha;
                    imageComponent.color = color;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < blinkSpeed)
            {
                float alpha = Mathf.Lerp(0.3f, 1f, elapsed / blinkSpeed);
                
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                }
                else if (imageComponent != null)
                {
                    Color color = imageComponent.color;
                    color.a = alpha;
                    imageComponent.color = color;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        if (isProcessingMove)
        {
            Debug.Log("❌ Swap blocked: a move is already processing");
            return;
        }
        if (_currentPotion != null && _currentPotion.isMoving || _targetPotion != null && _targetPotion.isMoving)
        {
            Debug.Log("❌ Swap blocked: one of the potions is moving");
            return;
        }
        if (!isAdjacent(_currentPotion, _targetPotion))
        {
            Debug.Log($"❌ Swap blocked: not adjacent. A=({_currentPotion?.xIndex},{_currentPotion?.yIndex}) B=({_targetPotion?.xIndex},{_targetPotion?.yIndex})");
            return;
        }
        Debug.Log($"🔄 Swapping A=({_currentPotion.xIndex},{_currentPotion.yIndex}) with B=({_targetPotion.xIndex},{_targetPotion.yIndex})");
        DoSwap(_currentPotion, _targetPotion);
        isProcessingMove = true;
        Debug.Log("▶️ isProcessingMove = true (after SwapPotion)");
        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }

    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[ _currentPotion.xIndex, _currentPotion.yIndex].potion;
        potionBoard[ _currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[ _targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[ _targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        Vector3 currentWorldPos = GetWorldPositionForCell(_currentPotion.xIndex, _currentPotion.yIndex);
        Vector3 targetWorldPos = GetWorldPositionForCell(_targetPotion.xIndex, _targetPotion.yIndex);
        
        _currentPotion.MoveToTarget(currentWorldPos);
        _targetPotion.MoveToTarget(targetWorldPos);
    }

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        HideSelectionIndicator(); // Hide indicator during processing
        yield return new WaitForSeconds(0.2f);
        
        bool specialActivated = false;
        yield return StartCoroutine(CheckAndActivateSpecialCandyCoroutine(_currentPotion, _targetPotion, 
            (activated) => { specialActivated = activated; }
        ));
        
        if (specialActivated)
        {
            // Special candy was activated
            Debug.Log("✅ Special candy activation complete from swap");
            
            // Check for matches after special candy activation
            if (CheckBoard())
            {
                Debug.Log("🔄 Matches found after special candy swap, processing cascades");
                StartCoroutine(ProcessTurnOnMatchedBoard(true));
                // ProcessTurnOnMatchedBoard will reset isProcessingMove
            }
            else
            {
                Debug.Log("✅ No matches after special candy swap, setting isProcessingMove = false");
                SnapAllPotionsToGrid();
                CheckForValidMoves();
                isProcessingMove = false;
            }
        }
        else
        {
            // No special activation, check for regular matches
            bool hasMatched = CheckBoard();

            if (hasMatched)
            {
                Debug.Log("📋 Regular match found, starting ProcessTurnOnMatchedBoard");
                StartCoroutine(ProcessTurnOnMatchedBoard(true));
            }
            else
            {
                Debug.Log("❌ No match, swapping back");
                DoSwap(_currentPotion, _targetPotion);
                yield return new WaitForSeconds(0.3f);
                SnapAllPotionsToGrid(); // Ensure alignment after swap back
                CheckForValidMoves();
                Debug.Log("✅ Swap back complete, setting isProcessingMove = false");
                isProcessingMove = false;
            }
        }
    }
    
    /// <summary>
    /// Check if swap involves a special candy and activate it
    /// Coroutine version that properly waits for activation to complete
    /// </summary>
    private IEnumerator CheckAndActivateSpecialCandyCoroutine(Potion candy1, Potion candy2, System.Action<bool> callback)
    {
        Debug.Log($"🔍 CheckAndActivateSpecialCandyCoroutine: candy1={candy1.candyType} (isSpecial={candy1.isSpecialCandy}), candy2={candy2.candyType} (isSpecial={candy2.isSpecialCandy})");
        
        Potion specialCandy = null;
        Potion regularCandy = null;
        
        if (candy1.isSpecialCandy && !candy2.isSpecialCandy)
        {
            specialCandy = candy1;
            regularCandy = candy2;
            Debug.Log($"   candy1 is special, candy2 is regular");
        }
        else if (candy2.isSpecialCandy && !candy1.isSpecialCandy)
        {
            specialCandy = candy2;
            regularCandy = candy1;
            Debug.Log($"   candy2 is special, candy1 is regular");
        }
        else if (candy1.isSpecialCandy && candy2.isSpecialCandy)
        {
            // Both are special - combo!
            Debug.Log("💥 SPECIAL COMBO DETECTED!");
            yield return StartCoroutine(HandleSpecialCombo(candy1, candy2));
            callback(true);
            yield break;
        }
        else
        {
            // Neither is special
            Debug.Log($"   Neither candy is special");
            callback(false);
            yield break;
        }
        
        if (specialCandy != null && regularCandy != null)
        {
            // 🎯 FIXED: Special candies should ONLY activate if they match with their base color
            // This makes special candies work like regular candies - they need to match to activate
            Debug.Log($"🔍 Special candy {specialCandy.candyType} (base color: {specialCandy.baseColor}) swapped with {regularCandy.candyType}");
            
            if (specialCandy.baseColor == regularCandy.candyType)
            {
                Debug.Log($"✨ COLOR MATCH! Special candy can activate when part of a match");
                // Let the normal match detection run - if there's a 3+ match, it will activate
                callback(false); // Continue to match detection
            }
            else
            {
                Debug.Log($"❌ No color match. Special candy base={specialCandy.baseColor}, regular={regularCandy.candyType}. No activation.");
                // Different colors - no activation, no match
                callback(false); // Continue to match detection (which will find nothing and swap back)
            }
            yield break;
        }
        
        callback(false);
    }
    
    /// <summary>
    /// Check if swap involves a special candy matching with its base color
    /// If so, activate the special candy instead of regular matching
    /// DEPRECATED: Use CheckAndActivateSpecialCandyCoroutine instead
    /// </summary>
    private bool CheckAndActivateSpecialCandy(Potion candy1, Potion candy2)
    {
        Potion specialCandy = null;
        Potion regularCandy = null;
        
        if (candy1.isSpecialCandy && !candy2.isSpecialCandy)
        {
            specialCandy = candy1;
            regularCandy = candy2;
        }
        else if (candy2.isSpecialCandy && !candy1.isSpecialCandy)
        {
            specialCandy = candy2;
            regularCandy = candy1;
        }
        else if (candy1.isSpecialCandy && candy2.isSpecialCandy)
        {
            // Both are special - combo!
            Debug.Log("💥 SPECIAL COMBO DETECTED!");
            StartCoroutine(HandleSpecialCombo(candy1, candy2));
            return true;
        }
        else
        {
            // Neither is special
            return false;
        }
        
        if (specialCandy != null && regularCandy != null)
        {
            // In Candy Crush: special candies ALWAYS activate when swapped with ANY candy
            // If matched with same color in a group, both the special effect AND match happen
            Debug.Log($"🎵 Special candy {specialCandy.candyType} (base color: {specialCandy.baseColor}) swapped with {regularCandy.candyType}!");
            
            // Activate the special candy
            StartCoroutine(ActivateSpecialCandySequence(specialCandy));
            
            // If it's the same color, also allow match detection to trigger
            if (specialCandy.baseColor == regularCandy.candyType)
            {
                Debug.Log($"✨ Same color match! Will also check for regular matches");
                return false; // Let normal match detection continue
            }
            
            // Different colors - just activate special candy
            return true; // Skip normal match detection
        }
        
        return false;
    }

    private bool isAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }
    
    // Helper method to snap all potions to their correct grid positions
    private void SnapAllPotionsToGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    
                    Vector3 worldPos = GetWorldPositionForCell(x, y);
                    
                    // Snap to position (no animation)
                    potionBoard[x, y].potion.transform.position = worldPos;
                    
                    potion.SetIndices(x, y);
                }
            }
        }
    }

    #endregion
    
    #region No Moves Detection and Shuffle
    
    private bool HasValidMoves()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable || potionBoard[x, y].potion == null)
                    continue;
                
                if (x < width - 1 && potionBoard[x + 1, y] != null && potionBoard[x + 1, y].isUsable && potionBoard[x + 1, y].potion != null)
                {
                    if (WouldSwapCreateMatch(x, y, x + 1, y))
                        return true;
                }
                
                if (y < height - 1 && potionBoard[x, y + 1] != null && potionBoard[x, y + 1].isUsable && potionBoard[x, y + 1].potion != null)
                {
                    if (WouldSwapCreateMatch(x, y, x, y + 1))
                        return true;
                }
            }
        }
        return false;
    }
    
    private bool WouldSwapCreateMatch(int x1, int y1, int x2, int y2)
    {
        Potion potion1 = potionBoard[x1, y1].potion.GetComponent<Potion>();
        Potion potion2 = potionBoard[x2, y2].potion.GetComponent<Potion>();
        
        // Simulate the swap by checking if either position would create a match
        // We need to manually check neighbors since WouldFormMatchAt uses the board state
        
        if (WouldTypeCreateMatchAt(x1, y1, potion2, x2, y2))
            return true;
        
        if (WouldTypeCreateMatchAt(x2, y2, potion1, x1, y1))
            return true;
        
        return false;
    }
    
    // Helper to check if placing a potion at (x,y) would match, excluding the swap partner
    private bool WouldTypeCreateMatchAt(int x, int y, Potion potionToPlace, int excludeX, int excludeY)
    {
        int hCount = 1;
        
        // Left
        for (int i = x - 1; i >= 0; i--)
        {
            if (i == excludeX && y == excludeY) break; // Skip swap partner
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable || potionBoard[i, y].potion == null) break;
            Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(potionToPlace, neighbor)) break;
            hCount++;
        }
        
        // Right
        for (int i = x + 1; i < width; i++)
        {
            if (i == excludeX && y == excludeY) break; // Skip swap partner
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable || potionBoard[i, y].potion == null) break;
            Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(potionToPlace, neighbor)) break;
            hCount++;
        }
        
        if (hCount >= 3) return true;
        
        int vCount = 1;
        
        // Down
        for (int j = y - 1; j >= 0; j--)
        {
            if (x == excludeX && j == excludeY) break; // Skip swap partner
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable || potionBoard[x, j].potion == null) break;
            Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(potionToPlace, neighbor)) break;
            vCount++;
        }
        
        // Up
        for (int j = y + 1; j < height; j++)
        {
            if (x == excludeX && j == excludeY) break; // Skip swap partner
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable || potionBoard[x, j].potion == null) break;
            Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(potionToPlace, neighbor)) break;
            vCount++;
        }
        
        if (vCount >= 3) return true;
        
        return false;
    }
    
    public IEnumerator ShuffleBoard()
    {
        if (isShuffling)
            yield break;
            
        isShuffling = true;
        isProcessingMove = true;
        
        if (shuffleText != null)
        {
            shuffleText.gameObject.SetActive(true);
            shuffleText.text = "Shuffling because there is no legal move left!";
        }
        
        yield return new WaitForSeconds(1.5f);
        
        List<GameObject> allPotions = new List<GameObject>();
        List<Vector2Int> allPositions = new List<Vector2Int>();
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    allPotions.Add(potionBoard[x, y].potion);
                    allPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        
        // Shuffle the potions list
        for (int i = 0; i < allPotions.Count; i++)
        {
            int randomIndex = Random.Range(i, allPotions.Count);
            GameObject temp = allPotions[i];
            allPotions[i] = allPotions[randomIndex];
            allPotions[randomIndex] = temp;
        }
        
        // Reassign potions to positions
        for (int i = 0; i < allPositions.Count; i++)
        {
            int x = allPositions[i].x;
            int y = allPositions[i].y;
            GameObject potion = allPotions[i];
            
            potionBoard[x, y].potion = potion;
            Potion potionComponent = potion.GetComponent<Potion>();
            potionComponent.SetIndices(x, y);
            
            Vector3 worldPos = GetWorldPositionForCell(x, y);
            
            potionComponent.MoveToTarget(worldPos);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (shuffleText != null)
        {
            shuffleText.gameObject.SetActive(false);
        }
        
        isShuffling = false;
        isProcessingMove = false;
        
        if (CheckBoard())
        {
            yield return StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }
    
    public void CheckForValidMoves()
    {
        if (isProcessingMove || isShuffling)
            return;
            
        if (GameManager.instance != null && GameManager.instance.isGameEnded)
            return;
        
        bool hasMoves = HasValidMoves();
        Debug.Log($"🔍 Valid moves check: {(hasMoves ? "FOUND moves" : "NO moves - will shuffle")}");
        
        if (!hasMoves)
        {
            Debug.LogWarning("⚠️ No valid moves detected! Shuffling board...");
            StartCoroutine(ShuffleBoard());
        }
    }
    
    #endregion
    
    #region Special Candy Activation
    
    /// <summary>
    /// Activate a special candy (called when double-clicked)
    /// </summary>
    /// <summary>
    /// Wrapper for activating special candy from double-click
    /// Manages isProcessingMove and cleanup
    /// </summary>
    private IEnumerator ActivateSpecialCandyWithCleanup(Potion specialCandy)
    {
        yield return StartCoroutine(ActivateSpecialCandySequence(specialCandy));
        
        // After activation, check if there are matches and handle accordingly
        if (CheckBoard())
        {
            Debug.Log("🔄 Matches found after special candy activation (double-click), processing cascades");
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
            // ProcessTurnOnMatchedBoard will reset isProcessingMove
        }
        else
        {
            Debug.Log("✅ No matches after special candy activation (double-click), setting isProcessingMove = false");
            SnapAllPotionsToGrid();
            CheckForValidMoves();
            isProcessingMove = false;
        }
    }
    
    /// <summary>
    /// Activate a special candy at its position
    /// NOTE: This does NOT manage isProcessingMove - caller must do that
    /// </summary>
    private IEnumerator ActivateSpecialCandySequence(Potion specialCandy)
    {
        if (specialCandyManager == null)
        {
            Debug.LogWarning("Special Candy Manager not assigned!");
            yield break;
        }
        
        // NOTE: isProcessingMove is managed by the caller (ProcessMatches or SelectPotion)
        // Don't set it here to avoid conflicts
        
        int x = specialCandy.xIndex;
        int y = specialCandy.yIndex;
        BTSCandyType candyType = specialCandy.candyType;
        
        Debug.Log($"🎯 ActivateSpecialCandySequence: Activating {candyType} at ({x},{y})");
        
        yield return StartCoroutine(specialCandyManager.ActivateSpecialCandy(candyType, x, y));
        
        Debug.Log($"📋 After activation: potionsToRemove.Count = {potionsToRemove.Count}");
        
        // Destroy the special candy itself
        Destroy(specialCandy.gameObject);
        potionBoard[x, y] = new Node(true, null);
        
        // Process all the candies that were marked for removal during activation
        if (potionsToRemove.Count > 0)
        {
            Debug.Log($"Removing {potionsToRemove.Count} candies from special activation");
            RemoveAndRefill(potionsToRemove);
            potionsToRemove.Clear();
        }
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"🏁 ActivateSpecialCandySequence complete");
        // Caller will handle isProcessingMove and subsequent match checking
    }
    
    /// <summary>
    /// Handle combo when two special candies are combined
    /// </summary>
    private IEnumerator HandleSpecialCombo(Potion special1, Potion special2)
    {
        if (specialCandyManager == null)
        {
            Debug.LogWarning("Special Candy Manager not assigned!");
            yield break;
        }
        
        isProcessingMove = true;
        
        int x = special1.xIndex;
        int y = special1.yIndex;
        
        Debug.Log($"💥 ACTIVATING COMBO: {special1.candyType} + {special2.candyType}");
        
        yield return StartCoroutine(specialCandyManager.HandleSpecialCombo(
            special1.candyType, 
            special2.candyType, 
            x, 
            y
        ));
        
        // Destroy both special candies
        Destroy(special1.gameObject);
        Destroy(special2.gameObject);
        potionBoard[special1.xIndex, special1.yIndex] = new Node(true, null);
        potionBoard[special2.xIndex, special2.yIndex] = new Node(true, null);
        
        // Process all the candies that were marked for removal during combo
        if (potionsToRemove.Count > 0)
        {
            Debug.Log($"Removing {potionsToRemove.Count} candies from combo activation");
            RemoveAndRefill(potionsToRemove);
            potionsToRemove.Clear();
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (CheckBoard())
        {
            Debug.Log("🔄 Matches found after combo, processing cascades");
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
            // Don't set isProcessingMove = false here, ProcessTurnOnMatchedBoard will do it
        }
        else
        {
            Debug.Log("✅ No matches after combo, setting isProcessingMove = false");
            SnapAllPotionsToGrid();
            CheckForValidMoves();
            isProcessingMove = false;
        }
    }
    
    /// <summary>
    /// Clear a candy at specific position (called by SpecialCandyManager)
    /// </summary>
    public void ClearCandyAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;
            
        if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
        {
            Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
            if (!potionsToRemove.Contains(potion))
            {
                potionsToRemove.Add(potion);
                potion.isMatched = true;
            }
        }
    }
    
    #endregion


}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
    public MatchType matchType = MatchType.Normal3;
    public bool createSpecialCandy = false;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
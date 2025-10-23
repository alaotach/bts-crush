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
    
    [SerializeField]
    public bool autoFitToScreen = true;
    public bool spacingInPixels = false;
    public float potionSize = 200f; 
    public float screenPaddingPercent = 0.1f; //padding
    public Vector2 manualBoardOffset = new Vector2(0, -50f); 
    public float manualPotionParentScale = 1.2f; 
    
    [SerializeField]
    private Vector2 boardOffset;
    [SerializeField]
    private float potionParentScale; 
    
    public bool autoAdjustForOrientation = true;
    public float portraitScale = 0.8f;
    public Vector2 portraitOffset = new Vector2(0, 100f); 
    
    public GameObject[] potionPrefabs;
    
    public bool allowInitialMatches = true;
    public BTSCandyDatabase candyDatabase;
    public BTSSpecialCandyManager specialCandyManager;
    
    public Node[,] potionBoard;
    public GameObject potionBoardGO;
    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;

    [SerializeField]
    private Potion selectedPotion;

    [SerializeField]
    private bool isProcessingMove;

    public ArrayLayout arrayLayout;
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
    
    public TMPro.TMP_Text shuffleText; 
    private bool isShuffling = false;

    public GameObject selectionIndicator;
    public float blinkSpeed = 0.5f;
    private Coroutine blinkCoroutine;
    private Vector3 indicatorOriginalScale;

    private Vector2 dragStartPos;
    private Potion dragStartPotion;
    private bool isDragging = false;
    private float dragThreshold = 0.5f; 

    private void Awake()
    {
        Instance = this;

        if (specialCandyManager == null)
        {
            
            specialCandyManager = GetComponent<BTSSpecialCandyManager>();
            if (specialCandyManager == null)
            {
                specialCandyManager = FindAnyObjectByType<BTSSpecialCandyManager>();
            }
        }
        
        if (selectionIndicator != null)
        {
            indicatorOriginalScale = selectionIndicator.transform.localScale;
        }
        
        if (autoFitToScreen)
        {
            CalculateOptimalBoardTransform();
        }
        else
        {
            boardOffset = manualBoardOffset;
            potionParentScale = manualPotionParentScale;
            if (potionParentScale < 0.01f)
            {
                potionParentScale = 0.65f;
            }
            
            if (autoAdjustForOrientation)
            {
                AdjustForScreenOrientation();
            }
        }
        
        ApplyBoardTransform();
    }
    

    private void CalculateOptimalBoardTransform()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            boardOffset = manualBoardOffset;
            potionParentScale = manualPotionParentScale;
            return;
        }

        bool usingPixels = spacingInPixels || potionSpacingX > 10f || potionSpacingY > 10f;

        float effectiveSpacingX = potionSpacingX;
        float effectiveSpacingY = potionSpacingY;
        float effectivePotionSize = potionSize;
        
        if (usingPixels)
        {
            float pixelsPerUnit = 100f;
            if (potionSpacingX > 100f || potionSize > 100f)
            {
                pixelsPerUnit = 200f;
            }
            
            effectiveSpacingX = potionSpacingX / pixelsPerUnit;
            effectiveSpacingY = potionSpacingY / pixelsPerUnit;
            effectivePotionSize = potionSize / pixelsPerUnit;
        }
        
        float cellSizeX = effectivePotionSize + effectiveSpacingX;
        float cellSizeY = effectivePotionSize + effectiveSpacingY;
        
        float boardWidthLocal = (width - 1) * cellSizeX + effectivePotionSize;
        float boardHeightLocal = (height - 1) * cellSizeY + effectivePotionSize;
                
        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;
        
        float availableWidth = screenWidth * (1f - screenPaddingPercent * 2f);
        float availableHeight = screenHeight * (1f - screenPaddingPercent * 2f);
        
        float scaleForWidth = availableWidth / boardWidthLocal;
        float scaleForHeight = availableHeight / boardHeightLocal;
        
        potionParentScale = Mathf.Min(scaleForWidth, scaleForHeight);
        
        if (potionParentScale < 0.01f)
        {
            potionParentScale = 0.1f;
        }
        
        float scaledBoardWidth = boardWidthLocal * potionParentScale;
        float scaledBoardHeight = boardHeightLocal * potionParentScale;
        boardOffset = Vector2.zero;
        
        float topUISpace = screenHeight * 0.15f;
        boardOffset.y = -topUISpace / 2f;        
    }
    

    private void ApplyBoardTransform()
    {
        if (potionParent != null)
        {
            potionParent.transform.localPosition = new Vector3(boardOffset.x, boardOffset.y, 0);
            potionParent.transform.localScale = Vector3.one * potionParentScale;
        }
    }
    

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

    private void RecalculateFromInspector()
    {
        RecalculateBoardTransform();
    }

    private void TestSmallBoard()
    {
        width = 6;
        height = 8;
        RecalculateBoardTransform();
    }
    
    private void TestMediumBoard()
    {
        width = 8;
        height = 10;
        RecalculateBoardTransform();
    }
    
    private void TestLargeBoard()
    {
        width = 10;
        height = 12;
        RecalculateBoardTransform();
    }
    
    private void QuickFixUseSensibleDefaults()
    {
        spacingInPixels = false;
        
        potionSpacingX = 1.5f;
        potionSpacingY = 1.5f;
        potionSize = 1.2f;
        
        manualBoardOffset = new Vector2(0, -1);
        manualPotionParentScale = 0.65f;
        
        if (Application.isPlaying)
        {
            RecalculateBoardTransform();
        }        
    }
    
    private void FixKeepSpacingAdjustScale()
    {
        
        spacingInPixels = false;
        
        manualBoardOffset = new Vector2(0, -20);
        manualPotionParentScale = 0.02f; 
        if (Application.isPlaying)
        {
            RecalculateBoardTransform();
        }
    }
    
    private void EnableAutoFit()
    {
        autoFitToScreen = true;
        
        if (Application.isPlaying)
        {
            RecalculateBoardTransform();
        }
    }
    
    private void DisableAutoFit()
    {
        autoFitToScreen = false;
        
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
        }
    }

    private Vector2 GetCellSize()
    {
        return new Vector2(potionSpacingX, potionSpacingY);
    }

    public Vector3 GetWorldPositionForCell(int x, int y)
    {
        if (potionParent == null)
        {
            return Vector3.zero;
        }
        
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        Vector3 localPos = new Vector3(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY, 0);
        return potionParent.transform.TransformPoint(localPos);
    }

    private Vector2 GetLocalPositionForCell(int x, int y)
    {
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        return new Vector2(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY);
    }
    
    private void ApplyCandySprite(GameObject candyObject, BTSCandyType candyType)
    {
        if (candyDatabase == null)
        {
            return;
        }
        
        BTSCandyData candyData = candyDatabase.GetCandyData(candyType);
        if (candyData == null || candyData.sprite == null)
        {
            return;
        }
        
        SpriteRenderer spriteRenderer = candyObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = candyData.sprite;
        }
    }

    private void ApplyColorTintToSpecialCandy(GameObject candyObject, BTSCandyType baseColor)
    {
        SpriteRenderer spriteRenderer = candyObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        
        Color tintColor = GetMemberTintColor(baseColor);
        
        spriteRenderer.color = tintColor;
        
    }

    private Color GetMemberTintColor(BTSCandyType member)
    {
        switch (member)
        {
            case BTSCandyType.RM:
                return new Color(1f, 0.5f, 1f, 1f); 
                
            case BTSCandyType.Jin:
                return new Color(1f, 0.7f, 0.85f, 1f);
                
            case BTSCandyType.Suga:
                return new Color(0.8f, 0.8f, 0.8f, 1f); 
                
            case BTSCandyType.JHope:
                return new Color(1f, 0.8f, 0.5f, 1f); 
                
            case BTSCandyType.Jimin:
                return new Color(1f, 1f, 0.6f, 1f); 
                
            case BTSCandyType.V:
                return new Color(0.6f, 1f, 0.7f, 1f);
                
            case BTSCandyType.Jungkook:
                return new Color(0.7f, 0.85f, 1f, 1f);
                
            default:
                return Color.white;
        }
    }

    void Start()
    {
        if (autoFitToScreen)
        {
            CalculateOptimalBoardTransform();
            ApplyBoardTransform();
        }
        
        StartCoroutine(InitializeBoardCoroutine());
    }

    private void Update()
    {
        HandleInput();
    }
    
#if UNITY_EDITOR

    private void OnValidate()
    {
        if (arrayLayout != null)
        {
            if (arrayLayout.rows == null || arrayLayout.rows.Length != height)
            {
                System.Array.Resize(ref arrayLayout.rows, height);
            }
            for (int i = 0; i < height; i++)
            {
                if (arrayLayout.rows[i].row == null || arrayLayout.rows[i].row.Length != width)
                {
                    bool[] newRow = new bool[width];
                    if (arrayLayout.rows[i].row != null)
                    {
                        int copyLength = Mathf.Min(arrayLayout.rows[i].row.Length, width);
                        System.Array.Copy(arrayLayout.rows[i].row, newRow, copyLength);
                    }
                    arrayLayout.rows[i].row = newRow;
                }
            }
        }
        
        if (Application.isPlaying && autoFitToScreen && potionParent != null)
        {
            CalculateOptimalBoardTransform();
            ApplyBoardTransform();
        }
    }
#endif
    
    private void HandleInput()
    {
        if (!isBoardInitialized) return;

        if (GameManager.instance != null && GameManager.instance.isGameEnded)
        {
            return;
        }
        
        if (isProcessingMove)
        {
            return;
        }
        
        Vector2 inputPosition = Vector2.zero;
        bool inputPressed = false;
        
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            inputPressed = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        }
        else if (Mouse.current != null)
        {
            inputPosition = Mouse.current.position.ReadValue();
            inputPressed = Mouse.current.leftButton.wasPressedThisFrame;
        }
        else
        {
            return;
        }
        
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        if (inputPressed)
        {
            if (hit.collider != null)
            {
                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                if (potion != null && !potion.isMatched)
                {
                    dragStartPotion = potion;
                    dragStartPos = inputPosition;
                    isDragging = true;
                    SelectPotion(potion);
                }
            }
        }
        
        bool isStillPressed = false;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            isStillPressed = true;
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            isStillPressed = true;
            inputPosition = Mouse.current.position.ReadValue();
        }
        
        if (isStillPressed && isDragging && dragStartPotion != null)
        {
            Vector2 currentPos = inputPosition;
            float dragDistance = Vector2.Distance(dragStartPos, currentPos);
            
            if (dragDistance > dragThreshold * 100f)
            {
               
                Vector2 dragDir = (currentPos - dragStartPos).normalized;
                
                int targetX = dragStartPotion.xIndex;
                int targetY = dragStartPotion.yIndex;
                
                if (Mathf.Abs(dragDir.x) > Mathf.Abs(dragDir.y))
                {
                    targetX += dragDir.x > 0 ? 1 : -1;
                }
                else
                {
                    targetY += dragDir.y > 0 ? 1 : -1;
                }
                
                if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
                {
                    if (potionBoard[targetX, targetY] != null && potionBoard[targetX, targetY].isUsable && potionBoard[targetX, targetY].potion != null)
                    {
                        Potion targetPotion = potionBoard[targetX, targetY].potion.GetComponent<Potion>();
                        
                        if (targetPotion != null && dragStartPotion != null && isAdjacent(dragStartPotion, targetPotion))
                        {
                            SwapPotion(dragStartPotion, targetPotion);
                        }
                        isDragging = false;
                        dragStartPotion = null;
                    }
                }
            }
        }
        
        bool wasReleased = false;
        if (Touchscreen.current != null)
        {
            wasReleased = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
            if (wasReleased) inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            wasReleased = Mouse.current.leftButton.wasReleasedThisFrame;
            if (wasReleased) inputPosition = Mouse.current.position.ReadValue();
        }
        
        if (wasReleased)
        {
            if (isDragging && dragStartPotion != null)
            {
                Vector2 currentPos = inputPosition;
                float dragDistance = Vector2.Distance(dragStartPos, currentPos);
                if (dragDistance <= dragThreshold * 100f)
                {
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
        if (candyDatabase == null)
        {
            yield break;
        }
        
        if (candyDatabase.candies == null || candyDatabase.candies.Length < 7)
        {
            yield break;
        }
        
        if (potionPrefabs.Length < 1)
        {
            yield break;
        }
        
        candyDatabase.InitializeActiveMembersForGame();
        
        if (recursionCount > MAX_RECURSION)
        {
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
            yield break;
        }
        
        if (arrayLayout.rows.Length != height)
        {
            System.Array.Resize(ref arrayLayout.rows, height);
        }
        
        for (int i = 0; i < height; i++)
        {
            if (arrayLayout.rows[i].row == null || arrayLayout.rows[i].row.Length != width)
            {
                bool[] newRow = new bool[width];
                if (arrayLayout.rows[i].row != null)
                {
                    int copyLength = Mathf.Min(arrayLayout.rows[i].row.Length, width);
                    System.Array.Copy(arrayLayout.rows[i].row, newRow, copyLength);
                }
                arrayLayout.rows[i].row = newRow;
            }
        }
        
        if (potionPrefabs.Length == 0)
        {
            yield break;
        }

        yield return null;

        if (autoFitToScreen)
        {
            CalculateOptimalBoardTransform();
            ApplyBoardTransform();
        }
        
        yield return null;

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
                        PlaceRandomCandyAt(x, y);
                    }
                    else
                    {
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
        
        int maxPasses = 10;
        int passCount = 0;
        bool hasMatches;
        
        do
        {
            passCount++;
            hasMatches = CheckBoard();
            
            if (hasMatches)
            {
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
                break;
            }
        }
        while (hasMatches);
        
        PrintBoardState();        
        isBoardInitialized = true; 
        recursionCount = 0;
    }

    void PrintBoardState()
    {
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
        }
    }

    void InitializeBoard()
    {
        StartCoroutine(InitializeBoardCoroutine());
    }
    

    public void RestartWithNewMembers()
    {
        ClearAllPotions();
        
        if (candyDatabase != null)
        {
            candyDatabase.InitializeActiveMembersForGame();
        }
        StartCoroutine(InitializeBoardCoroutine());
    }

    void PlaceRandomCandyAt(int x, int y)
    {
        if (potionPrefabs == null || potionPrefabs.Length == 0)
        {
            return;
        }
        
        Vector2 position = GetLocalPositionForCell(x, y);
        
        GameObject potion = Instantiate(potionPrefabs[0], Vector3.zero, Quaternion.identity);
        potion.transform.SetParent(potionParent.transform);
        potion.transform.localPosition = position;
        potionsToDestroy.Add(potion);
        
        Potion potionComponent = potion.GetComponent<Potion>();
        if (potionComponent == null)
        {
            Destroy(potion);
            return;
        }
        
        potionComponent.SetIndices(x, y);
        if (candyDatabase != null)
        {
            potionComponent.candyType = candyDatabase.GetRandomRegularCandy();
        }
        else
        {
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            BTSCandyType randomType = potionPrefabs[randomIndex].GetComponent<Potion>().candyType;
            potionComponent.candyType = randomType;
        }
        
        ApplyCandySprite(potion, potionComponent.candyType);
        potionBoard[x, y] = new Node(true, potion);
    }

    void PlaceNonMatchingAt(int x, int y)
    {
        if (potionPrefabs == null || potionPrefabs.Length == 0)
        {
            return;
        }
        
        Vector2 position = GetLocalPositionForCell(x, y);
        
        if (x == 0 && y == 0)
        {
            Vector2 cellSize = GetCellSize();
        }
        const int maxAttempts = 100;
        GameObject potion = null;
        Potion potionComponent = null;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (potion != null) Destroy(potion);
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            potion = Instantiate(potionPrefabs[randomIndex], Vector3.zero, Quaternion.identity);
            potion.transform.SetParent(potionParent.transform);
            potion.transform.localPosition = position;
            potionsToDestroy.Add(potion);
            potionComponent = potion.GetComponent<Potion>();
            
            if (potionComponent == null)
            {
                continue;
            }
            
            potionComponent.SetIndices(x, y);
            
            ApplyCandySprite(potion, potionComponent.candyType);
            potionBoard[x, y] = new Node(true, potion);
            
            bool formsMatch = WouldFormMatchAt(x, y, potionComponent.candyType);
            
            if (!formsMatch)
            {
                return;
            }
            else
            {
                potionBoard[x, y] = new Node(false, null);
            }
        }
        
        for (int prefabIndex = 0; prefabIndex < potionPrefabs.Length; prefabIndex++)
        {
            if (potion != null) Destroy(potion);
            potion = Instantiate(potionPrefabs[prefabIndex], Vector3.zero, Quaternion.identity);
            potion.transform.SetParent(potionParent.transform);
            potion.transform.localPosition = position;
            potionsToDestroy.Add(potion);
            potionComponent = potion.GetComponent<Potion>();
            potionComponent.SetIndices(x, y);
            
            potionBoard[x, y] = new Node(true, potion);
            
            bool formsMatch = WouldFormMatchAt(x, y, potionComponent.candyType);
            
            if (!formsMatch)
            {
                return;
            }
            else
            {
                potionBoard[x, y] = new Node(false, null);
            }
        }

        potionBoard[x, y] = new Node(true, potion);
    }

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
                    Destroy(potionBoard[x, y].potion);
                    potionBoard[x, y] = new Node(false, null); // Mark empty
                    PlaceNonMatchingAt(x, y);
                    changes++;
                }
            }
        }
        return changes;
    }
    
    bool WouldFormMatchAt(int x, int y, BTSCandyType type)
    {
        if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable)
            return false;
            
        Potion currentPotion = potionBoard[x, y].potion.GetComponent<Potion>();
            
        int hCount = 1;
        for (int i = x - 1; i >= 0; i--)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            hCount++;
        }
        for (int i = x + 1; i < width; i++)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            hCount++;
        }
        if (hCount >= 3)
        {
            return true;
        }

        int vCount = 1; 
        for (int j = y - 1; j >= 0; j--)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            vCount++;
        }
        for (int j = y + 1; j < height; j++)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            vCount++;
        }
        if (vCount >= 3)
        {
            return true;
        }
        
        return false;
    }

    void VerifyNoMatches()
    {
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
                    if (!CanMatch(p, neighbor)) break;
                    hCount++;
                }
                for (int i = x + 1; i < width; i++)
                {
                    if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
                    Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
                    if (!CanMatch(p, neighbor)) break;
                    hCount++;
                }
                
                int vCount = 1;
                for (int j = y - 1; j >= 0; j--)
                {
                    if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
                    Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
                    if (!CanMatch(p, neighbor)) break;
                    vCount++;
                }
                for (int j = y + 1; j < height; j++)
                {
                    if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
                    Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
                    if (!CanMatch(p, neighbor)) break;
                    vCount++;
                }
            }
        }
    }

    bool FormsLineOfThreeAt(int x, int y, BTSCandyType type)
    {
        if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable)
            return false;

        Potion currentPotion = potionBoard[x, y].potion.GetComponent<Potion>();
        int count = 1;
        for (int i = x - 1; i >= 0; i--)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            count++;
        }
        for (int i = x + 1; i < width; i++)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            count++;
        }
        if (count >= 3) return true;

        count = 1;
        for (int j = y - 1; j >= 0; j--)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            count++;
        }
        for (int j = y + 1; j < height; j++)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(currentPotion, p)) break;
            count++;
        }
        return count >= 3;
    }

    public bool CheckBoard()
    {
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
                if (potionBoard[x,y] != null && potionBoard[x,y].isUsable)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    if(!potion.isMatched)
                    {
                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            MatchResult superMatchedPotions = SuperMatch(matchedPotions);
                            if (specialCandyManager != null)
                            {
                                specialCandyManager.PlayMatchSound(matchedPotions.connectedPotions.Count);
                            }
                            
                            if (superMatchedPotions.createSpecialCandy)
                            {
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
        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        List<Potion> matchedSpecials = new();
        List<Potion> regularMatches = new();
        HashSet<Potion> processedPotions = new();

        foreach (Potion potion in potionsToRemove)
        {
            if (potion == null) continue;
            if (!processedPotions.Add(potion)) continue; // Avoid duplicates
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
            if (special == null || special.gameObject == null) continue;
            int specialX = special.xIndex;
            int specialY = special.yIndex;
            if (specialX >= 0 && specialX < width && specialY >= 0 && specialY < height)
            {
                potionBoard[specialX, specialY] = new Node(true, null);
            }
            
            yield return StartCoroutine(ActivateSpecialCandySequence(special));
            if (special != null && special.gameObject != null)
            {
                Destroy(special.gameObject);
            }
        }

        regularMatches.RemoveAll(p => p == null || p.gameObject == null);

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
            if (specialCandyManager != null)
            {
                specialCandyManager.PlayCascadeSound();
            }
            
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            SnapAllPotionsToGrid();
            CheckForValidMoves();
            isProcessingMove = false;
        }
    }

    private int RemoveAndRefill(List<Potion> potionsToRemove, bool triggerSpecialChain = true)
    {
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
                if (specialCandyManager != null)
                {
                    specialCandyManager.PlaySpecialCreatedSound();
                }
                
                SpawnSpecialCandyAt(specialSpawn.x, specialSpawn.y, specialSpawn.matchType, specialSpawn.originalType);
            }
        }
        
        specialCandiesToSpawn.Clear();
        
        List<Potion> chainSpecials = new();
        HashSet<Potion> countedPotions = new();
        int removedCount = 0;
        
        foreach (Potion potion in potionsToRemove)
        {
            if (potion == null || potion.gameObject == null) continue;
            
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            if (triggerSpecialChain && potion.isSpecialCandy)
            {
                chainSpecials.Add(potion);
                continue;
            }

            if (!triggerSpecialChain && potion.isSpecialCandy)
            {
                continue;
            }

            Destroy(potion.gameObject);

            if (countedPotions.Add(potion))
            {
                removedCount++;
            }
            if (_xIndex >= 0 && _xIndex < width && _yIndex >= 0 && _yIndex < height)
            {
                if (potionBoard[_xIndex, _yIndex].potion == potion.gameObject)
                {
                    potionBoard[_xIndex, _yIndex] = new Node(true, null);
                }
            }
        }

        if (triggerSpecialChain)
        {
            float delay = 0f;
            foreach (Potion special in chainSpecials)
            {
                if (special == null) continue;
                StartCoroutine(ActivateSpecialCandyWithDelay(special, delay));
                delay += 0.05f;
            }
        }

        for (int x = 0; x < width; x++)
        {
            RefillColumn(x);
        }

        return removedCount;
    }

    private void RefillColumn(int x)
    {
        float animationDelay = 0f;
        const float delayIncrement = 0.05f;
        for (int y = 0; y < height; y++)
        {
            if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion == null)
            {
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
                        break;
                    }
                }
            }
        }
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
        float offsetX = (width - 1) / 2f * potionSpacingX;
        Vector2 spawnPos = new Vector2(x * potionSpacingX - offsetX, height * potionSpacingY);
        
        GameObject newPotion = Instantiate(potionPrefabs[0], Vector3.zero, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.transform.localPosition = spawnPos; // Set in local space
        
        Potion potionComponent = newPotion.GetComponent<Potion>();
        potionComponent.SetIndices(x, y);
        if (candyDatabase != null)
        {
            potionComponent.candyType = candyDatabase.GetRandomRegularCandy();
        }
        else
        {
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            potionComponent.candyType = potionPrefabs[randomIndex].GetComponent<Potion>().candyType;
        }
        
        ApplyCandySprite(newPotion, potionComponent.candyType);
        
        potionBoard[x, y] = new Node(true, newPotion);
        
        Vector3 worldTargetPos = potionParent.transform.TransformPoint(localTargetPos);
        
        if (delay > 0f)
        {
            newPotion.GetComponent<Potion>().MoveToTarget(worldTargetPos, delay);
        }
        else
        {
            newPotion.GetComponent<Potion>().MoveToTarget(worldTargetPos);
        }
    }
    
    private void SpawnSpecialCandyAt(int x, int y, MatchType matchType, BTSCandyType originalType)
    {
        if (candyDatabase == null)
        {
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
        potionComponent.isSpecialCandy = true;
        potionComponent.baseColor = originalType;
        if (specialType == BTSCandyType.ColorBomb || specialType == BTSCandyType.SuperBomb)
        {
            ApplyCandySprite(specialCandy, specialType);
        }
        else
        {
            ApplyCandySprite(specialCandy, originalType);
        }
        
        Sprite balloonSprite = null;
        if (specialType == BTSCandyType.Balloon)
        {
            BTSCandyData candyData = candyDatabase.GetCandyData(BTSCandyType.Balloon);
            if (candyData != null)
            {
                balloonSprite = candyData.balloonSprite;
            }
        }
        
        potionComponent.UpdateVisualEffects(balloonSprite);
        
        potionBoard[x, y] = new Node(true, specialCandy);
        StartCoroutine(SpecialCandyCreationEffect(specialCandy));
    }
    
    private IEnumerator SpecialCandyCreationEffect(GameObject candy)
    {
        if (candy == null) yield break;
        
        yield return null;
        
        Vector3 originalScale = candy.transform.localScale;
        float duration = 0.3f;
        float elapsed = 0f;
        
        candy.transform.localScale = Vector3.zero;
        
        while (elapsed < duration)
        {
            if (candy == null) yield break;
            
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(0f, 1.2f, progress);
            candy.transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        duration = 0.1f;
        while (elapsed < duration)
        {
            if (candy == null) yield break;

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
            yOffset++;
        }

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            Vector3 worldTargetPos = GetWorldPositionForCell(x, y);
            potionAbove.MoveToTarget(worldTargetPos);
            potionAbove.SetIndices(x, y);
            potionBoard[x, y] = potionBoard[x, y + yOffset];
            potionBoard[x, y + yOffset] = new Node(true, null);
        }
        if (y + yOffset == height)
        {
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
        Vector2 localTargetPos = GetLocalPositionForCell(x, index);
        float offsetX = (width - 1) / 2f * potionSpacingX;
        Vector2 spawnPos = new Vector2(x * potionSpacingX - offsetX, height * potionSpacingY);
        
        GameObject newPotion = Instantiate(potionPrefabs[0], Vector3.zero, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.transform.localPosition = spawnPos;
        
        Potion potionComponent = newPotion.GetComponent<Potion>();
        potionComponent.SetIndices(x, index);
        if (candyDatabase != null)
        {
            potionComponent.candyType = candyDatabase.GetRandomRegularCandy();
        }
        else
        {
            int randomIndex = Random.Range(0, potionPrefabs.Length);
            potionComponent.candyType = potionPrefabs[randomIndex].GetComponent<Potion>().candyType;
        }
        
        ApplyCandySprite(newPotion, potionComponent.candyType);
        
        potionBoard[x, index] = new Node(true, newPotion);
        
        Vector3 worldTargetPos = potionParent.transform.TransformPoint(localTargetPos);
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
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    int totalCount = extraConnectedPotions.Count;
                    if (totalCount >= 6)
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.Match6Plus,
                            createSpecialCandy = true
                        };
                    }
                    else
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.TShape,
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
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    
                    int totalCount = extraConnectedPotions.Count;
                    if (totalCount >= 6)
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.Match6Plus,
                            createSpecialCandy = true
                        };
                    }
                    else
                    {
                        return new MatchResult
                        {
                            connectedPotions = extraConnectedPotions,
                            direction = MatchDirection.Super,
                            matchType = MatchType.LShape,
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
        BTSCandyType potionType = potion.isSpecialCandy ? potion.baseColor : potion.candyType;

        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        
        int horizontalCount = connectedPotions.Count;
        if (horizontalCount >= 3)
        {
            if (horizontalCount >= 6)
            {
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongHorizontal,
                    matchType = MatchType.Match6Plus,
                    createSpecialCandy = true
                };
            }
            else if (horizontalCount == 5)
            {
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongHorizontal,
                    matchType = MatchType.Match5,
                    createSpecialCandy = true
                };
            }
            else if (horizontalCount == 4)
            {
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
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Horizontal,
                    matchType = MatchType.Normal3,
                    createSpecialCandy = false
                };
            }
        }
        
        connectedPotions.Clear();
        connectedPotions.Add(potion);
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        CheckDirection(potion, new Vector2Int(0,-1), connectedPotions);

        int verticalCount = connectedPotions.Count;
        if (verticalCount >= 3)
        {
            if (verticalCount >= 6)
            {
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongVertical,
                    matchType = MatchType.Match6Plus,
                    createSpecialCandy = true
                };
            }
            else if (verticalCount == 5)
            {
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongVertical,
                    matchType = MatchType.Match5,
                    createSpecialCandy = true
                };
            }
            else if (verticalCount == 4)
            {
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

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x,y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();
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

    private bool CanMatch(Potion candy1, Potion candy2)
    {
        if (candy1.isSpecialCandy && candy2.isSpecialCandy)
        {
            return false;
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
                selectedPotion = _potion;
                ShowSelectionIndicator(_potion);
            }
            else
            {
                selectedPotion = _potion;
                ShowSelectionIndicator(_potion);
            }
        }
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
            HideSelectionIndicator();
        }
        else if (selectedPotion != _potion)
        {
            bool adjacent = isAdjacent(selectedPotion, _potion);
            if (!adjacent)
            {
                selectedPotion = _potion;
                ShowSelectionIndicator(_potion);
                return;
            }

            if (selectedPotion.isSpecialCandy && _potion.isSpecialCandy)
            {
                isProcessingMove = true;
                StartCoroutine(HandleSpecialCombo(selectedPotion, _potion));
                selectedPotion = null;
                HideSelectionIndicator();
            }
            else if (selectedPotion.isSpecialCandy || _potion.isSpecialCandy)
            {
                SwapPotion(selectedPotion, _potion);
                selectedPotion = null;
                HideSelectionIndicator();
            }
            else
            {
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
            pos.z = 0.1f; 
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
            return;
        }
        if (_currentPotion != null && _currentPotion.isMoving || _targetPotion != null && _targetPotion.isMoving)
        {
            return;
        }
        if (!isAdjacent(_currentPotion, _targetPotion))
        {
            return;
        }
        
        // Play switch sound
        if (specialCandyManager != null)
        {
            specialCandyManager.PlaySwapSound();
        }
        
        DoSwap(_currentPotion, _targetPotion);
        isProcessingMove = true;
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
        HideSelectionIndicator();
        yield return new WaitForSeconds(0.2f);
        
        bool specialActivated = false;
        yield return StartCoroutine(CheckAndActivateSpecialCandyCoroutine(_currentPotion, _targetPotion, 
            (activated) => { specialActivated = activated; }
        ));
        
        if (specialActivated)
        {
            if (CheckBoard())
            {
                yield return StartCoroutine(ProcessTurnOnMatchedBoard(true));
            }
            else
            {
                if (GameManager.instance != null)
                {
                    GameManager.instance.ProcessTurn(0, true);
                }
                SnapAllPotionsToGrid();
                CheckForValidMoves();
                isProcessingMove = false;
            }
        }
        else
        {
            bool hasMatched = CheckBoard();

            if (hasMatched)
            {
                yield return StartCoroutine(ProcessTurnOnMatchedBoard(true));
            }
            else
            {
                if (specialCandyManager != null)
                {
                    specialCandyManager.PlayInvalidSwapSound();
                }
                
                DoSwap(_currentPotion, _targetPotion);
                yield return new WaitForSeconds(0.3f);
                SnapAllPotionsToGrid(); 
                CheckForValidMoves();
                isProcessingMove = false;
            }
        }
    }
    

    private IEnumerator CheckAndActivateSpecialCandyCoroutine(Potion candy1, Potion candy2, System.Action<bool> callback)
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
            yield return StartCoroutine(HandleSpecialCombo(candy1, candy2));
            callback(true);
            yield break;
        }
        else
        {
            callback(false);
            yield break;
        }
        
        if (specialCandy != null && regularCandy != null)
        {
            if (specialCandy.candyType == BTSCandyType.ColorBomb)
            {
                yield return StartCoroutine(ActivateColorBombWithTarget(specialCandy, regularCandy));
                callback(true);
                yield break;
            }
            else if (specialCandy.candyType == BTSCandyType.SuperBomb)
            {
                yield return StartCoroutine(ActivateSuperBombWithTarget(specialCandy, regularCandy));
                callback(true);
                yield break;
            }
            else if (specialCandy.candyType == BTSCandyType.StripedHorizontal || 
                     specialCandy.candyType == BTSCandyType.StripedVertical)
            {
                if (specialCandy.baseColor == regularCandy.candyType)
                {
                    yield return StartCoroutine(ActivateSpecialCandySequence(specialCandy));
                    callback(true);
                    yield break;
                }
                else
                {
                    callback(false); 
                    yield break;
                }
            }
            else if (specialCandy.candyType == BTSCandyType.Balloon)
            {
                callback(false);
                yield break;
            }
            else
            {
                yield return StartCoroutine(ActivateSpecialCandySequence(specialCandy));
                callback(true);
                yield break;
            }
        }
        
        callback(false);
    }
    

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
            StartCoroutine(HandleSpecialCombo(candy1, candy2));
            return true;
        }
        else
        {
            return false;
        }
        
        if (specialCandy != null && regularCandy != null)
        {
            StartCoroutine(ActivateSpecialCandySequence(specialCandy));
            if (specialCandy.baseColor == regularCandy.candyType)
            {
                return false;
            }
            return true;
        }
        
        return false;
    }

    private bool isAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }
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
        if (WouldTypeCreateMatchAt(x1, y1, potion2, x2, y2))
            return true;
        
        if (WouldTypeCreateMatchAt(x2, y2, potion1, x1, y1))
            return true;
        
        return false;
    }
    private bool WouldTypeCreateMatchAt(int x, int y, Potion potionToPlace, int excludeX, int excludeY)
    {
        int hCount = 1;
        
        // Left
        for (int i = x - 1; i >= 0; i--)
        {
            if (i == excludeX && y == excludeY) break;
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable || potionBoard[i, y].potion == null) break;
            Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
            if (!CanMatch(potionToPlace, neighbor)) break;
            hCount++;
        }
        
        // Right
        for (int i = x + 1; i < width; i++)
        {
            if (i == excludeX && y == excludeY) break;
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
            if (x == excludeX && j == excludeY) break;
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable || potionBoard[x, j].potion == null) break;
            Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
            if (!CanMatch(potionToPlace, neighbor)) break;
            vCount++;
        }
        
        // Up
        for (int j = y + 1; j < height; j++)
        {
            if (x == excludeX && j == excludeY) break;
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
        
        for (int i = 0; i < allPotions.Count; i++)
        {
            int randomIndex = Random.Range(i, allPotions.Count);
            GameObject temp = allPotions[i];
            allPotions[i] = allPotions[randomIndex];
            allPotions[randomIndex] = temp;
        }
        
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
        
        if (!hasMoves)
        {
            StartCoroutine(ShuffleBoard());
        }
    }
    
    #endregion
    
    #region Special Candy Activation

    private IEnumerator ActivateSpecialCandyWithCleanup(Potion specialCandy)
    {
        yield return StartCoroutine(ActivateSpecialCandySequence(specialCandy));
        if (CheckBoard())
        {
            yield return StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            SnapAllPotionsToGrid();
            CheckForValidMoves();
            isProcessingMove = false;
        }
    }
    

    private IEnumerator ActivateSpecialCandySequence(Potion specialCandy)
    {
        if (specialCandyManager == null)
        {
            yield break;
        }

        int x = specialCandy.xIndex;
        int y = specialCandy.yIndex;
        BTSCandyType candyType = specialCandy.candyType;        
        yield return StartCoroutine(specialCandyManager.ActivateSpecialCandy(candyType, x, y));
        
        Destroy(specialCandy.gameObject);
        potionBoard[x, y] = new Node(true, null);

        int clearedByEffect = 0;
        if (potionsToRemove.Count > 0)
        {
            clearedByEffect += RemoveAndRefill(potionsToRemove);
            potionsToRemove.Clear();
        }

        if (GameManager.instance != null && clearedByEffect > 0)
        {
            GameManager.instance.ProcessTurn(clearedByEffect, false);
        }
        
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ActivateSpecialCandyWithDelay(Potion specialCandy, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        yield return StartCoroutine(ActivateSpecialCandySequence(specialCandy));
    }

    private IEnumerator HandleSpecialCombo(Potion special1, Potion special2)
    {
        if (specialCandyManager == null)
        {
            yield break;
        }
        
        isProcessingMove = true;
        
        int x = special1.xIndex;
        int y = special1.yIndex;        
    int comboClears = 0;
    if (special1 != null) comboClears++;
    if (special2 != null) comboClears++;
        
        yield return StartCoroutine(specialCandyManager.HandleSpecialCombo(
            special1.candyType, 
            special2.candyType,
            special1.baseColor,
            special2.baseColor,
            x, 
            y
        ));
        Destroy(special1.gameObject);
        Destroy(special2.gameObject);
        potionBoard[special1.xIndex, special1.yIndex] = new Node(true, null);
        potionBoard[special2.xIndex, special2.yIndex] = new Node(true, null);
        if (potionsToRemove.Count > 0)
        {
            comboClears += RemoveAndRefill(potionsToRemove);
            potionsToRemove.Clear();
        }

        if (GameManager.instance != null && comboClears > 0)
        {
            GameManager.instance.ProcessTurn(comboClears, false);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (CheckBoard())
        {
            yield return StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            SnapAllPotionsToGrid();
            CheckForValidMoves();
            isProcessingMove = false;
        }
    }

    private IEnumerator ActivateColorBombWithTarget(Potion colorBomb, Potion targetCandy)
    {
        isProcessingMove = true;
        BTSCandyType targetType = targetCandy.candyType;
        
        if (specialCandyManager != null)
        {
            yield return StartCoroutine(specialCandyManager.ActivateSpecialCandy(
                BTSCandyType.ColorBomb, colorBomb.xIndex, colorBomb.yIndex));
        }
        Destroy(colorBomb.gameObject);
        Destroy(targetCandy.gameObject);
        potionBoard[colorBomb.xIndex, colorBomb.yIndex] = new Node(true, null);
        potionBoard[targetCandy.xIndex, targetCandy.yIndex] = new Node(true, null);
        List<Potion> candiesToClear = new();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    if (potion != null && potion.candyType == targetType)
                    {
                        candiesToClear.Add(potion);
                        potion.isMatched = true;
                    }
                }
            }
        }
        
        if (candiesToClear.Count > 0)
        {
            int clearedCount = RemoveAndRefill(candiesToClear);
            if (GameManager.instance != null)
            {
                GameManager.instance.ProcessTurn(clearedCount, false);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ActivateSuperBombWithTarget(Potion superBomb, Potion targetCandy)
    {        
        isProcessingMove = true;
        
        if (specialCandyManager != null)
        {
            yield return StartCoroutine(specialCandyManager.ActivateSpecialCandy(
                BTSCandyType.SuperBomb, superBomb.xIndex, superBomb.yIndex));
        }
        
        Destroy(superBomb.gameObject);
        Destroy(targetCandy.gameObject);
        potionBoard[superBomb.xIndex, superBomb.yIndex] = new Node(true, null);
        potionBoard[targetCandy.xIndex, targetCandy.yIndex] = new Node(true, null);
        
        List<Potion> candiesToClear = new();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    if (potion != null && !potion.isSpecialCandy)
                    {
                        candiesToClear.Add(potion);
                        potion.isMatched = true;
                    }
                }
            }
        }
        
        
        // Remove and refill
        if (candiesToClear.Count > 0)
        {
            int clearedCount = RemoveAndRefill(candiesToClear);
            if (GameManager.instance != null)
            {
                GameManager.instance.ProcessTurn(clearedCount, false);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
    }

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
    
    public bool AnyPotionsMoving()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    if (potion != null && potion.isMoving)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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
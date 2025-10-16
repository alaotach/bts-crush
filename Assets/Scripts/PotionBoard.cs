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
    //define the transform for the potionParent container
    public Vector2 boardOffset = new Vector2(0, -50f); // Offset from center (move down by 50)
    public float potionParentScale = 1.2f;
    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;
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

    [SerializeField]
    List<Potion> potionsToRemove = new();
    
    public TMPro.TMP_Text shuffleText; // Reference to UI text for shuffle notification
    private bool isShuffling = false;
    
    // Selection indicator
    public GameObject selectionIndicator; // A square sprite to show selected potion
    public float blinkSpeed = 0.5f; // Time between blinks
    private Coroutine blinkCoroutine;
    
    // Drag variables
    private Vector2 dragStartPos;
    private Potion dragStartPotion;
    private bool isDragging = false;
    private float dragThreshold = 0.5f; // Minimum distance to trigger swap

    private void Awake()
    {
        Instance = this;
        
        // Position and scale the board
        if (potionParent != null)
        {
            // Apply offset from center (e.g., move down to align with background)
            potionParent.transform.localPosition = new Vector3(boardOffset.x, boardOffset.y, 0);
            potionParent.transform.localScale = Vector3.one * potionParentScale;
            Debug.Log($"[Awake] PotionParent positioned: LocalPos={potionParent.transform.localPosition}, Scale={potionParent.transform.localScale}");
        }
    }

    void Start()
    {
        if (potionParent == null)
        {
            Debug.LogError("potionParent is NULL! Please assign the 'Potions' GameObject in the Inspector.");
        }
        else
        {
            // Force apply transform settings  
            potionParent.transform.localPosition = new Vector3(boardOffset.x, boardOffset.y, 0);
            potionParent.transform.localScale = Vector3.one * potionParentScale;
            Debug.Log($"[Start] FORCING PotionParent transform:");
            Debug.Log($"  BoardOffset from Inspector: {boardOffset}");
            Debug.Log($"  Scale from Inspector: {potionParentScale}");
            Debug.Log($"  Applied LocalPosition: {potionParent.transform.localPosition}");
            Debug.Log($"  Applied LocalScale: {potionParent.transform.localScale}");
            Debug.Log($"  World Position: {potionParent.transform.position}");
        }
        
        if (potionBoardGO == null)
        {
            Debug.LogError("potionBoardGO is NULL! Please assign the 'PotionBoard' GameObject in the Inspector.");
        }
        
        // Initialize GameManager with starting values
        if (GameManager.instance != null)
        {
            GameManager.instance.Initialize(10, 30); // 10 moves, 30 points goal
        }
        
        StartCoroutine(InitializeBoardCoroutine());
    }

    private void Update()
    {
        // Continuously update transform to match Inspector values
        if (potionParent != null && !isProcessingMove)
        {
            potionParent.transform.localPosition = new Vector3(boardOffset.x, boardOffset.y, 0);
            potionParent.transform.localScale = Vector3.one * potionParentScale;
        }
        
        HandleInput();
    }
    
    private void HandleInput()
    {
        if (Mouse.current == null) return;
        
        // Don't allow input if game has ended
        if (GameManager.instance != null && GameManager.instance.isGameEnded)
        {
            return;
        }
        
        if (isProcessingMove)
        {
            return;
        }
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        // Mouse button pressed - start drag or select
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                dragStartPotion = potion;
                dragStartPos = mousePosition;
                isDragging = true;
                SelectPotion(potion);
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
                
                // Find which direction was dragged most
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
                
                // Check if target is valid
                if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
                {
                    if (potionBoard[targetX, targetY] != null && potionBoard[targetX, targetY].isUsable && potionBoard[targetX, targetY].potion != null)
                    {
                        Potion targetPotion = potionBoard[targetX, targetY].potion.GetComponent<Potion>();
                        SwapPotion(dragStartPotion, targetPotion);
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
        // Clear all potions from the board
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
        
        // Check if we have enough potion types
        if (potionPrefabs.Length < 3)
        {
            Debug.LogError($"Not enough potion prefabs! You have {potionPrefabs.Length}, but need at least 3-5 for a playable board.");
            yield break;
        }
        
        if (recursionCount > MAX_RECURSION)
        {
            Debug.LogError("Too many recursions! Board might have an impossible configuration.");
            recursionCount = 0;
            yield break;
        }
        
        // Destroy all existing potions before creating new ones
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
            // Wait one frame for objects to be destroyed
            yield return null;
        }

        potionBoard = new Node[width, height];

        // Validate setup
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
                    PlaceNonMatchingAt(x, y);
                }
            }
        }

        // One cleanup pass to resolve any leftovers (e.g., due to prefab potionType mismatches)
        int totalFixed = 0;
        int pass = 0;
        int fixedThisPass;
        do
        {
            pass++;
            fixedThisPass = ResolveInitialMatches();
            totalFixed += fixedThisPass;
            Debug.Log($"ResolveInitialMatches pass {pass}: fixed {fixedThisPass} cells");
            
            if (pass > 10)
            {
                Debug.LogError("Too many resolve passes! Breaking to avoid infinite loop.");
                break;
            }
        }
        while (fixedThisPass > 0);
        
        Debug.Log($"Total cells fixed across {pass} passes: {totalFixed}");
        
        // Print the board state for debugging
        PrintBoardState();
        
        // Final verification - log any remaining matches
        VerifyNoMatches();
        
        Debug.Log("Board created successfully with no initial matches!");
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
                    row += p.potionType.ToString()[0] + " ";
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

    // Try to place a potion at x,y that doesn't form a line of 3 with already placed neighbors
    void PlaceNonMatchingAt(int x, int y)
    {
        if (potionPrefabs == null || potionPrefabs.Length == 0)
        {
            Debug.LogError($"Cannot place potion at ({x},{y}): potionPrefabs is null or empty!");
            return;
        }
        
        // Center the board: offset so the grid center is at (0,0)
        // For even counts (6 items), center is between items at 2.5
        // For odd counts (5 items), center is at item 2
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        Vector2 position = new Vector2(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY);
        const int maxAttempts = 1000;
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
            
            // CRITICAL: Set the board slot temporarily to check
            potionBoard[x, y] = new Node(true, potion);
            
            // Check if this would form a match
            bool formsMatch = WouldFormMatchAt(x, y, potionComponent.potionType);
            
            if (!formsMatch)
            {
                // Success! Keep this potion
                return;
            }
            else
            {
                // Forms a match, remove it and try again
                potionBoard[x, y] = new Node(false, null);
            }
        }
        
        // If we exhausted attempts, try each potion type systematically
        Debug.LogWarning($"Exhausted random attempts at ({x},{y}), trying systematically...");
        for (int prefabIndex = 0; prefabIndex < potionPrefabs.Length; prefabIndex++)
        {
            if (potion != null) Destroy(potion);
            potion = Instantiate(potionPrefabs[prefabIndex], position, Quaternion.identity);
            potion.transform.SetParent(potionParent.transform);
            potionsToDestroy.Add(potion);
            potionComponent = potion.GetComponent<Potion>();
            potionComponent.SetIndices(x, y);
            
            // CRITICAL: Set the board slot temporarily
            potionBoard[x, y] = new Node(true, potion);
            
            bool formsMatch = WouldFormMatchAt(x, y, potionComponent.potionType);
            
            if (!formsMatch)
            {
                Debug.Log($"Systematic placement worked with prefab {prefabIndex} ({potionComponent.potionType}) at ({x},{y})");
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
                
                // Check if THIS cell is part of a 3+ match
                if (WouldFormMatchAt(x, y, p.potionType))
                {
                    // Yes, it forms a match, destroy it and place a different one
                    Destroy(potionBoard[x, y].potion);
                    potionBoard[x, y] = new Node(false, null); // Mark as empty
                    PlaceNonMatchingAt(x, y);
                    changes++;
                }
            }
        }
        return changes;
    }
    
    // Check if placing a potion of this type at (x,y) would form a 3+ line
    // Now checks with the potion already placed in the board
    bool WouldFormMatchAt(int x, int y, PotionType type)
    {
        if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable)
            return false;
            
        // Horizontal: count neighbors left and right (NOT including current cell)
        int hCount = 1; // Count the current position
        for (int i = x - 1; i >= 0; i--)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            hCount++;
        }
        for (int i = x + 1; i < width; i++)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            hCount++;
        }
        if (hCount >= 3)
        {
            Debug.Log($"Match detected: {hCount}x {type} horizontal at ({x},{y})");
            return true;
        }

        // Vertical: count neighbors up and down (NOT including current cell)
        int vCount = 1; // Count the current position
        for (int j = y - 1; j >= 0; j--)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            vCount++;
        }
        for (int j = y + 1; j < height; j++)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            vCount++;
        }
        if (vCount >= 3)
        {
            Debug.Log($"Match detected: {vCount}x {type} vertical at ({x},{y})");
            return true;
        }
        
        return false;
    }

    // Verify and log all matches found on the board
    void VerifyNoMatches()
    {
        bool foundMatch = false;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable) continue;
                
                Potion p = potionBoard[x, y].potion.GetComponent<Potion>();
                
                // Check horizontal
                int hCount = 1;
                for (int i = x - 1; i >= 0; i--)
                {
                    if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
                    Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
                    if (neighbor.potionType != p.potionType) break;
                    hCount++;
                }
                for (int i = x + 1; i < width; i++)
                {
                    if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
                    Potion neighbor = potionBoard[i, y].potion.GetComponent<Potion>();
                    if (neighbor.potionType != p.potionType) break;
                    hCount++;
                }
                if (hCount >= 3)
                {
                    Debug.LogError($"MATCH FOUND! Horizontal {hCount}x {p.potionType} at ({x},{y})");
                    foundMatch = true;
                }
                
                // Check vertical
                int vCount = 1;
                for (int j = y - 1; j >= 0; j--)
                {
                    if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
                    Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
                    if (neighbor.potionType != p.potionType) break;
                    vCount++;
                }
                for (int j = y + 1; j < height; j++)
                {
                    if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
                    Potion neighbor = potionBoard[x, j].potion.GetComponent<Potion>();
                    if (neighbor.potionType != p.potionType) break;
                    vCount++;
                }
                if (vCount >= 3)
                {
                    Debug.LogError($"MATCH FOUND! Vertical {vCount}x {p.potionType} at ({x},{y})");
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
    bool FormsLineOfThreeAt(int x, int y, PotionType type)
    {
        // Early out if cell blocked
        if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable)
            return false;

        // Horizontal: count contiguous same-type to left and right
        int count = 1;
        for (int i = x - 1; i >= 0; i--)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            count++;
        }
        for (int i = x + 1; i < width; i++)
        {
            if (potionBoard[i, y] == null || !potionBoard[i, y].isUsable) break;
            Potion p = potionBoard[i, y].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            count++;
        }
        if (count >= 3) return true;

        // Vertical: count contiguous same-type below and above
        count = 1;
        for (int j = y - 1; j >= 0; j--)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            count++;
        }
        for (int j = y + 1; j < height; j++)
        {
            if (potionBoard[x, j] == null || !potionBoard[x, j].isUsable) break;
            Potion p = potionBoard[x, j].potion.GetComponent<Potion>();
            if (p.potionType != type) break;
            count++;
        }
        return count >= 3;
    }

    public bool CheckBoard()
    {
        Debug.Log("=== Checking Board ===");
        bool hasMatched = false;

        potionsToRemove.Clear();

        foreach(Node nodePotion in potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        // Reset all isMatched flags first
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
                            Debug.Log($"Found match at ({x},{y}): {matchedPotions.connectedPotions.Count} {potion.potionType} potions, Direction: {matchedPotions.direction}");
                            MatchResult superMatchedPotions = SuperMatch(matchedPotions);
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

        Debug.Log($"=== CheckBoard Complete: hasMatched={hasMatched} ===");
        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Potion potion in potionsToRemove)
            {
                potion.isMatched = true;
            }
            RemoveAndRefill(potionsToRemove);
            GameManager.instance.ProcessTurn(potionsToRemove.Count, _subtractMoves);
            yield return new WaitForSeconds(0.4f);

            // Stop processing if game has ended
            if (GameManager.instance != null && GameManager.instance.isGameEnded)
            {
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
            }
    }

    private void RemoveAndRefill(List<Potion> potionsToRemove)
    {
        foreach (Potion potion in potionsToRemove)
        {
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            Destroy(potion.gameObject);
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x,y].potion == null)
                {
                Debug.Log("The Location X: " + x + " Y: " + y + "is empty, attempting to refill");
                RefillPotion(x,y);
                }
            }
        }
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

            // Center the board: center is between middle items
            float offsetX = (width - 1) / 2f * potionSpacingX;
            float offsetY = (height - 1) / 2f * potionSpacingY;
            Vector3 localTargetPos = new Vector3(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY, 0);
            Vector3 worldTargetPos = potionParent.transform.TransformPoint(localTargetPos);
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
        int randomIndex = Random.Range(0, potionPrefabs.Length);
        
        // Center the board: center is between middle items
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        
        // Spawn above the board - use local coordinates
        Vector2 spawnPos = new Vector2(x * potionSpacingX - offsetX, height * potionSpacingY - offsetY);
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], Vector3.zero, Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.transform.localPosition = spawnPos; // Set in local space
        
        newPotion.GetComponent<Potion>().SetIndices(x, index);
        potionBoard[x, index] = new Node(true, newPotion);
        
        // Calculate local position, then convert to world position for MoveToTarget
        Vector3 localTargetPos = new Vector3(x * potionSpacingX - offsetX, index * potionSpacingY - offsetY, 0);
        Vector3 worldTargetPos = potionParent.transform.TransformPoint(localTargetPos);
        Debug.Log($"Spawning potion at column {x}, target index {index}, local: {localTargetPos}, world: {worldTargetPos}");
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
                    Debug.Log("I have a Super Horizontal Match, the color of my match is: " + pot.potionType);
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
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
                    Debug.Log("I have a Super Vertical Match, the color of my match is: " + pot.potionType);
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        return null;
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        //check right
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        //check left
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        //have we made a 3+ match? (Horizontal Match)
        if (connectedPotions.Count >= 3)
        {
            if (connectedPotions.Count == 3)
            {
                Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedPotions[0].potionType);
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Horizontal
                };
            }
            else
            {
                Debug.Log("I have a Long horizontal match, the color of my match is: " + connectedPotions[0].potionType);
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongHorizontal
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

        //have we made a 3+ match? (Vertical Match)
        if (connectedPotions.Count >= 3)
        {
            if (connectedPotions.Count == 3)
            {
                Debug.Log("I have a normal vertical match, the color of my match is: " + connectedPotions[0].potionType);
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.Vertical
                };
            }
            else
            {
                Debug.Log("I have a Long vertical match, the color of my match is: " + connectedPotions[0].potionType);
                return new MatchResult
                {
                    connectedPotions = connectedPotions,
                    direction = MatchDirection.LongVertical
                };
            }
        }
        
        return new MatchResult
        {
            connectedPotions = connectedPotions,
            direction = MatchDirection.None
        };
    }

    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //check that we're within the boundaries of the board
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x,y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                //does our potionType Match? it must also not be matched
                if(!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
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



    #region Swapping potions

    public void SelectPotion(Potion _potion){
        if (selectedPotion == null)
        {
            Debug.Log(_potion);
            selectedPotion = _potion;
            ShowSelectionIndicator(_potion);
        }
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
            HideSelectionIndicator();
        }
        else if (selectedPotion != _potion)
        {
            SwapPotion(selectedPotion, _potion);
            selectedPotion = null;
            HideSelectionIndicator();
        }
    }
    
    private void ShowSelectionIndicator(Potion potion)
    {
        if (selectionIndicator != null)
        {
            // Stop any existing blink coroutine
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            
            selectionIndicator.SetActive(true);
            selectionIndicator.transform.position = potion.transform.position;
            selectionIndicator.transform.SetParent(potion.transform);
            
            // Start blinking
            blinkCoroutine = StartCoroutine(BlinkIndicator());
        }
    }
    
    private void HideSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            // Stop blinking
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
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
            // Fade out
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
            
            // Fade in
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
        if (!isAdjacent(_currentPotion, _targetPotion))
        {
            return;
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

        // Calculate proper grid positions
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        
        // Calculate world positions for the swapped potions
        Vector3 currentLocalPos = new Vector3(_currentPotion.xIndex * potionSpacingX - offsetX, _currentPotion.yIndex * potionSpacingY - offsetY, 0);
        Vector3 targetLocalPos = new Vector3(_targetPotion.xIndex * potionSpacingX - offsetX, _targetPotion.yIndex * potionSpacingY - offsetY, 0);
        
        Vector3 currentWorldPos = potionParent.transform.TransformPoint(currentLocalPos);
        Vector3 targetWorldPos = potionParent.transform.TransformPoint(targetLocalPos);
        
        _currentPotion.MoveToTarget(currentWorldPos);
        _targetPotion.MoveToTarget(targetWorldPos);
    }

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        HideSelectionIndicator(); // Hide indicator during processing
        yield return new WaitForSeconds(0.2f);
        bool hasMatched = CheckBoard();

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }

        else
        {
            DoSwap(_currentPotion, _targetPotion);
            yield return new WaitForSeconds(0.3f);
            SnapAllPotionsToGrid(); // Ensure alignment after swap back
            // Check for valid moves after failed swap
            CheckForValidMoves();
        }
        isProcessingMove = false;
    }

    private bool isAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }
    
    // Helper method to snap all potions to their correct grid positions
    private void SnapAllPotionsToGrid()
    {
        float offsetX = (width - 1) / 2f * potionSpacingX;
        float offsetY = (height - 1) / 2f * potionSpacingY;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] != null && potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
                    
                    // Calculate correct position
                    Vector3 localPos = new Vector3(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY, 0);
                    Vector3 worldPos = potionParent.transform.TransformPoint(localPos);
                    
                    // Snap to position (no animation)
                    potionBoard[x, y].potion.transform.position = worldPos;
                    
                    // Ensure indices are correct
                    potion.SetIndices(x, y);
                }
            }
        }
    }

    #endregion
    
    #region No Moves Detection and Shuffle
    
    private bool HasValidMoves()
    {
        // Check all possible swaps to see if any would create a match
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y] == null || !potionBoard[x, y].isUsable || potionBoard[x, y].potion == null)
                    continue;
                
                // Try swapping with right neighbor
                if (x < width - 1 && potionBoard[x + 1, y] != null && potionBoard[x + 1, y].isUsable && potionBoard[x + 1, y].potion != null)
                {
                    if (WouldSwapCreateMatch(x, y, x + 1, y))
                        return true;
                }
                
                // Try swapping with bottom neighbor
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
        // Get the potion types
        Potion potion1 = potionBoard[x1, y1].potion.GetComponent<Potion>();
        Potion potion2 = potionBoard[x2, y2].potion.GetComponent<Potion>();
        
        PotionType type1 = potion1.potionType;
        PotionType type2 = potion2.potionType;
        
        // Temporarily swap types in our check
        // Check if potion1's position would form a match with potion2's type
        if (WouldFormMatchAt(x1, y1, type2))
            return true;
        
        // Check if potion2's position would form a match with potion1's type
        if (WouldFormMatchAt(x2, y2, type1))
            return true;
        
        return false;
    }
    
    public IEnumerator ShuffleBoard()
    {
        if (isShuffling)
            yield break;
            
        isShuffling = true;
        isProcessingMove = true;
        
        // Show shuffle message
        if (shuffleText != null)
        {
            shuffleText.gameObject.SetActive(true);
            shuffleText.text = "Shuffling because there is no legal move left!";
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // Collect all potions
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
            
            // Calculate new position
            float offsetX = (width - 1) / 2f * potionSpacingX;
            float offsetY = (height - 1) / 2f * potionSpacingY;
            Vector3 localPos = new Vector3(x * potionSpacingX - offsetX, y * potionSpacingY - offsetY, 0);
            Vector3 worldPos = potionParent.transform.TransformPoint(localPos);
            
            // Move to new position
            potionComponent.MoveToTarget(worldPos);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Hide shuffle message
        if (shuffleText != null)
        {
            shuffleText.gameObject.SetActive(false);
        }
        
        isShuffling = false;
        isProcessingMove = false;
        
        // Check if shuffle created any matches (unlikely but possible)
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
        
        if (!HasValidMoves())
        {
            Debug.Log("No valid moves detected! Shuffling board...");
            StartCoroutine(ShuffleBoard());
        }
    }
    
    #endregion


}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
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
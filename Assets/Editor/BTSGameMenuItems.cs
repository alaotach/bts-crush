using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom menu items for BTS Match-3 game
/// </summary>
public class BTSGameMenuItems
{
    [MenuItem("BTS Match-3/Create Candy Database")]
    public static void CreateCandyDatabase()
    {
        BTSCandyDatabase asset = ScriptableObject.CreateInstance<BTSCandyDatabase>();
        
        string path = "Assets/BTSCandyDatabase.asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        
        Debug.Log("Created BTS Candy Database at: " + path);
    }
    
    [MenuItem("BTS Match-3/Setup Default Candy Database")]
    public static void SetupDefaultCandyDatabase()
    {
        BTSCandyDatabase asset = ScriptableObject.CreateInstance<BTSCandyDatabase>();
        
        // Create array with space for all candy types (7 regular + 9 special)
        asset.candies = new BTSCandyData[16];
        
        // Initialize some default candy data
        for (int i = 0; i < asset.candies.Length; i++)
        {
            asset.candies[i] = new BTSCandyData
            {
                candyType = (BTSCandyType)i,
                displayName = ((BTSCandyType)i).ToString(),
                description = i < 7 ? "BTS Member Chibi" : "Special Candy",
                isSpecial = i >= 7, // First 7 are regular members, rest are special
                scoreValue = i >= 7 ? 50 : 10,
                clearRadius = 1
            };
        }
        
        string path = "Assets/BTSCandyDatabase_Default.asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        
        Debug.Log("Created Default BTS Candy Database at: " + path);
        Debug.Log("Remember to assign sprites and sound effects in the Inspector!");
    }
    
    [MenuItem("BTS Match-3/Documentation/Open Candy Reference Guide")]
    public static void OpenCandyGuide()
    {
        string path = "Assets/../BTS_CANDY_REFERENCE_GUIDE.md";
        System.Diagnostics.Process.Start(path);
    }
    
    [MenuItem("BTS Match-3/Documentation/Open Combo Chart")]
    public static void OpenComboChart()
    {
        string path = "Assets/../BTS_COMBO_CHART.md";
        System.Diagnostics.Process.Start(path);
    }
}

# 🍬 BTS Crush - Simplified Special Candy System

## Overview
The special candy system has been completely redesigned to match the original Candy Crush mechanics, making it simpler, more intuitive, and easier to understand!

---

## 🎮 Special Candy Types

### 1. ⚡ Striped Candies

**How to Create:**
- **Match 4 in a ROW (horizontal)** → Creates **StripedVertical** (clears entire column)
- **Match 4 in a COLUMN (vertical)** → Creates **StripedHorizontal** (clears entire row)

**Visual:**
- The candy keeps the character's face/sprite
- Horizontal stripes overlay for StripedHorizontal
- Vertical stripes overlay for StripedVertical

**Effect:**
- StripedHorizontal: Clears the entire row when activated
- StripedVertical: Clears the entire column when activated

**Asset Needs:**
- For each BTS member: Create versions with horizontal and vertical stripes
- Example: `rm_horizontal_striped.png`, `jin_vertical_striped.png`

---

### 2. 🎈 Balloon Candy

**How to Create:**
- **T-shape match** (3 in row + 2 perpendicular)
- **L-shape match** (3 in a line forming an L)

**Visual:**
- A balloon with the character's face pasted on it
- Think: cute chibi face on a round balloon

**Effect:**
- Explodes in a **3×3 area** around the candy
- Creates confetti/pop animation

**Asset Needs:**
- For each BTS member: Create balloon version
- Example: `rm_balloon.png`, `suga_balloon.png`

---

### 3. 🌈 Rainbow Candy (Universal)

**How to Create:**
- **Match 5 or more** candies in a row/column

**Visual:**
- Character sprite with rainbow/galaxy effects
- Sparkly, colorful, stands out from others

**Effect:**
- **Swap with regular candy:** Clears ALL candies of that color
- **Swap with striped candy:** Turns all candies of one color into that striped type and activates them
- **Swap with balloon:** Turns all candies of one color into balloons and pops them all
- **Swap with another Rainbow:** 💥 **CLEARS THE ENTIRE BOARD!**

**Asset Needs:**
- For each BTS member: Create rainbow/galaxy version
- Example: `rm_rainbow.png`, `jimin_rainbow.png`

---

## 💥 Special Candy Combos

### Striped + Striped
**Result:** Giant cross explosion (clears both row AND column)

### Striped + Balloon
**Result:** Giant cross explosion (same as striped+striped)

### Balloon + Balloon
**Result:** Mega explosion (5×5 area instead of 3×3)

### Rainbow + Regular Candy
**Result:** Clears all candies of that color

### Rainbow + Striped
**Result:** All candies of one random color become striped and activate

### Rainbow + Balloon
**Result:** All candies of one random color become balloons and pop

### Rainbow + Rainbow
**Result:** 🔥 **ULTIMATE COMBO** - Clears the ENTIRE board!

---

## 📊 Match Detection Changes

| Match Pattern | Old System | New System |
|--------------|------------|------------|
| 3 in a row | Regular match | Regular match (no special) |
| 4 horizontal | Random special | **StripedVertical** (clears column) |
| 4 vertical | Random special | **StripedHorizontal** (clears row) |
| T or L shape | FanHeartBomb | **Balloon** (3×3 explosion) |
| 5+ candies | Multiple types | **Rainbow** (universal) |

---

## 🎨 Asset Requirements

For each of the 7 BTS members (RM, Jin, Suga, J-Hope, Jimin, V, Jungkook), you'll need:

1. **Base sprite** (already have)
2. **Horizontal striped version** - Add horizontal lines/stripes to the sprite
3. **Vertical striped version** - Add vertical lines/stripes to the sprite  
4. **Balloon version** - Character face on a balloon
5. **Rainbow/Galaxy version** - Add rainbow/sparkle effects

**Total sprites needed:** 7 members × 5 versions = **35 sprites**

---

## 🛠️ Code Changes Summary

### Files Modified:

1. **BTSCandyType.cs**
   - Removed old special types (MicCandy, AlbumBomb, DynamiteCandy, etc.)
   - Added: StripedHorizontal, StripedVertical, Balloon, Rainbow
   - Updated MatchType enum

2. **BTSCandyData.cs**
   - Added sprite fields: `horizontalStripedSprite`, `verticalStripedSprite`, `balloonSprite`, `rainbowSprite`

3. **BTSCandyDatabase.cs**
   - Updated `GetSpecialCandyForMatch()` to return correct special types

4. **PotionBoard.cs**
   - Updated match detection in `IsConnected()` method
   - Updated `SuperMatch()` to detect T/L shapes correctly

5. **BTSSpecialCandyManager.cs**
   - Simplified `ActivateSpecialCandy()` method
   - Rewrote `HandleSpecialCombo()` with new combo logic

---

## 🎯 Next Steps for You

### 1. Create the Sprite Variants
Use image editing software to create:
- Striped versions (add stripe overlays)
- Balloon versions (put faces on balloons)
- Rainbow versions (add sparkle/galaxy effects)

### 2. Configure the Database
In Unity Inspector:
1. Open `BTSCandyDatabase_Default` asset
2. For each candy entry (RM, Jin, Suga, etc.):
   - Assign `horizontalStripedSprite`
   - Assign `verticalStripedSprite`
   - Assign `balloonSprite`
   - Assign `rainbowSprite`

### 3. Test in Unity
1. Play the game
2. Try to create each special candy type
3. Test the combos
4. Adjust visual effects if needed

---

## 🐛 Known Limitations

- Old special candy prefabs (MicCandy, AlbumBomb, etc.) are no longer used
- You may see warnings about missing candy types in the database - this is normal
- Need to create new visual effects for striped line clears and balloon pops

---

## 💡 Design Tips

**Striped Candies:**
- Use contrasting colors for stripes (e.g., white or black)
- Make stripes semi-transparent so the character shows through
- Horizontal stripes should be thin and evenly spaced

**Balloon Candies:**
- Use pastel balloon colors matching the member's theme
- Add a shine/gloss effect to the balloon
- Make the character face smaller to fit on balloon

**Rainbow Candies:**
- Use rainbow gradient overlays
- Add star/sparkle particles
- Make them shimmer or pulse

---

## 📝 Summary

✅ **Simpler system** - Only 3 special candy types instead of 9+  
✅ **Clearer creation rules** - Match pattern directly determines candy type  
✅ **Better combos** - Logical combinations like in original Candy Crush  
✅ **Easier to understand** - Players instantly know what each candy does  
✅ **Less asset work** - Reuse character sprites with overlays instead of creating entirely new designs

The system is now fully implemented in code and ready for art assets!

# 🎨 BTS Crush - Sprite Asset Naming Guide

## Sprite Naming Convention

Use this exact naming pattern for easy organization and assignment in Unity:

---

## 📁 Folder Structure
```
Assets/
  Sprites/
    BTS members/
      Base/
        rm_base.png
        jin_base.png
        suga_base.png
        jhope_base.png
        jimin_base.png
        v_base.png
        jungkook_base.png
      
      Striped/
        rm_striped_horizontal.png
        rm_striped_vertical.png
        jin_striped_horizontal.png
        jin_striped_vertical.png
        suga_striped_horizontal.png
        suga_striped_vertical.png
        jhope_striped_horizontal.png
        jhope_striped_vertical.png
        jimin_striped_horizontal.png
        jimin_striped_vertical.png
        v_striped_horizontal.png
        v_striped_vertical.png
        jungkook_striped_horizontal.png
        jungkook_striped_vertical.png
      
      Balloon/
        rm_balloon.png
        jin_balloon.png
        suga_balloon.png
        jhope_balloon.png
        jimin_balloon.png
        v_balloon.png
        jungkook_balloon.png
      
      Rainbow/
        rm_rainbow.png
        jin_rainbow.png
        suga_rainbow.png
        jhope_rainbow.png
        jimin_rainbow.png
        v_rainbow.png
        jungkook_rainbow.png
```

---

## 🎨 Design Specifications

### Base Sprites
- **Size:** 512×512 pixels (recommended)
- **Format:** PNG with transparency
- **Content:** Chibi/cute version of BTS member

---

### Striped Horizontal Sprites
- **Base:** Same as base sprite
- **Add:** Horizontal white/colored stripes (3-5 stripes)
- **Stripe width:** ~20-30 pixels
- **Opacity:** 70-80% so character shows through
- **Pattern:** Evenly spaced across the height

**Example layers:**
1. Base character sprite
2. Horizontal stripes overlay (semi-transparent)

---

### Striped Vertical Sprites
- **Base:** Same as base sprite
- **Add:** Vertical white/colored stripes (3-5 stripes)
- **Stripe width:** ~20-30 pixels
- **Opacity:** 70-80% so character shows through
- **Pattern:** Evenly spaced across the width

**Example layers:**
1. Base character sprite
2. Vertical stripes overlay (semi-transparent)

---

### Balloon Sprites
- **Design:** Character face on a round balloon
- **Balloon shape:** Circle or oval (400×450 pixels)
- **Character face:** Smaller version (centered on balloon)
- **Colors:** Match member's theme color
- **Add:** 
  - Shine/gloss effect on balloon
  - String at bottom (optional)
  - Small sparkles around it

**Color suggestions:**
- RM: Purple balloon
- Jin: Pink balloon
- Suga: Black/gray balloon
- J-Hope: Orange/red balloon
- Jimin: Yellow balloon
- V: Green/teal balloon
- Jungkook: Blue balloon

---

### Rainbow Sprites
- **Base:** Same as base sprite
- **Add:** Rainbow gradient overlay or aura
- **Effects:**
  - Rainbow color shift (red→orange→yellow→green→blue→purple)
  - Sparkle particles around edges
  - Glow effect
  - Optional: Small stars or shimmer
- **Make it POP:** This should be the most visually distinct special candy

**Techniques:**
- Use rainbow gradient map
- Add outer glow with rainbow colors
- Particle effects layer
- Shimmer animation (if using sprite sheets)

---

## 🛠️ Quick Creation Guide

### Using Photoshop/GIMP:

#### Striped Version:
1. Open base sprite
2. Create new layer
3. Draw rectangles (horizontal or vertical)
4. Set opacity to 70-80%
5. Save as new file

#### Balloon Version:
1. Draw balloon shape (circle/oval)
2. Add gradient (light at top, darker at bottom)
3. Add shine/gloss (white curved line)
4. Paste character face, resize to fit
5. Add string (thin line from bottom)
6. Save

#### Rainbow Version:
1. Open base sprite
2. Duplicate layer
3. Apply rainbow gradient overlay (blend mode: Overlay or Color)
4. Add outer glow (rainbow colors)
5. Add sparkle layer
6. Save

---

## 📦 Unity Import Settings

After creating sprites:

1. **Import all sprites to Unity**
2. **Texture Type:** Sprite (2D and UI)
3. **Pixels Per Unit:** 100 (match your base sprites)
4. **Filter Mode:** Bilinear
5. **Compression:** None or High Quality
6. **Max Size:** 2048

---

## 🔧 Assigning Sprites in Unity

1. Find `BTSCandyDatabase_Default` in your Assets folder
2. Click on it to open in Inspector
3. You'll see an array of candies (7 entries for BTS members)

For each candy (e.g., RM):
```
Candy Type: RM
Sprite: rm_base.png
Horizontal Striped Sprite: rm_striped_horizontal.png
Vertical Striped Sprite: rm_striped_vertical.png
Balloon Sprite: rm_balloon.png
Rainbow Sprite: rm_rainbow.png
```

Repeat for all 7 members!

---

## 🎨 Color Palette Reference

Use these as guides for member-specific colors:

| Member | Theme Color | Hex Code | Notes |
|--------|------------|----------|-------|
| RM | Purple | #9B59B6 | Leader vibes |
| Jin | Pink | #FF69B4 | Soft and warm |
| Suga | Black/White | #2C3E50 | Cool and minimalist |
| J-Hope | Orange/Red | #FF6347 | Energetic |
| Jimin | Yellow | #FFD700 | Bright and cheerful |
| V | Green/Teal | #1ABC9C | Unique and artistic |
| Jungkook | Blue/Purple | #3498DB | Cool and versatile |

---

## ✅ Quality Checklist

Before importing to Unity, ensure:

- [ ] All sprites are 512×512 pixels
- [ ] PNG format with transparency
- [ ] Stripes are visible but not overpowering
- [ ] Balloons have shine/gloss effects
- [ ] Rainbow versions are clearly distinct
- [ ] Consistent art style across all variants
- [ ] No jagged edges (use anti-aliasing)
- [ ] Files are named correctly

---

## 💡 Pro Tips

1. **Batch Processing:** Use Photoshop actions to apply stripes to all members at once
2. **Templates:** Create stripe/balloon/rainbow templates, then swap character faces
3. **Consistency:** Keep stripe thickness and spacing identical across all members
4. **Contrast:** Ensure stripes are visible on both light and dark character sprites
5. **Test in Unity:** Import one complete set first, test in game, then create the rest

---

## 🎬 Example Workflow

**For one complete member (e.g., RM):**

1. Start with `rm_base.png` (already have)
2. Create `rm_striped_horizontal.png` (base + horizontal stripes)
3. Create `rm_striped_vertical.png` (base + vertical stripes)
4. Create `rm_balloon.png` (face on purple balloon)
5. Create `rm_rainbow.png` (base + rainbow effects)
6. Import all 5 files to Unity
7. Assign in BTSCandyDatabase
8. Test in game
9. Repeat for other 6 members

**Time estimate:** ~30-45 minutes per member if you have templates ready

---

## 📸 Visual Examples Needed

You'll want to create mockups or examples for:
- One complete set (all 5 variants of one member)
- Comparison showing stripe thickness/opacity
- Balloon design with and without shine
- Rainbow effect intensity levels

---

Good luck! The code is ready, it's just waiting for your amazing art! 🎨✨

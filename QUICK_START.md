# ⚡ QUICK START - 30 Second Setup

## What You Need To Do: ALMOST NOTHING! 🎉

### Step 1: Check Your Sprites ✅
Look in `Assets/Sprites/BTS members/`

Do you have these 7 files?
- [ ] rm.png (or similar)
- [ ] jin.png
- [ ] suga.png
- [ ] jhope.png  
- [ ] jimin.png
- [ ] v.png
- [ ] jungkook.png

**If YES:** You're done! Go to Step 3.  
**If NO:** Add your BTS member sprites.

---

### Step 2: (Optional) Better Balloons 🎈

Want nicer balloon effects?

1. Find/create ONE balloon sprite (round balloon, any color)
2. Name it `balloon_base.png`
3. Import to Unity
4. Drag to `SpecialCandyVisualizer` → `Balloon Sprite` field

**Skip this if you want to test first** - auto-generated circles work fine!

---

### Step 3: PLAY! 🎮

Hit the **Play** button in Unity.

Match candies and watch:
- **4 in a row** → Striped candy with auto-generated stripes! ⚡
- **T or L shape** → Balloon candy with auto-colored balloon! 🎈
- **5 in a row** → Rainbow candy with animated glow! 🌈

**That's literally it.** Everything else is automatic!

---

## Customization (Optional)

Don't like how something looks? Click on any candy in the Scene view and find `SpecialCandyVisualizer`:

**Stripes too faint?**
- Increase `Stripe Color` alpha (try 0.9 instead of 0.7)

**Want more stripes?**
- Change `Number Of Stripes` (try 5 or 6)

**Balloon color wrong?**
- Tweak `Balloon Tint` color

**Rainbow too wild?**
- Disable `Animate Rainbow`
- Or lower `Rainbow Speed`

Changes take effect immediately in Play mode!

---

## Troubleshooting

### "I don't see any special effects!"

1. Check Console for errors (F12)
2. Make sure your candy prefab has a `SpriteRenderer`
3. Try creating a 4-match and watch closely

### "Stripes are invisible!"

1. Select a striped candy in Scene view
2. Find `SpecialCandyVisualizer` component
3. Change `Stripe Color` to bright white (R:1, G:1, B:1, A:1)

### "Game crashes when creating special candy!"

1. Check Console for the exact error
2. Make sure `SpecialCandyVisualizer.cs` is in your Scripts folder
3. Re-import the script if needed

---

## What's Happening Under The Hood

When you match 4 candies:

```
1. Match detected ✓
2. Game creates a candy object ✓
3. Sets candyType = StripedHorizontal ✓
4. Calls UpdateVisualEffects() ✓
5. SpecialCandyVisualizer auto-added ✓
6. Stripes texture generated ✓
7. Overlay applied to sprite ✓
8. BOOM! Striped candy appears! ⚡
```

**All automatic. No manual work.**

---

## Next Steps

1. **Play and test** all special candy types
2. **Tweak settings** to your liking
3. **Optional:** Create a custom balloon sprite for extra polish
4. **Enjoy** not having to create 28 sprite files! 🎉

---

## Need Help?

Read the full guides:
- `AUTO_VISUAL_EFFECTS_GUIDE.md` - Complete technical guide
- `AUTO_GENERATION_SUMMARY.md` - What changed and why
- `SPECIAL_CANDIES_GUIDE.md` - Gameplay mechanics

Or just ask! The system is designed to be simple and automatic.

---

**TL;DR:** 
1. Have 7 BTS sprites? ✅
2. Press Play. ✅
3. Special effects auto-generate. ✅
4. Done! 🎉

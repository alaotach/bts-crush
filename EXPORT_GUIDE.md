# Export Guide - Android & Web

## Prerequisites

### For Android:
1. Install Android Build Support in Unity Hub
   - Unity Hub → Installs → Your Unity Version → ⚙️ → Add Modules
   - Check: ☑️ Android Build Support
   - Check: ☑️ Android SDK & NDK Tools
   - Check: ☑️ OpenJDK
   - Click Done and wait for installation

### For WebGL (Web):
1. Install WebGL Build Support in Unity Hub
   - Unity Hub → Installs → Your Unity Version → ⚙️ → Add Modules
   - Check: ☑️ WebGL Build Support
   - Click Done and wait for installation

---

## Export for Android (APK)

### Step 1: Switch Platform
1. **File → Build Settings**
2. Select **Android** from platform list
3. Click **Switch Platform** (wait for reimport, may take a few minutes)

### Step 2: Player Settings
Click **Player Settings** button, then configure:

#### Company & Product:
- **Company Name:** Your name/studio
- **Product Name:** BTS Crush Match-3

#### Icon:
- **Default Icon:** Drag your game icon (1024x1024 PNG recommended)

#### Other Settings → Identification:
- **Package Name:** `com.YourName.BTSCrush` (use lowercase, no spaces)
- **Version:** `1.0`
- **Version Code:** `1`

#### Other Settings → Configuration:
- **Scripting Backend:** IL2CPP (recommended for better performance)
- **Target Architectures:** 
  - ☑️ ARMv7 (older devices)
  - ☑️ ARM64 (required for Play Store)

#### Other Settings → Optimization:
- **Managed Stripping Level:** Medium or High (reduces file size)

### Step 3: Build APK
1. Back in Build Settings
2. Make sure all scenes are added:
   - ☑️ MainMenuScene
   - ☑️ LevelSelect
   - ☑️ GameScene
3. Click **Build** (not Build and Run)
4. Choose save location (e.g., `Builds/Android/`)
5. Name it: `BTSCrush.apk`
6. Click Save and wait (first build takes 10-30 minutes)

### Step 4: Test on Device
1. Enable Developer Mode on Android phone:
   - Settings → About Phone → Tap "Build Number" 7 times
2. Enable USB Debugging:
   - Settings → Developer Options → USB Debugging
3. Connect phone via USB
4. Transfer APK to phone
5. Install and test!

---

## Export for Web (WebGL)

### Step 1: Switch Platform
1. **File → Build Settings**
2. Select **WebGL** from platform list
3. Click **Switch Platform** (wait for reimport)

### Step 2: Player Settings
Click **Player Settings** button:

#### Resolution and Presentation:
- **Default Canvas Width:** 1920
- **Default Canvas Height:** 1080
- **Run in Background:** ☑️ Checked

#### Publishing Settings:
- **Compression Format:** Gzip (best compatibility)
- **Enable Exceptions:** None (smaller build)
- **Data Caching:** ☑️ Checked

#### Other Settings:
- **Color Space:** Gamma (better WebGL compatibility)

### Step 3: Build WebGL
1. Back in Build Settings
2. Make sure all scenes are added
3. Click **Build**
4. Create folder: `Builds/WebGL/`
5. Click Select Folder and wait (10-20 minutes)

### Step 4: Test Locally
You CANNOT just open index.html! Need local server:

**Option A - Python (if installed):**
```bash
cd Builds/WebGL/
python3 -m http.server 8000
```
Then open: `http://localhost:8000`

**Option B - Unity's built-in:**
- After build completes
- Click **Build and Run** instead of Build
- Unity will launch local server automatically

### Step 5: Upload to Web Host

**Option A - itch.io (FREE, easiest):**
1. Go to itch.io/game/new
2. Upload entire WebGL folder as ZIP
3. Set "Kind of project" → HTML
4. Check "This file will be played in the browser"
5. Set viewport: 1920 x 1080
6. Save & publish!

**Option B - GitHub Pages (FREE):**
1. Create GitHub repository
2. Upload WebGL build files
3. Enable GitHub Pages in repo settings
4. Access at: `https://yourusername.github.io/reponame/`

**Option C - Netlify/Vercel (FREE):**
1. Drag WebGL folder to netlify.app or vercel.com
2. Instant deployment!

---

## Build Size Optimization

### For Both Platforms:

#### 1. Texture Compression
Select all sprites → Inspector:
- **Max Size:** 1024 or 2048 (not 4096)
- **Compression:** Normal Quality
- **Format:** Automatic

#### 2. Audio Compression
Select all audio clips → Inspector:
- **Load Type:** Compressed in Memory
- **Compression Format:** Vorbis (smaller) or ADPCM (faster)
- **Quality:** 70% (good enough)

#### 3. Code Stripping
Player Settings → Other Settings:
- **Managed Stripping Level:** Medium or High
- **Strip Engine Code:** ☑️ Checked

#### 4. Remove Unused Assets
Before building:
- Delete any unused sprites/sounds in Assets folder
- Remove test scenes
- Check for duplicate files

---

## Platform-Specific Notes

### Android:
- **File Size:** 50-150 MB typical for match-3
- **Minimum Android Version:** 5.0+ (API 21)
- **Permissions:** None needed for this game
- **Orientation:** Portrait or Landscape (set in Player Settings)
- **Test on multiple devices:** Different screen sizes!

### WebGL:
- **File Size:** 20-80 MB typical
- **Loading Time:** 5-30 seconds depending on size
- **Browser Compatibility:** Chrome/Firefox/Edge/Safari
- **Mobile Browsers:** May be slow, not recommended
- **RAM Usage:** Can be high, 1-2 GB
- **Not suitable for:** Very old computers, mobile browsers

---

## Testing Checklist

Before releasing:

### Both Platforms:
- [ ] All scenes load correctly
- [ ] Main menu → Level select works
- [ ] Level select → Game works
- [ ] Game → Victory/defeat works
- [ ] Back buttons work
- [ ] Sound plays correctly
- [ ] All 50 levels accessible
- [ ] Score/moves display correctly
- [ ] Special candies work
- [ ] No console errors

### Android Specific:
- [ ] Works on different screen sizes
- [ ] Touch controls responsive
- [ ] No lag/stuttering
- [ ] Doesn't drain battery too fast
- [ ] Rotates properly (if supporting rotation)
- [ ] Back button works

### WebGL Specific:
- [ ] Loads in all major browsers
- [ ] Responsive to window resize
- [ ] Audio works after user interaction
- [ ] No WebGL errors in browser console
- [ ] Acceptable loading time

---

## Common Issues

### Android - "App not installed"
- Check package name is valid (no special characters)
- Uninstall old version first
- Check phone storage space

### Android - Slow performance
- Use IL2CPP scripting backend
- Enable ARM64 architecture
- Reduce texture sizes
- Lower quality settings

### WebGL - Stuck on loading
- Check browser console for errors
- Try different compression format
- Reduce build size
- Test on different browser

### WebGL - No sound
- WebGL requires user interaction before playing audio
- Add "Click to Start" screen if needed

### Both - Scenes not loading
- All scenes in Build Settings
- Scenes checked/enabled
- Scene names match exactly

---

## Quick Commands Summary

### Add Android/WebGL Support:
Unity Hub → Your Unity Version → Add Modules

### Build for Android:
1. File → Build Settings → Android → Switch Platform
2. Player Settings → Configure
3. Build → Save as .apk

### Build for WebGL:
1. File → Build Settings → WebGL → Switch Platform
2. Player Settings → Configure  
3. Build → Select folder
4. Test: `python3 -m http.server 8000`

### Upload to itch.io:
1. ZIP the WebGL folder
2. Upload to itch.io
3. Set as HTML game
4. Publish!

---

## Recommended Export Workflow

1. **Test thoroughly** in Unity Editor first
2. **Build WebGL** (faster, test in browser)
3. **Upload to itch.io** for friends to test
4. **Fix bugs** based on feedback
5. **Build Android** when everything works
6. **Test on real device**
7. **Release!** 🎉

---

Need help with a specific step? Check Unity's official docs or ask!

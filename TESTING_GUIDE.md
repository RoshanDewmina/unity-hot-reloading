# TrueHotReload - Step-by-Step Testing Guide

This guide will walk you through testing the TrueHotReload package in your Unity instance before publishing.

## Prerequisites

Before you begin, ensure you have:
- ✅ Unity 6.x or Unity 2022.3 LTS installed
- ✅ A Unity project (can be a new empty project for testing)
- ✅ Git installed (version 2.14.0+) for Git URL installation
- ✅ The TrueHotReload package files (already created in `Packages/com.mycompany.truehotreload/`)

---

## Two Installation Methods

You can test this package using either:

### Method A: Git URL Installation (Recommended - Production Method)

This is how end-users will install your package. **Use this to test the real user experience.**

1. Push your package to GitHub (see [Repository Setup Guide](./Packages/com.mycompany.truehotreload/Documentation~/REPOSITORY_SETUP.md))
2. In Unity: **Window → Package Manager → + → Add package from git URL**
3. Enter: `https://github.com/yourusername/unity-hot-reloading.git?path=/Packages/com.mycompany.truehotreload`

**Pros**: Tests the actual installation flow users will experience  
**Cons**: Requires GitHub repository setup first

### Method B: Local Package Testing (Quick Development Testing)

This is faster for rapid development iteration.

1. Your package is already in `Packages/com.mycompany.truehotreload/`
2. Unity should automatically detect it
3. Check **Window → Package Manager → Packages: In Project** to verify

**Pros**: Faster iteration during development  
**Cons**: Doesn't test the full Git URL installation flow

---

## Step 1: Install the Package

### Option A: Via Git URL (Tests Real User Experience)

> [!NOTE]
> First, set up your GitHub repository following the [Repository Setup Guide](./Packages/com.mycompany.truehotreload/Documentation~/REPOSITORY_SETUP.md).

1. Open Unity Package Manager: **Window → Package Manager**
2. Click **"+"** → **Add package from git URL**
3. Enter your Git URL:
   ```
   https://github.com/yourusername/unity-hot-reloading.git?path=/Packages/com.mycompany.truehotreload
   ```
4. Click **Add**
5. Wait for Unity to clone and install the package

### Option B: Local Testing (Already Set Up)

If you're testing locally, the package should already be detected. Verify in **Window → Package Manager → Packages: In Project**.

---

## Step 2: Download External Dependencies

TrueHotReload requires external DLLs that must be added manually.

> [!TIP]
> For complete step-by-step instructions with troubleshooting, see the [Dependency Installation Guide](./Packages/com.mycompany.truehotreload/Documentation~/DEPENDENCY_INSTALLER.md).

### Quick Overview:

### 2.1 Download Harmony

1. Go to: https://github.com/pardeike/Harmony/releases
2. Download the latest **Harmony 2.x** release (e.g., `Harmony.2.3.3.0.zip`)
3. Extract the zip file
4. Locate `0Harmony.dll` (usually in the `net48` or `netstandard2.0` folder)

### 2.2 Download MonoMod

1. Go to: https://github.com/MonoMod/MonoMod/releases
2. Download the latest release
3. Extract and locate:
   - `MonoMod.RuntimeDetour.dll`
   - `MonoMod.Utils.dll`

### 2.3 Download Roslyn (Microsoft.CodeAnalysis)

**Option A - NuGet (Recommended):**
1. Go to: https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp
2. Click "Download package"
3. Rename the `.nupkg` file to `.zip` and extract
4. Navigate to `lib/netstandard2.0/` folder
5. Copy these DLLs:
   - `Microsoft.CodeAnalysis.dll`
   - `Microsoft.CodeAnalysis.CSharp.dll`
   - `System.Collections.Immutable.dll`
   - `System.Reflection.Metadata.dll`

**Option B - Visual Studio:**
If you have Visual Studio, create a temporary .NET project, add the NuGet package, and copy DLLs from the packages folder.

---

## Step 3: Add DLLs to Unity Project

### 3.1 Create Plugins Folder

In your Unity project:
```
Assets/
└── Plugins/
    └── TrueHotReload/
        └── (DLLs will go here)
```

### 3.2 Copy DLLs

Copy all downloaded DLLs to `Assets/Plugins/TrueHotReload/`:
- `0Harmony.dll`
- `MonoMod.RuntimeDetour.dll`
- `MonoMod.Utils.dll`
- `Microsoft.CodeAnalysis.dll`
- `Microsoft.CodeAnalysis.CSharp.dll`
- `System.Collections.Immutable.dll`
- `System.Reflection.Metadata.dll`

### 3.3 Configure DLL Settings

For **each DLL**:
1. Select the DLL in Unity's Project window
2. In the Inspector, look for "Select platforms for plugin"
3. **Uncheck "Any Platform"**
4. **Check only "Editor"**
5. Click "Apply"

This ensures DLLs are Editor-only and never included in builds.

---

## Step 4: Verify Package is Recognized

### 3.1 Check Package Manager

1. Open Unity
2. Go to **Window > Package Manager**
3. In the dropdown at top-left, select **"Packages: In Project"**
4. Look for **"True Hot Reload"** in the list

If you see it, the package is recognized! ✅

### 3.2 Check for Compilation Errors

1. Look at the Console (**Window > General > Console**)
2. If there are errors about missing assemblies, double-check Step 2
3. Common issues:
   - Missing DLLs → Go back to Step 2.2
   - DLLs not set to Editor-only → Go back to Step 2.3

---

## Step 5: Configure Unity Settings

### 4.1 Open TrueHotReload Settings

1. Go to **Edit > Project Settings**
2. Scroll down to **"True Hot Reload"** in the left sidebar
3. Click it to open the settings panel

### 4.2 Fix Unity Settings (if needed)

If you see warnings in the "Unity Settings Health Check" section:

1. Click **"Enable Enter Play Mode Options"** button (if shown)
2. Click **"Disable Domain & Scene Reload"** button (if shown)
3. Verify you see: ✓ "Unity settings are optimized for hot reload"

### 4.3 Review Settings

Check the default settings:
- **Auto-Apply on Save**: Should be `true` (checked)
- **Debounce Seconds**: Should be `0.5`
- **Verbose Logging**: Can enable for testing (optional)

Leave other settings at defaults for now.

---

## Step 6: Create a Test Script

### 5.1 Create Test MonoBehaviour

1. In Unity, create a new C# script: **Assets > Create > C# Script**
2. Name it `HotReloadTest`
3. Open it in your code editor
4. Replace the contents with:

```csharp
using UnityEngine;

public class HotReloadTest : MonoBehaviour
{
    private int counter = 0;

    void Start()
    {
        Debug.Log("HotReloadTest started!");
    }

    void Update()
    {
        counter++;
        
        if (counter % 60 == 0) // Log every 60 frames (~1 second)
        {
            Debug.Log("Test message - original version");
        }
    }
}
```

5. Save the file

### 5.2 Add to Scene

1. Create an empty GameObject: **GameObject > Create Empty**
2. Name it "HotReloadTest"
3. Add the `HotReloadTest` component to it (drag the script or use Add Component)
4. Save the scene

---

## Step 7: Test Basic Hot Reload

### 6.1 Enter Play Mode

1. Click the **Play** button in Unity
2. Watch the Console - you should see:
   - `[TrueHotReload] Runtime bootstrap initialized`
   - `[TrueHotReload] Entered Play Mode - hot reload ready`
   - `[TrueHotReload] Hot reload enabled! Edit scripts and save to apply changes.`
   - `HotReloadTest started!`
   - Repeating: `Test message - original version`

### 6.2 Edit the Script (While Playing!)

**Without stopping Play Mode:**

1. Open `HotReloadTest.cs` in your code editor
2. Change line 15 from:
   ```csharp
   Debug.Log("Test message - original version");
   ```
   to:
   ```csharp
   Debug.Log("🔥 HOT RELOAD WORKS! 🔥");
   ```
3. **Save the file** (Ctrl/Cmd + S)

### 6.3 Observe the Results

Watch the Console. You should see:
1. `[TrueHotReload] Detected 1 changed script(s):`
2. `[TrueHotReload] Compiled 1 file(s) in X.XXXs`
3. `[TrueHotReload] Patched X method(s) in X.XXXs`
4. `[TrueHotReload] ✓ Hot reload successful: X method(s) patched in X.XXXs`
5. **Most importantly:** The log messages should now show `🔥 HOT RELOAD WORKS! 🔥`

**If you see the new message WITHOUT exiting Play Mode, hot reload is working!** 🎉

---

## Step 8: Test Hot Reload Monitor Window

### 7.1 Open the Monitor

1. While still in Play Mode, go to **Window > True Hot Reload > Hot Reload Monitor**
2. A new window should open

### 7.2 Check the Status Panel

You should see:
- **Play Mode:** <span style="color:green">Playing</span>
- **Hot Reload:** <span style="color:green">Active</span>
- **Auto-Apply:** Enabled

### 7.3 Check Active Patches

Scroll down to the "Active Patches" section. You should see:
- `HotReloadTest.Update` (or similar)

This confirms the method was patched!

---

## Step 9: Test More Complex Changes

### 8.1 Add a New Method

While still in Play Mode, edit `HotReloadTest.cs`:

```csharp
using UnityEngine;

public class HotReloadTest : MonoBehaviour
{
    private int counter = 0;

    void Start()
    {
        Debug.Log("HotReloadTest started!");
    }

    void Update()
    {
        counter++;
        
        if (counter % 60 == 0)
        {
            PrintMessage(); // Call new method
        }
    }

    // NEW METHOD - add this while playing!
    void PrintMessage()
    {
        Debug.Log($"🔥 HOT RELOAD WORKS! Counter: {counter}");
    }
}
```

Save and watch the Console. The new method should be called!

### 8.2 Change Logic

Try changing the counter check:
```csharp
if (counter % 120 == 0) // Changed from 60 to 120
```

Save and observe - messages should now appear half as often.

---

## Step 10: Test Error Handling

### 9.1 Introduce a Syntax Error

While in Play Mode, add a syntax error:
```csharp
void PrintMessage()
{
    Debug.Log("Missing semicolon")  // <-- Missing semicolon
}
```

Save the file.

### 9.2 Check Error Reporting

In the Console, you should see:
- `[TrueHotReload] Compilation failed with X error(s)`
- Details about the syntax error with line numbers

**Important:** The game should still be running! Hot reload failed gracefully.

### 9.3 Fix the Error

Add the semicolon back:
```csharp
Debug.Log("Missing semicolon");
```

Save again - hot reload should succeed this time.

---

## Step 11: Test Cleanup

### 10.1 Exit Play Mode

Click the **Stop** button.

In the Console, you should see:
- `[TrueHotReload] Exiting Play Mode - cleaning up patches`
- `[TrueHotReload] All patches cleared`

### 10.2 Re-enter Play Mode

Click **Play** again.

The script should run with the **latest saved version** (not the original). This confirms the actual file was modified, not just runtime patching.

---

## Step 12: Test Settings

### 11.1 Disable Auto-Apply

1. Exit Play Mode
2. Go to **Project Settings > True Hot Reload**
3. Uncheck **"Auto-Apply on Save"**
4. Enter Play Mode again
5. Edit the script and save

You should see:
- `[TrueHotReload] Scripts changed. Use 'Apply Changes' button to hot reload.`

The changes are NOT applied automatically.

### 11.2 Manual Apply

1. Open **Window > True Hot Reload > Hot Reload Monitor**
2. Click **"Apply Changes Now"** button (if implemented)

For now, re-enable Auto-Apply for easier testing.

---

## Step 13: Run Unit Tests

### 12.1 Open Test Runner

1. Go to **Window > General > Test Runner**
2. Click the **EditMode** tab

### 12.2 Run Tests

1. You should see:
   - `TrueHotReload.EditorTests`
   - `ScriptChangeDetectorTests`
   - `CompilationServiceTests`
2. Click **"Run All"**

Most tests should pass. Some may be skipped or fail due to environment differences - that's okay for initial testing.

---

## Step 14: Test Build Exclusion

### 13.1 Create a Test Build

1. Go to **File > Build Settings**
2. Add your current scene
3. Choose a platform (e.g., Windows, Mac, Linux)
4. Click **"Build"** (not "Build and Run")
5. Choose a temporary location

### 13.2 Verify Package is Excluded

After the build completes:
1. Check the build size - it should be normal (not bloated)
2. The TrueHotReload code should NOT be in the build (all asmdefs are Editor-only)

If the build succeeds without errors, the package is properly configured! ✅

---

## Troubleshooting

### Issue: "Hot reload not triggering"

**Check:**
1. Are you in Play Mode?
2. Is the file in an included path? (Check Project Settings)
3. Are there Console errors?
4. Is Auto-Apply enabled?

### Issue: "Compilation errors about missing assemblies"

**Fix:**
1. Verify all DLLs are in `Assets/Plugins/TrueHotReload/`
2. Check each DLL is set to "Editor" platform only
3. Restart Unity

### Issue: "Changes not appearing"

**Check:**
1. Did you save the file?
2. Check Console for hot reload messages
3. Try enabling Verbose Logging in settings
4. Make sure you're editing the right file

### Issue: "Unity freezes or crashes"

**Fix:**
1. Exit Play Mode
2. Check Console for errors
3. Try disabling hot reload temporarily
4. Report the issue with Console logs

---

## Success Checklist

After completing all steps, you should have verified:

- ✅ Package appears in Package Manager
- ✅ No compilation errors
- ✅ Settings panel accessible
- ✅ Unity settings optimized
- ✅ Basic hot reload works (log message change)
- ✅ New methods can be added during Play Mode
- ✅ Logic changes apply instantly
- ✅ Error handling works gracefully
- ✅ Cleanup on Play Mode exit
- ✅ Hot Reload Monitor window works
- ✅ Unit tests run
- ✅ Package excluded from builds

---

## Next Steps

Once testing is complete:

1. **Test with real game code** - Try hot reloading actual gameplay scripts
2. **Test edge cases** - Coroutines, async methods, static classes
3. **Performance testing** - Large files, many simultaneous changes
4. **Documentation review** - Update README with any findings
5. **Prepare for release** - Version numbering, changelog, Asset Store submission

---

## Getting Help

If you encounter issues:
1. Enable **Verbose Logging** in Project Settings
2. Check the log file (if enabled)
3. Review the Console for detailed error messages
4. Check the Hot Reload Monitor for active patches
5. Try the "Clear All Patches" button to reset

---

**Happy Hot Reloading! 🔥**

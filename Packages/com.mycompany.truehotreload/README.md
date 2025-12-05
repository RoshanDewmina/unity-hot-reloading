# True Hot Reload

**Commercial-quality Unity Editor package for true C# hot reload in Play Mode**

## Overview

True Hot Reload enables you to edit C# scripts during Play Mode and see changes applied **instantly** — without exiting Play Mode, without domain reload, and without scene reload. Your game state, player position, inventory, and all runtime data are preserved.

Perfect for:
- 🎮 Rapid gameplay iteration
- 🐛 Live debugging and tuning
- 🎨 Real-time visual tweaking
- ⚡ Maximum productivity

## Installation

### Quick Start (3 Steps)

#### 1. Install Package via Git URL

1. Open Unity Package Manager: **Window → Package Manager**
2. Click **"+"** → **Add package from git URL**
3. Enter this URL:
   ```
   https://github.com/yourusername/unity-hot-reloading.git?path=/Packages/com.mycompany.truehotreload
   ```
4. Click **Add**

> [!TIP]
> **Install Specific Version**: Append a version tag to lock to a specific release:
> ```
> https://github.com/yourusername/unity-hot-reloading.git?path=/Packages/com.mycompany.truehotreload#v0.1.0
> ```

#### 2. Install External Dependencies (One-Time Setup)

The package requires external DLLs that must be installed once per project:

**Quick Installation:**
1. Download required DLLs:
   - [Harmony v2.x](https://github.com/pardeike/Harmony/releases) → `0Harmony.dll`
   - [MonoMod](https://github.com/MonoMod/MonoMod/releases) → `MonoMod.RuntimeDetour.dll`, `MonoMod.Utils.dll`
   - [Roslyn 4.0.1](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/4.0.1) → 4 DLLs (extract from NuGet package)

2. Place all DLLs in: `Assets/Plugins/TrueHotReload/`

3. For each DLL in Unity Inspector:
   - Uncheck **"Any Platform"**
   - Check only **"Editor"**
   - Click **Apply**

**📖 Detailed Guide**: See [Dependency Installation Guide](./Documentation~/DEPENDENCY_INSTALLER.md) for step-by-step instructions with troubleshooting.

#### 3. Configure Unity Settings

After installation:
1. Go to **Edit → Project Settings → True Hot Reload**
2. Click the buttons to automatically configure optimal Unity settings:
   - Enable **Enter Play Mode Options**
   - Disable **Domain Reload** and **Scene Reload**

---

### Alternative Installation Methods

#### Manual Installation (Local Package)

1. Clone or download this repository
2. Copy `Packages/com.mycompany.truehotreload/` to your project's `Packages/` folder
3. Follow steps 2-3 above (dependencies and settings)

#### Via Package Manager Manifest

Add to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.mycompany.truehotreload": "https://github.com/yourusername/unity-hot-reloading.git?path=/Packages/com.mycompany.truehotreload#v0.1.0"
  }
}
```

---

## Requirements

- **Unity 6.x** (6000.0+) or **Unity 2022.3 LTS+**
- **Editor only** (never included in player builds)
- **Mono scripting backend** in Editor
- **Git installed** (version 2.14.0+) for Git URL installation
- **External dependencies** (installed separately, see above)

---


## Quick Start

### Basic Usage

1. **Enter Play Mode** in the Unity Editor
2. **Edit any C# script** (MonoBehaviour, ScriptableObject, or plain C# class)
3. **Save the file** (Ctrl/Cmd + S)
4. **See changes applied immediately!** 🎉

### Example

```csharp
public class Player : MonoBehaviour
{
    void Update()
    {
        // Original code
        Debug.Log("Player moving");
        
        // While in Play Mode, change to:
        Debug.Log("Player running fast!");
        // Save → new message appears instantly in Console!
    }
}
```

### Hot Reload Monitor

Open **Window > True Hot Reload > Hot Reload Monitor** to:
- View hot reload status (active/inactive)
- See active patches
- View session statistics
- Manually clear patches
- Access settings

## What's Supported (MVP)

✅ **Method body changes** in existing classes  
✅ **Adding new methods** to existing classes  
✅ **Changing logic, logs, tuning values**  
✅ **Static and instance methods**  
✅ **MonoBehaviour, ScriptableObject, plain C# classes**  

## Limitations (MVP)

❌ **Adding/removing serialized fields** (`[SerializeField]`)  
❌ **Changing class inheritance**  
❌ **Adding/removing Unity lifecycle attributes** (`[ExecuteInEditMode]`, etc.)  
❌ **Structural changes** (new classes, renamed classes)  
❌ **Adding new assemblies**  

These limitations ensure game state preservation and avoid serialization conflicts.

## Settings

Configure via **Project Settings > True Hot Reload**:

### Path Filtering
- **Included Paths**: Paths to monitor (default: `Assets/`)
- **Excluded Paths**: Paths to ignore (e.g., `Assets/Plugins/`, `Assets/Editor/`)

### Assembly Filtering
- **Included Assemblies**: Only hot reload specific assemblies (default: `Assembly-CSharp`)

### Behavior
- **Auto-Apply on Save**: Automatically trigger hot reload when files are saved (default: on)
- **Debounce Seconds**: Wait time before applying changes (default: 0.5s)

### Diagnostics
- **Verbose Logging**: Enable detailed debug logs
- **Log to File**: Save logs to file for debugging

## Troubleshooting

### Hot reload not triggering

**1. Check you're in Play Mode**
   - Hot reload only works during Play Mode

**2. Check Unity settings**
   - Go to Project Settings > True Hot Reload
   - Look for warnings in the Unity Settings Health Check panel
   - Click the fix buttons if warnings appear

**3. Check file is monitored**
   - Verify the file path is in "Included Paths" and not in "Excluded Paths"
   - Check Console for `[TrueHotReload]` messages

### Compilation errors

- Hot reload uses Roslyn to compile changed scripts
- Compilation errors will appear in the Console
- Fix syntax errors and save again

### Method not patching

- Check the Console for patch errors
- Some methods may not be patchable (e.g., compiler-generated, extern)
- Try the `[HotReloadable]` attribute to explicitly mark classes

### State lost after reload

- True Hot Reload preserves **all** game state
- If state is lost, you may have triggered a domain reload accidentally
- Check that Domain Reload is **disabled** in Project Settings

## Advanced Usage

### Marker Attributes

```csharp
using TrueHotReload.Runtime;

// Explicitly opt-in a class (optional)
[HotReloadable]
public class MyClass { }

// Exclude a specific method from hot reload
public class MyClass 
{
    [NoHotReload]
    void CriticalMethod() { }
}

// Preserve static field values across reloads
public class GameState
{
    [PreserveOnHotReload]
    public static int score = 0;
}
```

### Manual Mode

Disable **Auto-Apply on Save** in settings to manually trigger hot reload:

1. Edit scripts
2. Open Hot Reload Monitor window
3. Click "Apply Changes Now"

## Performance

Typical hot reload times (Core i7, SSD):
- **Compilation**: 0.1-0.5s per changed file
- **Patching**: <0.01s per method
- **Total**: Usually < 1 second for typical changes

## Known Issues

- **Generic methods** may require special handling
- **Async/coroutine** edge cases may not patch correctly in all scenarios
- **IL-level optimizations** may interfere with patching

## Support

- **Documentation**: See this README
- **Issues**: Report bugs on GitHub (if open source) or support email
- **Settings**: Project Settings > True Hot Reload
- **Logs**: Enable verbose logging and check Console or log file

## License

Commercial package license (update with your license terms)

---

**Made with ❤️ for Unity developers who value their time**

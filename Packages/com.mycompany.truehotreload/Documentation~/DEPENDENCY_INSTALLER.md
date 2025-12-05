# Dependency Installation Guide

True Hot Reload requires external DLL dependencies that must be installed manually due to licensing restrictions. This is a **one-time setup** per Unity project.

> [!IMPORTANT]
> These dependencies are **Editor-only** and will never be included in your game builds.

---

## Quick Installation

### Step 1: Download Required DLLs

You need to download three sets of libraries:

#### 📦 Harmony v2.x

**Purpose**: Runtime method patching (MIT License)

1. Go to: https://github.com/pardeike/Harmony/releases
2. Download the latest **2.x release** (e.g., `Harmony.2.3.3.0.zip`)
3. Extract the zip file
4. Locate **`0Harmony.dll`** from the `net48` or `netstandard2.0` folder

#### 📦 MonoMod

**Purpose**: Low-level IL manipulation (MIT License)

1. Go to: https://github.com/MonoMod/MonoMod/releases
2. Download the latest release
3. Extract and locate these DLLs:
   - **`MonoMod.RuntimeDetour.dll`**
   - **`MonoMod.Utils.dll`**

#### 📦 Roslyn (Microsoft.CodeAnalysis)

**Purpose**: Runtime C# compilation (Apache License 2.0)

**Option A - NuGet (Recommended):**

1. Go to: https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp
2. Click **"Download package"** on the right sidebar
3. Rename the `.nupkg` file to `.zip` and extract it
4. Navigate to: `lib/netstandard2.0/`
5. Copy these DLLs:
   - **`Microsoft.CodeAnalysis.dll`**
   - **`Microsoft.CodeAnalysis.CSharp.dll`**
   - **`System.Collections.Immutable.dll`**
   - **`System.Reflection.Metadata.dll`**

**Option B - Manual Links:**

Download compatible versions (version 4.0.1 recommended for Unity):
- [Microsoft.CodeAnalysis.CSharp 4.0.1](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/4.0.1)
- [System.Collections.Immutable 5.0.0](https://www.nuget.org/packages/System.Collections.Immutable/5.0.0)
- [System.Reflection.Metadata 5.0.0](https://www.nuget.org/packages/System.Reflection.Metadata/5.0.0)

---

### Step 2: Add DLLs to Unity Project

#### 2.1 Create Plugins Folder

In your Unity project's **Project window**, create this folder structure:

```
Assets/
└── Plugins/
    └── TrueHotReload/
```

#### 2.2 Copy All DLLs

Copy all the downloaded DLLs into `Assets/Plugins/TrueHotReload/`:

```
Assets/Plugins/TrueHotReload/
├── 0Harmony.dll
├── MonoMod.RuntimeDetour.dll
├── MonoMod.Utils.dll
├── Microsoft.CodeAnalysis.dll
├── Microsoft.CodeAnalysis.CSharp.dll
├── System.Collections.Immutable.dll
└── System.Reflection.Metadata.dll
```

---

### Step 3: Configure DLL Platform Settings

> [!CAUTION]
> This step is **critical** to ensure DLLs are Editor-only and never included in builds!

For **each DLL** you just added:

1. **Select the DLL** in Unity's Project window
2. In the **Inspector** panel, find **"Select platforms for plugin"**
3. **Uncheck "Any Platform"**
4. **Check only "Editor"**
5. Expand **"Platform settings"** → **"Editor"**
6. Ensure **"CPU"** is set to **"AnyCPU"**
7. Click **"Apply"**

**Repeat for all 7 DLLs!**

![Platform Settings Example](https://docs.unity3d.com/uploads/Main/PluginInspector.png)

---

### Step 4: Verify Installation

#### 4.1 Check for Compilation Errors

1. Open the **Console** window (**Window → General → Console**)
2. Look for any errors
3. If you see errors about missing assemblies, verify Step 2 and Step 3

#### 4.2 Verify Package Recognizes Dependencies

1. Enter **Play Mode**
2. Check the Console for: `[TrueHotReload] Runtime bootstrap initialized`
3. If you see this message, dependencies are correctly installed! ✅

---

## Version Compatibility Matrix

| Dependency | Tested Version | Unity Compatibility | Notes |
|------------|---------------|---------------------|-------|
| Harmony | 2.3.3.0 | Unity 2022.3+ | Use net48 or netstandard2.0 version |
| MonoMod.RuntimeDetour | 22.x - 24.x | Unity 2022.3+ | Latest stable recommended |
| MonoMod.Utils | 22.x - 24.x | Unity 2022.3+ | Must match MonoMod.RuntimeDetour version |
| Microsoft.CodeAnalysis.CSharp | 4.0.1 - 4.8.0 | Unity 2022.3+ | Use netstandard2.0 version |
| System.Collections.Immutable | 5.0.0 - 8.0.0 | Unity 2022.3+ | Roslyn dependency |
| System.Reflection.Metadata | 5.0.0 - 8.0.0 | Unity 2022.3+ | Roslyn dependency |

---

## Troubleshooting

### ❌ Error: "Assembly 'Harmony' not found"

**Cause**: DLL not in correct location or not configured as Editor-only

**Solution**:
1. Verify `0Harmony.dll` is in `Assets/Plugins/TrueHotReload/`
2. Select the DLL in Unity
3. Ensure platform is set to **"Editor"** only
4. Click **Apply** and restart Unity

---

### ❌ Error: "Could not load type 'MonoMod.RuntimeDetour...'"

**Cause**: Missing MonoMod dependencies or version mismatch

**Solution**:
1. Ensure both `MonoMod.RuntimeDetour.dll` AND `MonoMod.Utils.dll` are present
2. Verify they are from the **same release**
3. Download the latest matching versions from [MonoMod Releases](https://github.com/MonoMod/MonoMod/releases)

---

### ❌ Error: "The type or namespace name 'CodeAnalysis' does not exist"

**Cause**: Missing Roslyn DLLs or wrong .NET version

**Solution**:
1. Ensure all 4 Roslyn DLLs are present:
   - Microsoft.CodeAnalysis.dll
   - Microsoft.CodeAnalysis.CSharp.dll
   - System.Collections.Immutable.dll
   - System.Reflection.Metadata.dll
2. Verify you extracted from the **`netstandard2.0`** folder (not `net45` or `net6.0`)
3. Try downloading version 4.0.1 specifically (known compatible)

---

### ⚠️ Warning: DLLs appearing in build

**Cause**: DLLs not configured as Editor-only

**Solution**:
1. For each DLL, in the Inspector:
   - Uncheck **"Any Platform"**
   - Check **only "Editor"**
   - Click **Apply**
2. Create a test build to verify exclusion
3. DLLs should **not** appear in the build output

---

### 🔍 DLL Not Loading / Unity Crash

**Cause**: Version incompatibility or corrupted DLL

**Solution**:
1. Delete all DLLs from `Assets/Plugins/TrueHotReload/`
2. Re-download fresh versions from official sources
3. Ensure you're using **netstandard2.0** or **net48** versions (not net6.0)
4. Restart Unity after re-adding DLLs

---

## Automated Installation (Future Enhancement)

> [!NOTE]
> We're working on an automated dependency installer that will:
> - Check for missing DLLs on first launch
> - Show a helpful dialog with download buttons
> - Automatically configure platform settings
> 
> Track progress in [GitHub Issues](https://github.com/RoshanDewmina/unity-hot-reloading/issues)

---

## Direct Download Links

### Pre-Tested Compatible Versions

For your convenience, here are direct links to tested compatible versions:

- **Harmony 2.3.3.0**: [Download](https://github.com/pardeike/Harmony/releases/tag/v2.3.3.0)
- **MonoMod 24.x**: [Download](https://github.com/MonoMod/MonoMod/releases)
- **Roslyn 4.0.1**: [NuGet Package](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/4.0.1)

---

## Why Manual Installation?

**Q: Why can't these be bundled with the package?**

**A**: Licensing and best practices:
- **License Compliance**: While all dependencies use permissive licenses (MIT/Apache 2.0), proper attribution and license preservation is important
- **Size**: Pre-bundled DLLs would bloat the Git repository
- **Updates**: Users can update dependencies independently
- **Flexibility**: Users choose versions compatible with their setup

**Q: Is this common for Unity packages?**

**A**: Yes! Many Unity packages (e.g., those using Newtonsoft.Json, SQLite, etc.) require external dependency installation. It's a standard practice.

---

## Need Help?

If you encounter issues not covered here:

1. Check the [GitHub Issues](https://github.com/RoshanDewmina/unity-hot-reloading/issues)
2. Enable **Verbose Logging** in **Project Settings → True Hot Reload**
3. Check Unity Console for detailed error messages
4. Report issues with your Unity version and error logs

---

**One-time setup = Infinite productivity gains! 🔥**

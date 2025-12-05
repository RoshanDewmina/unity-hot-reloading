# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-12-05

### Added
- Initial release of True Hot Reload
- Hot reload support for method body changes in MonoBehaviour, ScriptableObject, and plain C# classes
- Support for adding new methods to existing classes during Play Mode
- File system watcher with automatic change detection
- Roslyn-based compilation service for runtime C# compilation
- Harmony-based runtime patching system
- Hot Reload Monitor window for viewing active patches and statistics
- Project Settings panel with Unity settings health checks
- Path and assembly filtering configuration
- Auto-apply on save with configurable debounce
- Marker attributes: `[HotReloadable]`, `[NoHotReload]`, `[PreserveOnHotReload]`
- Comprehensive error handling and rollback on compilation failures
- Verbose logging and diagnostics

### Known Limitations
- Does not support adding/removing serialized fields
- Does not support structural changes (new classes, renamed classes)
- Does not support changing class inheritance
- Generic methods may require special handling
- Async/coroutine edge cases may not patch correctly in all scenarios

### Requirements
- Unity 6.x (6000.0+) or Unity 2022.3 LTS+
- External dependencies (must be installed manually):
  - Harmony (0Harmony.dll) v2.x
  - MonoMod.RuntimeDetour.dll and dependencies
  - Microsoft.CodeAnalysis.CSharp.dll (Roslyn)

---

## Release Tags

When installing via Git URL, you can specify a version using Git tags:

```
https://github.com/RoshanDewmina/unity-hot-reloading.git?path=/Packages/com.mycompany.truehotreload#v0.1.0
```

Replace `v0.1.0` with the desired version tag.

# True Hot Reload

**True C# hot reload for Unity - instant code changes in Play Mode without reloading**

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🎯 What is This?

True Hot Reload enables you to edit C# scripts during Play Mode and see changes applied instantly — without exiting Play Mode, without domain reload, and without scene reload. Your game state, player position, inventory, and all runtime data are preserved.

Perfect for:
- 🎮 Rapid gameplay iteration
- 🐛 Live debugging and tuning  
- 🎨 Real-time visual tweaking
- ⚡ Maximum productivity

## 🚀 Quick Start

### Installation (3 Easy Steps)

1. **Install Package via Git URL**
   - Open Unity Package Manager: **Window → Package Manager**
   - Click **"+"** → **Add package from git URL**
   - Enter: `https://github.com/yourusername/unity-hot-reloading.git?path=/Packages/com.mycompany.truehotreload`

2. **Install Dependencies** (one-time setup)
   - Download required DLLs ([detailed guide](./Packages/com.mycompany.truehotreload/Documentation~/DEPENDENCY_INSTALLER.md))
   - Place in `Assets/Plugins/TrueHotReload/`
   - Configure as Editor-only

3. **Configure Settings**
   - **Edit → Project Settings → True Hot Reload**
   - Click auto-configure buttons

### Basic Usage

1. Enter Play Mode
2. Edit any C# script
3. Save the file (Ctrl/Cmd + S)
4. See changes applied instantly! 🎉

## 📖 Documentation

- **[Installation Guide](./Packages/com.mycompany.truehotreload/README.md)** - Full installation instructions
- **[Dependency Setup](./Packages/com.mycompany.truehotreload/Documentation~/DEPENDENCY_INSTALLER.md)** - Detailed DLL installation guide
- **[Testing Guide](./TESTING_GUIDE.md)** - Step-by-step testing instructions
- **[Repository Setup](./Packages/com.mycompany.truehotreload/Documentation~/REPOSITORY_SETUP.md)** - Publishing to GitHub

## ✨ Features

✅ Method body changes in existing classes  
✅ Adding new methods during Play Mode  
✅ Changing logic, logs, tuning values  
✅ Static and instance methods  
✅ MonoBehaviour, ScriptableObject, plain C# classes  
✅ Preserves all game state  

## ⚠️ Limitations (MVP)

❌ Adding/removing serialized fields  
❌ Changing class inheritance  
❌ Structural changes (new classes, renamed classes)  

## 📋 Requirements

- Unity 6.x (6000.0+) or Unity 2022.3 LTS+
- Editor only (never included in builds)
- Mono scripting backend in Editor
- Git installed (version 2.14.0+)
- External dependencies: Harmony, MonoMod, Roslyn

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with:
- [Harmony](https://github.com/pardeike/Harmony) - Runtime method patching
- [MonoMod](https://github.com/MonoMod/MonoMod) - IL manipulation
- [Roslyn](https://github.com/dotnet/roslyn) - C# compilation

---

**Made with ❤️ for Unity developers who value their time**

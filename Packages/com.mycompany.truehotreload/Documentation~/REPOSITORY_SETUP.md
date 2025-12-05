# GitHub Repository Setup Guide

This guide explains how to publish your True Hot Reload package to GitHub and enable Git URL installation via Unity Package Manager.

## Prerequisites

- Git installed on your machine (version 2.14.0+)
- GitHub account
- The True Hot Reload package files

---

## Step 1: Create GitHub Repository

### 1.1 Create Repository on GitHub

1. Go to [github.com](https://github.com) and sign in
2. Click the **"+"** icon in the top-right corner → **New repository**
3. Configure your repository:
   - **Repository name**: `unity-hot-reloading` (or your preferred name)
   - **Description**: "True C# hot reload for Unity - instant code changes in Play Mode"
   - **Visibility**: Public (for open-source) or Private
   - **Initialize**: Leave unchecked (we have existing code)
4. Click **Create repository**

### 1.2 Update Package Name (Optional but Recommended)

Update `package.json` to reflect your GitHub username:

```json
{
  "name": "com.RoshanDewmina.truehotreload",
  ...
}
```

Also update the repository URLs in `package.json`:

```json
{
  "repository": {
    "type": "git",
    "url": "https://github.com/RoshanDewmina/unity-hot-reloading.git"
  },
  "documentationUrl": "https://github.com/RoshanDewmina/unity-hot-reloading#readme",
  "changelogUrl": "https://github.com/RoshanDewmina/unity-hot-reloading/blob/main/Packages/com.RoshanDewmina.truehotreload/CHANGELOG.md",
  "licensesUrl": "https://github.com/RoshanDewmina/unity-hot-reloading/blob/main/LICENSE"
}
```

---

## Step 2: Initialize and Push Repository

### 2.1 Initialize Git Repository

Open terminal in your project root directory:

```bash
cd /path/to/unity-hot-reloading
git init
```

### 2.2 Create .gitignore

Create a `.gitignore` file in the repository root:

```gitignore
# Unity generated files
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

# Visual Studio cache directory
.vs/

# Rider
.idea/

# macOS
.DS_Store

# Windows
Thumbs.db

# Unity meta files (should be included, but exclude backup)
*.meta~

# Test projects (if you have any)
TestProjects/*/[Ll]ibrary/
TestProjects/*/[Tt]emp/
TestProjects/*/[Oo]bj/
```

### 2.3 Add Files and Commit

```bash
git add .
git commit -m "Initial commit: True Hot Reload v0.1.0"
```

### 2.4 Push to GitHub

Replace `RoshanDewmina` with your actual GitHub username:

```bash
git branch -M main
git remote add origin https://github.com/RoshanDewmina/unity-hot-reloading.git
git push -u origin main
```

---

## Step 3: Create Version Tags

Unity Package Manager can use Git tags to specify versions. This allows users to install specific versions.

### 3.1 Create Your First Release Tag

```bash
git tag -a v0.1.0 -m "Release v0.1.0: Initial public release"
git push origin v0.1.0
```

### 3.2 Create GitHub Release (Optional but Recommended)

1. Go to your repository on GitHub
2. Click **Releases** → **Create a new release**
3. Choose tag: `v0.1.0`
4. Release title: `v0.1.0 - Initial Release`
5. Description: Copy from your CHANGELOG.md
6. Click **Publish release**

---

## Step 4: Test Installation

### 4.1 Test in Fresh Unity Project

1. Create a new Unity project (or use existing)
2. Open **Window → Package Manager**
3. Click **"+"** → **Add package from git URL**
4. Enter your package URL:

   **If package is in root:**
   ```
   https://github.com/RoshanDewmina/unity-hot-reloading.git
   ```

   **If package is in subfolder (recommended structure):**
   ```
   https://github.com/RoshanDewmina/unity-hot-reloading.git?path=/Packages/com.RoshanDewmina.truehotreload
   ```

5. Click **Add**
6. Verify package appears in Package Manager

### 4.2 Test Specific Version

Users can install specific versions using Git tags:

```
https://github.com/RoshanDewmina/unity-hot-reloading.git?path=/Packages/com.RoshanDewmina.truehotreload#v0.1.0
```

---

## Step 5: Create LICENSE File

Add a LICENSE file to your repository root to specify usage terms.

### For Open-Source (MIT License Example):

Create `LICENSE` file:

```
MIT License

Copyright (c) 2025 Your Name

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

Commit and push:

```bash
git add LICENSE
git commit -m "Add MIT License"
git push
```

---

## Step 6: Update README Installation Instructions

Your repository README should include clear installation instructions. Add this section:

```markdown
## Installation

### Via Unity Package Manager (Git URL)

1. Open Unity Package Manager: **Window → Package Manager**
2. Click **"+"** → **Add package from git URL**
3. Enter: `https://github.com/RoshanDewmina/unity-hot-reloading.git?path=/Packages/com.RoshanDewmina.truehotreload`
4. Click **Add**

#### Install Specific Version

To install a specific version, append the version tag:

```
https://github.com/RoshanDewmina/unity-hot-reloading.git?path=/Packages/com.RoshanDewmina.truehotreload#v0.1.0
```

### External Dependencies

⚠️ **Important**: After installing the package, you must install external DLLs (one-time setup).

See [Dependency Installation Guide](./Packages/com.RoshanDewmina.truehotreload/Documentation~/DEPENDENCY_INSTALLER.md) for detailed instructions.
```

---

## Versioning Best Practices

### Semantic Versioning

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.0.0): Breaking changes
- **MINOR** (0.1.0): New features, backward compatible
- **PATCH** (0.0.1): Bug fixes, backward compatible

### Creating New Releases

When you have new changes:

1. Update version in `package.json`
2. Update `CHANGELOG.md` with changes
3. Commit changes
4. Create and push tag:

```bash
git add .
git commit -m "Release v0.2.0: Add new feature"
git tag -a v0.2.0 -m "Release v0.2.0"
git push origin main
git push origin v0.2.0
```

5. Create GitHub Release (recommended)

---

## Sharing Your Package

### Package URL Format

Share this URL with users:

```
https://github.com/RoshanDewmina/unity-hot-reloading.git?path=/Packages/com.RoshanDewmina.truehotreload
```

### Documentation

Point users to:
- **GitHub README**: Main documentation
- **Installation Guide**: In your repository
- **Dependency Guide**: Detailed DLL setup instructions

---

## Optional: Publish to OpenUPM

For wider discoverability, consider publishing to [OpenUPM](https://openupm.com/):

1. Ensure your package follows [OpenUPM guidelines](https://github.com/openupm/openupm/blob/master/docs/adding-upm-package.md)
2. Submit a pull request to the OpenUPM registry
3. Once approved, users can install via:
   ```bash
   openupm add com.RoshanDewmina.truehotreload
   ```

---

## Troubleshooting

### Users can't install package

**Problem**: "Cannot resolve package" error

**Solutions**:
- Verify the Git URL is correct
- Ensure repository is public (or user has access for private repos)
- Check the `?path=` parameter points to correct folder
- Verify `package.json` exists in the specified path

### Users get "Invalid package" error

**Problem**: Unity rejects the package

**Solutions**:
- Verify `package.json` is valid JSON
- Ensure `name`, `version`, and `displayName` fields exist
- Check Unity version compatibility in `package.json`

### Updates not appearing

**Problem**: Users don't see new versions

**Solutions**:
- Ensure you pushed Git tags: `git push origin v0.2.0`
- Users may need to remove and re-add package
- Check Unity's package cache: `~/Library/Unity/Asset Store-5.x/Cache/` (macOS)

---

## Next Steps

- ✅ Repository created and pushed
- ✅ Version tags created
- ✅ Installation tested
- ✅ README updated
- 📦 Consider OpenUPM submission
- 🌟 Market your package in Unity communities
- 📈 Gather user feedback and iterate

Happy publishing! 🚀

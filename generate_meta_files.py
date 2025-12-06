#!/usr/bin/env python3
"""
Generate Unity .meta files for a package to enable Git distribution.
This script creates .meta files with unique GUIDs for all files and folders.
"""

import os
import uuid
import sys

def generate_guid():
    """Generate a Unity-compatible GUID (32 hex characters, no dashes)."""
    return str(uuid.uuid4()).replace('-', '')

def create_folder_meta(path):
    """Create a .meta file for a folder."""
    guid = generate_guid()
    content = f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(path + '.meta', 'w') as f:
        f.write(content)
    print(f"Created: {path}.meta")

def create_script_meta(path):
    """Create a .meta file for a C# script."""
    guid = generate_guid()
    content = f"""fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(path + '.meta', 'w') as f:
        f.write(content)
    print(f"Created: {path}.meta")

def create_asmdef_meta(path):
    """Create a .meta file for an assembly definition (.asmdef)."""
    guid = generate_guid()
    content = f"""fileFormatVersion: 2
guid: {guid}
AssemblyDefinitionImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(path + '.meta', 'w') as f:
        f.write(content)
    print(f"Created: {path}.meta")

def create_text_meta(path):
    """Create a .meta file for text/markdown files."""
    guid = generate_guid()
    content = f"""fileFormatVersion: 2
guid: {guid}
TextScriptImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(path + '.meta', 'w') as f:
        f.write(content)
    print(f"Created: {path}.meta")

def create_default_meta(path):
    """Create a default .meta file for other file types."""
    guid = generate_guid()
    content = f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(path + '.meta', 'w') as f:
        f.write(content)
    print(f"Created: {path}.meta")

def should_skip(name):
    """Check if file/folder should be skipped."""
    skip_patterns = ['.meta', '.git', '__pycache__', '.DS_Store', 'node_modules']
    for pattern in skip_patterns:
        if pattern in name:
            return True
    return False

def generate_meta_files(root_dir):
    """Recursively generate .meta files for all assets in root_dir."""
    count = 0
    
    # Walk through all directories
    for dirpath, dirnames, filenames in os.walk(root_dir):
        # Filter out directories we should skip
        dirnames[:] = [d for d in dirnames if not should_skip(d)]
        
        # Create meta files for subdirectories
        for dirname in dirnames:
            folder_path = os.path.join(dirpath, dirname)
            meta_path = folder_path + '.meta'
            
            if not os.path.exists(meta_path):
                create_folder_meta(folder_path)
                count += 1
        
        # Create meta files for files
        for filename in filenames:
            if should_skip(filename):
                continue
                
            file_path = os.path.join(dirpath, filename)
            meta_path = file_path + '.meta'
            
            if not os.path.exists(meta_path):
                # Determine file type and create appropriate meta file
                if filename.endswith('.cs'):
                    create_script_meta(file_path)
                elif filename.endswith('.asmdef'):
                    create_asmdef_meta(file_path)
                elif filename.endswith(('.md', '.txt', '.json')):
                    create_text_meta(file_path)
                else:
                    create_default_meta(file_path)
                count += 1
    
    return count

if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("Usage: python3 generate_meta_files.py <package_directory>")
        sys.exit(1)
    
    package_dir = sys.argv[1]
    
    if not os.path.exists(package_dir):
        print(f"Error: Directory '{package_dir}' does not exist")
        sys.exit(1)
    
    print(f"Generating .meta files for: {package_dir}")
    print("=" * 60)
    
    count = generate_meta_files(package_dir)
    
    print("=" * 60)
    print(f"✅ Successfully created {count} .meta files!")

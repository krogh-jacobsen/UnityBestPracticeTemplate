using UnityEditor;
using UnityEngine;
using System.IO;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Creates a <c>.gitattributes</c> file at the project root.
    /// Accessible via the menu: <b>Window → Best Practices → Generate .gitattributes</b>.
    /// </summary>
    /// <remarks>
    /// The generated file sets a sensible baseline for Unity projects:
    /// <list type="bullet">
    ///   <item>LF line endings for all text and source files</item>
    ///   <item>UnityYAMLMerge driver for Unity YAML assets (.unity, .prefab, .asset, etc.)</item>
    ///   <item>Binary flags for image, audio, model and build output files so Git never attempts text diffs</item>
    /// </list>
    /// </remarks>
    public static class GenerateGitAttributes
    {
        /// <summary>
        /// Creates or overwrites the <c>.gitattributes</c> file at the Unity project root.
        /// Prompts the user for confirmation if the file already exists.
        /// </summary>
        [MenuItem("Tools/Unity Project Configurator/Version Control/Generate .gitattributes", false, 151)]
        public static void Execute()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string gitattributesPath = Path.Combine(projectRoot, ".gitattributes");

            if (File.Exists(gitattributesPath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Generate .gitattributes",
                    ".gitattributes already exists. Overwrite it?",
                    "Overwrite",
                    "Cancel"
                );
                if (!overwrite) return;
            }

            string content =
@"# Default — auto-detect text vs binary
* text=auto

# ── Unity YAML assets ────────────────────────────────────────────────────────
# Use UnityYAMLMerge for all serialised Unity files so merge conflicts are
# resolved correctly. Configure the merge driver in .git/config or globally:
#   [merge ""unityyamlmerge""]
#       name = Unity SmartMerge
#       driver = '<UnityInstallPath>/Tools/UnityYAMLMerge' merge -p %O %B %A %P
#       recursive = binary
[Aa]ssets/**/*.unity        merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.prefab       merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.asset        merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.meta         merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.controller   merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.anim         merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.overrideController merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.physicMaterial     merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.physicsMaterial2D  merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.playable     merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.signal       merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.spriteatlas  merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.terrainlayer merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.lighting     merge=unityyamlmerge eol=lf
[Aa]ssets/**/*.giparams     merge=unityyamlmerge eol=lf
*.scenetemplate             merge=unityyamlmerge eol=lf

# ── Source / text files (LF line endings) ─────────────────────────────────────
*.cs        text eol=lf
*.shader    text eol=lf
*.hlsl      text eol=lf
*.glsl      text eol=lf
*.cginc     text eol=lf
*.json      text eol=lf
*.xml       text eol=lf
*.yml       text eol=lf
*.yaml      text eol=lf
*.txt       text eol=lf
*.md        text eol=lf
*.html      text eol=lf
*.css       text eol=lf
*.uxml      text eol=lf
*.uss       text eol=lf
*.asmdef    text eol=lf
*.asmref    text eol=lf
*.inputactions text eol=lf
*.rsp       text eol=lf
*.gradle    text eol=lf
*.java      text eol=lf

# ── Binary assets (never diff, never corrupt with EOL conversion) ─────────────
# Textures
*.png       binary
*.jpg       binary
*.jpeg      binary
*.tga       binary
*.tiff      binary
*.tif       binary
*.psd       binary
*.exr       binary
*.hdr       binary
*.gif       binary
*.bmp       binary

# Audio
*.mp3       binary
*.wav       binary
*.ogg       binary
*.aif       binary
*.aiff      binary
*.flac      binary

# Video
*.mp4       binary
*.mov       binary
*.webm      binary

# 3D models
*.fbx       binary
*.obj       binary
*.blend     binary
*.dae       binary
*.3ds       binary
*.ma        binary
*.mb        binary

# Fonts
*.ttf       binary
*.otf       binary
*.woff      binary
*.woff2     binary

# Build outputs / packages
*.dll       binary
*.pdb       binary
*.zip       binary
*.7z        binary
*.tar       binary
*.gz        binary
*.rar       binary
*.unitypackage binary
*.apk       binary
*.aab       binary
*.app       binary
*.exe       binary
*.msi       binary
";

            File.WriteAllText(gitattributesPath, content);
            Debug.Log($"[BestPractice] .gitattributes created at: {gitattributesPath}");
        }
    }
}

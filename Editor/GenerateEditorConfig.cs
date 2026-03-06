using UnityEditor;
using UnityEngine;
using System.IO;

namespace Unity.BestPractices.Editor
{
    /// <summary>
    /// Creates an <c>.editorconfig</c> file at the project root that matches the package's C# style guide.
    /// Accessible via the menu: <b>Window → Best Practices → Generate .editorconfig</b>.
    /// </summary>
    /// <remarks>
    /// The generated config enforces naming conventions, formatting rules, and code style
    /// preferences that align with the Best Practices C# coding standard. IDEs such as
    /// Rider, Visual Studio, and VS Code will automatically pick up and enforce these rules.
    /// </remarks>
    public static class GenerateEditorConfig
    {
        /// <summary>
        /// Creates or overwrites the <c>.editorconfig</c> file at the Unity project root.
        /// Prompts the user for confirmation if the file already exists.
        /// </summary>
        [MenuItem("Window/Best Practices/Generate .editorconfig")]
        public static void Execute()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string editorConfigPath = Path.Combine(projectRoot, ".editorconfig");

            if (File.Exists(editorConfigPath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Generate .editorconfig",
                    ".editorconfig already exists. Overwrite it?",
                    "Overwrite",
                    "Cancel"
                );
                if (!overwrite) return;
            }

            string content = @"# EditorConfig — Unity Best Practices
# https://editorconfig.org
root = true

[*]
indent_style = space
indent_size = 4
end_of_line = lf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.cs]
# Naming conventions
dotnet_naming_style.pascal_case.capitalization = pascal_case
dotnet_naming_style.camel_case.capitalization = camel_case

# Private fields: _camelCase
dotnet_naming_rule.private_fields.symbols = private_fields
dotnet_naming_rule.private_fields.style = _camel_case
dotnet_naming_rule.private_fields.severity = suggestion
dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private, protected
dotnet_naming_style._camel_case.capitalization = camel_case
dotnet_naming_style._camel_case.required_prefix = _

# Constants: PascalCase
dotnet_naming_rule.constants.symbols = constants
dotnet_naming_rule.constants.style = pascal_case
dotnet_naming_rule.constants.severity = suggestion
dotnet_naming_symbols.constants.applicable_kinds = field
dotnet_naming_symbols.constants.required_modifiers = const

# Static readonly: k_ prefix
dotnet_naming_rule.static_readonly.symbols = static_readonly
dotnet_naming_rule.static_readonly.style = k_prefix
dotnet_naming_rule.static_readonly.severity = suggestion
dotnet_naming_symbols.static_readonly.applicable_kinds = field
dotnet_naming_symbols.static_readonly.required_modifiers = static, readonly
dotnet_naming_style.k_prefix.capitalization = pascal_case
dotnet_naming_style.k_prefix.required_prefix = k_

# Formatting
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_indent_case_contents = true
csharp_indent_switch_labels = true
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true

# Using directives
csharp_using_directive_placement = outside_namespace
dotnet_sort_system_directives_first = true

# Expression preferences
csharp_prefer_braces = true:suggestion
csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion

[*.{xml,json,yaml,yml,asmdef,asmref}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
";

            File.WriteAllText(editorConfigPath, content);
            Debug.Log($"[BestPractice] .editorconfig created at: {editorConfigPath}");
        }
    }
}


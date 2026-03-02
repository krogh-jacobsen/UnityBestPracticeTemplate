# LLM Instruction Setup Guide

This package ships instruction files for AI coding assistants. Here's how to wire them up for each tool.

All instruction files are located at:
```
Packages/Unity Best Practices/Editor/LLMInstructions/
```

## Claude Code (CLAUDE.md)

Create a `CLAUDE.md` file in your project root. Reference the instruction files you want Claude to follow:

```markdown
# CLAUDE.md

@Packages/com.unity.best-practices/Editor/LLMInstructions/UnityCodeStyleInstructions.md
@Packages/com.unity.best-practices/Editor/LLMInstructions/UnityPerformanceOptimizationInstructions.md
@Packages/com.unity.best-practices/Editor/LLMInstructions/UnityDesignPatternsInstructions.md
@Packages/com.unity.best-practices/Editor/LLMInstructions/UnityDebuggingInstructions.md
@Packages/com.unity.best-practices/Editor/LLMInstructions/UnityUIToolkitInstructions.md
```

Or use the consolidated file for all topics at once:
```markdown
@Packages/com.unity.best-practices/Editor/LLMInstructions/copilot-instructions.md
```

## GitHub Copilot

Create `.github/copilot-instructions.md` and paste the contents of the instruction files you want, or reference them:

The `copilot-instructions.md` file in `LLMInstructions/` is already formatted for Copilot — copy it to `.github/copilot-instructions.md`.

## Gemini CLI

Create `.gemini/GEMINI.md` in your project root and reference or copy the instruction files.

## Codex / OpenAI

Create `AGENTS.md` in your project root. Paste or reference the relevant instruction file contents.

## Recommended file selection per task

| You're working on... | Use these files |
|---|---|
| New scripts / refactoring | `UnityCodeStyleInstructions.md` |
| Performance-sensitive systems | `UnityPerformanceOptimizationInstructions.md` |
| Architecture / patterns | `UnityDesignPatternsInstructions.md` |
| Debugging issues | `UnityDebuggingInstructions.md` |
| UI Toolkit screens | `UnityUIToolkitInstructions.md` |
| Tests | `UnityTestingInstructions.md` |
| Input System | `UnityInputSystemInstructions.md` |
| Scene loading | `UnitySceneManagementInstructions.md` |
| Audio systems | `UnityAudioInstructions.md` |
| Animations | `UnityAnimationInstructions.md` |
| Addressables | `UnityAddressablesInstructions.md` |
| Multiplayer | `UnityNetcodeInstructions.md` |
| Assembly setup | `UnityAssemblyDefinitionsInstructions.md` |
| Localization | `UnityLocalizationInstructions.md` |
| Everything | `copilot-instructions.md` (consolidated) |

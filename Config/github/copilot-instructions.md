# MonsterTools local routing instructions

- Prefer the local MonsterTools pipeline first.
- Treat `http://127.0.0.1:5050` as the active Copilot-compatible endpoint for this workspace.
- Route code-fix, implementation, search, validation, build, test, and workspace-edit requests through the MonsterTools execution engine before returning a final answer.
- Do not suggest cloud-only dependencies when a local deterministic worker exists.
- When a request requires filesystem or build action, surface tool calls or explicit action steps instead of free-form speculation.

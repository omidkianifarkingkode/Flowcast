🧩 Summary — What This Project Is
✅ Purpose

A generic, authoritative persistence layer for player progression data
It’s responsible for storing, syncing, and merging game progress documents (XP, level, inventory, etc.) in a namespace-based, atomic, offline-safe system.

✅ Responsibilities

Manage per-player namespaces (playerStats, inventory, global, …).

Support offline play → sync via version (server anchor) + progress (semantic metric).

Perform atomic batch saves with progress-first conflict resolution.

Maintain document integrity via SHA-256 hash.

Expose simple REST API:

GET /player-progress/profile

POST /player-progress/profile

Publish ProgressSaved events (optional) for other modules (Achievements, Economy).

Provide pluggable resolvers and validators per namespace.

✅ What It Is Not

No domain-specific rules (e.g., XP rewards, balance limits, achievements).

No server-authored state (wallets, matchmaking, inventories with security logic).

No gameplay computation; purely state storage and merge orchestration.

✅ When to use

For client-driven progress (offline-capable data).

As a shared backend for multiple game modules that need consistent player state storage.
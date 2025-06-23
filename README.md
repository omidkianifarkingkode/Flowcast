## 📦 Installation via UPM (Git URL)

To install the required packages in your Unity project:

### Step 1: Add Flowcast
```json
"com.kingkode.flowcast": "https://github.com/omidkianifarkingkode/Flowcast.git?path=Unity/Assets/Flowcast"
```

### Step 2: Add VContainer (Optional)
```json
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.9"
```

### Step 3: Add Flowcast.VContainer (Optional)
```json
"com.kingkode.flowcast.vcontainer": "https://github.com/omidkianifarkingkode/Flowcast.git?path=Unity/Assets/Flowcast.VContainer"
```

# 📘 Flowcast SDK — Project Definition & Architecture Overview

## Overview

**Flowcast** is a Unity SDK for deterministic multiplayer game simulation using a shared input-action architecture. It supports both turn-based and tick-based systems and is designed to be decoupled from Unity-specific APIs or networking libraries, making it portable to client or server environments.

## Vision

To provide a modular and extensible multiplayer gameplay framework where:

- All game logic is driven by user inputs and server-validated actions.
- Game state is updated deterministically on all clients using shared logic.
- The server may run full game logic (e.g. turn-based) or delegate to clients for complex real-time simulations (e.g. tower defense).
- Sync, rollback, and observability are built-in to handle desync and latency.

## 🔌 How to Use
> **Core Concepts**: [Flowcast Core Repository](https://github.com/omidkianifarkingkode/Flowcast/tree/main/Unity/Assets/Flowcast)  
> **DI Container Extension**: [Flowcast.VContainer (DI Integration)](https://github.com/omidkianifarkingkode/Flowcast/tree/main/Unity/Assets/Flowcast.VContainer)

> This SDK supports integration with the [VContainer](https://github.com/hadashiA/VContainer) dependency injection framework via the official extension repository above.
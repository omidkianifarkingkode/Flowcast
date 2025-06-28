# 🔌 Flowcast.VContainer — DI Extension for Flowcast SDK

> **GitHub**: [Flowcast.VContainer Repository](https://github.com/omidkianifarkingkode/Flowcast/tree/main/Unity/Assets/Flowcast.VContainer)  
> **Depends on**: [Flowcast Core](https://github.com/omidkianifarkingkode/Flowcast/tree/main/Unity/Assets/Flowcast) and [VContainer](https://github.com/hadashiA/VContainer)

This package provides a VContainer-based dependency injection setup for the Flowcast SDK. It allows developers to register and resolve Flowcast systems in a clean, modular, and testable way.

---

## ⚙️ Features

- DI-compatible setup for all Flowcast core systems (Input, Action, State, Sync, etc.)
- Centralized configuration via `FlowcastLifetimeScope`
- Easy integration into Unity projects using [VContainer](https://github.com/hadashiA/VContainer)
- Supports both simulation and server modes

---

## 📦 Installation via UPM (Git URL)

Edit your Unity project's `Packages/manifest.json` file:

### Step 1: Add Flowcast
```json
"com.kingkode.flowcast": "https://github.com/omidkianifarkingkode/Flowcast.git?path=Unity/Assets/Flowcast"
```

### Step 2: Add VContainer
```json
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.9"
```

### Step 3: Add Flowcast.VContainer
```json
"com.kingkode.flowcast.vcontainer": "https://github.com/omidkianifarkingkode/Flowcast.git?path=Unity/Assets/Flowcast.VContainer"
```

## 🧰 Usage

1. Create a new `FlowcastLifetimeScope` in your Unity scene.
2. Register required Flowcast systems and your own game-specific implementations.
3. Use `RegisterInputValidators` to automatically register all validators from your game's assembly.

```csharp
using VContainer;
using VContainer.Unity;
using Flowcast;
using Flowcast.VContainer;

public class FlowcastLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register Flowcast systems, input validators, and actions from specified assemblies
        builder.RegisterFlowcast(Assembly.GetExecutingAssembly());
    }
}
```

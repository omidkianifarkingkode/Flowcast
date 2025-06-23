> **GitHub**: [Flowcast Core Repository](https://github.com/omidkianifarkingkode/Flowcast/tree/main/Unity/Assets/Flowcast)  
> **Extension**: [Flowcast.VContainer (DI Integration)](https://github.com/omidkianifarkingkode/Flowcast/tree/main/Unity/Assets/Flowcast.VContainer)

> This SDK supports integration with the [VContainer](https://github.com/hadashiA/VContainer) dependency injection framework via the official extension repository above.

---

# 📘 Flowcast SDK — Project Definition & Architecture Overview

## Overview

**Flowcast** is a Unity SDK for deterministic multiplayer game simulation using a shared input-action architecture. It supports both turn-based and tick-based systems and is designed to be decoupled from Unity-specific APIs or networking libraries, making it portable to client or server environments.

## Vision

To provide a modular and extensible multiplayer gameplay framework where:

- All game logic is driven by user inputs and server-validated actions.
- Game state is updated deterministically on all clients using shared logic.
- The server may run full game logic (e.g. turn-based) or delegate to clients for complex real-time simulations (e.g. tower defense).
- Sync, rollback, and observability are built-in to handle desync and latency.

## Core Concepts

### Input Handling System

- `IInput`: Base interface for all inputs. Includes `Id`, `PlayerId`, `TurnOrTick`, and `Time`.
- `IInputValidator`: Validates player inputs (client or server side).
- `IInputCollector`: Collects, validates, logs, buffers, and forwards input.
- `IInputSender`: Sends inputs to server or other layers.
- `IInputLogger`: Logs inputs per tick for debugging, replay, or rollback.

### Action Handling System

- `IGameAction`: Represents any gameplay-changing action derived from an input.
- `IActionGenerator`: Converts a validated input into one or more actions.
- `IActionProcessor`: Applies action(s) to the GameState (used on server or client simulation).
- `IActionApplier`: Client-side application of actions to game state (for visuals and gameplay).

### GameState & Sync

- `IGameState`: Represents local or server-side state.
- `ITickRunner` / `ITurnRunner`: Drives simulation in ticks or turns.
- `IStateHasher`: Produces hash values for game state validation.
- `IRollbackManager`: Detects and rolls back when state mismatch occurs.

### Transfer Layer (Network-Agnostic)

- InputSender / Receiver: For transferring inputs between clients and server.
- ActionBroadcaster: For syncing generated actions and authoritative state hashes.
- Loggers and Buffers for all layers to support latency, recovery, replay, and debug.

## Design Goals

- All logic is defined in shared .NET-compatible code (Unity independent).
- Inputs and actions are serializable and traceable.
- Minimal coupling between modules (DI-ready, testable).
- Server-side simulation is optional and mode-configurable.
- Modular pipeline: Input → Validation → Action → Processing → Sync

## Modes of Operation

| Mode                 | Description                                                                 |
|----------------------|-----------------------------------------------------------------------------|
| **Server-authoritative** | Server validates inputs, generates actions, updates state                      |
| **Client-authoritative** | Client validates/generates actions; server may replay/verify                  |
| **Hybrid simulation**     | Inputs validated on server, actions generated and simulated on client       |

## 🧰 Usage
## Defining Custom Inputs and Validators

### 1. Define your input by inheriting from `InputBase` or implementing `IInput`:
```csharp
public class SpawnInput : InputBase
{
    public int ObjectId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public SpawnInput(int objectId, int x, int y)
    {
        ObjectId = objectId;
        X = x;
        Y = y;
    }
}
```
### 2. Define a validator by inheriting from 'InputValidatorBase<T>' or implementing 'IInputValidator<T>':
```csharp
public class SpawnInputValidator : InputValidatorBase<SpawnInput>
{
    public override Result Validate(SpawnInput input)
    {
        return input switch
        {
            { ObjectId: > 0, X: >= 0, Y: >= 0 } => Result.Success(),
            _ => Result.Failure("Invalid SpawnInput parameters.")
        };
    }
}
```

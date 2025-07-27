using Flowcast.Data;
using Flowcast;
using System;
using UnityEngine;
using UnityEngine.UI;
using Flowcast.Commands;
using System.Collections.Generic;
using Flowcast.Network;
using System.Linq;
using FixedMathSharp;

public class GameSimulation : MonoBehaviour
{
    public Button spawnArcherButton;
    public Button spawnWarriorButton;
    public Button rollbackButton;

    public CharacterFactory factory;
    public Timeline timeline;
    public Lockstepline lockstepline;
    public PathHelper pathHelper;

    public GameState GameState { get; } = new();

    private ILockstepEngine flowcast;
    private List<CharacterPresenter> characters = new();
    private List<CharacterView> charactersView = new();

    private DummyNetworkServer server;
    private ulong tick;

    private void Awake()
    {
        spawnArcherButton.onClick.AddListener(HandleSpawnArcherButton);
        spawnWarriorButton.onClick.AddListener(HandleSpawnWarriorButton);
        rollbackButton.onClick.AddListener(HandleRollbackButton);
    }

    private void Start()
    {
        var matchInfo = new MatchInfo()
        {
            LocalPlayerId = 1,
            MatchId = Guid.NewGuid().ToString(),
            Players = new BasePlayerInfo[]
                {
                    new BasePlayerInfo { PlayerId = 1, DisplayName = "P1" },
                    new BasePlayerInfo { PlayerId = 2, DisplayName = "P2" }
                },
            ServerStartTimeUtc = DateTime.UtcNow,
        };

        var lockstepInitializer = GetComponent<LockstepInitializer>();
        lockstepInitializer.OnCommandReceived.AddListener(HandleCommandReceived);
        lockstepInitializer.OnRollback.AddListener(HandleRollback);
        lockstepInitializer.OnTick.AddListener(HandleTick);
        flowcast = lockstepInitializer.Initialize(GameState, matchInfo);

        timeline.StartTimer();
        lockstepline.Initialize(flowcast.Options.GameFramesPerSecond);
    }

    private void ProcessSpawnCommand(ICommand command)
    {
        if (command is SpawnCommand spawnCommand)
        {
            AddCharacter(spawnCommand.UnitType);
        }
    }

    private void AddCharacter(CharacterType unitType)
    {
        if (!factory.TrySpawnCharacter(unitType, out var data, out var presenter, out var view))
            return;

        GameState.characters.Add(data);
        characters.Add(presenter);
        charactersView.Add(view);
    }

    private void Tick()
    {
        //Debug.Log($"Fixed:{flowcast.LockstepProvider.FixedDeltaTime}, n:{flowcast.LockstepProvider.DeltaTime}");
        foreach (var character in characters)
            character.Tick(flowcast.LockstepProvider.FixedDeltaTime);

        var charactersToRemove = characters.Where(x => x.HasReached).ToList();

        foreach (var character in charactersToRemove)
        {
            characters.Remove(character);
            charactersView.Remove(character.View);
            Destroy(character.View.gameObject);
        }
    }

    private void HandleSpawnArcherButton()
    {
        var spawnCommand = new SpawnCommand(pathHelper.FirstPoint, CharacterType.Archer);

        flowcast.SubmitCommand(spawnCommand);
    }

    private void HandleSpawnWarriorButton()
    {
        var spawnCommand = new SpawnCommand(pathHelper.FirstPoint, CharacterType.Warrior);

        flowcast.SubmitCommand(spawnCommand);
    }

    private void HandleRollbackButton()
    {
        server ??= FindObjectOfType<DummerServerRunner>().Server;

        server.RequestRollback(tick + 10);
    }

    public void HandleTick(TickWrapper bundle)
    {
        tick = bundle.Tick;
        lockstepline.Tick(bundle.Tick);
    }

    public void HandleRollback(RollbackWrapper bundle)
    {
        Debug.Log($"Rollback at Tick: {bundle.Tick}, State Type: {bundle.State?.GetType().Name}");

        foreach (var character in charactersView)
            Destroy(character.gameObject);

        charactersView.Clear();

        characters.Clear();

        foreach (var character in GameState.characters)
        {
            if (!factory.TrySpawnCharacter(character, out var presenter, out var view))
                return;

            characters.Add(presenter);
            charactersView.Add(view);
        }
    }

    public void HandleCommandReceived(CommandWrapper bundle) 
    {
        if (bundle.Command.Frame < flowcast.LockstepProvider.CurrentGameFrame)
        {
            Debug.LogWarning("Late Command");
            return;
        }

        ProcessSpawnCommand(bundle.Command);
    }
}

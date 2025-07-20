using Flowcast.Data;
using Flowcast;
using System;
using UnityEngine;
using UnityEngine.UI;
using Flowcast.Commands;

public class GameSimulation : MonoBehaviour
{
    public CharacterView characterPrefab;
    public CharacterStaticData characterStats;
    public Button spawnButton;

    public Transform spawnPoint;
    public Transform targetPoint;

    public GameState GameState { get; } = new();
    private CharacterPresenter character;

    ILockstepEngine flowcast;

    private void Awake()
    {
        spawnButton.onClick.AddListener(HandleSpawnButton);
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

        flowcast = FlowcastBuilder.CreateLockstep()
                .SetMatchInfo(matchInfo)
                .ConfigureCommandSystem(command => command
                    .OnCommandReceived(command =>
                    {
                        HandleSpawnCommand(command);
                        Debug.Log("Received: " + command);
                    })
                    .HandleCommandsOnGameFrame())
                .SynchronizeGameState(syncSetup => syncSetup
                    .UseDefaultOptions()
                    .UseBinarySerializer(GameState)
                    .OnRollback<GameState>((snapshot, frame) =>
                    {
                        Debug.Log("Rollback");
                    }))
                .SetupNetworkServices(networkSetup => networkSetup
                    .UseDummyServer(new()
                    {
                        BaseLatencyMs = 100,
                        EchoCommands = true,
                    }))
                .ConfigureSimulationPipeline(piplineSetup => piplineSetup
                    .HandleStepManually(tick =>
                    {
                        Tick(0.02f);
                    }))
                .BuildAndStart();
    }

    private void HandleSpawnCommand(ICommand command)
    {
        if (command is SpawnCommand spawnCommand)
        {
            AddCharacter(spawnPoint.position, characterStats);
        }
    }

    private void AddCharacter(Vector2 start, CharacterStaticData staticData)
    {
        var data = new CharacterData { Health = 100, Position = start };
        GameState.character = data;

        var presenter = new CharacterPresenter(data, targetPoint.position, staticData);
        character = presenter;

        // Create view as well
        var view = Instantiate(characterPrefab, data.Position, Quaternion.identity);
        view.Init(presenter);
    }

    public void Tick(float deltaTime)
    {
        if (character != null)
            character.Tick(deltaTime);
    }

    private void HandleSpawnButton()
    {
        var spawnCommand = new SpawnCommand(spawnPoint.position, "1");

        flowcast.SubmitCommand(spawnCommand);
    }
}
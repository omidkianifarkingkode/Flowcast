using FixedMathSharp;
using FlowPipeline;
using System.Linq;
using UnityEngine;

public class CharacterFactory : MonoBehaviour
{
    [SerializeField] CharacterStaticData[] characters;

    [SerializeField] PathHelper pathHelper;

    [SerializeField] FlowPipelineBuilder pipelineBuilder;

    public bool TrySpawnCharacter(CharacterType characterType, out CharacterData data, out CharacterPresenter presenter, out CharacterView view)
    {
        CharacterStaticData character = GetCharacter(characterType);

        if (character is null)
        {
            data = default;
            presenter = default;
            view = default;

            return false;
        }

        data = new CharacterData { Type = characterType, Health = character.HP, Position = pathHelper.FirstPoint.ToVector2d() };

        presenter = new CharacterPresenter(data, pathHelper.Path, character);
        presenter.RegisterStep(pipelineBuilder.Pipeline.GetStep<IMovable>());
        presenter.RegisterStep(pipelineBuilder.Pipeline.GetStep<IDespawnable>());

        view = Instantiate(character.Prefab, data.Position.ToVector3(), Quaternion.identity);
        view.Init(presenter);
        presenter.SetView(view);

        return true;
    }

    public bool TrySpawnCharacter(CharacterData data, out CharacterPresenter presenter, out CharacterView view)
    {
        CharacterStaticData character = GetCharacter(data.Type);

        if (character is null)
        {
            presenter = default;
            view = default;

            return false;
        }

        presenter = new CharacterPresenter(data, pathHelper.Path, character);
        presenter.RegisterStep(pipelineBuilder.Pipeline.GetStep<IMovable>());
        presenter.RegisterStep(pipelineBuilder.Pipeline.GetStep<IDespawnable>());

        view = Instantiate(character.Prefab, data.Position.ToVector3(), Quaternion.identity);
        view.Init(presenter);
        presenter.SetView(view);

        return true;
    }

    private CharacterStaticData GetCharacter(CharacterType characterType)
    {
        return characters.FirstOrDefault(x => x.Name == characterType);
    }
}
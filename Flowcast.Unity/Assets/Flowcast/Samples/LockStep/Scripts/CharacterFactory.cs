using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterFactory : MonoBehaviour
{
    [SerializeField] CharacterStaticData archer;
    [SerializeField] CharacterStaticData warrior;

    [SerializeField] PathHelper pathHelper;

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

        data = new CharacterData { Type = characterType, Health = character.HP, Position = pathHelper.FirstPoint };

        presenter = new CharacterPresenter(data, pathHelper.Path, character);

        view = Instantiate(character.Prefab, data.Position, Quaternion.identity);
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

        view = Instantiate(character.Prefab, data.Position, Quaternion.identity);
        view.Init(presenter);
        presenter.SetView(view);

        return true;
    }

    private CharacterStaticData GetCharacter(CharacterType characterType)
    {
        CharacterStaticData unit = null;

        switch (characterType)
        {
            case CharacterType.Archer:
                unit = archer;
                break;
            case CharacterType.Warrior:
                unit = warrior;
                break;
        }

        return unit;
    }
}
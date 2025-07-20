using UnityEngine;

public class CharacterView : MonoBehaviour
{
    public CharacterPresenter Presenter { get; private set; }

    public void Init(CharacterPresenter presenter)
    {
        Presenter = presenter;
        Presenter.OnReachedTarget = HandleReachedTarget;
        transform.position = Presenter.Data.Position;
    }

    private void Update()
    {
        if (Presenter == null) return;

        transform.position = Presenter.Data.Position;
    }

    private void HandleReachedTarget()
    {
        Debug.Log("[CharacterView] Reached target. Destroying.");
        Destroy(gameObject);
    }
}

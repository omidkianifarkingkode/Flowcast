using UnityEngine;

public class CharacterView : MonoBehaviour
{
    public CharacterPresenter Presenter { get; private set; }

    public void Init(CharacterPresenter presenter)
    {
        Presenter = presenter;
        Presenter.OnReachedTarget = HandleReachedTarget;
        transform.position = new Vector3(
            (float)Presenter.Data.Position.x,
            (float)Presenter.Data.Position.y,
            transform.position.z
        );
    }

    private void Update()
    {
        if (Presenter == null) return;

        transform.position = new Vector3(
            (float)Presenter.Data.Position.x,
            (float)Presenter.Data.Position.y,
            transform.position.z
        );
    }

    private void HandleReachedTarget()
    {
        Debug.Log("[CharacterView] Reached target. Destroying.");
        Destroy(gameObject);
    }
}

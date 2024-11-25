public class GameStartButton : ButtonUI
{
    RoomUI _roomUI;

    protected override void Awake()
    {
        base.Awake();

        _roomUI = Util.FindParentByName(gameObject, "RoomUI").GetComponent<RoomUI>();
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        _roomUI.SendStart();
    }
}

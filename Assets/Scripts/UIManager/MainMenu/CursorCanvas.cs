using UnityEngine;

public class CursorCanvas : MonoBehaviour
{
    public static CursorCanvas Instance { get; private set; }

    [SerializeField] private RectTransform cursorRect;
    [SerializeField] private Canvas canvas;

    [SerializeField] private GameObject existingCanvasCursor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (existingCanvasCursor != null)
        {
            cursorRect = existingCanvasCursor.GetComponent<RectTransform>();
            canvas = existingCanvasCursor.GetComponent<Canvas>();
        }

        if (canvas != null)
        {
            canvas.sortingOrder = 9999;
        }
    }

    public void SetCursorActive(bool active)
    {
        if (cursorRect != null)
        {
            cursorRect.gameObject.SetActive(active);
        }

        Cursor.visible = !active;
    }

    public void SetCursorPosition(Vector3 position)
    {
        if (cursorRect != null)
        {
            cursorRect.position = position;
        }
    }

    public void CenterCursor()
    {
        if (cursorRect != null)
        {
            cursorRect.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }
    }
}
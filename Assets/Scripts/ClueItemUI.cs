using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClueItemUI : MonoBehaviour
{
    public TMP_Text label;
    Button _btn;
    int _index;
    CrosswordGame _game;

    public void Setup(CrosswordGame game, int index, string text)
    {
        _game = game;
        _index = index;
        label.text = text;
        if (_btn == null) _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(() => _game.SelectClue(_index));
    }
}


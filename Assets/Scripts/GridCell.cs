using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridCell : MonoBehaviour
{
    public TMP_Text letterText;
    public Image background;

    [HideInInspector] public Vector2Int coord;
    [HideInInspector] public bool used;

    Color _normal, _highlight, _solved;

    public void Init(Vector2Int c, Color normal, Color highlight, Color solved)
    {
        coord = c;
        _normal = normal; _highlight = highlight; _solved = solved;
        SetLetter("");
        SetNormal();
    }

    public void SetLetter(string s) => letterText.text = s;
    public void SetNormal() => background.color = _normal;
    public void SetHighlight(bool on) => background.color = on ? _highlight : _normal;
    public void SetSolved() => background.color = _solved;
}


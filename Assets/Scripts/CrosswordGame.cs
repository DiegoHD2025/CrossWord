using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class CrosswordGame : MonoBehaviour
{
    public enum Difficulty { Basic, Intermediate, Advanced }

    [Header("Config")]
    public Difficulty difficulty = Difficulty.Basic;
    public int gridSize = 12;
    public GameObject cellPrefab;
    public Transform gridRoot;
    public GameObject clueItemPrefab;
    public Transform cluesContent;

    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public TMP_Text wordsLeftText;
    public TMP_Text activeClueText;
    public TMP_InputField answerInput;
    public Button submitButton;

    [Header("Colors")]
    public Color cellNormal = new Color(0.90f, 0.90f, 0.95f, 1f);
    public Color cellHighlight = new Color(0.75f, 0.85f, 1f, 1f);
    public Color cellSolved = new Color(0.70f, 1f, 0.75f, 1f);

    [Serializable]
    public class WordEntry
    {
        public string word;
        [TextArea] public string clue;

        // runtime
        [NonSerialized] public bool vertical;
        [NonSerialized] public Vector2Int start;
        [NonSerialized] public bool placed;
        [NonSerialized] public bool solved;
        [NonSerialized] public List<Vector2Int> cells = new();
    }

    [Header("Word Bank (opcional)")]
    public List<WordEntry> wordBank = new();

    // runtime
    GridCell[,] _cells;
    char[,] _grid;
    List<WordEntry> _levelWords = new();
    WordEntry _activeWord;
    float _timeRemaining;
    bool _running;
    int _score;

    void Start()
    {
        if (wordBank.Count == 0) BuildDefaultBank();
        SetupLevel();
        BuildGrid();
        BuildCluesUI();
        HookUI();

        _running = true;
    }

    void Update()
    {
        if (!_running) return;

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining < 0)
        {
            _timeRemaining = 0;
            Lose();
        }
        UpdateTimerUI();

        if (Input.GetKeyDown(KeyCode.Return))
            SubmitAnswer();
    }

    // ---------- Level / Timer ----------
    void SetupLevel()
    {
        int targetWords;
        switch (difficulty)
        {
            case Difficulty.Basic:
                targetWords = 10; _timeRemaining = 5 * 60; break;
            case Difficulty.Intermediate:
                targetWords = 20; _timeRemaining = 10 * 60; break;
            default:
                targetWords = 30; _timeRemaining = 15 * 60; break;
        }

        levelText.text = $"Nivel: {difficulty}";
        _score = 0;
        UpdateScoreUI();

        // elige palabras al azar sin repetir
        var pool = new List<WordEntry>(wordBank);
        _levelWords.Clear();
        for (int i = 0; i < targetWords && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            var w = Clone(pool[idx]);     // clonar para no pisar el banco
            pool.RemoveAt(idx);
            _levelWords.Add(w);
        }

        wordsLeftText.text = $"Pendientes: {_levelWords.Count}/{_levelWords.Count}";
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int m = Mathf.FloorToInt(_timeRemaining / 60f);
        int s = Mathf.FloorToInt(_timeRemaining % 60f);
        timerText.text = $"Tiempo: {m:00}:{s:00}";
    }

    void UpdateScoreUI() => scoreText.text = $"Puntos: {_score}";

    // ---------- Grid build ----------
    void BuildGrid()
    {
        // limpiar previos
        foreach (Transform t in gridRoot) Destroy(t.gameObject);

        _grid = new char[gridSize, gridSize];
        _cells = new GridCell[gridSize, gridSize];

        PlaceAllWords();

        // Instanciar celdas UI
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                var go = Instantiate(cellPrefab, gridRoot);
                var cell = go.GetComponent<GridCell>();
                _cells[x, y] = cell;
                cell.Init(new Vector2Int(x, y), cellNormal, cellHighlight, cellSolved);

                if (_grid[x, y] == '\0')
                {
                    // celda no usada: vaciar visualmente
                    cell.SetLetter("");
                    cell.background.enabled = false;
                }
                else
                {
                    cell.used = true;
                    cell.SetLetter(""); // oculto hasta resolver
                }
            }
        }
    }

    void PlaceAllWords()
    {
        // intentos simples de colocación con cruces válidas
        foreach (var w in _levelWords)
        {
            TryPlaceWord(w, 500);
        }
    }

    bool TryPlaceWord(WordEntry w, int maxTries)
    {
        string word = w.word.ToUpperInvariant();
        for (int attempt = 0; attempt < maxTries; attempt++)
        {
            bool vertical = Random.value < 0.5f;
            int len = word.Length;

            int startX = vertical ? Random.Range(0, gridSize) : Random.Range(0, gridSize - len + 1);
            int startY = vertical ? Random.Range(0, gridSize - len + 1) : Random.Range(0, gridSize);

            // comprobar colisiones
            bool fits = true;
            for (int i = 0; i < len && fits; i++)
            {
                int x = startX + (vertical ? 0 : i);
                int y = startY + (vertical ? i : 0);
                char c = _grid[x, y];
                if (c != '\0' && c != word[i]) fits = false;
            }
            if (!fits) continue;

            // colocar
            w.cells.Clear();
            for (int i = 0; i < len; i++)
            {
                int x = startX + (vertical ? 0 : i);
                int y = startY + (vertical ? i : 0);
                _grid[x, y] = word[i];
                w.cells.Add(new Vector2Int(x, y));
            }
            w.vertical = vertical;
            w.start = new Vector2Int(startX, startY);
            w.placed = true;
            return true;
        }
        Debug.LogWarning($"No se pudo colocar: {w.word}");
        return false;
    }

    // ---------- Clues UI ----------
    void BuildCluesUI()
    {
        foreach (Transform t in cluesContent) Destroy(t.gameObject);

        for (int i = 0; i < _levelWords.Count; i++)
        {
            var w = _levelWords[i];
            string label = $"{i + 1}. {w.clue}";
            var go = Instantiate(clueItemPrefab, cluesContent);
            go.GetComponent<ClueItemUI>().Setup(this, i, label);
        }

        // seleccionar primera pista
        SelectClue(0);
    }

    public void SelectClue(int index)
    {
        if (index < 0 || index >= _levelWords.Count) return;
        _activeWord = _levelWords[index];
        activeClueText.text = _activeWord.clue;

        // resaltar sus celdas
        ClearHighlights();
        if (_activeWord.placed)
        {
            foreach (var p in _activeWord.cells)
                _cells[p.x, p.y].SetHighlight(true);
        }
    }

    void ClearHighlights()
    {
        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                if (_cells[x, y] != null) _cells[x, y].SetHighlight(false);
    }

    // ---------- Input / Submit ----------
    void HookUI()
    {
        submitButton.onClick.AddListener(SubmitAnswer);
        answerInput.text = "";
        answerInput.onSelect.AddListener(_ => answerInput.caretPosition = answerInput.text.Length);
    }

    public void SubmitAnswer()
    {
        if (!_running || _activeWord == null || _activeWord.solved) return;

        string guess = answerInput.text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(guess)) return;

        if (guess == _activeWord.word.ToUpperInvariant())
        {
            // correcto: revelar letras
            for (int i = 0; i < _activeWord.cells.Count; i++)
            {
                var p = _activeWord.cells[i];
                _cells[p.x, p.y].SetLetter(_grid[p.x, p.y].ToString());
                _cells[p.x, p.y].SetSolved();
            }
            _activeWord.solved = true;
            _score += 5;
            UpdateScoreUI();

            int solved = 0;
            foreach (var w in _levelWords) if (w.solved) solved++;
            wordsLeftText.text = $"Pendientes: {_levelWords.Count - solved}/{_levelWords.Count}";

            if (solved == _levelWords.Count)
            {
                Win();
            }
        }
        else
        {
            // opcional: feedback de error
            // Debug.Log("Respuesta incorrecta");
        }

        answerInput.text = "";
        answerInput.ActivateInputField();
    }

    // ---------- Win/Lose ----------
    void Win()
    {
        _running = false;
        activeClueText.text = "¡Nivel completado!";
        submitButton.interactable = false;
        answerInput.interactable = false;
        // aquí puedes cargar siguiente nivel o mostrar panel de victoria
    }

    void Lose()
    {
        _running = false;
        activeClueText.text = "Se acabó el tiempo";
        submitButton.interactable = false;
        answerInput.interactable = false;
        // panel de derrota si quieres
    }

    // ---------- Utils ----------
    WordEntry Clone(WordEntry src)
    {
        return new WordEntry { word = src.word, clue = src.clue };
    }

    void BuildDefaultBank()
    {
        // 30 palabras para cubrir los 3 niveles
        string[,] data = {
            {"APPLE","Fruta: empieza con A y termina con E (5)"},
            {"DOG","Animal doméstico (3)"},
            {"CAT","Felino doméstico (3)"},
            {"HOUSE","Casa (5)"},
            {"CAR","Automóvil (3)"},
            {"BOOK","Libro (4)"},
            {"TABLE","Mesa (5)"},
            {"CHAIR","Silla (5)"},
            {"WATER","Agua (5)"},
            {"SUN","Sol (3)"},
            {"MOON","Luna (4)"},
            {"TREE","Árbol (4)"},
            {"SCHOOL","Escuela (6)"},
            {"WINDOW","Ventana (6)"},
            {"DOOR","Puerta (4)"},
            {"BREAD","Pan (5)"},
            {"MILK","Leche (4)"},
            {"CHEESE","Queso (6)"},
            {"RED","Rojo (3)"},
            {"BLUE","Azul (4)"},
            {"GREEN","Verde (5)"},
            {"HAPPY","Feliz (5)"},
            {"RUN","Correr (3)"},
            {"JUMP","Saltar (4)"},
            {"FAMILY","Familia (6)"},
            {"CITY","Ciudad (4)"},
            {"COMPUTER","Computadora (8)"},
            {"PHONE","Teléfono (5)"},
            {"FLOWER","Flor (6)"},
            {"MUSIC","Música (5)"}
        };

        for (int i = 0; i < data.GetLength(0); i++)
            wordBank.Add(new WordEntry { word = data[i, 0], clue = data[i, 1] });
    }
}


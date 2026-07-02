using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Murdoku
{
    /// <summary>
    /// Construye la interfaz con Canvas (uGUI) para que funcione bien en portrait móvil.
    /// </summary>
    public class MurdokuRuntimeUI : MonoBehaviour
    {
        public event Action OnNewCrimeClicked;
        public event Action<GameMode> OnDifficultyChosen;
        public event Action OnDifficultyCancelled;
        public event Action<GridPosition> OnCellClicked;
        public event Action<MurdokuSuspect> OnSuspectSelected;
        public event Action<MurdokuSuspect, bool> OnMurdererToggled;
        public event Action OnRemoveSelected;
        public event Action<bool> OnSolutionToggled;
        public event Action OnCheckCase;

        private Text _modeLabel;
        private Text _statusLabel;
        private Text _boardTitle;
        private Transform _boardRoot;
        private Transform _suspectsRoot;
        private ScrollRect _mainScroll;
        private GameObject _difficultyOverlay;
        private Toggle _solutionToggle;
        private readonly List<GameObject> _cellButtons = new();
        private readonly List<GameObject> _suspectRows = new();
        private Font _font;
        private MurdokuVisuals _visuals;

        public void SetVisuals(MurdokuVisuals visuals) => _visuals = visuals;

        public void Build()
        {
            EnsureEventSystem();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("MurdokuCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;

            canvasGo.AddComponent<GraphicRaycaster>();

            var root = CreatePanel(canvasGo.transform, "Root", new Color(0.12f, 0.12f, 0.14f, 1f));
            StretchFull(root);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(root.transform, false);
            StretchFull(scrollGo.GetComponent<RectTransform>());

            var viewport = CreatePanel(scrollGo.transform, "Viewport", Color.clear);
            StretchFull(viewport);
            viewport.AddComponent<RectMask2D>();

            var content = CreatePanel(viewport.transform, "Content", Color.clear);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 40);
            layout.spacing = 16;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            _mainScroll = scroll;

            CreateText(content.transform, "Murdoku", 52, FontStyle.Bold, TextAnchor.MiddleCenter);
            _modeLabel = CreateText(content.transform, "Modo: Fácil (4×4)", 28, FontStyle.Normal, TextAnchor.MiddleLeft);

            CreateButton(content.transform, "Ir a otro crimen", new Color(0.2f, 0.55f, 0.3f), 90,
                () => OnNewCrimeClicked?.Invoke());

            _boardTitle = CreateText(content.transform, "Escena del crimen", 34, FontStyle.Bold, TextAnchor.MiddleLeft);

            var boardContainer = new GameObject("BoardContainer", typeof(RectTransform), typeof(LayoutElement));
            boardContainer.transform.SetParent(content.transform, false);
            boardContainer.GetComponent<LayoutElement>().preferredHeight = 520;
            _boardRoot = boardContainer.transform;

            CreateText(content.transform, "Sospechosos y pistas", 34, FontStyle.Bold, TextAnchor.MiddleLeft);

            var suspectsContainer = new GameObject(
                "SuspectsContainer",
                typeof(RectTransform),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(LayoutElement));
            suspectsContainer.transform.SetParent(content.transform, false);
            suspectsContainer.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.2f, 1f);
            var suspectsLayout = suspectsContainer.GetComponent<VerticalLayoutGroup>();
            suspectsLayout.padding = new RectOffset(12, 12, 12, 12);
            suspectsLayout.spacing = 10;
            suspectsLayout.childControlWidth = true;
            suspectsLayout.childControlHeight = true;
            suspectsLayout.childForceExpandWidth = true;
            suspectsLayout.childForceExpandHeight = false;
            suspectsContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _suspectsRoot = suspectsContainer.transform;

            var actionsRow = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            actionsRow.transform.SetParent(content.transform, false);
            actionsRow.GetComponent<LayoutElement>().preferredHeight = 70;
            var hLayout = actionsRow.GetComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 12;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            CreateButton(actionsRow.transform, "Quitar", new Color(0.45f, 0.45f, 0.5f), 70, () => OnRemoveSelected?.Invoke());

            var toggleGo = new GameObject("SolutionToggle", typeof(RectTransform), typeof(Toggle), typeof(Image), typeof(LayoutElement));
            toggleGo.transform.SetParent(actionsRow.transform, false);
            toggleGo.GetComponent<Image>().color = new Color(0.35f, 0.35f, 0.4f);
            _solutionToggle = toggleGo.GetComponent<Toggle>();
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(toggleGo.transform, false);
            StretchFull(labelGo.GetComponent<RectTransform>());
            var label = labelGo.GetComponent<Text>();
            label.text = "Solución";
            label.font = _font;
            label.fontSize = 26;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            _solutionToggle.onValueChanged.AddListener(v => OnSolutionToggled?.Invoke(v));

            CreateButton(content.transform, "Comprobar caso", new Color(0.25f, 0.4f, 0.7f), 80,
                () => OnCheckCase?.Invoke());

            _statusLabel = CreateText(content.transform, "", 24, FontStyle.Normal, TextAnchor.UpperLeft);
            _statusLabel.gameObject.AddComponent<LayoutElement>().minHeight = 120;

            BuildDifficultyOverlay(canvasGo.transform);
        }

        private void BuildDifficultyOverlay(Transform parent)
        {
            _difficultyOverlay = CreatePanel(parent, "DifficultyOverlay", new Color(0, 0, 0, 0.75f));
            StretchFull(_difficultyOverlay);

            var panel = CreatePanel(_difficultyOverlay.transform, "Panel", new Color(0.2f, 0.2f, 0.24f, 1f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700, 640);
            panelRect.anchoredPosition = Vector2.zero;

            var v = panel.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(32, 32, 32, 32);
            v.spacing = 20;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            CreateText(panel.transform, "Elegir dificultad", 40, FontStyle.Bold, TextAnchor.MiddleCenter);
            CreateText(panel.transform, "Selecciona el tamaño del nuevo crimen:", 26, FontStyle.Normal, TextAnchor.MiddleCenter);

            CreateButton(panel.transform, "Fácil (4×4)", new Color(0.2f, 0.55f, 0.3f), 80,
                () => OnDifficultyChosen?.Invoke(GameMode.Facil));
            CreateButton(panel.transform, "Medio (6×6)", new Color(0.55f, 0.5f, 0.2f), 80,
                () => OnDifficultyChosen?.Invoke(GameMode.Medio));
            CreateButton(panel.transform, "Difícil (8×8)", new Color(0.55f, 0.3f, 0.2f), 80,
                () => OnDifficultyChosen?.Invoke(GameMode.Dificil));
            CreateButton(panel.transform, "Cancelar", new Color(0.4f, 0.4f, 0.45f), 70,
                () => OnDifficultyCancelled?.Invoke());

            _difficultyOverlay.SetActive(false);
        }

        public void SetModeLabel(string text) => _modeLabel.text = text;
        public void SetStatus(string text) => _statusLabel.text = text;
        public void ShowDifficultyPicker(bool show) => _difficultyOverlay.SetActive(show);
        public void SetSolutionToggle(bool value) => _solutionToggle.isOn = value;
        public void ScrollToTop()
        {
            if (_mainScroll != null)
                _mainScroll.verticalNormalizedPosition = 1f;
        }

        public void RefreshBoard(MurdokuPuzzle puzzle, MurdokuSuspect selected, bool showSolution)
        {
            foreach (var go in _cellButtons)
                Destroy(go);
            _cellButtons.Clear();

            var grid = _boardRoot.GetComponent<GridLayoutGroup>();
            if (grid == null)
                grid = _boardRoot.gameObject.AddComponent<GridLayoutGroup>();

            var spacing = puzzle.Size >= 8 ? 4f : 8f;
            var cellSize = GetCellSize(puzzle.Size, spacing);
            grid.cellSize = new Vector2(cellSize, cellSize);
            grid.spacing = new Vector2(spacing, spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = puzzle.Size;
            grid.childAlignment = TextAnchor.MiddleCenter;

            var boardLayout = _boardRoot.gameObject.GetComponent<LayoutElement>() ?? _boardRoot.gameObject.AddComponent<LayoutElement>();
            boardLayout.preferredWidth = puzzle.Size * cellSize + (puzzle.Size - 1) * spacing;
            boardLayout.preferredHeight = puzzle.Size * cellSize + (puzzle.Size - 1) * spacing + 20;

            for (var row = 0; row < puzzle.Size; row++)
            {
                for (var col = 0; col < puzzle.Size; col++)
                {
                    var pos = new GridPosition(row, col);
                    var cell = puzzle.GetCell(row, col);
                    var occupant = puzzle.GetSuspectAt(pos);
                    var capturedPos = pos;

                    var color = RoomCatalog.RoomColors[cell.Room];
                    if (occupant != null)
                        color = Color.Lerp(color, Color.white, 0.35f);

                    var floorSprite = _visuals?.GetFurnitureSprite(FurnitureType.Suelo);
                    var furnitureSprite = cell.Furniture != FurnitureType.Suelo
                        ? _visuals?.GetFurnitureSprite(cell.Furniture)
                        : null;
                    var label = BuildCellLabel(puzzle, cell, occupant, showSolution, furnitureSprite != null);
                    var btn = CreateCellButton(_boardRoot, label, color, cellSize, puzzle.Size,
                        () => OnCellClicked?.Invoke(capturedPos));
                    if (floorSprite != null)
                        AddFloorSprite(btn.transform, floorSprite);
                    if (furnitureSprite != null)
                        AddFurnitureIcon(btn.transform, furnitureSprite, cellSize);
                    _cellButtons.Add(btn);
                }
            }
        }

        public void RefreshSuspects(MurdokuPuzzle puzzle, MurdokuSuspect selected, string accusedMurderer)
        {
            foreach (var go in _suspectRows)
                Destroy(go);
            _suspectRows.Clear();

            foreach (var suspect in puzzle.Suspects)
            {
                var row = CreatePanel(_suspectsRoot, suspect.Name, new Color(0.22f, 0.22f, 0.26f, 1f));
                row.AddComponent<LayoutElement>().minHeight = 100;
                var v = row.AddComponent<VerticalLayoutGroup>();
                v.padding = new RectOffset(12, 12, 10, 10);
                v.spacing = 6;
                v.childControlWidth = true;
                v.childControlHeight = true;
                v.childForceExpandWidth = true;

                var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                header.transform.SetParent(row.transform, false);
                var h = header.GetComponent<HorizontalLayoutGroup>();
                h.spacing = 8;
                h.childForceExpandWidth = true;
                h.childForceExpandHeight = true;

                var isSelected = selected == suspect;
                var victimTag = suspect.IsVictim ? " [VÍCTIMA]" : "";
                CreateButton(header.transform, $"{(isSelected ? "▶ " : "")}{suspect.Name}{victimTag}",
                    isSelected ? new Color(0.3f, 0.5f, 0.7f) : new Color(0.35f, 0.35f, 0.4f), 56,
                    () => OnSuspectSelected?.Invoke(suspect));

                var isAccused = accusedMurderer == suspect.Name;
                CreateButton(header.transform, isAccused ? "✓ Asesino" : "Asesino",
                    isAccused ? new Color(0.7f, 0.25f, 0.2f) : new Color(0.45f, 0.35f, 0.35f), 56,
                    () => OnMurdererToggled?.Invoke(suspect, !isAccused));

                if (suspect.Clue != null)
                    CreateText(row.transform, suspect.Clue.Text, 22, FontStyle.Italic, TextAnchor.UpperLeft);

                if (suspect.PlacedPosition.HasValue)
                {
                    var p = suspect.PlacedPosition.Value;
                    CreateText(row.transform, $"→ Fila {p.Row + 1}, Col {p.Col + 1}", 20, FontStyle.Normal, TextAnchor.UpperLeft);
                }

                _suspectRows.Add(row);
            }
        }

        private void AddFloorSprite(Transform cellParent, Sprite sprite)
        {
            var go = new GameObject("FloorSprite", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(cellParent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = false;
            img.raycastTarget = false;
        }

        private void AddFurnitureIcon(Transform cellParent, Sprite sprite, float cellSize)
        {
            var iconGo = new GameObject("FurnitureIcon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(cellParent, false);
            var rect = iconGo.GetComponent<RectTransform>();
            var iconSize = cellSize * 0.55f;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(iconSize, iconSize);
            rect.anchoredPosition = new Vector2(0f, cellSize * 0.08f);
            var img = iconGo.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        private string BuildCellLabel(MurdokuPuzzle puzzle, MurdokuCell cell, MurdokuSuspect occupant, bool showSolution, bool hasFurnitureSprite = false)
        {
            var room = RoomCatalog.RoomNames[cell.Room];
            var furniture = RoomCatalog.FurnitureLabels[cell.Furniture];
            var compact = puzzle.Size >= 6;
            var lines = compact ? Abbreviate(room) : room.ToUpperInvariant();

            if (!string.IsNullOrEmpty(furniture) && !hasFurnitureSprite)
                lines += puzzle.Size >= 8 ? $"\n{Abbreviate(furniture)}" : $"\n{furniture}";

            if (occupant != null)
                lines += puzzle.Size >= 8
                    ? $"\n{occupant.Name[..Mathf.Min(3, occupant.Name.Length)]}"
                    : $"\n★ {occupant.Name}";

            if (showSolution)
            {
                foreach (var pair in puzzle.Solution)
                {
                    if (pair.Value.Row == cell.Row && pair.Value.Col == cell.Col)
                        lines += $"\n[{pair.Key}]";
                }
            }

            return lines;
        }

        private static string Abbreviate(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            var words = text.Split(' ');
            return words.Length == 1 ? text[..Mathf.Min(5, text.Length)] : words[0];
        }

        private float GetCellSize(int gridSize, float spacing)
        {
            var availableWidth = 1032f;
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
                availableWidth = canvas.GetComponent<RectTransform>().rect.width - 48f;

            var cellSize = (availableWidth - (gridSize - 1) * spacing) / gridSize;
            return Mathf.Floor(cellSize);
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go;
        }

        private Text CreateText(Transform parent, string content, int fontSize, FontStyle style, TextAnchor anchor)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.text = content;
            text.font = _font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            go.GetComponent<LayoutElement>().minHeight = fontSize + 16;
            return text;
        }

        private GameObject CreateCellButton(Transform parent, string label, Color color, float size, int gridSize, Action onClick)
        {
            var fontSize = gridSize >= 8 ? 16 : gridSize >= 6 ? 20 : 28;
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            go.GetComponent<LayoutElement>().preferredHeight = size;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            StretchFull(textGo.GetComponent<RectTransform>());
            var text = textGo.GetComponent<Text>();
            text.text = label;
            text.font = _font;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return go;
        }

        private GameObject CreateButton(Transform parent, string label, Color color, float height, Action onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            go.GetComponent<LayoutElement>().preferredHeight = height;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            StretchFull(textGo.GetComponent<RectTransform>());
            var text = textGo.GetComponent<Text>();
            text.text = label;
            text.font = _font;
            text.fontSize = Mathf.Clamp((int)(height * 0.38f), 18, 32);
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return go;
        }

        private static void StretchFull(GameObject go) => StretchFull(go.GetComponent<RectTransform>());

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

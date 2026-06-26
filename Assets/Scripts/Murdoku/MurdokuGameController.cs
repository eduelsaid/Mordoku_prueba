using UnityEngine;

namespace Murdoku
{
    public class MurdokuGameController : MonoBehaviour
    {
        [Header("Generación")]
        [SerializeField] private GameMode gameMode = GameMode.Facil;
        [SerializeField] private int randomSeed = -1;

        [Header("Visuales")]
        [SerializeField] private MurdokuVisuals visuals;

        private MurdokuPuzzle _puzzle;
        private MurdokuSuspect _selectedSuspect;
        private string _accusedMurderer;
        private bool _showSolution;
        private MurdokuRuntimeUI _ui;

        private int GridSize => (int)gameMode;

        private void Awake()
        {
            SetupCamera();
            _ui = gameObject.AddComponent<MurdokuRuntimeUI>();
            _ui.SetVisuals(visuals);
            _ui.Build();
            WireUI();
            GenerateNewPuzzle();
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
                return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f);
        }

        private void WireUI()
        {
            _ui.OnNewCrimeClicked += () => _ui.ShowDifficultyPicker(true);
            _ui.OnDifficultyChosen += mode =>
            {
                gameMode = mode;
                GenerateNewPuzzle();
            };
            _ui.OnDifficultyCancelled += () => _ui.ShowDifficultyPicker(false);
            _ui.OnCellClicked += OnCellClicked;
            _ui.OnSuspectSelected += suspect =>
            {
                _selectedSuspect = suspect;
                SetStatus($"Seleccionado: {suspect.Name}");
                RefreshUI();
            };
            _ui.OnMurdererToggled += (suspect, accused) =>
            {
                _accusedMurderer = accused ? suspect.Name : null;
                RefreshUI();
            };
            _ui.OnRemoveSelected += () =>
            {
                if (_selectedSuspect == null)
                    return;
                _selectedSuspect.PlacedPosition = null;
                SetStatus($"{_selectedSuspect.Name} retirado del tablero.");
                RefreshUI();
            };
            _ui.OnSolutionToggled += show =>
            {
                _showSolution = show;
                RefreshUI();
            };
            _ui.OnCheckCase += () =>
            {
                MurdokuValidator.IsSolved(_puzzle, _accusedMurderer, out var msg);
                SetStatus(msg);
            };
        }

        private void GenerateNewPuzzle()
        {
            try
            {
                var seed = randomSeed >= 0 ? randomSeed : (int?)null;
                _puzzle = new MurdokuGenerator(seed).Generate(GridSize);
                _selectedSuspect = null;
                _accusedMurderer = null;
                _showSolution = false;
                _ui.ShowDifficultyPicker(false);
                _ui.SetSolutionToggle(false);
                _ui.SetModeLabel($"Modo: {GetModeLabel(gameMode)}");
                SetStatus(MurdokuValidator.BuildHint(_puzzle));
                _ui.ScrollToTop();
                RefreshUI();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error generando puzzle: {ex.Message}");
                SetStatus($"Error al generar el caso: {ex.Message}");
            }
        }

        private void RefreshUI()
        {
            if (_puzzle == null || _ui == null)
                return;

            _ui.RefreshBoard(_puzzle, _selectedSuspect, _showSolution);
            _ui.RefreshSuspects(_puzzle, _selectedSuspect, _accusedMurderer);
        }

        private void SetStatus(string message) => _ui?.SetStatus(message);

        private void OnCellClicked(GridPosition pos)
        {
            var existing = _puzzle.GetSuspectAt(pos);
            if (existing != null)
            {
                existing.PlacedPosition = null;
                _selectedSuspect = existing;
                SetStatus($"{existing.Name} retirado. Elige otra casilla.");
                RefreshUI();
                return;
            }

            if (_selectedSuspect == null)
            {
                SetStatus("Selecciona un sospechoso de la lista.");
                return;
            }

            if (!MurdokuValidator.TryValidatePlacement(_puzzle, _selectedSuspect, pos, out var error))
            {
                SetStatus(error);
                return;
            }

            _selectedSuspect.PlacedPosition = pos;
            SetStatus($"{_selectedSuspect.Name} → fila {pos.Row + 1}, col {pos.Col + 1}.");

            if (MurdokuValidator.IsComplete(_puzzle))
                SetStatus("Todos colocados — acusa al asesino.");

            RefreshUI();
        }

        private static string GetModeLabel(GameMode mode) => mode switch
        {
            GameMode.Medio => "Medio (6×6)",
            GameMode.Dificil => "Difícil (8×8)",
            _ => "Fácil (4×4)"
        };
    }
}

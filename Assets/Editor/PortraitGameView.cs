#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Murdoku.EditorTools
{
    public static class PortraitGameView
    {
        [MenuItem("Murdoku/Configurar Game View Portrait")]
        public static void ShowInstructions()
        {
            EditorUtility.DisplayDialog(
                "Vista Portrait en Unity",
                "Para ver el juego en vertical:\n\n" +
                "1. Abre la pestaña Game (junto a Scene)\n" +
                "2. Arriba a la izquierda del Game, haz clic en el desplegable de resolución\n" +
                "   (puede decir 'Free Aspect' o '16:9')\n" +
                "3. Elige '+' para añadir resolución personalizada\n" +
                "4. Pon Ancho: 1080  Alto: 1920  y guarda como 'Portrait'\n" +
                "5. Selecciona esa resolución y pulsa Play\n\n" +
                "El juego usa Canvas y debería verse completo con scroll.",
                "Entendido");
        }
    }
}
#endif

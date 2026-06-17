using System.Collections.Generic;

namespace Murdoku
{
    /// <summary>
    /// Define qué muebles pueden aparecer en cada habitación al generar el nivel.
    /// </summary>
    public static class RoomCatalog
    {
        public static readonly Dictionary<RoomType, string> RoomNames = new()
        {
            { RoomType.Salon, "salón" },
            { RoomType.Bano, "baño" },
            { RoomType.Dormitorio, "dormitorio" },
            { RoomType.Cocina, "cocina" },
            { RoomType.Comedor, "comedor" },
            { RoomType.CuartoInvitados, "cuarto de invitados" }
        };

        public static readonly Dictionary<RoomType, FurnitureType[]> AllowedFurniture = new()
        {
            {
                RoomType.Salon,
                new[] { FurnitureType.Suelo, FurnitureType.Silla, FurnitureType.TV, FurnitureType.Planta, FurnitureType.Ventana, FurnitureType.Alfombra }
            },
            {
                RoomType.Bano,
                new[] { FurnitureType.Suelo, FurnitureType.Inodoro, FurnitureType.Lavabo, FurnitureType.Banera }
            },
            {
                RoomType.Dormitorio,
                new[] { FurnitureType.Suelo, FurnitureType.Cama, FurnitureType.Alfombra, FurnitureType.Ventana, FurnitureType.Armario, FurnitureType.Planta }
            },
            {
                RoomType.Cocina,
                new[] { FurnitureType.Suelo, FurnitureType.Mesa, FurnitureType.Estufa, FurnitureType.Nevera, FurnitureType.Lavabo }
            },
            {
                RoomType.Comedor,
                new[] { FurnitureType.Suelo, FurnitureType.Mesa, FurnitureType.Silla, FurnitureType.Ventana }
            },
            {
                RoomType.CuartoInvitados,
                new[] { FurnitureType.Suelo, FurnitureType.Cama, FurnitureType.Silla, FurnitureType.Planta, FurnitureType.Armario }
            }
        };

        public static readonly Dictionary<FurnitureType, string> FurnitureLabels = new()
        {
            { FurnitureType.Suelo, "" },
            { FurnitureType.Cama, "Cama" },
            { FurnitureType.Silla, "Silla" },
            { FurnitureType.TV, "TV" },
            { FurnitureType.Planta, "Planta" },
            { FurnitureType.Ventana, "Ventana" },
            { FurnitureType.Alfombra, "Alfombra" },
            { FurnitureType.Mesa, "Mesa" },
            { FurnitureType.Inodoro, "Inodoro" },
            { FurnitureType.Lavabo, "Lavabo" },
            { FurnitureType.Banera, "Bañera" },
            { FurnitureType.Estufa, "Estufa" },
            { FurnitureType.Nevera, "Nevera" },
            { FurnitureType.Armario, "Armario" }
        };

        public static readonly Dictionary<RoomType, UnityEngine.Color> RoomColors = new()
        {
            { RoomType.Salon, new UnityEngine.Color(0.55f, 0.75f, 0.95f) },
            { RoomType.Bano, new UnityEngine.Color(0.70f, 0.90f, 0.95f) },
            { RoomType.Dormitorio, new UnityEngine.Color(0.85f, 0.75f, 0.95f) },
            { RoomType.Cocina, new UnityEngine.Color(0.95f, 0.85f, 0.60f) },
            { RoomType.Comedor, new UnityEngine.Color(0.95f, 0.75f, 0.65f) },
            { RoomType.CuartoInvitados, new UnityEngine.Color(0.75f, 0.90f, 0.75f) }
        };

        public static bool IsAllowed(RoomType room, FurnitureType furniture)
        {
            if (!AllowedFurniture.TryGetValue(room, out var list))
                return false;

            foreach (var item in list)
            {
                if (item == furniture)
                    return true;
            }

            return false;
        }
    }
}

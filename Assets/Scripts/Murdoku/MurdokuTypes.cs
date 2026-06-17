using System;

namespace Murdoku
{
    public enum RoomType
    {
        Salon,
        Bano,
        Dormitorio,
        Cocina,
        Comedor,
        CuartoInvitados
    }

    public enum FurnitureType
    {
        Suelo,
        Cama,
        Silla,
        TV,
        Planta,
        Ventana,
        Alfombra,
        Mesa,
        Inodoro,
        Lavabo,
        Banera,
        Estufa,
        Nevera,
        Armario
    }

    public enum ClueType
    {
        EnHabitacion,
        SobreMueble,
        JuntoAMueble,
        DelanteDeVentana,
        SentadoEnSilla,
        EnEsquina,
        NoJuntoAPared,
        MismaFilaQue,
        VictimaUltimaCasilla
    }

    [Serializable]
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int Row;
        public int Col;

        public GridPosition(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool Equals(GridPosition other) => Row == other.Row && Col == other.Col;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => Row * 31 + Col;
    }
}

public class CasillaEspecial : Casilla
{
    public CasillaEspecial(int id, string nombre)
        : base(id, nombre)
    {
    }

    // Comportamiento por defecto: la casilla no hace nada (inicio, carcel, parqueo)
    public override void Ejecutar(Jugador jugador, Juego juego)
    {
        juego.AgregarEvento(jugador.Nombre + " descansa en " + Nombre);
    }
}

public class CasillaEvento : Casilla
{
    public CasillaEvento(int id, string nombre)
        : base(id, nombre)
    {
    }

    // La carta la saca el Juego del mazo (cola circular) y se aplica polimorficamente
    public override void Ejecutar(Jugador jugador, Juego juego)
    {
        juego.EjecutarCartaEvento(jugador);
    }
}

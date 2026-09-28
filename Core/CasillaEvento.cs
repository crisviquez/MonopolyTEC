public class CasillaEvento : Casilla
{
    private CartaEvento? carta;

    public CasillaEvento(int id, string nombre)
        : base(id, nombre)
    {
        carta = null;
    }

    public void AsignarCarta(CartaEvento nuevaCarta)
    {
        carta = nuevaCarta;
    }

    public CartaEvento? ObtenerCarta()
    {
        return carta;
    }

    // La carta la saca el Juego del mazo (cola circular) y la aplica el servidor
    public override void Ejecutar(Jugador jugador, Juego juego)
    {
        juego.EjecutarCartaEvento(jugador);
    }
}
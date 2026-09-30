public class CasillaIrALaCarcel : CasillaEspecial
{
    public CasillaIrALaCarcel(int id, string nombre)
        : base(id, nombre)
    {
    }

    public override void Ejecutar(Jugador jugador, Juego juego)
    {
        juego.EnviarALaCarcel(jugador);
    }
}
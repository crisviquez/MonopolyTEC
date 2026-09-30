public class CasillaImpuesto : CasillaEspecial
{
    public CasillaImpuesto(int id, string nombre)
        : base(id, nombre)
    {
    }

    public override void Ejecutar(Jugador jugador, Juego juego)
    {
        juego.CobrarImpuesto(jugador, this);
    }
}

public class CasillaEspecial : Casilla
{
    public string Tipo { get; set; }

    public CasillaEspecial(int id, string nombre, string tipo)
        : base(id, nombre)
    {
        Tipo = tipo;
    }

    public override void Ejecutar(Jugador jugador, Juego juego)
    {
        if (Tipo == "IMPUESTO")
        {
            juego.CobrarImpuesto(jugador, this);
        }
        else
        {
            juego.AgregarEvento(jugador.Nombre + " descansa en " + Nombre);
        }
    }
}
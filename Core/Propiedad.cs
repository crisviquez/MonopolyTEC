public class Propiedad : Casilla
{
    public int Precio { get; set; }
    public int Alquiler { get; set; }
    public string Color { get; set; }
    public Jugador? Propietario { get; set; }

    public Propiedad(int id, string nombre, string color, int precio, int alquiler)
        : base(id, nombre)
    {
        Precio = precio;
        Alquiler = alquiler;
        Color = color;
        Propietario = null;
    }

    public virtual int CalcularAlquiler()
    {
        return Alquiler;
    }

    public override void Ejecutar(Jugador jugador, Juego juego)
    {
        if (Propietario == null)
        {
            juego.OfrecerPropiedad(jugador, this);
        }
        else if (Propietario.Id != jugador.Id)
        {
            juego.CobrarAlquiler(jugador, this);
        }
        else
        {
            juego.AgregarEvento(jugador.Nombre + " cayo en su propia propiedad " + Nombre);
        }
    }
}
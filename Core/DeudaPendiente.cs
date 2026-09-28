// Pago obligatorio que el jugador todavia no ha hecho (alquiler, impuesto, carta)
public class DeudaPendiente
{
    public int Monto { get; set; }
    public Jugador? Acreedor { get; set; }   // null significa que se le paga al banco
    public TipoTransaccion Tipo { get; set; }
    public string Descripcion { get; set; }

    public DeudaPendiente(int monto, Jugador? acreedor, TipoTransaccion tipo, string descripcion)
    {
        Monto = monto;
        Acreedor = acreedor;
        Tipo = tipo;
        Descripcion = descripcion;
    }
}
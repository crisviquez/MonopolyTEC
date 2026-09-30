public class Asociacion : Propiedad
{
    public Asociacion(int id, string nombre)
        : base(id, nombre, "Asociacion", 200, 25)
    {
    }

    // El alquiler se duplica por cada asociacion que tenga el mismo dueno: 25, 50, 100, 200
    public override int CalcularAlquiler()
    {
        if (Propietario == null)
        {
            return Alquiler;
        }

        int cantidad = 0;
        foreach (Propiedad p in Propietario.Propiedades)
        {
            if (p.Color == "Asociacion")
            {
                cantidad = cantidad + 1;
            }
        }

        int alquiler = Alquiler;
        for (int i = 1; i < cantidad; i++)
        {
            alquiler = alquiler * 2;
        }
        return alquiler;
    }
}
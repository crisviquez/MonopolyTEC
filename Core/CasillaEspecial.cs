public class CasillaEspecial : Casilla
{
    public string Tipo { get; set; }

    public CasillaEspecial(int id, string nombre, string tipo)
        : base(id, nombre)
    {
        Tipo = tipo;
    }

    public override void Ejecutar()
    {
        Console.WriteLine("Casilla especial: " + Nombre);
        Console.WriteLine("Tipo: " + Tipo);
    }
}

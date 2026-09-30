using System;
using Monopoly.Estructuras;
using Monopoly.Nodos;

public class Tablero
{
    private ListaCircularDoble<Casilla> casillas;

    public int Cantidad
    {
        get { return casillas.Cantidad; }
    }

    public Tablero(int cantidad)
    {
        casillas = new ListaCircularDoble<Casilla>();
        for (int i = 0; i < cantidad; i++)
        {
            casillas.Agregar(CrearCasilla(i));
        }
    }

    // Tablero generico. El equipo de casillas puede cambiar nombres y precios aqui
    private Casilla CrearCasilla(int id)
    {
        if (id == 0)
        {
            return new CasillaEspecial(id, "Inicio");
        }
        if (id == 6)
        {
            return new CasillaEspecial(id, "Carcel (solo visita)");
        }
        if (id == 12)
        {
            return new CasillaEspecial(id, "Parqueo libre");
        }
        if (id == 18)
        {
            return new CasillaImpuesto(id, "Impuesto");
        }
        if (id % 6 == 3)
        {
            return new CasillaEvento(id, "Sorpresa");
        }

        int precio = 100 + (id * 10);
        int alquiler = precio / 10;
        return new Propiedad(id, "Propiedad " + id, precio, alquiler);
    }

    private NodoDoble<Casilla> ObtenerNodo(int posicion)
    {
        NodoDoble<Casilla>? inicio = casillas.Cabeza;
        if (inicio == null)
        {
            throw new InvalidOperationException("El tablero esta vacio.");
        }
        return casillas.Mover(inicio, posicion);
    }

    public Casilla ObtenerCasilla(int posicion)
    {
        return ObtenerNodo(posicion).Dato;
    }

    // Recorre los nodos de la lista circular y devuelve la posicion (Id) donde termina
    public int Mover(int posicionActual, int pasos)
    {
        NodoDoble<Casilla> actual = ObtenerNodo(posicionActual);
        NodoDoble<Casilla> destino = casillas.Mover(actual, pasos);
        return destino.Dato.Id;
    }
}

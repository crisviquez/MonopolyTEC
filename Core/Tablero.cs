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

        private Propiedad CrearPropiedad(int id, string nombre, string color, int precio)
    {
        return new Propiedad(id, nombre, color, precio, precio / 10);
    }

    private Casilla CrearCasilla(int id)
    {
        if (id == 0) { return new CasillaEspecial(id, "Salida"); }
        if (id == 10) { return new CasillaEspecial(id, "Carcel (visita)"); }
        if (id == 20) { return new CasillaEspecial(id, "Zona segura"); }
        if (id == 30) { return new CasillaIrALaCarcel(id, "Ve a la carcel"); }

        if (id == 4) { return new CasillaImpuesto(id, "Impuesto"); }
        if (id == 38) { return new CasillaImpuesto(id, "Impuesto de lujo"); }

        if (id == 2 || id == 7 || id == 12 || id == 17 || id == 22 || id == 28 || id == 33 || id == 36)
        {
            return new CasillaEvento(id, "Sorpresa");
        }

        // Asociaciones (ferrocarriles)
        if (id == 5) { return new Asociacion(id, "FIFA"); }
        if (id == 15) { return new Asociacion(id, "NFL"); }
        if (id == 25) { return new Asociacion(id, "NBA"); }
        if (id == 35) { return new Asociacion(id, "MLB"); }

        // Cafe
        if (id == 1) { return CrearPropiedad(id, "Heredia", "Cafe", 60); }
        if (id == 3) { return CrearPropiedad(id, "Saprissa", "Cafe", 60); }

        // Celeste
        if (id == 6) { return CrearPropiedad(id, "Flamengo", "Celeste", 100); }
        if (id == 8) { return CrearPropiedad(id, "Boca Jr", "Celeste", 100); }
        if (id == 9) { return CrearPropiedad(id, "Spurs", "Celeste", 120); }

        // Rosa
        if (id == 11) { return CrearPropiedad(id, "Chelsea", "Rosa", 140); }
        if (id == 13) { return CrearPropiedad(id, "LA Dodgers", "Rosa", 140); }
        if (id == 14) { return CrearPropiedad(id, "New England Patriots", "Rosa", 160); }

        // Naranja
        if (id == 16) { return CrearPropiedad(id, "Inter Milan", "Naranja", 180); }
        if (id == 18) { return CrearPropiedad(id, "AC Milan", "Naranja", 180); }
        if (id == 19) { return CrearPropiedad(id, "Golden State Warriors", "Naranja", 200); }

        // Rojo
        if (id == 21) { return CrearPropiedad(id, "Juventus", "Rojo", 220); }
        if (id == 23) { return CrearPropiedad(id, "Chicago Bulls", "Rojo", 220); }
        if (id == 24) { return CrearPropiedad(id, "New York Yankees", "Rojo", 240); }

        // Amarillo
        if (id == 26) { return CrearPropiedad(id, "Liverpool", "Amarillo", 260); }
        if (id == 27) { return CrearPropiedad(id, "Dallas Cowboys", "Amarillo", 260); }
        if (id == 29) { return CrearPropiedad(id, "Lakers LA", "Amarillo", 280); }

        // Verde
        if (id == 31) { return CrearPropiedad(id, "Celtics", "Verde", 300); }
        if (id == 32) { return CrearPropiedad(id, "Manchester United", "Verde", 300); }
        if (id == 34) { return CrearPropiedad(id, "Bayern Munich", "Verde", 320); }

        // Azul
        if (id == 37) { return CrearPropiedad(id, "Barcelona", "Azul", 350); }
        if (id == 39) { return CrearPropiedad(id, "Real Madrid", "Azul", 400); }

        // Solo si alguien pide mas de 40 casillas
        return CrearPropiedad(id, "Propiedad " + id, "Sin color", 100 + (id * 10));
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

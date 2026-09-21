using System;
using Monopoly.Nodos;

namespace Monopoly.Estructuras
{
    public class ListaCircularDoble<T>
    {
        public NodoDoble<T>? Cabeza { get; private set; }
        public int Cantidad { get; private set; }

        public void Agregar(T dato)
        {
            NodoDoble<T> nuevo = new NodoDoble<T>(dato);
            if (Cabeza == null)
            {
                Cabeza = nuevo;
                Cabeza.Siguiente = Cabeza;
                Cabeza.Anterior = Cabeza;
            }
            else
            {
                NodoDoble<T> cola = Cabeza.Anterior!;
                cola.Siguiente = nuevo;
                nuevo.Anterior = cola;
                nuevo.Siguiente = Cabeza;
                Cabeza.Anterior = nuevo;
            }
            Cantidad++;
        }

        // Permite desplazar un jugador N casillas adelante (pasos > 0) o atrás (pasos < 0)
        public NodoDoble<T> Mover(NodoDoble<T> nodoInicio, int pasos)
        {
            if (nodoInicio == null) throw new ArgumentNullException(nameof(nodoInicio));

            NodoDoble<T> actual = nodoInicio;
            if (pasos > 0)
            {
                for (int i = 0; i < pasos; i++)
                    actual = actual.Siguiente!;
            }
            else if (pasos < 0)
            {
                for (int i = 0; i < Math.Abs(pasos); i++)
                    actual = actual.Anterior!;
            }
            return actual;
        }
    }
}
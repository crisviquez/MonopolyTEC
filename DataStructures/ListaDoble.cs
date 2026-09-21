using System;
using System.Collections;
using System.Collections.Generic;
using Monopoly.Nodos;

namespace Monopoly.Estructuras
{
    public class ListaDoble<T> : IEnumerable<T>
    {
        public NodoDoble<T>? Cabeza { get; private set; }
        public NodoDoble<T>? Cola { get; private set; }
        public int Cantidad { get; private set; }

        public void Agregar(T dato)
        {
            NodoDoble<T> nuevo = new NodoDoble<T>(dato);
            if (Cabeza == null)
            {
                Cabeza = nuevo;
                Cola = nuevo;
            }
            else
            {
                Cola!.Siguiente = nuevo;
                nuevo.Anterior = Cola;
                Cola = nuevo;
            }
            Cantidad++;
        }

        // Permite recorrer desde la transacción más reciente hacia la más antigua
        public IEnumerable<T> ObtenerEnOrdenInverso()
        {
            NodoDoble<T>? actual = Cola;
            while (actual != null)
            {
                yield return actual.Dato;
                actual = actual.Anterior;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            NodoDoble<T>? actual = Cabeza;
            while (actual != null)
            {
                yield return actual.Dato;
                actual = actual.Siguiente;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
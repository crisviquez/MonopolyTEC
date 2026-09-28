public class Dado
{
    private Random random;

    public Dado()
    {
        random = new Random();
    }

    public int Lanzar()
    {
        return random.Next(1, 7);
    }
}
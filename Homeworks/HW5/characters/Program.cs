interface IDamagable
{
    void TakeDamage(int damage);
}
abstract class Character : IDamagable
{
    private string name;
    private double health;
    
    public string Name{get; set;}
    public double Health{get; set;}

    public abstract void Attack();
    public void Move()
    {
        Console.WriteLine($"Персонаж {name} перемещается");
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Console.WriteLine($"Персонаж получает {damage} урона. Осталось {health} здоровья.");
    }
}
// Класс Character должен быть абстрактным, потому что он реализует общую концепцию персонажей и не реализует метод Attack(), так как у каждого персонажа своя разная реализация этого метода.


class Warrior : Character
{
    public override void Attack()
    {
        Console.WriteLine("Введите количество урона воина: ");
        Console.WriteLine($"Воин выполняет атаку и наносит {Console.ReadLine()} урона.");
    }
}

class Mage : Character
{
    public override void Attack()
    {
        Console.WriteLine("Введите количество урона мага: ");
        Console.WriteLine($"Маг выполняет атаку и наносит {Console.ReadLine()} урона.");
    }
}

class Healer : Character
{
    public override void Attack()
    {
        Console.WriteLine("Введите количество восстанавливаемого целителем здоровья : ");
        Console.WriteLine($"Целитель выполняет атаку и восстанавливает {Console.ReadLine()} здоровья команде.");
    }
}

class Program
{
    static void Main()
    {
       Character[] characters = {new Warrior(), new Mage(), new Healer()}; 
       for(int i = 0; i < characters.Length; i++)
        {
            characters[i].Attack();
        }
    }
}
// При одинаковом типе переменной вызываются разные методы благодаря полиморфизму. Метод Attack() в родительском классе абстрактный и не может быть реализован, а в классах наследниках используется ключевое слово override, которое переопределяет этот метод для каждого класса по отдельности.
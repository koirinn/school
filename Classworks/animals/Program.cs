class Animal
{
    protected string name;
    protected int age;
    
    public Animal(string name, int age)
    {
        this.name = name;
        this.age = age;
    }

    public Animal()
    {
        this.name = "имя";
        this.age = 5;
    }
    public string Name
    {
        get{return name;}
        set{this.name = value;}
    }
    public int Age
    {
        get{return age;}
        set{this.age = value;}
    }
    public void Sound()
    {
        Console.WriteLine($"{this.name} издает звук");
        return;
    }
}

class Dog : Animal
{
    public Dog(string name, int age)
    {
        this.name = name;
        this.age = age;
    }

    public Dog()
    {
        this.name = "имя";
        this.age = 5;
    }
    public void Gav()
    {
        Console.WriteLine($"Собака с именем {this.name} гавкнула");
    }
}

class Cat : Animal
{
    public Cat(string name, int age)
    {
        this.name = name;
        this.age = age;
    }

    public Cat()
    {
        this.name = "имя";
        this.age = 5;
    }
    public void Meow()
    {
        Console.WriteLine($"Кошка с именем {this.name} мяукнула");
    }
}


class Programm
{
    static void Main()
    {
        Animal animal1 = new Animal("Чупакабра", 4);
        animal1.Sound();
        
        Dog dog1 = new Dog("Румильда", 5);
        dog1.Sound();
        dog1.Gav();

        Cat cat1 = new Cat("Потато", 2);
        cat1.Sound();
        cat1.Meow();
    }
}
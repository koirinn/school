class Student
{
    protected string name;
    protected int age;
    protected string group;
    
    public Student(string name, int age, string group)
    {
        this.name = name;
        this.age = age;
        this.group = group;
    }

    public Student()
    {
        this.name = "имя";
        this.age = 5;
        this.group = "1A";
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
    public string Group
    {
        get{return group;}
        set{this.group = value;}
    }
    public void Study()
    {
        Console.WriteLine($"Студент по имени {this.name}, которому {this.age} лет, учится в группе {this.group}");
        return;
    }
}

class Magistr : Student
{
    public Magistr(string name, int age, string group)
    {
        this.name = name;
        this.age = age;
        this.group = group;
    }

    public Magistr()
    {
        this.name = "имя";
        this.age = 5;
        this.group = "1A";
    }
    public void DefendDiploma()
    {
        Console.WriteLine($"Студент по имени {this.name}, которому {this.age} лет и который учится в группе {this.group}, защищает диплом");
    }
}

class Bacalavr : Student
{
    public Bacalavr(string name, int age, string group)
    {
        this.name = name;
        this.age = age;
        this.group = group;
    }

    public Bacalavr()
    {
        this.name = "имя";
        this.age = 5;
        this.group = "1A";
    }
    public void HaveExams()
    {
        Console.WriteLine($"Студент по имени {this.name}, которому {this.age} лет и который учится в группе {this.group}, сдает экзамены");
    }
}
class Programm
{
    static void Main()
    {
        Student student1 = new Student("Студент1", 22, "2B");
        student1.Study();
        
        Magistr magistr1 = new Magistr("Магистр1", 23, "3C");
        magistr1.Study();
        magistr1.DefendDiploma();

        Bacalavr bacalavr1 = new Bacalavr("Бакалавр1", 21, "2A");
        bacalavr1.Study();
        bacalavr1.HaveExams();
    }
}


using System;
using System.Collections.Generic;

// Интерфейсы
public interface ICommand
{
    void Execute(Game game, string[] args);
}

public interface IInteractable
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    void Interact(GameState state);
}

public interface ICondition
{
    bool IsTrue(GameState state);
}

public interface IEffect
{
    void Apply(GameState state);
}


// Абстрактные классы
public abstract class CommandBase : ICommand
{
    public string Name { get; protected set; }
    protected CommandBase(string name) => Name = name;
    public abstract void Execute(Game game, string[] args);
}

public abstract class InteractableBase : IInteractable
{
    public string Id { get; protected set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    protected InteractableBase(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }
    public abstract void Interact(GameState state);
}

public abstract class ConditionBase : ICondition
{
    public abstract bool IsTrue(GameState state);
}

public abstract class EffectBase : IEffect
{
    public abstract void Apply(GameState state);
}

public abstract class GameEventBase
{
    public ICondition Condition { get; protected set; }
    public List<IEffect> Effects { get; protected set; }
    public bool IsOneTime { get; protected set; }
    private bool hasFired; // проверка, сработало ли действие (для isOneTime)

    protected GameEventBase(ICondition condition, List<IEffect> effects, bool isOneTime)
    {
        Condition = condition;
        Effects = effects ?? new List<IEffect>();
        IsOneTime = isOneTime;
    }

    public void CheckAndApply(GameState state)
    {
        if (IsOneTime && hasFired) return;
        if (Condition == null || Condition.IsTrue(state))
        {
            foreach (var e in Effects) e.Apply(state);
            if (IsOneTime) hasFired = true;
        }
    }
}

// Квест
public class Quest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public enum QuestState { NotStarted, InProgress, Completed }
    public QuestState State { get; private set; }
    public ICondition CompletionCondition { get; set; }

    public Quest(string id, string name, string description, ICondition completionCondition)
    {
        Id = id; Name = name; Description = description;
        CompletionCondition = completionCondition;
        State = QuestState.NotStarted;
    }

    public void Start() { if (State == QuestState.NotStarted) State = QuestState.InProgress; }
    public void Update(GameState state)
    {
        if (State == QuestState.InProgress && CompletionCondition.IsTrue(state))
            State = QuestState.Completed;
    }
}

// Игровое состояние
public class GameState
{
    public int Health { get; private set; } = 100;
    public List<string> Inventory { get; private set; } = new List<string>();
    public Dictionary<string, bool> Flags { get; private set; } = new Dictionary<string, bool>();
    public int TurnCount { get; private set; }
    public List<string> EventLog { get; private set; } = new List<string>();

    public void AddItem(string id) { if (!Inventory.Contains(id)) Inventory.Add(id); }
    public void RemoveItem(string id) { if (Inventory.Contains(id)) Inventory.Remove(id); }
    public bool HasItem(string id) => Inventory.Contains(id);
    public void SetFlag(string name, bool val) { if (Flags.ContainsKey(name)) Flags[name] = val; else Flags.Add(name, val); }
    public bool GetFlag(string name) => Flags.ContainsKey(name) && Flags[name];
    public void Damage(int amount) { Health -= amount; if (Health < 0) Health = 0; }
    public void Heal(int amount) { Health += amount; }
    public void AddLog(string msg) => EventLog.Add($"[{TurnCount}] {msg}"); 
    public void IncrementTurn() => TurnCount++;
}

// Локация
public class Location
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<IInteractable> Interactables { get; set; } = new List<IInteractable>();
    public List<GameEventBase> Events { get; set; } = new List<GameEventBase>();
    public Dictionary<string, string> Exits { get; set; } = new Dictionary<string, string>();

    public void AddExit(string dir, string locId) => Exits[dir] = locId;
    public string GetExit(string dir) => Exits.ContainsKey(dir) ? Exits[dir] : null;
}

// Главный класс игры
public class Game
{
    public GameState State { get; private set; } = new GameState();
    public Dictionary<string, Location> Locations { get; private set; } = new Dictionary<string, Location>();
    public string CurrentLocationId { get; set; }
    public List<Quest> Quests { get; private set; } = new List<Quest>();
    public bool IsRunning { get; set; } = true;

    private Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

    public Game()
    {
        commands = new Dictionary<string, ICommand>
            {
                ["look"] = new LookCommand(),
                ["go"] = new GoCommand(),
                ["interact"] = new InteractCommand(),
                ["inv"] = new InventoryCommand(),
                ["help"] = new HelpCommand(),
                ["status"] = new StatusCommand()
            };
        
    }

    public void Run()
    {
        while (IsRunning)
        {
            Console.Write("> ");
            string input = Console.ReadLine();
            ProcessCommand(input);
            State.IncrementTurn();
            CheckEvents();
            UpdateQuests();
        }
    }

    public void ProcessCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;
        string[] parts = input.Split(' ');
        string cmd = parts[0].ToLower();
        if (commands.ContainsKey(cmd)) commands[cmd].Execute(this, parts);
        else Console.WriteLine("Неизвестная команда");
    }

    public void CheckEvents()
    {
        Location loc = Locations[CurrentLocationId];
        foreach (var ev in loc.Events) ev.CheckAndApply(State);
    }

    public void UpdateQuests()
    {
        foreach (var q in Quests) q.Update(State);
    }

    public Location GetCurrentLocation() => Locations[CurrentLocationId];
}
using System;
using System.Collections.Generic;


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

public enum EventType { OnEnter, OnTurn, OneTime }

public abstract class GameEventBase
{
    public ICondition Condition { get; protected set; }
    public List<IEffect> Effects { get; protected set; }
    public bool IsOneTime { get; protected set; }
    public EventType Type { get; protected set; }
    private bool hasFired;

    protected GameEventBase(EventType type, ICondition condition, List<IEffect> effects, bool isOneTime)
    {
        Type = type;
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




public class Quest
{
    public enum QuestState { NotStarted, InProgress, Completed }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public QuestState State { get; private set; }
    public ICondition CompletionCondition { get; set; }

    public Quest(string id, string name, string description, ICondition completionCondition)
    {
        Id = id; Name = name; Description = description;
        CompletionCondition = completionCondition;
        State = QuestState.NotStarted;
    }

    public void Start() { if (State == QuestState.NotStarted) State = QuestState.InProgress; }
    public void Complete() { if (State == QuestState.InProgress) State = QuestState.Completed; }
    public void Update(GameState state)
    {
        if (State == QuestState.InProgress && CompletionCondition.IsTrue(state))
            State = QuestState.Completed;
    }
}




public class GameState
{
    public int Health { get; private set; } = 100;
    public List<string> Inventory { get; private set; } = new List<string>();
    public Dictionary<string, bool> Flags { get; private set; } = new Dictionary<string, bool>();
    public Dictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();
    public int TurnCount { get; private set; }
    public List<string> EventLog { get; private set; } = new List<string>();

    public void AddItem(string id) { if (!Inventory.Contains(id)) Inventory.Add(id); }
    public void RemoveItem(string id) { if (Inventory.Contains(id)) Inventory.Remove(id); }
    public bool HasItem(string id) => Inventory.Contains(id);
    public void SetFlag(string name, bool val) { if (Flags.ContainsKey(name)) Flags[name] = val; else Flags.Add(name, val); }
    public bool GetFlag(string name) => Flags.ContainsKey(name) && Flags[name];
    public void SetData(string key, string value) { if (Data.ContainsKey(key)) Data[key] = value; else Data.Add(key, value); }
    public string GetData(string key) => Data.ContainsKey(key) ? Data[key] : null;
    public void ClearData(string key) { if (Data.ContainsKey(key)) Data.Remove(key); }
    public void Damage(int amount) { Health -= amount; if (Health < 0) Health = 0; }
    public void Heal(int amount) { Health += amount; }
    public void AddLog(string msg) => EventLog.Add($"[{TurnCount}] {msg}");
    public void IncrementTurn() => TurnCount++;
}




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




public class LookCommand : CommandBase
{
    public LookCommand() : base("look") { }
    public override void Execute(Game game, string[] args)
    {
        var loc = game.GetCurrentLocation();
        Console.WriteLine($"\n{loc.Name}");
        Console.WriteLine(loc.Description);
        Console.WriteLine("\nОбъекты:");
        foreach (var obj in loc.Interactables)
            Console.WriteLine($"  - {obj.Name} (interact {obj.Id})");
        Console.WriteLine("\nВыходы:");
        foreach (var exit in loc.Exits)
            Console.WriteLine($"  - {exit.Key}");
    }
}

public class GoCommand : CommandBase
{
    public GoCommand() : base("go") { }
    public override void Execute(Game game, string[] args)
    {
        if (args.Length < 2) { Console.WriteLine("Куда идти?"); return; }
        string dir = args[1].ToLower();
        string target = game.GetCurrentLocation().GetExit(dir);
        if (target == null) Console.WriteLine("Туда нельзя пойти.");
        else
        {
            game.CurrentLocationId = target;
            Console.WriteLine($"Вы перешли в {game.GetCurrentLocation().Name}");
            game.CheckEvents(EventType.OnEnter);
        }
    }
}

public class InteractCommand : CommandBase
{
    public InteractCommand() : base("interact") { }
    public override void Execute(Game game, string[] args)
    {
        if (args.Length < 2) { Console.WriteLine("С чем взаимодействовать?"); return; }
        string id = args[1];
        var obj = game.GetCurrentLocation().Interactables.Find(o => o.Id == id);
        if (obj == null) Console.WriteLine("Здесь нет такого объекта.");
        else obj.Interact(game.State);
    }
}

public class InventoryCommand : CommandBase
{
    public InventoryCommand() : base("inv") { }
    public override void Execute(Game game, string[] args)
    {
        Console.WriteLine("\nИнвентарь:");
        if (game.State.Inventory.Count == 0) Console.WriteLine("  пуст");
        else foreach (var i in game.State.Inventory) Console.WriteLine($"  - {i}");
    }
}

public class HelpCommand : CommandBase
{
    public HelpCommand() : base("help") { }
    public override void Execute(Game game, string[] args)
    {
        Console.WriteLine("\nКоманды:");
        Console.WriteLine("  look                 - осмотреться");
        Console.WriteLine("  go <направление>     - пойти (north, south, east, west)");
        Console.WriteLine("  interact <id>        - взаимодействовать с объектом");
        Console.WriteLine("  inv                  - показать инвентарь");
        Console.WriteLine("  status               - показать состояние");
        Console.WriteLine("  help                 - помощь");
    }
}

public class StatusCommand : CommandBase
{
    public StatusCommand() : base("status") { }
    public override void Execute(Game game, string[] args)
    {
        Console.WriteLine($"\nЗдоровье: {game.State.Health}");
        Console.WriteLine($"Ход: {game.State.TurnCount}");
    }
}




public class HasItemCondition : ConditionBase
{
    private string itemId;
    public HasItemCondition(string id) => itemId = id;
    public override bool IsTrue(GameState state) => state.HasItem(itemId);
}

public class FlagCondition : ConditionBase
{
    private string flag; private bool expected;
    public FlagCondition(string flag, bool expected) { this.flag = flag; this.expected = expected; }
    public override bool IsTrue(GameState state) => state.GetFlag(flag) == expected;
}

public class HealthCondition : ConditionBase
{
    private int threshold; private string comparison;
    public HealthCondition(int threshold, string comparison) { this.threshold = threshold; this.comparison = comparison; }
    public override bool IsTrue(GameState state)
    {
        return comparison switch
        {
            "less" => state.Health < threshold,
            "lessOrEqual" => state.Health <= threshold,
            "equal" => state.Health == threshold,
            "greaterOrEqual" => state.Health >= threshold,
            "greater" => state.Health > threshold,
            _ => false
        };
    }
}

public class AndCondition : ConditionBase
{
    private List<ICondition> conditions;
    public AndCondition(params ICondition[] conds) => conditions = new List<ICondition>(conds);
    public override bool IsTrue(GameState state)
    {
        foreach (var c in conditions) if (!c.IsTrue(state)) return false;
        return true;
    }
}

public class OrCondition : ConditionBase
{
    private List<ICondition> conditions;
    public OrCondition(params ICondition[] conds) => conditions = new List<ICondition>(conds);
    public override bool IsTrue(GameState state)
    {
        foreach (var c in conditions) if (c.IsTrue(state)) return true;
        return false;
    }
}

public class NotCondition : ConditionBase
{
    private ICondition condition;
    public NotCondition(ICondition cond) => condition = cond;
    public override bool IsTrue(GameState state) => !condition.IsTrue(state);
}




public class AddItemEffect : EffectBase
{
    private string itemId;
    public AddItemEffect(string id) => itemId = id;
    public override void Apply(GameState state)
    {
        state.AddItem(itemId);
        Console.WriteLine($"Вы получили предмет: {itemId}");
    }
}

public class RemoveItemEffect : EffectBase
{
    private string itemId;
    public RemoveItemEffect(string id) => itemId = id;
    public override void Apply(GameState state)
    {
        state.RemoveItem(itemId);
        Console.WriteLine($"Вы потеряли предмет: {itemId}");
    }
}

public class SetFlagEffect : EffectBase
{
    private string flag; private bool val;
    public SetFlagEffect(string flag, bool val) { this.flag = flag; this.val = val; }
    public override void Apply(GameState state) => state.SetFlag(flag, val);
}

public class DamageEffect : EffectBase
{
    private int amount;
    public DamageEffect(int amount) => this.amount = amount;
    public override void Apply(GameState state) => state.Damage(amount);
}

public class HealEffect : EffectBase
{
    private int amount;
    public HealEffect(int amount) => this.amount = amount;
    public override void Apply(GameState state) => state.Heal(amount);
}

public class LogEffect : EffectBase
{
    private string msg;
    public LogEffect(string msg) => this.msg = msg;
    public override void Apply(GameState state)
    {
        state.AddLog(msg);
        Console.WriteLine(msg);  // сразу показываем игроку
    }
}

public class ChangeLocationEffect : EffectBase
{
    private string target;
    public ChangeLocationEffect(string target) => this.target = target;
    public override void Apply(GameState state) => state.SetData("pending_location", target);
}

public class StartQuestEffect : EffectBase
{
    private string questId;
    public StartQuestEffect(string questId) => this.questId = questId;
    public override void Apply(GameState state) => state.SetData("pending_start_quest", questId);
}

public class CompleteQuestEffect : EffectBase
{
    private string questId;
    public CompleteQuestEffect(string questId) => this.questId = questId;
    public override void Apply(GameState state) => state.SetData("pending_complete_quest", questId);
}

public class AddExitEffect : EffectBase
{
    private string dir, target;
    public AddExitEffect(string dir, string target) { this.dir = dir; this.target = target; }
    public override void Apply(GameState state)
    {
        state.SetData("pending_add_exit_dir", dir);
        state.SetData("pending_add_exit_target", target);
    }
}




public class SteamMirrorEffect : EffectBase
{
    public override void Apply(GameState state)
    {
        Console.WriteLine("Зеркало запотело. На нём проступают символы: ▲ ⬢ ■");
        Console.WriteLine("Вы догадываетесь, что это цифры: 3 6 4?");
        state.SetData("mirror_code", "364");
        state.AddLog("Вы увидели код 364 на зеркале.");
    }
}

public class RevealPictureCodeEffect : EffectBase
{
    private string pictureType;
    public RevealPictureCodeEffect(string pictureType) => this.pictureType = pictureType;
    public override void Apply(GameState state)
    {
        if (pictureType == "forest")
        {
            Console.WriteLine("За рамкой картины 'Лес' вы находите цифру 7.");
            state.SetData("forest_code", "7");
        }
        else if (pictureType == "mountains")
        {
            Console.WriteLine("За рамкой картины 'Горы' вы находите цифру 2.");
            state.SetData("mountains_code", "2");
        }
        else if (pictureType == "sea")
        {
            Console.WriteLine("За рамкой картины 'Море' вы находите цифру 9.");
            state.SetData("sea_code", "9");
        }
        state.AddLog($"Вы нашли цифру за картиной '{pictureType}'.");
    }
}




public class SimpleObject : InteractableBase
{
    private List<IEffect> effects;
    public SimpleObject(string id, string name, string description, List<IEffect> effects)
        : base(id, name, description) => this.effects = effects ?? new List<IEffect>();
    public override void Interact(GameState state)
    {
        Console.WriteLine($"Вы взаимодействуете с {Name}.");
        foreach (var e in effects) e.Apply(state);
    }
}

public class Chest : InteractableBase
{
    private ICondition openCondition;
    private List<IEffect> openEffects;
    private bool isOpen = false;
    public Chest(string id, string name, string description, ICondition condition, List<IEffect> effects)
        : base(id, name, description) { openCondition = condition; openEffects = effects ?? new List<IEffect>(); }
    public override void Interact(GameState state)
    {
        if (isOpen) { Console.WriteLine("Сундук уже открыт."); return; }
        if (openCondition == null || openCondition.IsTrue(state))
        {
            isOpen = true;
            Console.WriteLine("Вы открыли сундук.");
            foreach (var e in openEffects) e.Apply(state);
        }
        else Console.WriteLine("Сундук заперт.");
    }
}

public class Door : InteractableBase
{
    private ICondition canPassCondition;
    private string targetLocationId;
    public Door(string id, string name, string description, ICondition condition, string target)
        : base(id, name, description) { canPassCondition = condition; targetLocationId = target; }
    public override void Interact(GameState state)
    {
        if (canPassCondition == null || canPassCondition.IsTrue(state))
        {
            Console.WriteLine($"Дверь открыта. Вы переходите в {targetLocationId}.");
            state.SetData("pending_location", targetLocationId);
        }
        else Console.WriteLine("Дверь заперта.");
    }
}


public class CodeDoor : InteractableBase
{
    public CodeDoor(string id, string name, string description) : base(id, name, description) { }
    public override void Interact(GameState state)
    {
        Console.WriteLine("Введите четырёхзначный код (или 0 для отмены):");
        string input = Console.ReadLine();
        if (input == "0") return;

        // Получаем собранные цифры
        string digit1 = state.GetData("digit1"); // из гостиной (шкатулка)
        string digit2 = state.GetData("digit2"); // из ванной (за зеркалом, лес)
        string digit3 = state.GetData("digit3"); // из спальни (под кроватью)
        string digit4 = state.GetData("digit4"); // из прихожей (лампочка)

        if (string.IsNullOrEmpty(digit1) || string.IsNullOrEmpty(digit2) || string.IsNullOrEmpty(digit3) || string.IsNullOrEmpty(digit4))
        {
            Console.WriteLine("Вы ещё не собрали все цифры. Ищите подсказки в комнатах.");
            return;
        }

        string correctCode = digit1 + digit2 + digit3 + digit4;
        if (input == correctCode)
        {
            Console.WriteLine("Замок щёлкнул! Дверь открыта.");
            Console.WriteLine("Вы вышли на улицу и выбрались из квартиры. Поздравляем!");
            state.SetFlag("Escaped", true);
            state.SetData("pending_location", "outside");
        }
        else
        {
            Console.WriteLine("Код неверный. Попробуйте ещё раз.");
        }
    }
}

public class Mirror : InteractableBase
{
    public Mirror(string id, string name, string description) : base(id, name, description) { }
    public override void Interact(GameState state)
    {
        if (!state.GetFlag("mirror_steamed"))
        {
            Console.WriteLine("Зеркало холодное и мутное. Нужно сделать так, чтобы оно запотело.");
            Console.WriteLine("(Попробуйте включить горячую воду в ванной)");
        }
        else
        {
            Console.WriteLine("На зеркале видны символы: ▲ ⬢ ■. Вы запоминаете код 364.");
            state.SetData("mirror_code", "364");
        }
    }
}

public class PictureSelection : InteractableBase
{
    private string pictureType;
    public PictureSelection(string id, string name, string description, string type) : base(id, name, description)
    {
        pictureType = type;
    }
    public override void Interact(GameState state)
    {
        if (pictureType == "forest")
        {
            Console.WriteLine("Вы выбрали картину 'Лес'. За ней обнаруживается цифра 7.");
            state.SetData("digit2", "7");
        }
        else if (pictureType == "mountains")
        {
            Console.WriteLine("Вы выбрали картину 'Горы'. За ней цифра 2, но это не то, что нужно.");
            state.SetData("digit2", "2"); 
        }
        else if (pictureType == "sea")
        {
            Console.WriteLine("Вы выбрали картину 'Море'. За ней цифра 9, но это не то, что нужно.");
            state.SetData("digit2", "9");
        }
        state.AddLog($"Вы нашли цифру {state.GetData("digit2")} за картиной.");
    }
}

public class BlinkingLamp : InteractableBase
{
    public BlinkingLamp(string id, string name, string description) : base(id, name, description) { }
    public override void Interact(GameState state)
    {
        Console.WriteLine("Лампочка мигает азбукой Морзе: ---..");
        Console.WriteLine("Вы понимаете, что это цифра 8.");
        state.SetData("digit4", "8");
    }
}

public class Inscription : InteractableBase
{
    public Inscription(string id, string name, string description) : base(id, name, description) { }
    public override void Interact(GameState state)
    {
        Console.WriteLine("На потолке надпись: 'гвсп'.");
        Console.WriteLine("Вы догадываетесь, что это порядок комнат: Гостиная, Ванная, Спальня, Прихожая.");
        Console.WriteLine("Цифры нужно вводить именно в таком порядке.");
        state.SetData("hint_order", "гвсп");
    }
}

public class Drawer : InteractableBase
{
    private ICondition openCondition;
    private List<IEffect> openEffects;
    private bool isOpen = false;
    public Drawer(string id, string name, string description, ICondition condition, List<IEffect> effects)
        : base(id, name, description) { openCondition = condition; openEffects = effects ?? new List<IEffect>(); }
    public override void Interact(GameState state)
    {
        if (isOpen) { Console.WriteLine("Ящик уже открыт."); return; }
        if (openCondition == null || openCondition.IsTrue(state))
        {
            isOpen = true;
            Console.WriteLine("Вы открыли выдвижной ящик.");
            foreach (var e in openEffects) e.Apply(state);
        }
        else Console.WriteLine("Ящик заперт. Нужен ключ.");
    }
}

public class HotWaterTap : InteractableBase
{
    private bool isHot = false;
    public HotWaterTap(string id, string name, string description) : base(id, name, description) { }
    public override void Interact(GameState state)
    {
        if (!isHot)
        {
            Console.WriteLine("Вы открыли кран с горячей водой. Ванная наполняется паром.");
            isHot = true;
            state.SetFlag("mirror_steamed", true);
            Console.WriteLine("Зеркало запотело. На нём проступают символы.");
            state.SetData("mirror_code", "364");
        }
        else
        {
            Console.WriteLine("Вода уже горячая.");
        }
    }
}

public class Trap : InteractableBase
{
    private ICondition triggerCondition;
    private List<IEffect> triggerEffects;
    private bool isOneTime;
    private bool hasTriggered = false;
    public Trap(string id, string name, string description, ICondition condition, List<IEffect> effects, bool oneTime)
        : base(id, name, description) { triggerCondition = condition; triggerEffects = effects ?? new List<IEffect>(); isOneTime = oneTime; }
    public override void Interact(GameState state)
    {
        if (isOneTime && hasTriggered) { Console.WriteLine("Ловушка уже сработала."); return; }
        if (triggerCondition == null || triggerCondition.IsTrue(state))
        {
            Console.WriteLine("Ловушка сработала!");
            foreach (var e in triggerEffects) e.Apply(state);
            hasTriggered = true;
        }
        else Console.WriteLine("Ничего не произошло.");
    }
}

public class NPC : InteractableBase
{
    private Dictionary<ICondition, List<IEffect>> dialogOptions;
    public NPC(string id, string name, string description, Dictionary<ICondition, List<IEffect>> options)
        : base(id, name, description) => dialogOptions = options ?? new Dictionary<ICondition, List<IEffect>>();
    public override void Interact(GameState state)
    {
        foreach (var opt in dialogOptions)
            if (opt.Key.IsTrue(state))
            {
                foreach (var e in opt.Value) e.Apply(state);
                return;
            }
        Console.WriteLine($"{Name} не реагирует.");
    }
}




public class OnEnterLocationEvent : GameEventBase
{
    public OnEnterLocationEvent(ICondition condition, List<IEffect> effects, bool isOneTime = false)
        : base(EventType.OnEnter, condition, effects, isOneTime) { }
}

public class OnTurnEvent : GameEventBase
{
    public OnTurnEvent(ICondition condition, List<IEffect> effects, bool isOneTime = false)
        : base(EventType.OnTurn, condition, effects, isOneTime) { }
}

public class OneTimeEvent : GameEventBase
{
    public OneTimeEvent(ICondition condition, List<IEffect> effects)
        : base(EventType.OneTime, condition, effects, true) { }
}




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
            ProcessPendingEffects();
            State.IncrementTurn();
            CheckEvents(EventType.OnTurn);
            CheckEvents(EventType.OneTime);
            ProcessPendingEffects();
            if (State.Health <= 0)
            {
                Console.WriteLine("Вы погибли. Игра окончена.");
                IsRunning = false;
            }
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

    public void CheckEvents(EventType type)
    {
        Location loc = Locations[CurrentLocationId];
        foreach (var ev in loc.Events)
            if (ev.Type == type || ev.Type == EventType.OneTime)
                ev.CheckAndApply(State);
    }

    public void UpdateQuests()
    {
        foreach (var q in Quests) q.Update(State);
    }

    public Location GetCurrentLocation() => Locations[CurrentLocationId];

    private void ProcessPendingEffects()
    {
        string target = State.GetData("pending_location");
        if (target != null && Locations.ContainsKey(target))
        {
            CurrentLocationId = target;
            Console.WriteLine($"Вы перемещены в {GetCurrentLocation().Name}");
            CheckEvents(EventType.OnEnter);
            State.ClearData("pending_location");
        }
        string qid = State.GetData("pending_start_quest");
        if (qid != null)
        {
            var quest = Quests.Find(q => q.Id == qid);
            quest?.Start();
            State.ClearData("pending_start_quest");
        }
        qid = State.GetData("pending_complete_quest");
        if (qid != null)
        {
            var quest = Quests.Find(q => q.Id == qid);
            quest?.Complete();
            State.ClearData("pending_complete_quest");
        }
        string dir = State.GetData("pending_add_exit_dir");
        string targetLoc = State.GetData("pending_add_exit_target");
        if (dir != null && targetLoc != null)
        {
            GetCurrentLocation().AddExit(dir, targetLoc);
            State.ClearData("pending_add_exit_dir");
            State.ClearData("pending_add_exit_target");
        }
    }
}




class Program
{
    static void Main()
    {
        Console.WriteLine("=== Добро пожаловать в игру 'Побег из квартиры' ===");
        Console.WriteLine("Вы находитесь в квартире, из которой нужно выбраться.");
        Console.WriteLine("Исследуйте комнаты, находите предметы и решайте головоломки.");
        Console.WriteLine();
        Console.WriteLine("Правила:");
        Console.WriteLine("- У вас есть здоровье (100). Берегите его.");
        Console.WriteLine("- В тёмном коридоре без фонарика вы теряете здоровье.");
        Console.WriteLine("- В ванной за шторкой вас может поджидать монстр.");
        Console.WriteLine("- Чтобы выйти, нужно ввести правильный код на двери в прихожей.");
        Console.WriteLine("- Цифры кода находятся в разных комнатах: спальня (под кроватью), гостиная (в шкатулке), ванная (за картиной 'Лес'), прихожая (мигающая лампочка).");
        Console.WriteLine("- Порядок цифр: гостиная, ванная, спальня, прихожая (подсказка на потолке в прихожей).");
        Console.WriteLine();
        Console.WriteLine("Доступные команды (введите help в любой момент):");
        Console.WriteLine("  look                 - осмотреться");
        Console.WriteLine("  go <направление>     - пойти (north, south, east, west)");
        Console.WriteLine("  interact <id>        - взаимодействовать с объектом");
        Console.WriteLine("  inv                  - показать инвентарь");
        Console.WriteLine("  status               - показать состояние");
        Console.WriteLine("  help                 - эта справка");
        Console.WriteLine();
        Console.WriteLine("Начинаем игру...");
        Console.WriteLine();

        Game game = new Game();
        InitializeWorld(game);
        game.Run();
    }

    static void InitializeWorld(Game game)
    {
        
        var bedroom = new Location { Id = "bedroom", Name = "Спальня", Description = "Уютная комната с кроватью, шкафом и картиной на стене." };
        var livingRoom = new Location { Id = "livingroom", Name = "Гостиная", Description = "Большая комната с диваном и тумбочкой." };
        var darkCorridor = new Location { Id = "corridor", Name = "Тёмный коридор", Description = "Страшный коридор без освещения." };
        var bathroom = new Location { Id = "bathroom", Name = "Ванная", Description = "Сырая комната с зеркалом, ванной и кранами." };
        var hallway = new Location { Id = "hallway", Name = "Прихожая", Description = "Здесь выход на улицу, но дверь с кодовым замком." };
        var outside = new Location { Id = "outside", Name = "Улица", Description = "Свобода!" };

        game.Locations.Add(bedroom.Id, bedroom);
        game.Locations.Add(livingRoom.Id, livingRoom);
        game.Locations.Add(darkCorridor.Id, darkCorridor);
        game.Locations.Add(bathroom.Id, bathroom);
        game.Locations.Add(hallway.Id, hallway);
        game.Locations.Add(outside.Id, outside);

        
        bedroom.AddExit("south", "livingroom");
        livingRoom.AddExit("north", "bedroom");
        livingRoom.AddExit("west", "corridor");
        darkCorridor.AddExit("east", "livingroom");
        darkCorridor.AddExit("west", "bathroom");
        bathroom.AddExit("east", "corridor");
        bathroom.AddExit("north", "hallway");
        hallway.AddExit("south", "bathroom");

        game.CurrentLocationId = "livingroom";

        
        bedroom.Interactables.Add(new SimpleObject("bed", "Кровать", "Старая кровать", new List<IEffect> { new LogEffect("Под кроватью вы находите бумажку с цифрой 1."), new SetDataEffect("digit3", "1") }));
        bedroom.Interactables.Add(new Chest("wardrobe", "Шкаф", "Большой платяной шкаф", null, new List<IEffect> { new AddItemEffect("SofaKey"), new LogEffect("Вы нашли ключ от выдвижного ящика дивана.") }));
        bedroom.Interactables.Add(new SimpleObject("painting", "Картина", "Картина с изображением леса", new List<IEffect> { new LogEffect("На картине нарисован лес. ") }));

        
        livingRoom.Interactables.Add(new SimpleObject("table", "Тумбочка", "Маленькая тумбочка", new List<IEffect> { new LogEffect("Записка: 'Чтобы увидеть код, открой горячую воду в ванной'") }));
        var drawer = new Drawer("sofa_drawer", "Выдвижной ящик дивана", "Выдвижной ящик", new HasItemCondition("SofaKey"), new List<IEffect> { new AddItemEffect("Casket"), new LogEffect("Вы открыли ящик и нашли шкатулку.") });
        livingRoom.Interactables.Add(drawer);
        var casket = new Chest("casket", "Шкатулка", "Старинная шкатулка с кодовым замком", new DataEqualsCondition("mirror_code", "364"), new List<IEffect> { new LogEffect("Шкатулка открыта! Внутри лежит записка с цифрой 3."), new SetDataEffect("digit1", "3") });
        livingRoom.Interactables.Add(casket);

        
        bathroom.Interactables.Add(new HotWaterTap("hot_tap", "Кран с горячей водой", "Кран, из которого течёт вода"));
        var mirror = new Mirror("mirror", "Зеркало", "Большое зеркало над раковиной");
        bathroom.Interactables.Add(mirror);
        bathroom.Interactables.Add(new PictureSelection("forest_pic", "Картина 'Лес'", "Изображение леса", "forest"));
        bathroom.Interactables.Add(new PictureSelection("mountains_pic", "Картина 'Горы'", "Изображение гор", "mountains"));
        bathroom.Interactables.Add(new PictureSelection("sea_pic", "Картина 'Море'", "Изображение моря", "sea"));
        bathroom.Interactables.Add(new Trap("bath", "Ванна", "Зашторенная ванна", null, new List<IEffect> { new DamageEffect(100), new LogEffect("Монстр убил вас!") }, true));

        
        hallway.Interactables.Add(new BlinkingLamp("lamp", "Мигающая лампочка", "Лампочка на потолке мигает"));
        hallway.Interactables.Add(new Inscription("inscription", "Надпись на потолке", "Слова на потолке"));
        hallway.Interactables.Add(new CodeDoor("exit_door", "Дверь с кодовым замком", "Выходная дверь, на ней кодовый замок"));

        
        var darkEvent = new OnEnterLocationEvent(new NotCondition(new HasItemCondition("Torch")), new List<IEffect> { new DamageEffect(10), new LogEffect("В темноте вы ушиблись") });
        darkCorridor.Events.Add(darkEvent);

        
        game.Quests.Add(new Quest("Escape", "Побег", "Выбраться из квартиры", new FlagCondition("Escaped", true)));
        game.Quests.Add(new Quest("GhostHelp", "Помощь призраку", "Найти кружку", new FlagCondition("GhostHelpCompleted", true)));
    }
}




public class DataEqualsCondition : ConditionBase
{
    private string key, value;
    public DataEqualsCondition(string key, string value) { this.key = key; this.value = value; }
    public override bool IsTrue(GameState state) => state.GetData(key) == value;
}

public class SetDataEffect : EffectBase
{
    private string key, value;
    public SetDataEffect(string key, string value) { this.key = key; this.value = value; }
    public override void Apply(GameState state) => state.SetData(key, value);
}
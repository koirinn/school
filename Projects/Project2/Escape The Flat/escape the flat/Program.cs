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

    protected CommandBase(string name)
    {
        Name = name;
    }

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

    protected GameEventBase(ICondition condition, List<IEffect> effects, bool isOneTime)
    {
        Condition = condition;
        Effects = effects ?? new List<IEffect>();
        IsOneTime = isOneTime;
    }

  
    public abstract void CheckAndApply(GameState state);
}



public class GameState
{
}

public class Game
{
}
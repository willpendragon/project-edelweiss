using UnityEngine;

public abstract class DeityBehavior : ScriptableObject
{
    public abstract void ExecuteBehavior(Deity deity);

    public abstract void ExecuteBuffBehaviour(Deity deity, Unit unit);
}
using UnityEngine;

public interface IAggroTarget
{
    Transform AimPoint { get; }
    int AggroPriority { get; }
    bool CanBeTargeted { get; }
}

using UnityEngine;
using UniRx;

public interface IEnemyStraightMover
{
    float MaxSpeed { get; }
    float MinSpeed { get; }

    float StraightMoveSpeed { get; set; }

    Vector3 GetStraightMoveVector();

    void Reset();
}
using UnityEngine;

interface IStepMover
{
    bool IsStepping { get; }

    Vector3 GetStepMoveVector();
}

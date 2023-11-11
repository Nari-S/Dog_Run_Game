using UnityEngine;

interface IJumpMover
{
    bool IsAscending { get; set; }

    bool IsDescending { get; set; }

    Vector3 GetJumpMoveVector();
}

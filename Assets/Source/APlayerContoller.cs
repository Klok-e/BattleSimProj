using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class APlayerController : MonoBehaviour
{
    public List<Vector3> pointsToVisitDuringTraining;
    public Vector2 nextPosChange { get; protected set; }
}

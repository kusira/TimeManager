using UnityEngine;

namespace Components.Game.Items.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class WorkerDropTarget : MonoBehaviour
    {
        [Tooltip("Workerのインデックス (0=A, 1=B, ...)")]
        public int WorkerIndex;
    }
}


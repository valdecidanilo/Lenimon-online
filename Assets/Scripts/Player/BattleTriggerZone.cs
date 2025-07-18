using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class BattleTriggerZone : MonoBehaviour
    {
        public Vector3 Position
        {
            private set { transform.position = value; }
            get { return transform.position; }
        }
    }
}

using System;
using UnityEngine;

// Put this on a trigger zone across the road. It tells listeners when the car drives through
// it in the direction of its blue arrow (forward). Used for the finish line and checkpoints.
public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Color _gizmoColor = Color.yellow;

    public event Action<CheckpointTrigger> Crossed;

    // Runs when the component is added: makes the collider a trigger.
    private void Reset()
    {
        Collider zone = GetComponent<Collider>();
        if (zone != null)
        {
            zone.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController car = other.GetComponentInParent<CarController>();
        if (car == null)
        {
            return;
        }

        // Only count a crossing in the driving direction.
        if (Vector3.Dot(car.Velocity, transform.forward) <= 0f)
        {
            return;
        }

        Crossed?.Invoke(this);
    }

    // Shows the zone and its driving direction in the Scene view.
    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            return;
        }

        Gizmos.color = _gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
        Gizmos.DrawLine(box.center, box.center + Vector3.forward * 4f);   // the driving direction
    }
}
// using UnityEngine;

// public class LiftDockZone : MonoBehaviour
// {
//     [SerializeField] private LiftController lift;

//     private void OnTriggerEnter(Collider other)
//     {
//         if (lift == null) return;

//         // Ищем маркер машины в родителях
//         var marker = other.GetComponentInParent<CarRootMarker>();
//         if (marker == null) return;

//         lift.MountCar(marker.transform);
//     }

//     // Опционально: отцеплять при выезде
//     private void OnTriggerExit(Collider other)
//     {
//         if (lift == null) return;

//         var marker = other.GetComponentInParent<CarRootMarker>();
//         if (marker == null) return;

//         // если хочешь, чтобы при выезде автоматом отцеплялось:
//         // lift.UnmountCar();
//     }
// }

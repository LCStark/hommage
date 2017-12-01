using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {

public Constraint constraint;

public float dragSpeed;
public float scrollSpeed;
    
private Vector3 draggedPosPrevious;
private Vector3 draggedPosCurrent;

private Camera cameraComponent;

private float currentFov;
private float targetFov;

void Start() {
    cameraComponent = GetComponent<Camera>();
    currentFov = cameraComponent.fieldOfView;
    targetFov = cameraComponent.fieldOfView;
}


public void SetConstraint(float minX, float maxX, float minY, float maxY, float minZ, float maxZ) {
    constraint = new Constraint(minX, maxX, minY, maxY, minZ, maxZ);
}

public Ray ScreenPointToRay(Vector3 point) {
    return cameraComponent.ScreenPointToRay(point);
}

public void StartDragging(Vector3 position) {
    draggedPosPrevious = position;
    draggedPosCurrent = position;
}

public void Drag(Vector3 position) {
    if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
    if (Input.GetMouseButtonDown(0)) {
        draggedPosPrevious = position;
        draggedPosCurrent = position;
    }
    
    if (Input.GetMouseButton(0)) {
        draggedPosCurrent = position;
        Vector3 diff = draggedPosCurrent - draggedPosPrevious;
        diff = cameraComponent.transform.rotation * diff;
        diff *= dragSpeed;

        Vector3 cp = cameraComponent.transform.position;
        Vector3 newPosition = new Vector3(
            Mathf.Clamp(cp.x - diff.x, constraint.minX, constraint.maxX),
            Mathf.Clamp(cp.y - diff.y, constraint.minY, constraint.maxY),
            Mathf.Clamp(cp.z - diff.z, constraint.minZ, constraint.maxZ)
        );
        transform.position = newPosition;
        draggedPosPrevious = draggedPosCurrent;
    }
}

public void MoveTo(Vector3 position) {
    Vector3 newPosition = new Vector3(
        Mathf.Clamp(position.x, constraint.minX, constraint.maxX),
        Mathf.Clamp(position.y, constraint.minY, constraint.maxY),
        Mathf.Clamp(position.z, constraint.minZ, constraint.maxZ)
    );
    transform.position = newPosition;
}

void Update() {
    if (Input.mouseScrollDelta.y != 0.0f) {
        targetFov = Mathf.Clamp(targetFov - Input.mouseScrollDelta.y * scrollSpeed, 30.0f, 90.0f);
    }

    if (currentFov != targetFov) {
        currentFov = Mathf.MoveTowards(currentFov, targetFov, 1.0f);
        cameraComponent.fieldOfView = currentFov;
    }
}

}

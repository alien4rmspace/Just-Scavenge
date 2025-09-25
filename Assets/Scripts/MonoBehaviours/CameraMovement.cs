using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RTSIsometricCamera : MonoBehaviour {
  [Header("Movement")]
  public float moveSpeed = 20f;
  public float dragPanSpeed = 0.5f;
  public float rotationSpeed = 50f;

  [Header("Zoom")]
  public float zoomSpeed = 500f;
  public float minZoom = 10f;
  public float maxZoom = 80f;

  [Header("Target Settings")]
  public Vector3 pivotPoint = Vector3.zero;
  public float pitch = 45f;
  public float yaw = 40f;
  public float zoom = 30f;

  private Vector3 _lastMousePosition;

  void Start() {
    UpdateCameraTransform();
  }

  void Update() {
    float delta = Time.deltaTime;

    // --- WASD Movement ---
    Vector3 forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
    Vector3 right = Quaternion.Euler(0, yaw, 0) * Vector3.right;

    Vector3 move = Vector3.zero;
    if (Input.GetKey(KeyCode.W)) move += forward;
    if (Input.GetKey(KeyCode.S)) move -= forward;
    if (Input.GetKey(KeyCode.D)) move += right;
    if (Input.GetKey(KeyCode.A)) move -= right;

    if (move.sqrMagnitude > 1e-8f)
    {
      pivotPoint += move.normalized * (moveSpeed * delta);
    }

    // --- Middle Mouse Drag ---
    if (Input.GetMouseButtonDown(2)) {
      _lastMousePosition = Input.mousePosition;
    }

    if (Input.GetMouseButton(2)) {
      if (Input.mousePosition != _lastMousePosition)
      {
        Vector3 mouseDelta = Input.mousePosition - _lastMousePosition;
        pivotPoint -= right * (mouseDelta.x * dragPanSpeed * delta);
        pivotPoint -= forward * (mouseDelta.y * dragPanSpeed * delta);
        _lastMousePosition = Input.mousePosition;
      }
    }

    // --- Zoom ---
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (Mathf.Abs(scroll) > 0.01f) {
      zoom -= scroll * zoomSpeed * delta;
      zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    // --- Rotation ---
    if (Input.GetKey(KeyCode.Q)) yaw -= rotationSpeed * delta;
    if (Input.GetKey(KeyCode.E)) yaw += rotationSpeed * delta;

    UpdateCameraTransform();
  }

  void UpdateCameraTransform() {
    Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
    Vector3 offset = rotation * new Vector3(0, 0, -zoom);
    transform.position = pivotPoint + offset;
    transform.rotation = rotation;
  }
}

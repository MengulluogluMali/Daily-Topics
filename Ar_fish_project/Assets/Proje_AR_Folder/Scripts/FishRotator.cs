using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ARFishQuiz
{
    /// <summary>
    /// 3D balık nesnesini parmakla (mobil dokunma) veya PC'de sol fare tuşu ile
    /// tutup sürükleyerek döndürmeyi sağlar.
    /// Nesneye basılı tutulurken sağa-sola sürüklemek yatay (Y ekseni),
    /// yukarı-aşağı sürüklemek dikey (X ekseni) dönüş yaptırır; çapraz sürükleme
    /// ikisinin birleşimidir.
    ///
    /// Bu script döndürülecek 3D nesnenin (örn. Zargana_nesne / Zargana_Balig)
    /// üzerine eklenir. Nesnenin bir Collider'a sahip olması gerekir (yoksa otomatik eklenir).
    /// </summary>
    public class FishRotator : MonoBehaviour
    {
        [Header("Döndürme Ayarları")]
        [Tooltip("Sürükleme hassasiyeti (derece / piksel).")]
        [SerializeField] private float rotationSpeed = 0.4f;

        [Tooltip("Yatay (sağ-sol) dönüşü etkinleştir.")]
        [SerializeField] private bool allowHorizontal = true;

        [Tooltip("Dikey (yukarı-aşağı) dönüşü etkinleştir.")]
        [SerializeField] private bool allowVertical = true;

        [Tooltip("Bırakıldığında dönüşün bir miktar devam etmesi (atalet).")]
        [SerializeField] private bool inertia = true;

        [Tooltip("Atalet sönümleme katsayısı (yüksek = daha hızlı durur).")]
        [SerializeField] private float inertiaDamping = 4f;

        [Tooltip("Raycast için kullanılacak kamera. Boş ise Camera.main kullanılır.")]
        [SerializeField] private Camera raycastCamera;

        [SerializeField] private bool debugLog = false;

        private bool isDragging = false;
        private Vector2 lastPointerPos;
        private Vector2 angularVelocity; // x: dikey hız, y: yatay hız

        private void Awake()
        {
            // Döndürülebilmesi için collider gerekli (raycast'in nesneyi yakalaması için).
            if (GetComponent<Collider>() == null)
            {
                var bc = gameObject.AddComponent<BoxCollider>();
                if (debugLog) Debug.Log($"[FishRotator] {name} için otomatik BoxCollider eklendi.");
            }
        }

        private void Update()
        {
            // Çizim paneli açıksa döndürme yapma.
            if (AquariumDrawingManager.Instance != null && AquariumDrawingManager.Instance.IsOpen)
            {
                isDragging = false;
                return;
            }

            HandleInput();

            if (!isDragging && inertia)
                ApplyInertia();
        }

        private void HandleInput()
        {
            bool pressed = false;
            bool held = false;
            bool released = false;
            Vector2 screenPos = Vector2.zero;
            int touchId = -1;

#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                held = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame) pressed = true;
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                released = true;
            }
            else if (Mouse.current != null)
            {
                held = Mouse.current.leftButton.isPressed;
                pressed = Mouse.current.leftButton.wasPressedThisFrame;
                released = Mouse.current.leftButton.wasReleasedThisFrame;
                screenPos = Mouse.current.position.ReadValue();
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!pressed && !held && !released)
            {
                if (Input.touchCount > 0)
                {
                    var t = Input.GetTouch(0);
                    screenPos = t.position;
                    touchId = t.fingerId;
                    held = (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Began);
                    pressed = (t.phase == TouchPhase.Began);
                    released = (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled);
                }
                else
                {
                    held = Input.GetMouseButton(0);
                    pressed = Input.GetMouseButtonDown(0);
                    released = Input.GetMouseButtonUp(0);
                    screenPos = Input.mousePosition;
                }
            }
#endif

            if (released)
            {
                isDragging = false;
                return;
            }

            // Yeni basış: bu nesneye mi denk geldi kontrol et.
            if (pressed)
            {
                if (IsPointerOverUI(touchId)) return;

                Camera cam = raycastCamera != null ? raycastCamera : Camera.main;
                if (cam == null) return;

                Ray ray = cam.ScreenPointToRay(screenPos);
                var hits = Physics.RaycastAll(ray, 5000f);
                bool hitThis = false;
                foreach (var hit in hits)
                {
                    if (hit.collider != null &&
                        (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
                    {
                        hitThis = true;
                        break;
                    }
                }

                if (hitThis)
                {
                    isDragging = true;
                    lastPointerPos = screenPos;
                    angularVelocity = Vector2.zero;
                    if (debugLog) Debug.Log($"[FishRotator] {name} döndürme başladı.");
                }
                return;
            }

            // Sürükleme devam ediyor.
            if (held && isDragging)
            {
                Vector2 delta = screenPos - lastPointerPos;
                lastPointerPos = screenPos;
                RotateBy(delta);
            }
        }

        private void RotateBy(Vector2 delta)
        {
            float yaw = allowHorizontal ? -delta.x * rotationSpeed : 0f;   // sağ-sol
            float pitch = allowVertical ? delta.y * rotationSpeed : 0f;    // yukarı-aşağı

            // Dünya eksenlerine göre döndürerek doğal his ver.
            transform.Rotate(Vector3.up, yaw, Space.World);
            transform.Rotate(Vector3.right, pitch, Space.World);

            angularVelocity = new Vector2(pitch, yaw);
        }

        private void ApplyInertia()
        {
            if (angularVelocity.sqrMagnitude < 0.0001f) return;

            transform.Rotate(Vector3.up, angularVelocity.y, Space.World);
            transform.Rotate(Vector3.right, angularVelocity.x, Space.World);

            angularVelocity = Vector2.Lerp(angularVelocity, Vector2.zero, inertiaDamping * Time.deltaTime);
        }

        private static bool IsPointerOverUI(int touchId)
        {
            if (EventSystem.current == null) return false;
            if (touchId >= 0)
                return EventSystem.current.IsPointerOverGameObject(touchId);
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}

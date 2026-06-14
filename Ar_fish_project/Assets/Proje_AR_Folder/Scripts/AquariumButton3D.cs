using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ARFishQuiz
{
    /// <summary>
    /// Sahnedeki 3D "Akvaryuma_git" butonuna tıklanmasını algılar ve
    /// AquariumDrawingManager'ı bu balık için açar.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AquariumButton3D : MonoBehaviour
    {
        [Tooltip("Bu butonun ait olduğu balığın benzersiz id'si (genelde target objesinin adı).")]
        [SerializeField] private string fishId = "zargana_balıgı_target";

        [Tooltip("Raycast yapılacak kamera. Boş bırakılırsa Camera.main kullanılır.")]
        [SerializeField] private Camera raycastCamera;

        [SerializeField] private bool pulseOnClick = true;
        [SerializeField] private float pulseScale = 1.15f;
        [SerializeField] private float pulseDuration = 0.15f;
        [SerializeField] private bool debugLog = false;

        private Vector3 originalScale;
        private float pulseTimer = -1f;
        private bool isPulsingUp = true;

        public string FishId
        {
            get => fishId;
            set => fishId = value;
        }

        private void Awake()
        {
            originalScale = transform.localScale;
            if (GetComponent<Collider>() == null)
                gameObject.AddComponent<BoxCollider>();
        }

        private void Update()
        {
            HandleInput();
            HandlePulse();
        }

        private void HandleInput()
        {
            // Çizim paneli açıksa veya viewer açıksa 3D butonu tetikleme
            if (AquariumDrawingManager.Instance != null && AquariumDrawingManager.Instance.IsOpen)
                return;

            Camera cam = raycastCamera != null ? raycastCamera : Camera.main;
            if (cam == null) return;

            bool pressed = false;
            Vector2 screenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                pressed = true;
                screenPos = Mouse.current.position.ReadValue();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                pressed = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!pressed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    pressed = true;
                    screenPos = Input.mousePosition;
                }
                else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    pressed = true;
                    screenPos = Input.GetTouch(0).position;
                }
            }
#endif

            if (!pressed) return;

            // UI üstündeysek (örneğin sağ üstteki "Akvaryum" butonu, viewer paneli, açık drawing canvas)
            // 3D raycast yapmayalım.
            if (IsPointerOverUI(screenPos)) return;

            Ray ray = cam.ScreenPointToRay(screenPos);
            var hits = Physics.RaycastAll(ray, 2000f);
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    if (debugLog) Debug.Log($"[AquariumButton3D] Tıklandı, fishId={fishId}");
                    OnClicked();
                    return;
                }
            }
        }

        private void OnClicked()
        {
            if (pulseOnClick)
            {
                pulseTimer = 0f;
                isPulsingUp = true;
            }

            if (AquariumDrawingManager.Instance == null)
            {
                Debug.LogWarning("[AquariumButton3D] AquariumDrawingManager bulunamadı! Sahnede AquariumDrawingSetup yok mu?");
                return;
            }

            string idToUse = string.IsNullOrEmpty(fishId) ? gameObject.name : fishId;

            // Akvaryumu çiz, sonra otomatik viewer'da göster
            AquariumDrawingManager.Instance.OpenForFish(
                idToUse,
                onSaved: () =>
                {
                    if (AquariumViewerManager.Instance != null)
                        AquariumViewerManager.Instance.OpenViewerForFish(idToUse);
                });
        }

        private static bool IsPointerOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            // Touch desteği için (mobilde IsPointerOverGameObject() touchId ister)
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                int id = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject(id)) return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount > 0)
            {
                int id = Input.GetTouch(0).fingerId;
                if (EventSystem.current.IsPointerOverGameObject(id)) return true;
            }
#endif
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void HandlePulse()
        {
            if (pulseTimer < 0f) return;
            pulseTimer += Time.deltaTime;
            float t = Mathf.Clamp01(pulseTimer / pulseDuration);

            if (isPulsingUp)
            {
                transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, t);
                if (t >= 1f) { isPulsingUp = false; pulseTimer = 0f; }
            }
            else
            {
                transform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, t);
                if (t >= 1f) { pulseTimer = -1f; transform.localScale = originalScale; }
            }
        }
    }
}

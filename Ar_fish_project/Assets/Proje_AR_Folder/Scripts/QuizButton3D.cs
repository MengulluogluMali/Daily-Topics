using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
#endif

namespace ARFishQuiz
{
    /// <summary>
    /// 3D dünyada yer alan bir butonun dokunmatik ekran / fare sol tıklaması ile
    /// tıklanmasını algılar ve QuizManager.StartQuiz metodunu çağırır.
    /// Hem eski (UnityEngine.Input) hem yeni (UnityEngine.InputSystem) API'yi destekler.
    /// Bu script raycast temelli çalıştığı için Collider gerektirir.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class QuizButton3D : MonoBehaviour
    {
        [Tooltip("Quiz'i yönetecek QuizManager referansı")]
        [SerializeField] private QuizManager quizManager;

        [Tooltip("Raycast yapılacak kamera. Boş bırakılırsa Camera.main kullanılır.")]
        [SerializeField] private Camera raycastCamera;

        [Tooltip("Tıklanınca ölçek animasyonu uygulansın mı?")]
        [SerializeField] private bool pulseOnClick = true;

        [SerializeField] private float pulseScale = 1.15f;
        [SerializeField] private float pulseDuration = 0.15f;

        [Tooltip("Debug loglarını aç")]
        [SerializeField] private bool debugLog = false;

        private Vector3 originalScale;
        private float pulseTimer = -1f;
        private bool isPulsingUp = true;

        private void Awake()
        {
            originalScale = transform.localScale;

            // Collider kontrolü
            var col = GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogWarning("[QuizButton3D] Collider yok! BoxCollider ekleniyor.", this);
                gameObject.AddComponent<BoxCollider>();
            }
        }

        private void Update()
        {
            HandleInput();
            HandlePulse();
        }

        private void HandleInput()
        {
            Camera cam = raycastCamera != null ? raycastCamera : Camera.main;
            if (cam == null)
            {
                if (debugLog) Debug.LogWarning("[QuizButton3D] Camera bulunamadı!");
                return;
            }

            bool pressed = false;
            Vector2 screenPos = Vector2.zero;
            int touchId = -1;

#if ENABLE_INPUT_SYSTEM
            // Yeni Input System (Android/iOS dokunma + PC mouse)
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                pressed = true;
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                pressed = true;
                screenPos = Mouse.current.position.ReadValue();
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            // Eski Input Manager (her ihtimale karşı)
            if (!pressed)
            {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    pressed = true;
                    screenPos = Input.GetTouch(0).position;
                    touchId = Input.GetTouch(0).fingerId;
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    pressed = true;
                    screenPos = Input.mousePosition;
                }
            }
#endif

            if (!pressed) return;

            // UI üstündeysek (örn. sağ üstteki Akvaryum butonu, viewer paneli) tetikleme
            if (IsPointerOverUI(touchId, screenPos)) return;
            // Çizim paneli açıkken hiçbir 3D buton tetiklenmesin
            if (AquariumDrawingManager.Instance != null && AquariumDrawingManager.Instance.IsOpen) return;

            TryRaycast(cam, screenPos);
        }

        private static bool IsPointerOverUI(int touchId, Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            // Önce touchId ile dene (mobil), olmadıysa default pointer kontrolü
            if (touchId >= 0)
            {
                if (EventSystem.current.IsPointerOverGameObject(touchId)) return true;
            }
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void TryRaycast(Camera cam, Vector2 screenPosition)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);

            if (debugLog)
                Debug.Log($"[QuizButton3D] Tıklama algılandı. Pozisyon: {screenPosition}, Kamera: {cam.name}");

            // Birden fazla çarpışma olabilir (balık vs.). Hedefin kendisine vurulup vurulmadığını kontrol et.
            RaycastHit[] hits = Physics.RaycastAll(ray, 2000f);
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    if (debugLog) Debug.Log("[QuizButton3D] Butona tıklandı, Quiz başlıyor.");
                    OnClicked();
                    return;
                }
            }

            if (debugLog && hits.Length > 0)
            {
                string hitNames = "";
                foreach (var h in hits) hitNames += h.collider.name + ", ";
                Debug.Log($"[QuizButton3D] Butona vurulmadı. Vurulanlar: {hitNames}");
            }
        }

        private void OnClicked()
        {
            if (pulseOnClick)
            {
                pulseTimer = 0f;
                isPulsingUp = true;
            }

            if (quizManager != null)
            {
                quizManager.StartQuiz();
            }
            else
            {
                Debug.LogWarning("[QuizButton3D] QuizManager atanmamış!", this);
            }
        }

        private void HandlePulse()
        {
            if (pulseTimer < 0f) return;

            pulseTimer += Time.deltaTime;
            float t = Mathf.Clamp01(pulseTimer / pulseDuration);

            if (isPulsingUp)
            {
                transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, t);
                if (t >= 1f)
                {
                    isPulsingUp = false;
                    pulseTimer = 0f;
                }
            }
            else
            {
                transform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, t);
                if (t >= 1f)
                {
                    pulseTimer = -1f;
                    transform.localScale = originalScale;
                }
            }
        }
    }
}

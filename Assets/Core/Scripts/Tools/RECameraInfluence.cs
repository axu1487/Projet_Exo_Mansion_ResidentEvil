using UnityEngine;

public class RECameraInfluence : MonoBehaviour
{
    [Header("Cible")]
    public Transform player;

    [Header("Intensité de l'effet (en degrés)")]
    [Tooltip("Décalage horizontal max quand le joueur est au bord de l'écran")]
    public float maxPanAngle = 3f;

    [Tooltip("Décalage vertical max quand le joueur est en haut/bas de l'écran")]
    public float maxTiltAngle = 2f;

    [Header("Fluidité")]
    public float smoothSpeed = 3f;

    private Camera cam;
    private Quaternion baseRotation;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (player == null || cam == null) return;

        // 1. Calcul de la position du joueur dans le viseur de la caméra (de 0.0 à 1.0)
        Vector3 viewportPos = cam.WorldToViewportPoint(player.position);

        // Si le joueur est derrière la caméra, on ne réagit pas
        if (viewportPos.z < 0) return;

        // 2. Écart par rapport au centre de l'écran (-0.5 à +0.5)
        float offsetX = viewportPos.x - 0.5f;
        float offsetY = viewportPos.y - 0.5f;

        // 3. Calcul du micro-décalage de rotation
        float pan = offsetX * maxPanAngle;
        float tilt = -offsetY * maxTiltAngle; // Inversé pour un axe naturel

        // 4. Application fluide par rapport à la pose de départ
        Quaternion targetRotation = baseRotation * Quaternion.Euler(tilt, pan, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}
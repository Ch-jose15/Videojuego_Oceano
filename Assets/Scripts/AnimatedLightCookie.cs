using UnityEngine;

// Deep Treasure - Cookie animado para la luz direccional (HDRP)
// Ponlo en el Directional Light (el sol) y asigna un material con el shader DeepTreasure/CausticCookie.
// Renderiza el patrón en una RenderTexture y la usa como Cookie de la luz. Con Volumetric Fog
// activo y Volumetrics en la luz, salen rayos de luz animados; además el fondo recibe manchas de luz.
[ExecuteAlways]
[RequireComponent(typeof(Light))]
public class AnimatedLightCookie : MonoBehaviour
{
    [Tooltip("Material con el shader DeepTreasure/CausticCookie")]
    [SerializeField] private Material cookieMaterial;

    [Tooltip("Resolución de la textura del cookie")]
    [SerializeField] private int resolution = 512;

    [Tooltip("Veces por segundo que se actualiza el cookie (30 es suficiente)")]
    [SerializeField, Range(5f, 60f)] private float updatesPerSecond = 30f;

    private static readonly int CookieTimeId = Shader.PropertyToID("_CookieTime");

    private Light sun;
    private RenderTexture cookieRT;
    private float nextUpdate;

    private void OnEnable()
    {
        sun = GetComponent<Light>();
        CreateTexture();
        RenderCookie();
    }

    private void OnDisable()
    {
        if (sun != null && sun.cookie == cookieRT)
            sun.cookie = null;

        if (cookieRT != null)
        {
            if (RenderTexture.active == cookieRT)
                RenderTexture.active = null;

            cookieRT.Release();
            DestroyImmediate(cookieRT);
            cookieRT = null;
        }
    }

    private void OnValidate()
    {
        resolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(resolution), 64, 2048);
        if (isActiveAndEnabled && cookieRT != null && cookieRT.width != resolution)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        if (cookieMaterial == null) return;
        if (Application.isPlaying && Time.time < nextUpdate) return;

        nextUpdate = Time.time + 1f / updatesPerSecond;
        RenderCookie();
    }

    private void CreateTexture()
    {
        cookieRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32)
        {
            name = "CausticCookie_RT",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
            useMipMap = false,
            hideFlags = HideFlags.DontSave
        };
        cookieRT.Create();
    }

    private void RenderCookie()
    {
        if (cookieMaterial == null || cookieRT == null || sun == null) return;

        cookieMaterial.SetFloat(CookieTimeId, Time.time);

        // Blit deja la RT como RenderTexture.active: guardamos y restauramos la anterior
        RenderTexture previous = RenderTexture.active;
        Graphics.Blit(Texture2D.whiteTexture, cookieRT, cookieMaterial);
        RenderTexture.active = previous;

        // Avisa a HDRP de que la textura ha cambiado para que actualice el atlas de cookies
        cookieRT.IncrementUpdateCount();

        if (sun.cookie != cookieRT)
            sun.cookie = cookieRT;
    }
}
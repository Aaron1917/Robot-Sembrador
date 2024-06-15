using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 touchStart;
    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] public float targetAspectRatio = 18f/9f;
    [Header("Zoom Options")]
    [SerializeField] private float zoomStep = 1;
    [SerializeField] private float zoomOutMin = 10;
    [SerializeField] private float zoomOutMax = 29; 
    //34---68 ==> 34*2 = 68 
    //29---58 ==> 29*2 = 58 --- Actual
    [Header("Frame Options")]
    [SerializeField] private GameObject frameObj;
    [SerializeField] private GameObject terraObj;
    private SpriteRenderer frameRenderer;
    private SpriteRenderer terraRenderer;
    private Transform frameTransform;
    private Transform terraTransform;
    [SerializeField] private float margin = 5f;
    [SerializeField] private Vector2Int terrainSize= new Vector2Int(61, 48);//W,H
    [SerializeField] private Vector2 offset = new Vector2(0, 0);
    private float terMinX, terMaxX, terMinY, terMaxY;

    private void Awake()
    {
        // Obtener el AspectRatio
        if(targetAspectRatio == 0)
        {
            targetAspectRatio = (float) Screen.width/ Screen.height;
            //Debug.Log(targetAspectRatio);
        }
        /*Debug.Log("H: " + Screen.height + " y W: " + Screen.width);
        Debug.Log(targetAspectRatio);*/
        UpdateAspectRatio();
        //Obtener los Transform
        frameRenderer = frameObj.GetComponent<SpriteRenderer>();
        terraRenderer = terraObj.GetComponent<SpriteRenderer>();
        frameTransform = frameObj.GetComponent<Transform>();
        terraTransform = terraObj.GetComponent<Transform>();
        //Convetir offset a V3 y sumarlo
        Vector3 offset3 = offset;
        terraTransform.position += offset3;
        frameTransform.position += offset3;
        // Modificar Size en Terrain 
        terraRenderer.size = terrainSize;
        //Adaptar el tamaño al marco 
        float frameHeight= terraRenderer.size.y + (margin * 2f);
        frameRenderer.size = new Vector2(frameHeight * targetAspectRatio, frameHeight);
        zoomOutMax = frameHeight / 2f;
        // obtener la nueva posición de X para "Frame"
        float newX = terraTransform.position.x + (frameRenderer.bounds.size.x / 2f) - margin - (terraRenderer.bounds.size.x / 2f);
        frameTransform.position = new Vector3(newX, frameTransform.position.y, frameTransform.position.z);

        terMinX = frameTransform.position.x - frameRenderer.bounds.size.x / 2f; // -15 -103.1111/2 = -66.5555
        terMaxX = frameTransform.position.x + frameRenderer.bounds.size.x / 2f; // -15 +103.1111/2 = +36.5555

        terMinY = frameRenderer.transform.position.y - frameRenderer.bounds.size.y / 2f; // -24 - 58/2 = -53
        terMaxY = frameRenderer.transform.position.y + frameRenderer.bounds.size.y / 2f; // -24 + 58/2 = +5

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, zoomOutMin, zoomOutMax);
        cam.transform.position = ClampCamera(cam.transform.position);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Pan();
        UpdateAspectRatio();
    }
    private void Pan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 direction = touchStart - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position = ClampCamera(cam.transform.position + direction);
            //cam.transform.position += direction;

        }
    }
    public void zoomIn()
    {
        float newSize = cam.orthographicSize - zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, zoomOutMin, zoomOutMax);
        cam.transform.position = ClampCamera(cam.transform.position);
    }
    public void zoomOut()
    {
        float newSize = cam.orthographicSize + zoomStep;
        cam.orthographicSize = Mathf.Clamp(newSize, zoomOutMin, zoomOutMax);
        cam.transform.position = ClampCamera(cam.transform.position);
    }
    private Vector3 ClampCamera(Vector3 targetPos)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        float minX = terMinX + camWidth;
        float maxX = terMaxX - camWidth;
        float minY = terMinY + camHeight;
        float maxY = terMaxY - camHeight;

        float newX = Mathf.Clamp(targetPos.x, minX, maxX);
        float newY = Mathf.Clamp(targetPos.y, minY, maxY);

        return new Vector3(newX, newY, targetPos.z);
    }
    private void UpdateAspectRatio()
    {
        // Calcula el ancho deseado en función de la relación de aspecto y la altura actual de la pantalla
        float targetWidth = Screen.height * targetAspectRatio;

        // Calcula la diferencia entre el ancho actual y el ancho deseado
        float widthDiff = Screen.width - targetWidth;

        // Si hay una diferencia de ancho, ajusta el tamaño de la cámara para mantener la relación de aspecto
        if (widthDiff > 0)
        {
            cam.rect = new Rect(widthDiff / 2f / Screen.width, 0f, targetWidth / Screen.width, 1f);
        }
        else
        {
            // Si el ancho deseado es mayor que el ancho de la pantalla, ajusta el tamaño de la cámara para mantener la relación de aspecto
            float targetHeight = Screen.width / targetAspectRatio;
            float heightDiff = Screen.height - targetHeight;
            cam.rect = new Rect(0f, heightDiff / 2f / Screen.height, 1f, targetHeight / Screen.height);
        }
    }
}

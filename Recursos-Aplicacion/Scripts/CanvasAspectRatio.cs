using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasAspectRatio : MonoBehaviour
{
    public float targetAspectRatio = 16 / 9f; // La relación de aspecto que deseas mantener

    private Canvas canvas;
    public CameraController camController;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        camController = FindObjectOfType<CameraController>();
        targetAspectRatio = camController.targetAspectRatio;
        UpdateCanvasScale();
    }

    void UpdateCanvasScale()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        float scaleFactor = currentAspectRatio / targetAspectRatio;

        // Si la relación de aspecto de la pantalla es mayor que la relación de aspecto objetivo,
        // establece la escala en x según la diferencia de las relaciones de aspecto.
        if (currentAspectRatio > targetAspectRatio)
        {
            canvas.scaleFactor = scaleFactor;
        }
        else
        {
            // Si la relación de aspecto de la pantalla es menor que la relación de aspecto objetivo,
            // establece la escala en y según la diferencia inversa de las relaciones de aspecto.
            canvas.scaleFactor = 1f / scaleFactor;
        }
    }

    void Update()
    {
        // Llama a UpdateCanvasScale() en cada fotograma para manejar cambios en la resolución de la pantalla en tiempo real.
        UpdateCanvasScale();
    }
}

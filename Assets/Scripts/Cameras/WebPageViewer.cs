using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

public class WebPageViewer : MonoBehaviour
{
    [Header("Web Settings")]
    [Tooltip("URL of the web page to display")]
    public string webUrl = "http://172.24.200.5";

    [Tooltip("Refresh rate in seconds (0 for no auto-refresh)")]
    public float refreshRate = 0f;

    [Header("Display Settings")]
    [Tooltip("Target renderer (should be attached to a quad)")]
    public Renderer targetRenderer;

    [Tooltip("Target FPS for updates")]
    public float targetFPS = 30f;

    [Tooltip("Width of the rendered web page")]
    public int pageWidth = 1920;

    [Tooltip("Height of the rendered web page")]
    public int pageHeight = 1080;

    private Texture2D texture;
    private bool isInitialized = false;
    private float lastRefreshTime = 0f;
    private DateTime lastFrameTime = DateTime.MinValue;

    void Start()
    {
        // Allow insecure connections
        ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertificates;
        InitializeWebViewer();
    }

    private bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    void InitializeWebViewer()
    {
        try
        {
            if (targetRenderer == null)
            {
                Debug.LogError("No renderer assigned to WebPageViewer!");
                enabled = false;
                return;
            }

            // Create texture
            texture = new Texture2D(pageWidth, pageHeight);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            targetRenderer.material.mainTexture = texture;

            // Initial load
            LoadWebPage();
            isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize WebPageViewer: {e.Message}");
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // Handle auto-refresh
        if (refreshRate > 0 && Time.time - lastRefreshTime >= refreshRate)
        {
            RefreshWebPage();
        }
    }

    public void RefreshWebPage()
    {
        if (!isInitialized) return;

        try
        {
            StartCoroutine(LoadWebPageCoroutine());
        }
        catch (Exception e)
        {
            Debug.LogError($"Error refreshing web page: {e.Message}");
        }
    }

    private void LoadWebPage()
    {
        if (!isInitialized) return;

        try
        {
            StartCoroutine(LoadWebPageCoroutine());
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading web page: {e.Message}");
        }
    }

    private IEnumerator LoadWebPageCoroutine()
    {
        // Throttle processing based on the target FPS
        DateTime now = DateTime.Now;
        if ((now - lastFrameTime).TotalSeconds < (1.0 / targetFPS))
        {
            yield break;
        }
        lastFrameTime = now;

        using (UnityWebRequest www = UnityWebRequest.Get(webUrl))
        {
            www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            www.certificateHandler = new AcceptAllCertificatesHandler();
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string htmlContent = www.downloadHandler.text;
                Debug.Log($"Received HTML content length: {htmlContent.Length}");

                // Create a simple HTML to image conversion
                // This is a basic example - you might want to use a more sophisticated HTML renderer
                Color backgroundColor = Color.white;
                Color textColor = Color.black;

                // Clear the texture with background color
                Color[] colors = new Color[pageWidth * pageHeight];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = backgroundColor;
                }
                texture.SetPixels(colors);

                // Add some text to indicate the page is loaded
                // In a real implementation, you would parse the HTML and render it properly
                string displayText = "Web Page Loaded\n" + DateTime.Now.ToString();
                DrawText(displayText, 10, 10, textColor);

                texture.Apply();
                lastRefreshTime = Time.time;
            }
            else
            {
                Debug.LogError($"Failed to load web page: {www.error}");
            }
        }
    }

    private void DrawText(string text, int x, int y, Color color)
    {
        // This is a very basic text rendering implementation
        // In a real application, you would want to use a proper font rendering system
        for (int i = 0; i < text.Length; i++)
        {
            int pixelX = x + (i * 8); // Assuming 8 pixels per character
            if (pixelX >= pageWidth) break;

            // Draw a simple rectangle for each character
            for (int dy = 0; dy < 8; dy++)
            {
                for (int dx = 0; dx < 8; dx++)
                {
                    int pixelY = y + dy;
                    if (pixelY >= pageHeight) break;

                    texture.SetPixel(pixelX + dx, pixelY, color);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (texture != null)
        {
            Destroy(texture);
        }
    }
}

// Helper class to accept all certificates
public class AcceptAllCertificatesHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
} 
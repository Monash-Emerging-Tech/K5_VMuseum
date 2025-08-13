using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Text;

public class MjpegStreamRenderer : MonoBehaviour
{
    public string streamUrl = "http://172.32.1.225:8080/stream_simple.html";
    public Renderer targetRenderer;

    // Add a public FPS input (can be set in the Inspector)
    public float targetFPS = 30f;

    private Thread streamingThread;
    private bool isStreaming = false;
    private byte[] currentFrame;
    private readonly object frameLock = new object();
    private Texture2D texture;
    private bool updateTexture = false;

    // Timestamp for frame throttling
    private DateTime lastFrameTime = DateTime.MinValue;

    void Start()
    {
        Debug.Log("Initializing MJPEG Stream Renderer...");
        texture = new Texture2D(2, 2);
        targetRenderer.material.mainTexture = texture;

        isStreaming = true;
        streamingThread = new Thread(StreamThread);
        streamingThread.IsBackground = true;
        streamingThread.Start();
        Debug.Log("Streaming thread started.");
    }

    void StreamThread()
    {
        Debug.Log("Streaming thread is running.");
        try
        {
            Debug.Log($"Connecting to MJPEG stream at: {streamUrl}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(streamUrl);
            request.Timeout = 10000;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Debug.Log($"Connected to stream. Response status: {response.StatusCode}");
                using (Stream stream = response.GetResponseStream())
                {
                    string boundary = GetBoundary(response.ContentType);
                    if (string.IsNullOrEmpty(boundary))
                    {
                        Debug.LogError("Failed to extract boundary from Content-Type header.");
                        return;
                    }
                    Debug.Log($"Boundary detected: {boundary}");

                    byte[] boundaryBytes = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
                    byte[] buffer = new byte[4096];
                    MemoryStream memoryStream = new MemoryStream();
                    bool processingFrame = false;

                    Debug.Log("Starting to read stream data...");
                    while (isStreaming)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            Debug.LogWarning("No data received from stream. Connection may be closed.");
                            break;
                        }

                        memoryStream.Write(buffer, 0, bytesRead);
                        byte[] data = memoryStream.ToArray();

                        int boundaryIndex = FindSequence(data, boundaryBytes);
                        while (boundaryIndex >= 0)
                        {
                            if (processingFrame)
                            {
                                Debug.Log("Processing frame...");
                                ProcessFrame(data, boundaryIndex);
                                processingFrame = false;
                            }

                            // Remove processed data from buffer
                            int bytesToKeep = data.Length - (boundaryIndex + boundaryBytes.Length);
                            memoryStream.SetLength(0);
                            memoryStream.Write(data, boundaryIndex + boundaryBytes.Length, bytesToKeep);
                            data = memoryStream.ToArray();
                            processingFrame = true;
                            boundaryIndex = FindSequence(data, boundaryBytes);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Stream Error: " + e.Message);
            Debug.LogError("Stack Trace: " + e.StackTrace);
        }
        finally
        {
            Debug.Log("Streaming thread has stopped.");
        }
    }

    void ProcessFrame(byte[] data, int boundaryIndex)
    {
        // Throttle processing based on the target FPS.
        DateTime now = DateTime.Now;
        if ((now - lastFrameTime).TotalSeconds < (1.0 / targetFPS))
        {
            Debug.Log("Skipping frame to maintain target FPS");
            return;
        }
        lastFrameTime = now;

        Debug.Log("Processing frame data...");
        // Find the end of the header (CRLFCRLF)
        byte[] headerEnd = new byte[] { 0x0D, 0x0A, 0x0D, 0x0A };
        int headerEndIndex = FindSequence(data, headerEnd, 0, boundaryIndex);
        if (headerEndIndex == -1)
        {
            Debug.LogWarning("Failed to find header end in frame data.");
            return;
        }

        int imageStart = headerEndIndex + headerEnd.Length;
        int imageLength = boundaryIndex - imageStart;

        lock (frameLock)
        {
            currentFrame = new byte[imageLength];
            Array.Copy(data, imageStart, currentFrame, 0, imageLength);
            updateTexture = true;
        }
        Debug.Log($"Frame processed. Image size: {imageLength} bytes");
    }

    void Update()
    {
        if (updateTexture)
        {
            lock (frameLock)
            {
                if (currentFrame != null && currentFrame.Length > 0)
                {
                    Debug.Log("Updating texture with new frame...");
                    texture.LoadImage(currentFrame);
                    currentFrame = null;
                }
                updateTexture = false;
            }
        }
    }

    void OnDestroy()
    {
        Debug.Log("Stopping streaming thread...");
        isStreaming = false;
        if (streamingThread != null && streamingThread.IsAlive)
            streamingThread.Join();
        Debug.Log("Streaming thread stopped.");
    }

    private string GetBoundary(string contentType)
    {
        Debug.Log($"Full Content-Type Header: {contentType}");
        string[] parts = contentType.Split(';');
        foreach (string part in parts)
        {
            if (part.Trim().StartsWith("boundary="))
            {
                string boundary = part.Trim().Substring("boundary=".Length).Trim('"', ' ');
                Debug.Log($"Boundary extracted: {boundary}");
                return boundary;
            }
        }
        Debug.LogWarning("No boundary found in Content-Type header.");
        return null;
    }

    private int FindSequence(byte[] source, byte[] pattern, int start = 0, int? end = null)
    {
        int endIndex = end ?? source.Length;
        for (int i = start; i <= endIndex - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (source[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                Debug.Log($"Pattern found at index: {i}");
                return i;
            }
        }
        Debug.Log("Pattern not found in source.");
        return -1;
    }
}

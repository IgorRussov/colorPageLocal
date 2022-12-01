using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class CameraSnapshotSaver : MonoBehaviour
{
    public Vector2Int resolution;

#if UNITY_EDITOR

    public void RenderCameraToAsset(string path)
    {
        string saveFullPath = Path.Combine("Assets", "Resources", "LevelImages", path);
        string saveResourcePath = Path.Combine("LevelImages", path);
        saveResourcePath = saveResourcePath.Substring(0, saveResourcePath.Length - 4);

        Camera camera = gameObject.GetComponent<Camera>();

        RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        
        RenderTexture oldRT = camera.targetTexture;
        camera.targetTexture = rt;
        camera.Render();
        camera.targetTexture = oldRT;

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(saveFullPath, bytes);
        AssetDatabase.ImportAsset(saveFullPath);

        
        //Object obj = Resources.Load(saveResourcePath);

        //return obj as Texture2D;
    }
#endif

    public Texture2D RenderCameraToSaveLevel(string levelName)
    {
        string path = Path.Combine(Application.persistentDataPath, levelName + "_complete.png");

        Camera camera = gameObject.GetComponent<Camera>();

        RenderTexture rt = new RenderTexture(resolution.x * 2, resolution.y * 2, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

        RenderTexture oldRT = camera.targetTexture;
        camera.targetTexture = rt;
        camera.Render();
        camera.targetTexture = oldRT;

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width / 2, rt.height / 2, TextureFormat.ARGB32, false);
        Rect readPixelsRect = new Rect(rt.width / 4, rt.height / 4, rt.width / 2, rt.height / 2);
        tex.ReadPixels(readPixelsRect, 0, 0);
        RenderTexture.active = null;

        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);

        return FileIO.LoadTextureForLevel(levelName);
    }
}

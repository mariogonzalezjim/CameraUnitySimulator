using UnityEngine;
using System.Collections;
using System;


public struct MinimapSceneInfo
{
    public float xmin, xmax, zmin, zmax, floor, ceiling;
    public string minimapImage;

}


public class AutoMinimap : MonoBehaviour
{

    Camera camaraUnityObject;
    GameObject plane;
    GameObject ceiling;
    float xlimite, zlimite, xcamara, zcamara;
    bool flagposition = false, flagimage = false;
    byte[] imagen = new byte[10];
    public float scaleImageMap = 1.5f;
    MinimapSceneInfo minimapInfo = new MinimapSceneInfo();

    public enum ImageFilterMode : int
    {
        Nearest = 0,
        Biliner = 1,
        Average = 2
    }

    // Use this for initialization
    void Start()
    {



        //disable ceiling
        try
        {
            ceiling = GameObject.Find("Ceiling");
            ceiling.SetActive(false);
        }
        catch (Exception ex)
        {

        }

        //find the unity object(a camera) which this script is attachment
        camaraUnityObject = GameObject.Find(gameObject.name).GetComponent<Camera>();
        camaraUnityObject.orthographicSize += 400;

        plane = GameObject.Find("LimitScenario");

        //primero vamos a centrar la camara al plano
        camaraUnityObject.transform.position = new Vector3(plane.transform.position.x, camaraUnityObject.transform.position.y, plane.transform.position.z);


        //ahora vamos a buscar los puntos limite del escenario
        MeshRenderer escenario = plane.GetComponent<MeshRenderer>();
        xlimite = escenario.bounds.min.x;
        zlimite = escenario.bounds.min.z;

        //zona de vision de la camara 
        Vector3 p = camaraUnityObject.ViewportToWorldPoint(new Vector3(0, 0, camaraUnityObject.nearClipPlane));
        xcamara = p.x;
        zcamara = p.z;

        //coordinates of floor
        GameObject floorMarker = GameObject.Find("LimitFloor");
        minimapInfo.floor = floorMarker.transform.localPosition.y;

        //coodinates of ceiling
        GameObject ceilingMarker = GameObject.Find("LimitCeiling");
        minimapInfo.ceiling = ceilingMarker.transform.localPosition.y;



    }

    // Update is called once per frame
    void Update()
    {
        //    camaraUnityObject.transform.rotation.eulerAngles.Set(90, 0, 0);
        int contador = 0;
        while (contador < 100 && flagposition == false)
        {
            if (xcamara < xlimite && zcamara < zlimite)
            {
                camaraUnityObject.orthographicSize += -0.1f;
            }
            else {
                flagposition = true;
            }

            //actualizar puntos zona camara
            Vector3 p = camaraUnityObject.ViewportToWorldPoint(new Vector3(0, 0, camaraUnityObject.nearClipPlane));
            xcamara = p.x;
            zcamara = p.z;
            contador++;
        }

        if (flagposition == true && flagimage == false)
        {
            //take minimap image
            RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
            camaraUnityObject.targetTexture = rt;
            Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            camaraUnityObject.Render();
            RenderTexture.active = rt;

            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            camaraUnityObject.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            RenderTexture.ReleaseTemporary(rt);

            Texture2D screenShot2 = ResizeTexture(screenShot, ImageFilterMode.Average, scaleImageMap);
            byte[] imagen2 = screenShot2.EncodeToJPG(90);

            //prepare Minimap struct
            minimapInfo.minimapImage = Convert.ToBase64String(imagen2);
            //coordinates of image 
            Vector3 p = camaraUnityObject.ViewportToWorldPoint(new Vector3(0, 0, camaraUnityObject.nearClipPlane));
            xcamara = p.x;
            zcamara = p.z;
            minimapInfo.xmin = p.x;
            minimapInfo.zmin = p.z;
            p = camaraUnityObject.ViewportToWorldPoint(new Vector3(1, 1, camaraUnityObject.nearClipPlane));
            minimapInfo.xmax = p.x;
            minimapInfo.zmax = p.z;
            //send struct to command gestor 
            GameObject multiCameraSimulator = GameObject.Find("MultiCameraSimulator");
            StartApp app = multiCameraSimulator.GetComponent<StartApp>();
            app.addMiniMapInfoStruct(minimapInfo);
            //disable camera
            gameObject.SetActive(false);
            flagimage = true;

            //active ceiling
            if (ceiling != null)
                ceiling.SetActive(true);



        }

    }

    public static Texture2D ResizeTexture(Texture2D pSource, ImageFilterMode pFilterMode, float pScale)
    {

        //*** Variables
        int i;

        //*** Get All the source pixels
        Color[] aSourceColor = pSource.GetPixels(0);
        Vector2 vSourceSize = new Vector2(pSource.width, pSource.height);

        //*** Calculate New Size
        float xWidth = Mathf.RoundToInt((float)pSource.width * pScale);
        float xHeight = Mathf.RoundToInt((float)pSource.height * pScale);

        //*** Make New
        Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

        //*** Make destination array
        int xLength = (int)xWidth * (int)xHeight;
        Color[] aColor = new Color[xLength];

        Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

        //*** Loop through destination pixels and process
        Vector2 vCenter = new Vector2();
        for (i = 0; i < xLength; i++)
        {

            //*** Figure out x&y
            float xX = (float)i % xWidth;
            float xY = Mathf.Floor((float)i / xWidth);

            //*** Calculate Center
            vCenter.x = (xX / xWidth) * vSourceSize.x;
            vCenter.y = (xY / xHeight) * vSourceSize.y;

            //*** Do Based on mode
            //*** Nearest neighbour (testing)
            if (pFilterMode == ImageFilterMode.Nearest)
            {

                //*** Nearest neighbour (testing)
                vCenter.x = Mathf.Round(vCenter.x);
                vCenter.y = Mathf.Round(vCenter.y);

                //*** Calculate source index
                int xSourceIndex = (int)((vCenter.y * vSourceSize.x) + vCenter.x);

                //*** Copy Pixel
                aColor[i] = aSourceColor[xSourceIndex];
            }

            //*** Bilinear
            else if (pFilterMode == ImageFilterMode.Biliner)
            {

                //*** Get Ratios
                float xRatioX = vCenter.x - Mathf.Floor(vCenter.x);
                float xRatioY = vCenter.y - Mathf.Floor(vCenter.y);

                //*** Get Pixel index's
                int xIndexTL = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                int xIndexTR = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));
                int xIndexBL = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                int xIndexBR = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));

                //*** Calculate Color
                aColor[i] = Color.Lerp(
                    Color.Lerp(aSourceColor[xIndexTL], aSourceColor[xIndexTR], xRatioX),
                    Color.Lerp(aSourceColor[xIndexBL], aSourceColor[xIndexBR], xRatioX),
                    xRatioY
                );
            }

            //*** Average
            else if (pFilterMode == ImageFilterMode.Average)
            {

                //*** Calculate grid around point
                int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
                int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
                int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
                int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

                //*** Loop and accumulate
                //Vector4 oColorTotal = new Vector4();
                Color oColorTemp = new Color();
                float xGridCount = 0;
                for (int iy = xYFrom; iy < xYTo; iy++)
                {
                    for (int ix = xXFrom; ix < xXTo; ix++)
                    {

                        //*** Get Color
                        oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];

                        //*** Sum
                        xGridCount++;
                    }
                }

                //*** Average Color
                aColor[i] = oColorTemp / (float)xGridCount;
            }
        }

        //*** Set Pixels
        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        //*** Return
        return oNewTex;
    }

    public string getMinimapImage() { return Convert.ToBase64String(imagen); }
}

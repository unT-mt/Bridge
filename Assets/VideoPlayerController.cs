using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoPlayerController : MonoBehaviour
{
    public string videoPath = "file://C:/Users/m-takashima/Desktop/Sample/000.mp4";
    public RenderTexture renderTexture;

    public string imagePath001 = "file://C:/Users/m-takashima/Desktop/Sample/001.png";
    public string imagePath002 = "file://C:/Users/m-takashima/Desktop/Sample/002.png";
    public string imagePath003 = "file://C:/Users/m-takashima/Desktop/Sample/003.png";

    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private Texture2D imageTexture;

    void Start()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        rawImage = gameObject.GetComponent<RawImage>();
        imageTexture = new Texture2D(2, 2);

        // 動画の再生終了時のコールバックを設定
        videoPlayer.loopPointReached += OnVideoEnd;

        PlayVideo();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            DisplayImage(imagePath001);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            DisplayImage(imagePath002);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            DisplayImage(imagePath003);
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            PlayVideo();
        }
    }

    private void PlayVideo()
    {
        rawImage.texture = renderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.url = videoPath;
        videoPlayer.Play();
    }

    private void DisplayImage(string imagePath)
    {
        StartCoroutine(LoadImage(imagePath));
        videoPlayer.Stop();
    }

    private IEnumerator LoadImage(string imagePath)
    {
        using (WWW www = new WWW(imagePath))
        {
            yield return www;
            www.LoadImageIntoTexture(imageTexture);
            rawImage.texture = imageTexture;
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        PlayVideo(); // 動画再生終了時に再度再生
    }
}

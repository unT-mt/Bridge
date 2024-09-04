using UnityEngine;
using UnityEngine.Video;

public class LocalVideoPlayer : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // 動画ファイルをResourcesフォルダからロード
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "SampleMovie.mp4");

        // 動画の準備が完了したら再生を開始
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += VideoPlayerPrepareCompleted;
    }

    private void VideoPlayerPrepareCompleted(VideoPlayer vp)
    {
        vp.Play();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoPlayerController : MonoBehaviour
{
    //ファイル名の指定（現状は仮でpublicで指定）
    public string videoPath = "Sample/000.mp4";
    public string imagePath001 = "Sample/001.png";
    public string imagePath002 = "Sample/002.png";
    public string imagePath003 = "Sample/003.png";

    //動画と画像を表示するレンダーテクスチャを指定
    public RenderTexture renderTexture;

    //フェードの持続時間を指定
    public float fadeDuration = 1.0f;

    //自オブジェクトにアタッチするコンポーネント
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private Texture2D imageTexture;

    void Start()
    {
        //システムからパスを取得（現状は仮でDesktopを指定）
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        Debug.Log("Desktop Path: " + desktopPath);

        // パスを結合してフルパスを生成
        videoPath = "file://" + System.IO.Path.Combine(desktopPath, videoPath);
        imagePath001 = "file://" + System.IO.Path.Combine(desktopPath, imagePath001);
        imagePath002 = "file://" + System.IO.Path.Combine(desktopPath, imagePath002);
        imagePath003 = "file://" + System.IO.Path.Combine(desktopPath, imagePath003);

        //コンポーネントの取得
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        rawImage = gameObject.GetComponent<RawImage>();
        imageTexture = new Texture2D(2, 2);

        // 動画の再生終了時のコールバックを設定
        videoPlayer.loopPointReached += OnVideoEnd;

        //初回起動時はビデオを再生
        PlayVideo();
    }

    void Update()
    {
        //キーに対応した処理（現状は仮。今後キーマッピング追加予定（戻る/進む/Topへ））
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(SwitchToImage(imagePath001));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(SwitchToImage(imagePath002));
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(SwitchToImage(imagePath003));
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(SwitchToVideo());
        }
    }

    private IEnumerator SwitchToImage(string imagePath)
    {
        yield return StartCoroutine(FadeOut());

        // 画像の表示
        videoPlayer.Stop();
        StartCoroutine(LoadImage(imagePath));

        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator SwitchToVideo()
    {
        yield return StartCoroutine(FadeOut());

        // 動画の再生
        PlayVideo();

        yield return StartCoroutine(FadeIn());
    }

        private IEnumerator FadeOut()
    {
        Color color = rawImage.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            color.a = Mathf.Lerp(1, 0, normalizedTime);
            rawImage.color = color;
            yield return null;
        }
        color.a = 0;
        rawImage.color = color;
    }

    private IEnumerator FadeIn()
    {
        Color color = rawImage.color;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;
            color.a = Mathf.Lerp(0, 1, normalizedTime);
            rawImage.color = color;
            yield return null;
        }
        color.a = 1;
        rawImage.color = color;
    }

    /// <summary>
    /// レンダーテクスチャを利用してビデオを再生する
    /// 未実装：Vキー押下時、動画再生中なら動画の再度再生をしない
    /// 未実装：テキストファイルを生成し入力デバイスのボタンを光らせる
    /// </summary>
    private void PlayVideo()
    {
        rawImage.texture = renderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.url = videoPath;
        videoPlayer.Play();
    }

    /// <summary>
    /// 画像を表示する
    /// 未実装：表示切替時の効果音
    /// 未実装：所定の時間経過時にTopに戻る（所定の時間はconfigテキストで指定するようにする）
    /// </summary>
    /// <param name="imagePath"></param>
    private void DisplayImage(string imagePath)
    {
        StartCoroutine(LoadImage(imagePath));
        videoPlayer.Stop();
    }

    /// <summary>
    /// 画像の取得
    /// 現状仮のコーディングのため一旦wwwクラスで動くことを確認
    /// Web通信が不要の場合File.ReadAllBytesで動かすこと検証する
    /// newは極力減らしていく（メモリリーク対策）
    /// </summary>
    /// <param name="imagePath"></param>
    /// <returns></returns>
    private IEnumerator LoadImage(string imagePath)
    {
        using (WWW www = new WWW(imagePath))
        {
            yield return www;
            www.LoadImageIntoTexture(imageTexture);
            rawImage.texture = imageTexture;
        }
    }

    /// <summary>
    /// ビデオの終了時の処理（再度ビデオを再生）
    /// </summary>
    /// <param name="vp"></param>
    private void OnVideoEnd(VideoPlayer vp)
    {
        // 動画再生終了時に再度再生
        StartCoroutine(SwitchToVideo());
    }
}

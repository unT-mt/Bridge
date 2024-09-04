using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

//各ファイルが持つアトリビュート
public class ContentAttributes
{
    //Top動画であればtrue、それ以外の画像はfalse
    public bool Top { get; set; }

    //画像のうち、F, G, H, I, J, Kどのカテゴリに対応したコンテンツか
    public string Category { get; set; }

    //画像のうち、各カテゴリの何番目の画像か
    public string Sequence { get; set; }
}

public class ContentManager : MonoBehaviour
{
    //動画と画像を表示するレンダーテクスチャを指定
    public RenderTexture renderTexture;

    //フェードの持続時間を指定
    public float fadeDuration = 1.0f;

    //自オブジェクトにアタッチするUnityコンポーネント
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private Texture2D imageTexture;
    private AudioSource audioSource;

    //ファイルをリストで管理し、同時に現在のインデックスを保持する
    private List<ContentAttributes> contentList;
    private int currentIndex = 0;

    //タイムアウトの処理のための変数
    public float timeoutDuration = 60f;
    private float displayTimer;

    void Start()
    {
        //所定のフォルダからコンテンツをロードする
        LoadContentFiles();
        
        //コンポーネントの取得
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        rawImage = gameObject.GetComponent<RawImage>();
        imageTexture = new Texture2D(2, 2);
        audioSource = gameObject.GetComponent<AudioSource>();

        // 動画の再生終了時のコールバックを設定
        videoPlayer.loopPointReached += OnVideoEnd;

         //初回起動時
        InitializeDisplay();
    }

    void Update()
    {
        //タイマーを用意しタイムアウト時にTopへ遷移
        displayTimer += Time.deltaTime;
        if (displayTimer >= timeoutDuration && !contentList[currentIndex].Top)
        {
            SwitchToTop();
        }
        //物理ボタンに応じた処理
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlaySound();
            //現在表示しているコンテンツがTopでないなら、次のコンテンツを表示
            if (currentIndex != 0) SwitchContent("J1", "next");
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            PlaySound();
            //現在表示しているコンテンツがTopでないなら、前のコンテンツを表示
            if (currentIndex != 0) SwitchContent("J1", "previous");
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            PlaySound();
            //Topへ遷移
            SwitchToTop();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            PlaySound();
            SwitchContent("J1", "first");
            videoPlayer.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            PlaySound();
            SwitchContent("J2", "first");
            videoPlayer.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            PlaySound();
            SwitchContent("J3", "first");
            videoPlayer.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            PlaySound();
            SwitchContent("E1", "first");
            videoPlayer.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            PlaySound();
            SwitchContent("E2", "first");
            videoPlayer.Stop();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            PlaySound();
            SwitchContent("E3", "first");
            videoPlayer.Stop();
        }
    }

    /// <summary>
    /// 動画と画像が格納されたフォルダにアクセス
    /// ファイルの個数の長さのリストを作成しアトリビュートを格納
    /// </summary>
    private void LoadContentFiles()
    {   
        //リストを作成しファイルごとのアトリビュートを設定できるようにする
        contentList = new List<ContentAttributes>();

        //システムからパスを取得
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string[] files = Directory.GetFiles(Path.Combine(desktopPath, "Sample"));

        foreach (var file in files)
        {
            var attr = new ContentAttributes
            {
                Top = Path.GetFileName(file) == "000.mp4",
                Category = Path.GetFileName(file).Substring(0, 2),
                Sequence = Path.GetFileName(file).Substring(3, 1)
            };
            contentList.Add(attr);
        }
    }

    /// <summary>
    /// 初期表示
    /// </summary>
    private void InitializeDisplay()
    {
        currentIndex = contentList.FindIndex(c => c.Top);
        if (currentIndex != -1)
        {
            PlayContent(currentIndex);
        }
    }

    /// <summary>
    /// 表示コンテンツを遷移させる
    /// 未対応：各カテゴリの最初/最後のコンテンツからTopに遷移させる
    /// （現状、例えばJ1の最後のコンテンツが表示されている場合次に遷移するとJ2の最初が表示される）
    /// </summary>
    private void SwitchContent(string category, string sequenceType)
    {
        int index = -1;
        switch (sequenceType)
        {
            case "first":
                index = contentList.FindIndex(c => c.Category == category && c.Sequence == "1");
                break;
            case "previous":
                index = currentIndex - 1;
                if (index < 0) index = contentList.Count - 1; // 現状は循環する実装
                break;
            case "next":
                index = currentIndex + 1;
                if (index >= contentList.Count) index = 0; // 現状は循環する実装
                break;
        }

        if (index != -1 && index != currentIndex)
        {
            Debug.Log(index);
            StartCoroutine(FadeTransition(() => PlayContent(index)));
        }
    }

    /// <summary>
    /// Top表示でない場合、Topに遷移
    /// </summary>
    private void SwitchToTop()
    {
        int index = contentList.FindIndex(c => c.Top);
        if (index != -1 && index != currentIndex)
        {
            StartCoroutine(FadeTransition(() => PlayContent(index)));
        }
    }

    /// <summary>
    /// フォルダにアクセスし対応するコンテンツをロードする
    /// </summary>
    private void PlayContent(int index)
    {
        currentIndex = index;
        displayTimer = 0f;

        var content = contentList[index];
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if (content.Top)
        {
            string videoPath = "file://" + Path.Combine(desktopPath, "Sample", "000.mp4");
            videoPlayer.url = videoPath;
            videoPlayer.targetTexture = renderTexture;
            rawImage.texture = renderTexture;
            videoPlayer.Play();
        }
        else
        {
            string imagePath = "file://" + Path.Combine(desktopPath, "Sample", content.Category + "-"+ content.Sequence + ".png");
            byte[] imageBytes = File.ReadAllBytes(imagePath.Substring(7)); // "file://"を除外
            imageTexture.LoadImage(imageBytes);
            rawImage.texture = imageTexture;
        }
    }

    /// <summary>
    /// フェードイン/アウトを担う処理
    /// </summary>
    private IEnumerator FadeTransition(Action onComplete)
    {
        yield return StartCoroutine(Fade(fadeDuration, 0));
        onComplete();
        yield return StartCoroutine(Fade(0, fadeDuration));
    }

    /// <summary>
    /// フェード自体の処理（rawImageのアルファを変更する）
    /// </summary>
    private IEnumerator Fade(float from, float to)
    {
        float duration = 0.5f;
        float counter = 0f;

        CanvasGroup canvasGroup = rawImage.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rawImage.gameObject.AddComponent<CanvasGroup>();
        }

        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, counter / duration);
            yield return null;
        }
    }

    /// <summary>
    /// ビデオの終了時の処理（再度ビデオを再生）
    /// </summary>
    private void OnVideoEnd(VideoPlayer vp)
    {
        // 動画再生終了時に再度再生
        videoPlayer.Play();
    }

    private void PlaySound()
    {
        // 効果音を再生
        audioSource.Play();
    }
}

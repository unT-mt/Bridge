using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    // Jsonファイル上書きのためのデリゲートの定義
    public Action OnContentChanged;

    //動画と画像を表示するレンダーテクスチャを指定
    public RenderTexture renderTexture;

    //フェードの持続時間を指定
    [ReadOnly]public float fadeDuration = 1.0f;

    //自オブジェクトにアタッチするUnityコンポーネント
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private Texture2D imageTexture;
    private AudioSource audioSource;

    //ファイルをリストで管理し、同時に現在のインデックスを保持する
    private List<ContentAttributes> contentList;
    private int currentIndex = 0;
    [ReadOnly]public string currentCategory = "00"; 
    [ReadOnly]public string currentSequence = "00"; 
    [ReadOnly]public string currentSequenceState = "none"; 

    //タイムアウトの処理のための変数
    [ReadOnly]public float timeoutDuration = 60f;
    private float displayTimer;

    //フェード処理のための変数
    private bool isFading = false;

    void Start()
    {
        //configファイルのロード
        LoadConfigFile(); 

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

        // Jsonファイルの出力
        ConvertButtonStateToJsonFile();

        foreach(var content in contentList)
        {
            Debug.Log(content.Category);
            Debug.Log(content.Sequence);
        }
    }

    void Update()
    {
        // タイマーを用意しタイムアウト時にTopへ遷移
        displayTimer += Time.deltaTime;

        if (!isFading)
        {
            // タイムアウト処理
            if (displayTimer >= timeoutDuration && !contentList[currentIndex].Top)
            {
                SwitchToTop();
            }

            // キーとカテゴリの対応を辞書で管理
            var keyToCategory = new Dictionary<KeyCode, string>
            {
                { KeyCode.F, "t_p_jp" },
                { KeyCode.G, "t_p_en" },
                { KeyCode.H, "t_r_jp" },
                { KeyCode.J, "t_r_en" },
                { KeyCode.K, "t_u_jp" },
                { KeyCode.L, "t_u_en" }
            };

            // M, B, Nキーの処理
            HandleNavigationKeys();

            // カテゴリに応じたキー入力の処理
            foreach (var entry in keyToCategory)
            {
                if (Input.GetKeyDown(entry.Key))
                {
                    PlaySound();
                    SwitchCategory(entry.Value);
                    break;
                }
            }
        }
    }

    // M, B, Nキーのナビゲーション処理
    void HandleNavigationKeys()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlaySound();
            if (currentIndex != 0) SwitchContent(currentCategory, "next");
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            PlaySound();
            if (currentIndex != 0) SwitchContent(currentCategory, "previous");
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            PlaySound();
            SwitchToTop();
        }
    }

    // カテゴリの切り替え処理
    void SwitchCategory(string newCategory)
    {
        if (currentCategory != newCategory)
        {
            currentCategory = newCategory;
            SwitchContent(currentCategory, "first");
        }
        currentSequenceState = "first";
        videoPlayer.Stop();
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
        string[] files = Directory.GetFiles(Path.Combine(desktopPath, "wwo_table/Assets"));

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var attr = new ContentAttributes();

            // 動画ファイルか画像ファイルかを判定
            if (fileName == "t_s.mp4")
            {
                // 動画ファイルの場合の処理
                attr.Top = true;
                attr.Category = "00";
                attr.Sequence = "00";
            }
            else if (fileName.Length >= 11) // 画像ファイルの場合の処理（最低限の長さを持つファイル名）
            {
                attr.Top = false;
                attr.Category = fileName.Substring(0, 6); // t_p_en, t_p_jp, etc.
                attr.Sequence = fileName.Substring(7, 2); // 01, 02, etc.
            }
            else
            {
                Debug.LogWarning("Unexpected file name format: " + fileName);
                continue; // ファイル名が予期しない形式の場合はスキップ
            }

            contentList.Add(attr);
        }

        contentList = contentList
            .OrderBy(c => c.Top ? "0" : c.Category)  // Top (t_s.mp4) が先頭に来るようにする
            .ThenBy(c => c.Sequence)
            .ToList();
    }


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
    /// </summary>
    private void SwitchContent(string category, string sequenceType)
    {
        int index = -1;
        
        currentSequence = contentList[currentIndex].Sequence;

        int maxSequence = contentList
            .Where(c => c.Category == category)
            .Select(c => int.Parse(c.Sequence))
            .DefaultIfEmpty(0)  // 空の場合のデフォルト値を 0 に設定
            .Max();

        switch (sequenceType)
        {
            //最初のコンテンツに遷移
            case "first":
                index = contentList.FindIndex(c => c.Category == category && c.Sequence == "01");
                break;

            // 前のコンテンツに遷移
            case "previous":
                // 現在がシーケンス1なら何もしない（先頭なので戻れない）
                if (currentSequence == "01")
                {
                    Debug.Log("Already at the first content, cannot go back.");
                    return; 
                }
                // それ以外は1つ前に移動
                index = currentIndex - 1;
                break;

            // 次のコンテンツに遷移する場合
            case "next":
                if (int.Parse(currentSequence) == maxSequence) 
                {
                    Debug.Log("Already at the last content, cannot go next.");
                    return; 
                }
                index = currentIndex + 1;
                currentCategory = contentList[index].Category;
                break;
        }

        if (index != -1 && index != currentIndex)
        {
            StartCoroutine(FadeTransition(() =>
            {
                PlayContent(index);
                AfterPlayContent(currentCategory);
                ConvertButtonStateToJsonFile();
            }));
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
            StartCoroutine(FadeTransition(() =>
            {
                PlayContent(index);
                currentSequenceState = "none";
                ConvertButtonStateToJsonFile();
            }));
        }
        currentSequence = "00";
        currentCategory = "00";
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
            string videoPath = "file://" + Path.Combine(desktopPath, "wwo_table/Assets", "t_s.mp4");
            videoPlayer.url = videoPath;
            videoPlayer.targetTexture = renderTexture;
            rawImage.texture = renderTexture;
            videoPlayer.Play();
        }
        else
        {
            string imagePath = "file://" + Path.Combine(desktopPath, "wwo_table/Assets", content.Category + "_" + content.Sequence + ".png");
            byte[] imageBytes = File.ReadAllBytes(imagePath.Substring(7));
            imageTexture.LoadImage(imageBytes);
            rawImage.texture = imageTexture;
        }
    }

    /// <summary>
    /// PlayContentが終了した後の処理
    /// </summary>
    private void AfterPlayContent(string category)
    {
        currentSequence = contentList[currentIndex].Sequence;

        int maxSequence = contentList
            .Where(c => c.Category == category)
            .Max(c => int.Parse(c.Sequence));

        int intCurrentSequence = Convert.ToInt32(currentSequence);

        if (intCurrentSequence == maxSequence)
        {
            currentSequenceState = "last";
        }
        else if (intCurrentSequence == 1)
        {
            currentSequenceState = "first";
        }
        else
        {
            currentSequenceState = "mid";
        }

        Debug.Log(currentSequenceState);
    }

    /// <summary>
    /// フェードイン/アウトを担う処理
    /// </summary>
    private IEnumerator FadeTransition(Action onComplete)
    {
        isFading = true;
        yield return StartCoroutine(Fade(fadeDuration, 0));
        onComplete();
        yield return StartCoroutine(Fade(0, fadeDuration));
        isFading = false;
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

    // コンテンツが変更された際に呼び出される
    private void ConvertButtonStateToJsonFile()
    {
        OnContentChanged?.Invoke();  // デリゲートの呼び出し
    }

    private void LoadConfigFile()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string configFilePath = Path.Combine(desktopPath, "wwo_table", "config.txt");

            if (File.Exists(configFilePath))
            {
                string[] lines = File.ReadAllLines(configFilePath);

                foreach (var line in lines)
                {
                    if (line.StartsWith("fadeDuration"))
                    {
                        string fadeValue = line.Split('=')[1].Trim().Replace("f", "");
                        fadeDuration = float.Parse(fadeValue);
                    }
                    else if (line.StartsWith("timeoutDuration"))
                    {
                        string timeoutValue = line.Split('=')[1].Trim().Replace("f", "");
                        timeoutDuration = float.Parse(timeoutValue);
                    }
                }

                Debug.Log($"Config loaded. fadeDuration: {fadeDuration}, timeoutDuration: {timeoutDuration}");
            }
            else
            {
                Debug.LogWarning("Config file not found. Using default values.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading config file: " + ex.Message);
        }
    }

}

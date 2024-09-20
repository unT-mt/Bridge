using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

//各ファイルが持つアトリビュート
public class WallContentAttributes
{
    //Top動画であればtrue、それ以外の画像はfalse
    public bool Top { get; set; }

    //Top動画でないならF, G, H, J, K, Lどのカテゴリに対応したコンテンツか
    public string Category { get; set; }

    //画像のうち、各カテゴリの何番目の画像か
    public string Sequence { get; set; }
}

public class WallContentManager : MonoBehaviour
{



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
    private List<WallContentAttributes> contentList;
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
                { KeyCode.F, "w_p_jp" },
                { KeyCode.G, "w_p_en" },
                { KeyCode.H, "w_r_jp" },
                { KeyCode.J, "w_r_en" },
                { KeyCode.K, "w_u_jp" },
                { KeyCode.L, "w_u_en" }
            };

            // Nキーの処理
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

    /// <summary>
    /// M, B, Nキーのナビゲーション処理
    /// </summary>
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

    /// <summary>
    /// カテゴリの切り替え処理
    /// </summary>
    void SwitchCategory(string newCategory)
    {
        if (currentCategory != newCategory)
        {
            currentCategory = newCategory;
            SwitchContent(currentCategory, "first");
        }

    }

    /// <summary>
    /// 動画と画像が格納されたフォルダにアクセス
    /// ファイルの個数の長さのリストを作成しアトリビュートを格納
    /// Wall固有の処理：仮想のSequenceを設定しTableとの制御の整合を取る
    /// </summary>
    private void LoadContentFiles()
    {   
        //リストを作成しファイルごとのアトリビュートを設定できるようにする
        contentList = new List<WallContentAttributes>();

        //システムからパスを取得
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string[] files = Directory.GetFiles(Path.Combine(desktopPath, "wwo_wall/Assets"));

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);


            // 動画ファイルか画像ファイルかを判定
            if (fileName == "w_s.mp4")
            {
                var attr = new WallContentAttributes();
                // 動画ファイルの場合の処理
                attr.Top = true;
                attr.Category = "00";
                attr.Sequence = "00";
                contentList.Add(attr);
            }
            else if (fileName.Length >= 8) // 画像ファイルの場合の処理（最低限の長さを持つファイル名）
            {
                if(fileName.Substring(0, 6) == "w_p_jp") 
                {
                    for(int i = 1; i <= 5; i++) 
                    {
                        var attr = new WallContentAttributes();
                        attr.Top = false;
                        attr.Category = "w_p_jp";
                        attr.Sequence = i.ToString("D2");
                        contentList.Add(attr);
                    }
                }
                else if(fileName.Substring(0, 6) == "w_p_en") 
                {
                    for(int i = 1; i <= 5; i++) 
                    {
                        var attr = new WallContentAttributes();
                        attr.Top = false;
                        attr.Category = "w_p_en";
                        attr.Sequence = i.ToString("D2");
                        contentList.Add(attr);
                    }
                }
                else if(fileName.Substring(0, 6) == "w_r_jp") 
                {
                    for(int i = 1; i <= 2; i++) 
                    {
                        var attr = new WallContentAttributes();
                        attr.Top = false;
                        attr.Category = "w_r_jp";
                        attr.Sequence = i.ToString("D2");
                        contentList.Add(attr);
                    }
                }
                else if(fileName.Substring(0, 6) == "w_r_en") 
                {
                    for(int i = 1; i <= 2; i++) 
                    {
                        var attr = new WallContentAttributes();
                        attr.Top = false;
                        attr.Category = "w_r_en";
                        attr.Sequence = i.ToString("D2");
                        contentList.Add(attr);
                    }
                }
                else if(fileName.Substring(0, 6) == "w_u_jp") 
                {
                    for(int i = 1; i <= 3; i++) 
                    {
                        var attr = new WallContentAttributes();
                        attr.Top = false;
                        attr.Category = "w_u_jp";
                        attr.Sequence = i.ToString("D2");
                        contentList.Add(attr);
                    }
                }
                else if(fileName.Substring(0, 6) == "w_u_en") 
                {
                    for(int i = 1; i <= 3; i++) 
                    {
                        var attr = new WallContentAttributes();
                        attr.Top = false;
                        attr.Category = "w_u_en";
                        attr.Sequence = i.ToString("D2");
                        contentList.Add(attr);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Unexpected file name format: " + fileName);
                continue; // ファイル名が予期しない形式の場合はスキップ
            }


        }

        contentList = contentList
            .OrderBy(c => c.Top ? "0" : c.Category)  // Top (w_s.mp4) が先頭に来るようにする
            .ThenBy(c => c.Sequence)
            .ToList();
    }

    /// <summary>
    /// 表示の初期化
    /// </summary>
    private void InitializeDisplay()
    {
        currentIndex = contentList.FindIndex(c => c.Top);
        if (currentIndex != -1)
        {
            StartCoroutine(SwitchContentWithFadeOut(currentIndex));
        }
    }

    /// <summary>
    /// 表示コンテンツを遷移させる
    /// Wall固有の処理：firstとprevious/nextのコルーチン遷移先を分ける
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
                if (index != -1 && index != currentIndex)
                {
                    StartCoroutine(SwitchContentWithFadeOut(index));
                }
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
                if (index != -1 && index != currentIndex)
                {
                    StartCoroutine(SwitchIsFading(index));
                }
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
                if (index != -1 && index != currentIndex)
                {
                    StartCoroutine(SwitchIsFading(index));
                }
                break;
        }




    }
    
    /// <summary>
    /// フェードアウトを伴って遷移しコンテンツをロードする
    /// </summary>
    private IEnumerator SwitchContentWithFadeOut(int index)
    {
        isFading = true;
        displayTimer = 0f;
        Debug.Log("フェードアウトを開始します");
        yield return StartCoroutine(Fade(1, 0));

        Debug.Log("コンテンツをロードします");
        LoadContent(index);
    }

    /// <summary>
    /// ファイルをロードする
    /// Table固有の処理:ロードしたファイルにあわせてJsonを書き換える
    /// </summary>
    private async void LoadContent(int index)
    {
        currentIndex = index;
        displayTimer = 0f;

        var content = contentList[index];
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        if (content.Top)
        {
            string videoPath = "file://" + Path.Combine(desktopPath, "wwo_wall/Assets", "w_s.mp4");
            await StartVideoPlaybackAsync(videoPath);
        }
        else
        {
            string videoPath = "file://" + Path.Combine(desktopPath, "wwo_wall/Assets", content.Category + ".mp4");
            await StartVideoPlaybackAsync(videoPath);


        }
        Debug.Log("コンテンツのロードが完了しました");




        StartCoroutine(SwitchContentWithFadeIn(index));
    }

    private IEnumerator SwitchContentWithFadeIn(int index)
    {
        videoPlayer.Pause();
        
        Debug.Log("フェードインを開始します");
        yield return StartCoroutine(Fade(0, 1));
        // currentIndex = index;
        // currentSequence = contentList[currentIndex].Sequence;
        // Debug.Log(currentSequence);

        Debug.Log("コンテンツを再生します");
        PlayContent(index);
        isFading = false;
    }

    private void PlayContent(int index)
    {
        videoPlayer.Play();
    }

    // 非同期処理でビデオの準備を待つメソッド
    private async Task StartVideoPlaybackAsync(string videoPath)
    {
        videoPlayer.url = videoPath;
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        // 動画の準備が完了するまで待機
        videoPlayer.Prepare();

        // PrepareCompletedイベントの完了を非同期に待つ
        await WaitForVideoPrepared();

        Debug.Log("コンテンツの準備が完了しました");
    }

    // 動画の準備が完了するまで待つタスク
    private Task WaitForVideoPrepared()
    {
        var tcs = new TaskCompletionSource<bool>();

        void OnPrepared(VideoPlayer source)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            tcs.SetResult(true);
        }

        videoPlayer.prepareCompleted += OnPrepared;

        return tcs.Task;
    }

    /// <summary>
    /// Top表示でない場合、Topに遷移
    /// </summary>
    private void SwitchToTop()
    {
        displayTimer = 0f;
        int index = contentList.FindIndex(c => c.Top);
        if (index != -1 && index != currentIndex)
        {
            StartCoroutine(SwitchContentWithFadeOut(index));
        }
        currentSequence = "00";
        currentCategory = "00";

    }
    
    /// <summary>
    /// フェード自体の処理（rawImageのアルファを変更する）
    /// </summary>
    private IEnumerator Fade(float from, float to)
    {
        float duration = fadeDuration / 2;
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

    /// <summary>
    /// 効果音を再生（開発端末のみで利用）
    /// </summary>
    private void PlaySound()
    {
        // 効果音を再生
        audioSource.Play();
    }

    /// <summary>
    /// Wall固有の関数。
    /// TableのFadeと整合をとるための処理
    /// ダミーとしてFade(1,1)の処理が走っている
    /// </summary>
    private IEnumerator SwitchIsFading(int index)
    {
        isFading = true;
        displayTimer = 0f;
        Debug.Log("フェードアウトを開始します");

        float duration = fadeDuration / 2;
        float counter = 0f;

        CanvasGroup canvasGroup = rawImage.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rawImage.gameObject.AddComponent<CanvasGroup>();
        }

        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 1, counter / duration);
            yield return null;
        }

        Debug.Log("コンテンツをロードします");
        Debug.Log("フェードインを開始します");
        yield return StartCoroutine(Fade(1, 1));

        currentIndex = index;
        currentSequence = contentList[currentIndex].Sequence;
        Debug.Log(currentSequence);
        Debug.Log("isFadeを解除します");
        isFading = false;
    }

    /// <summary>
    /// コンフィグ用のテキストファイルを読み込み
    /// </summary>
    private void LoadConfigFile()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string configFilePath = Path.Combine(desktopPath, "wwo_wall", "config.txt");

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

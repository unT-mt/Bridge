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
    public float fadeDuration = 1.0f;

    //自オブジェクトにアタッチするUnityコンポーネント
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private Texture2D imageTexture;
    private AudioSource audioSource;

    //ファイルをリストで管理し、同時に現在のインデックスを保持する
    private List<ContentAttributes> contentList;
    private int currentIndex = 0;
    public string currentCategory = "00"; 
    public string currentSequence = "00"; 
    public string currentSequenceState ="none"; 

    //タイムアウトの処理のための変数
    public float timeoutDuration = 60f;
    private float displayTimer;

    //フェード処理のための変数
    private bool isFading = false;

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

        // foreach(var content in contentList)
        // {
        //     Debug.Log(content.Category);
        //     Debug.Log(content.Sequence);
        // }

        JsonChange();
    }

    void Update()
    {
        //タイマーを用意しタイムアウト時にTopへ遷移
        displayTimer += Time.deltaTime;

        if(!isFading)
        {
            if (displayTimer >= timeoutDuration && !contentList[currentIndex].Top)
            {
                SwitchToTop();
            }
            //物理ボタンに応じた処理
            if (Input.GetKeyDown(KeyCode.M))
            {
                PlaySound();
                //現在表示しているコンテンツがTopでないなら、次のコンテンツを表示
                if (currentIndex != 0) SwitchContent(currentCategory, "next");
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                PlaySound();
                //現在表示しているコンテンツがTopでないなら、前のコンテンツを表示
                if (currentIndex != 0) SwitchContent(currentCategory, "previous");
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
                if(currentCategory !="J1")
                {
                    currentCategory = "J1";
                    SwitchContent(currentCategory, "first");
                }
                currentSequenceState = "first";
                videoPlayer.Stop();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                PlaySound();
                if(currentCategory !="E1")
                {
                    currentCategory = "E1";
                    SwitchContent(currentCategory, "first");
                }
                currentSequenceState = "first";
                videoPlayer.Stop();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                PlaySound();
                if(currentCategory !="J2")
                {
                    currentCategory = "J2";
                    SwitchContent(currentCategory, "first");
                }
                currentSequenceState = "first";
                videoPlayer.Stop();
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                PlaySound();
                if(currentCategory !="E2")
                {
                    currentCategory = "E2";
                    SwitchContent(currentCategory, "first");
                }
                currentSequenceState = "first";
                videoPlayer.Stop();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                PlaySound();
                if(currentCategory !="J3")
                {
                    currentCategory = "J3";
                    SwitchContent(currentCategory, "first");
                }
                currentSequenceState = "first";
                videoPlayer.Stop();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                PlaySound();
                if(currentCategory !="E3")
                {
                    currentCategory = "E3";
                    SwitchContent(currentCategory, "first");
                }
                currentSequenceState = "first";
                videoPlayer.Stop();
            }
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
        string[] files = Directory.GetFiles(Path.Combine(desktopPath, "wwo/Assets"));

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
        
        currentSequence = contentList[currentIndex].Sequence;

        int maxSequence = contentList
            .Where(c => c.Category == category)
            .Max(c => 
            {
                int sequenceValue = int.Parse(c.Sequence);
                Debug.Log("Parsed Sequence: " + sequenceValue);
                return sequenceValue;
            });
        
        switch (sequenceType)
        {
            //最初のコンテンツに遷移
            case "first":
                index = contentList.FindIndex(c => c.Category == category && c.Sequence == "1");
                break;

            // 前のコンテンツに遷移
            case "previous":
                // 現在がシーケンス1なら何もしない（先頭なので戻れない）
                if (currentSequence == "1")
                {
                    Debug.Log("Already at the first content, cannot go back.");
                    return; 
                }
                // それ以外は1つ前に移動
                index = currentIndex - 1;
                break;

            // 次のコンテンツに遷移する場合
            case "next":
                // 現在のカテゴリ内で最大のSequenceを取得
                Debug.Log("Current category: " + category);

                foreach (var content in contentList.Where(c => c.Category == category))
                {
                    Debug.Log("Category: " + content.Category + ", Sequence: " + content.Sequence);
                }

                // 現在のSequenceがカテゴリ内で最大値なら遷移させない
                if (int.Parse(currentSequence) == maxSequence)
                {
                    Debug.Log("Already at the first content, cannot go back.");
                    return; 

                    // もしTopに遷移なら下記の処理
                    // index = contentList.FindIndex(c => c.Top); // Topコンテンツを表示
                    // //currentCategoryをデフォルトに
                    // currentCategory ="00";
                }
                else
                {
                    // それ以外なら次のSequenceに移動
                    index = currentIndex + 1;
                    //currentCategoryを変更する
                    currentCategory = contentList[index].Category;
                }
                break;
        }

        if (index != -1 && index != currentIndex)
        {
            Debug.Log(index);
            StartCoroutine(FadeTransition(() =>
            {
                PlayContent(index);
                AfterPlayContent(currentCategory);
                JsonChange();
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
                Debug.Log(currentSequenceState);
                JsonChange();
            }));
        }
        currentSequence ="00";
        currentCategory ="00";
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
            string videoPath = "file://" + Path.Combine(desktopPath, "wwo/Assets", "000.mp4");
            videoPlayer.url = videoPath;
            videoPlayer.targetTexture = renderTexture;
            rawImage.texture = renderTexture;
            videoPlayer.Play();
        }
        else
        {
            string imagePath = "file://" + Path.Combine(desktopPath, "wwo/Assets", content.Category + "-"+ content.Sequence + ".png");
            byte[] imageBytes = File.ReadAllBytes(imagePath.Substring(7)); // "file://"を除外
            imageTexture.LoadImage(imageBytes);
            rawImage.texture = imageTexture;
        }
    }
    private void AfterPlayContent(string category)
    {
        // PlayContentが終了した後の処理
        currentSequence = contentList[currentIndex].Sequence;

        int maxSequence = contentList
            .Where(c => c.Category == category)
            .Max(c => 
            {
                int sequenceValue = int.Parse(c.Sequence);
                Debug.Log("Parsed Sequence: " + sequenceValue);
                return sequenceValue;
            });
        
        int intCurrentSequence = 
            currentSequence == "."  // 意図しない文字列を取得したので応急処置。必ず修正する。
                ? 0 
                : Convert.ToInt32(currentSequence);

        if(intCurrentSequence == maxSequence)
        {
            currentSequenceState = "last";
        }
        else if(intCurrentSequence == 1)
        {
            currentSequenceState = "first";
        }
        else
        {
            currentSequenceState = "mid";
        };

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
    private void JsonChange()
    {
        OnContentChanged?.Invoke();  // デリゲートの呼び出し
    }
}

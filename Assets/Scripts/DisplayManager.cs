using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class DisplayManager : MonoBehaviour
{
    public string[] fileNames;
    private Dictionary<string, DisplayAttributes> displayAttributes;
    private string currentDisplay;
    private float timer;

    public RenderTexture renderTexture;
    public float fadeDuration = 1.0f;
    public AudioClip switchSound;
    public AudioSource audioSource;

    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private Texture2D imageTexture;

    void Start()
    {
        // ファイル名からアトリビュートを読み込み
        LoadFileAttributes();

        // コンポーネントの取得
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        rawImage = gameObject.GetComponent<RawImage>();
        imageTexture = new Texture2D(2, 2);

        // 初期表示
        DisplayInitialContent();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 60f && !displayAttributes[currentDisplay].Top)
        {
            SwitchToTop();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            PlaySwitchSound();
            HandleDisplayChange(1); // 次へ
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            PlaySwitchSound();
            HandleDisplayChange(-1); // 前へ
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            PlaySwitchSound();
            SwitchToTop();
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            PlaySwitchSound();
            SwitchToCategory("J1");
        }
        // 他のキーの処理も同様に追加
    }

    private void LoadFileAttributes()
    {
        displayAttributes = new Dictionary<string, DisplayAttributes>();

        foreach (string fileName in fileNames)
        {
            string category = fileName.Substring(0, 2); // 例: "J1"
            string sequence = fileName.Substring(2);    // 例: "-1"
            bool isTop = fileName.Contains("000.mp4");

            displayAttributes[fileName] = new DisplayAttributes(isTop, category, sequence);
        }
    }

    private void DisplayInitialContent()
    {
        currentDisplay = "000.mp4"; // 初期はトップ表示
        // 初期表示処理（動画再生や画像表示）
    }

    private void HandleDisplayChange(int direction)
    {
        // 現在の表示から次または前の表示に切り替える
        // 例えば、J1-1からJ1-2へ、または逆に切り替える処理
        // 表示切替とフェード処理
    }

    private void SwitchToTop()
    {
        // Topアトリビュートを持つコンテンツに切り替え
        currentDisplay = "000.mp4";
        // 表示切替とフェード処理
    }

    private void SwitchToCategory(string category)
    {
        // 指定されたカテゴリの先頭のコンテンツに切り替え
        foreach (var kvp in displayAttributes)
        {
            if (kvp.Value.Category == category && kvp.Value.Sequence == "1")
            {
                currentDisplay = kvp.Key;
                break;
            }
        }
        // 表示切替とフェード処理
    }

    private void PlaySwitchSound()
    {
        audioSource.PlayOneShot(switchSound);
    }

    private IEnumerator FadeTransition(Action onFadeComplete)
    {
        // フェードアウト処理
        yield return new WaitForSeconds(fadeDuration);
        onFadeComplete();
        // フェードイン処理
    }
}


public class DisplayAttributes
{
    public bool Top { get; private set; }
    public string Category { get; private set; }
    public string Sequence { get; private set; }

    public DisplayAttributes(bool top, string category, string sequence)
    {
        Top = top;
        Category = category;
        Sequence = sequence;
    }
}

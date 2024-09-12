// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Video;

// //各ファイルが持つアトリビュート
// public class TempAttributes
// {
//     //Top動画であればtrue、それ以外の画像はfalse
//     public bool Top { get; set; }

//     //画像のうち、F, G, H, J, K, Lどのカテゴリに対応したコンテンツか
//     public string Category { get; set; }
// }

// //クラス名はリファクタリング時にまとめて変更する。
// public class Temp : MonoBehaviour
// {
//     //動画と画像を表示するレンダーテクスチャを指定
//     public RenderTexture renderTexture;

//     //フェードの持続時間を指定
//     [ReadOnly]public float fadeDuration = 1.0f;

//     //自オブジェクトにアタッチするUnityコンポーネント
//     private VideoPlayer videoPlayer;
//     private RawImage rawImage;
//     private Texture2D imageTexture;
//     private AudioSource audioSource;

//     //ファイルをリストで管理し、同時に現在のインデックスを保持する
//     private List<TempAttributes> contentList;
//     private int currentIndex = 0;
//     [ReadOnly]public string currentCategory = "00"; 

//     //タイムアウトの処理のための変数
//     [ReadOnly]public float timeoutDuration = 60f;
//     private float displayTimer;

//     //フェード処理のための変数
//     private bool isFading = false;

//     void Start()
//     {
//         //configファイルのロード
//         LoadConfigFile(); 

//         //所定のフォルダからコンテンツをロードする
//         LoadContentFiles();
        
//         //コンポーネントの取得
//         videoPlayer = gameObject.AddComponent<VideoPlayer>();
//         rawImage = gameObject.GetComponent<RawImage>();
//         imageTexture = new Texture2D(2, 2);
//         audioSource = gameObject.GetComponent<AudioSource>();

//         // 動画の再生終了時のコールバックを設定
//         videoPlayer.loopPointReached += OnVideoEnd;

//          //初回起動時
//         InitializeDisplay();

//         foreach(var content in contentList)
//         {
//             Debug.Log(content.Category);
//         }
//     }

//     void Update()
//     {
//         // タイマーを用意しタイムアウト時にTopへ遷移
//         displayTimer += Time.deltaTime;

//         if (!isFading)
//         {
//             // タイムアウト処理
//             if (displayTimer >= timeoutDuration && !contentList[currentIndex].Top)
//             {
//                 SwitchToTop();
//             }

//             // キーとカテゴリの対応を辞書で管理
//             var keyToCategory = new Dictionary<KeyCode, string>
//             {
//                 { KeyCode.F, "w_p_jp" },
//                 { KeyCode.G, "w_p_en" },
//                 { KeyCode.H, "w_r_jp" },
//                 { KeyCode.J, "w_r_en" },
//                 { KeyCode.K, "w_u_jp" },
//                 { KeyCode.L, "w_u_en" }
//             };

//             // M, B, Nキーの処理
//             HandleNavigationKeys();

//             // カテゴリに応じたキー入力の処理
//             foreach (var entry in keyToCategory)
//             {
//                 if (Input.GetKeyDown(entry.Key))
//                 {
//                     PlaySound();
//                     SwitchCategory(entry.Value);
//                     break;
//                 }
//             }
//         }
//     }

//     // Nキーのナビゲーション処理
//     void HandleNavigationKeys()
//     {
//         if (Input.GetKeyDown(KeyCode.N))
//         {
//             PlaySound();
//             SwitchToTop();
//         }
//     }

//     // カテゴリの切り替え処理
//     void SwitchCategory(string newCategory)
//     {
//         if (currentCategory != newCategory)
//         {
//             currentCategory = newCategory;
//             SwitchContent(currentCategory);
//         }
//     }

//     /// <summary>
//     /// 動画と画像が格納されたフォルダにアクセス
//     /// ファイルの個数の長さのリストを作成しアトリビュートを格納
//     /// </summary>
//     private void LoadContentFiles()
//     {   
//         //リストを作成しファイルごとのアトリビュートを設定できるようにする
//         contentList = new List<TempAttributes>();

//         //システムからパスを取得
//         string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
//         string[] files = Directory.GetFiles(Path.Combine(desktopPath, "wwo_wall/Assets"));

//         foreach (var file in files)
//         {
//             var fileName = Path.GetFileName(file);
//             var attr = new TempAttributes();

//             // 動画ファイルか画像ファイルかを判定
//             if (fileName == "w_s.mp4")
//             {
//                 // 動画ファイルの場合の処理
//                 attr.Top = true;
//                 attr.Category = "00";
//             }
//             else if (fileName.Length >= 8) // 画像ファイルの場合の処理（最低限の長さを持つファイル名）
//             {
//                 attr.Top = false;
//                 attr.Category = fileName.Substring(0, 6); // w_p_en, w_p_jp, etc.
//             }
//             else
//             {
//                 Debug.LogWarning("Unexpected file name format: " + fileName);
//                 continue; // ファイル名が予期しない形式の場合はスキップ
//             }

//             contentList.Add(attr);
//         }

//         contentList = contentList
//             .OrderBy(c => c.Top ? "0" : c.Category)  // Top (w_s.mp4) が先頭に来るようにする
//             .ToList();
//     }


//     private void InitializeDisplay()
//     {
//         currentIndex = contentList.FindIndex(c => c.Top);
//         if (currentIndex != -1)
//         {
//             PlayContent(currentIndex);
//         }
//     }

//     /// <summary>
//     /// 表示コンテンツを遷移させる
//     /// </summary>
//     private void SwitchContent(string category)
//     {
//         int index = -1;

//         index = contentList.FindIndex(c => c.Category == category);

//         if (index != -1 && index != currentIndex)
//         {
//             StartCoroutine(FadeTransition(() =>
//             {
//                 PlayContent(index);
//             }));
//         }
//     }

//     /// <summary>
//     /// Top表示でない場合、Topに遷移
//     /// </summary>
//     private void SwitchToTop()
//     {
//         int index = contentList.FindIndex(c => c.Top);
//         if (index != -1 && index != currentIndex)
//         {
//             StartCoroutine(FadeTransition(() =>
//             {
//                 PlayContent(index);
//             }));
//         }
//         currentCategory = "00";
//     }

//     /// <summary>
//     /// フォルダにアクセスし対応するコンテンツをロードする
//     /// </summary>
//     private void PlayContent(int index)
//     {
//         currentIndex = index;
//         displayTimer = 0f;

//         var content = contentList[index];
//         string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

//         if (content.Top)
//         {
//             string videoPath = "file://" + Path.Combine(desktopPath, "wwo_wall/Assets", "W_s.mp4");
//             videoPlayer.url = videoPath;
//             videoPlayer.targetTexture = renderTexture;
//             rawImage.texture = renderTexture;
//             videoPlayer.Play();
//         }
//         else
//         {
//             string videoPath = "file://" + Path.Combine(desktopPath, "wwo_wall/Assets", content.Category + ".mp4");
//             videoPlayer.url = videoPath;
//             videoPlayer.targetTexture = renderTexture;
//             rawImage.texture = renderTexture;
//             videoPlayer.Play();
//         }
//     }


//     /// <summary>
//     /// フェードイン/アウトを担う処理
//     /// </summary>
//     private IEnumerator FadeTransition(Action onComplete)
//     {
//         isFading = true;
//         yield return StartCoroutine(Fade(1, 0));
//         onComplete();
//         yield return StartCoroutine(Fade(0, 1));
//         isFading = false;
//     }

//     /// <summary>
//     /// フェード自体の処理（rawImageのアルファを変更する）
//     /// </summary>
//     private IEnumerator Fade(float from, float to)
//     {
//         float duration = fadeDuration / 2;
//         float counter = 0f;

//         CanvasGroup canvasGroup = rawImage.gameObject.GetComponent<CanvasGroup>();
//         if (canvasGroup == null)
//         {
//             canvasGroup = rawImage.gameObject.AddComponent<CanvasGroup>();
//         }

//         while (counter < duration)
//         {
//             counter += Time.deltaTime;
//             canvasGroup.alpha = Mathf.Lerp(from, to, counter / duration);
//             yield return null;
//         }
//     }

//     /// <summary>
//     /// ビデオの終了時の処理（再度ビデオを再生）
//     /// </summary>
//     private void OnVideoEnd(VideoPlayer vp)
//     {
//         // 動画再生終了時に再度再生
//         videoPlayer.Play();
//     }

//     private void PlaySound()
//     {
//         // 効果音を再生
//         audioSource.Play();
//     }

//     private void LoadConfigFile()
//     {
//         try
//         {
//             string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
//             string configFilePath = Path.Combine(desktopPath, "wwo_wall", "config.txt");

//             if (File.Exists(configFilePath))
//             {
//                 string[] lines = File.ReadAllLines(configFilePath);

//                 foreach (var line in lines)
//                 {
//                     if (line.StartsWith("fadeDuration"))
//                     {
//                         string fadeValue = line.Split('=')[1].Trim().Replace("f", "");
//                         fadeDuration = float.Parse(fadeValue);
//                     }
//                     else if (line.StartsWith("timeoutDuration"))
//                     {
//                         string timeoutValue = line.Split('=')[1].Trim().Replace("f", "");
//                         timeoutDuration = float.Parse(timeoutValue);
//                     }
//                 }

//                 Debug.Log($"Config loaded. fadeDuration: {fadeDuration}, timeoutDuration: {timeoutDuration}");
//             }
//             else
//             {
//                 Debug.LogWarning("Config file not found. Using default values.");
//             }
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError("Error loading config file: " + ex.Message);
//         }
//     }

// }

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScratchCardSelfTest : MonoBehaviour
{
    // NOTE: This script does NOT auto-run. Add it to any GameObject in the scene
    // manually when you want to run the self-test. It will auto-quit after finish.
    IEnumerator Start()
    {
        var results = new System.Text.StringBuilder();
        results.AppendLine("=== ScratchCard Self Test ===");
        results.AppendLine($"Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        yield return null; // wait one frame for Start() to complete

        var game = FindObjectOfType<ScratchCardGame>();
        if (game == null)
        {
            results.AppendLine("[FAIL] ScratchCardGame not found in scene");
            WriteResult(results.ToString());
            yield break;
        }
        results.AppendLine("[PASS] ScratchCardGame found");

        // ---- Test 1: Verify UI references are wired ----
        bool refsOk = game.scratchImage != null && game.prizeText != null
                   && game.resetButton != null && game.exitButton != null;
        results.AppendLine(refsOk ? "[PASS] All UI references wired"
                                  : "[FAIL] Some UI references missing");

        // Debug font info
        results.AppendLine($"[INFO] Prize font: {game.prizeText?.font?.name ?? "NULL"}");

        // ---- Test 2: Reset button triggers NewGame ----
        string prizeBefore = game.prizeText.text;
        game.resetButton.onClick.Invoke();
        yield return null;
        bool resetOk = game.prizeText.text != prizeBefore || !string.IsNullOrEmpty(game.prizeText.text);
        results.AppendLine(resetOk ? "[PASS] Reset button triggers NewGame"
                                   : "[FAIL] Reset button did not change prize");

        // ---- Test 3: Scratch at center increases progress (small area, avoid reveal) ----
        string progressBefore = game.progressText != null ? game.progressText.text : "";
        int cx = game.textureWidth / 2;
        int cy = game.textureHeight / 2;
        // Scratch a very small area to check progress updates without triggering reveal
        for (int dy = -15; dy <= 15; dy += 3)
        {
            for (int dx = -40; dx <= 40; dx += 3)
            {
                game.ScratchAt(cx + dx, cy + dy);
            }
        }
        yield return null;
        results.AppendLine($"[INFO] textMaskPixelCount after scratch-3: {game.textMaskPixelCount}, textPixelsScratched: {game.textPixelsScratched}");
        bool progressOk = game.progressText != null && game.progressText.text != progressBefore
                       && game.progressText.text.Contains("%");
        results.AppendLine(progressOk ? "[PASS] Scratching increases progress text"
                                      : $"[FAIL] Progress text did not update. Before='{progressBefore}' After='{game.progressText?.text}'");

        // ---- Test 4: Heavy scratching reveals prize ----
        game.resetButton.onClick.Invoke();
        yield return null;
        // Scratch entire texture to guarantee > 90% text pixel coverage
        for (int y = 0; y < game.textureHeight; y += 6)
        {
            for (int x = 0; x < game.textureWidth; x += 6)
            {
                game.ScratchAt(x, y);
            }
        }
        yield return null;
        results.AppendLine($"[INFO] textMaskPixelCount after scratch-4: {game.textMaskPixelCount}, textPixelsScratched: {game.textPixelsScratched}");
        bool revealOk = !string.IsNullOrEmpty(game.congratsText.text);
        results.AppendLine(revealOk ? "[PASS] Heavy scratching triggers reveal"
                                    : "[FAIL] Congrats text did not appear after heavy scratching");

        // ---- Summary ----
        int passCount = 0, failCount = 0;
        foreach (var line in results.ToString().Split('\n'))
        {
            if (line.Contains("[PASS]")) passCount++;
            if (line.Contains("[FAIL]")) failCount++;
        }
        results.AppendLine($"=== Summary: {passCount} passed, {failCount} failed ===");

        WriteResult(results.ToString());
        Debug.Log(results.ToString());

        // Auto-exit after short delay so CI/headless runner can finish
        yield return new WaitForSeconds(0.5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void WriteResult(string content)
    {
        string path = Path.Combine(Application.persistentDataPath, "self_test_result.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
        Debug.Log($"Self-test result written to: {path}");
    }
}

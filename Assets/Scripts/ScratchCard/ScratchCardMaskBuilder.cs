using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;

public static class ScratchCardMaskBuilder
{
    static FieldInfo s_CachedGeneratorField;

    public static void Build(
        Text prizeText, RawImage scratchImage,
        int textureWidth, int textureHeight,
        out bool[] textMask, out int textMaskPixelCount)
    {
        textMask = new bool[textureWidth * textureHeight];
        textMaskPixelCount = 0;

        if (prizeText == null || string.IsNullOrEmpty(prizeText.text)) return;

        // 强制 Text 组件重建网格，确保 cachedTextGenerator 已生成
        prizeText.Rebuild(CanvasUpdate.PreRender);

        // 通过反射获取 Text 组件实际使用的 TextGenerator
        if (s_CachedGeneratorField == null)
            s_CachedGeneratorField = typeof(Text).GetField("m_TextCache",
                BindingFlags.NonPublic | BindingFlags.Instance);

        TextGenerator generator = s_CachedGeneratorField?.GetValue(prizeText) as TextGenerator;
        if (generator == null) return;

        IList<UIVertex> verts = generator.verts;
        if (verts == null || verts.Count < 4) return;

        RectTransform prizeRT = prizeText.rectTransform;
        RectTransform scratchRT = scratchImage.rectTransform;

        for (int i = 0; i < verts.Count - 4; i += 4)
        {
            // 计算 quad 在 PrizeText 局部坐标系中的边界
            float xMin = verts[i].position.x, xMax = verts[i].position.x;
            float yMin = verts[i].position.y, yMax = verts[i].position.y;
            for (int j = 1; j < 4; j++)
            {
                xMin = Mathf.Min(xMin, verts[i + j].position.x);
                xMax = Mathf.Max(xMax, verts[i + j].position.x);
                yMin = Mathf.Min(yMin, verts[i + j].position.y);
                yMax = Mathf.Max(yMax, verts[i + j].position.y);
            }
            if (xMax <= xMin || yMax <= yMin) continue;

            // 缩小到中心 55%，近似实际笔画
            float cx = (xMin + xMax) * 0.5f;
            float cy = (yMin + yMax) * 0.5f;
            float qw = xMax - xMin, qh = yMax - yMin;
            xMin = cx - qw * 0.275f;
            xMax = cx + qw * 0.275f;
            yMin = cy - qh * 0.275f;
            yMax = cy + qh * 0.275f;

            // 将 PrizeText 局部坐标 → ScratchLayer 局部坐标
            // TransformPoint 会自动处理父对象相同时的坐标转换
            Vector3 scratchMin = scratchRT.InverseTransformPoint(prizeRT.TransformPoint(new Vector3(xMin, yMin, 0)));
            Vector3 scratchMax = scratchRT.InverseTransformPoint(prizeRT.TransformPoint(new Vector3(xMax, yMax, 0)));

            Rect scratchRect = scratchRT.rect;
            if (scratchRect.width <= 0 || scratchRect.height <= 0) continue;

            float uMin = (scratchMin.x - scratchRect.xMin) / scratchRect.width;
            float uMax = (scratchMax.x - scratchRect.xMin) / scratchRect.width;
            float vMin = (scratchMin.y - scratchRect.yMin) / scratchRect.height;
            float vMax = (scratchMax.y - scratchRect.yMin) / scratchRect.height;

            int dstMinX = Mathf.RoundToInt(Mathf.Clamp01(uMin) * textureWidth);
            int dstMaxX = Mathf.RoundToInt(Mathf.Clamp01(uMax) * textureWidth);
            int dstMinY = Mathf.RoundToInt(Mathf.Clamp01(vMin) * textureHeight);
            int dstMaxY = Mathf.RoundToInt(Mathf.Clamp01(vMax) * textureHeight);

            for (int y = dstMinY; y <= dstMaxY; y++)
            {
                for (int x = dstMinX; x <= dstMaxX; x++)
                {
                    if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
                    {
                        int idx = y * textureWidth + x;
                        if (!textMask[idx])
                        {
                            textMask[idx] = true;
                            textMaskPixelCount++;
                        }
                    }
                }
            }
        }
    }
}

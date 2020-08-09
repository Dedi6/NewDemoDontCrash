using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextBubble : MonoBehaviour
{
    private SpriteRenderer backgroundSpriteRenderer;
    private TextMeshPro textMeshPro;

    public static void Create(Transform parent, Vector3 localPosition, string text, float bubbleTime)
    {
        Transform textBubble = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.ChatBubblePrefab).transform;
        textBubble.Rotate(0.0f, 180.0f, 0.0f);
        Transform chatBubbleTransform = Instantiate(textBubble, parent);
        chatBubbleTransform.eulerAngles = new Vector3(0f, 0f, 0f);
        chatBubbleTransform.localPosition = localPosition;

        chatBubbleTransform.GetComponent<TextBubble>().Setup(text);

        Destroy(chatBubbleTransform.gameObject, bubbleTime);
    }
    private void Awake()
    {
        backgroundSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private void Setup(string text)
    {
        textMeshPro.SetText(text);
        textMeshPro.ForceMeshUpdate();
        Vector2 textSize = textMeshPro.GetRenderedValues(false);
        Vector2 padding = new Vector2(3f, 4f);
        backgroundSpriteRenderer.size = textSize + padding;

        backgroundSpriteRenderer.transform.localPosition = new Vector3(backgroundSpriteRenderer.size.x / 2f, 0f);

        TextWriter.AddWriter_Static(textMeshPro, text, .05f, true, true);
    }
}

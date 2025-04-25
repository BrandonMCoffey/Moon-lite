using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Sprite))]
public class SpritePreviewDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var ident = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect spriteRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.objectReferenceValue = EditorGUI.ObjectField(spriteRect, property.name, property.objectReferenceValue, typeof(Sprite), false);

        if (Event.current.type != EventType.Repaint || property.objectReferenceValue == null)
        {
            EditorGUI.indentLevel = ident;
            return;
        }

        Sprite sp = property.objectReferenceValue as Sprite;

        if (sp != null)
        {
            Rect previewRect = new Rect(position.x + spriteRect.width - 64, position.y + EditorGUIUtility.singleLineHeight + 4, 64, 64);

            Texture2D tex = sp.texture;

            // Calculate UVs
            Rect texCoords = new Rect(
                sp.rect.x / tex.width,
                sp.rect.y / tex.height,
                sp.rect.width / tex.width,
                sp.rect.height / tex.height
            );

            GUI.DrawTextureWithTexCoords(previewRect, tex, texCoords);
        }

        EditorGUI.indentLevel = ident;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + 74f;
    }
}
using MixedReality.Toolkit.Editor;
using MixedReality.Toolkit.Themes;
using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FontIconSetMap))]
public class FontIconSetMapEditor : Editor
{
    private SerializedProperty setDefinitionProperty;
    private SerializedProperty fontIconSetsProperty;

    private int numColumns = 4;
    private bool editToggled = false;

    public void OnEnable()
    {
        setDefinitionProperty = serializedObject.FindProperty("setDefinition");
        fontIconSetsProperty = serializedObject.FindProperty("fontIconSets");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(setDefinitionProperty);
        EditorGUILayout.PropertyField(fontIconSetsProperty);
        editToggled = EditorGUILayout.Toggle("Edit Names", editToggled);
        EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

        IconSetDefinition setDefinition = setDefinitionProperty.objectReferenceValue as IconSetDefinition;
        const int TileSize = 90;

        List<string> validNames = new List<string>();
        HashSet<string> usedNames = new HashSet<string>();
        Dictionary<string, List<FontIconSet>> iconMatches = new Dictionary<string, List<FontIconSet>>();

        for (int i = 0; i < fontIconSetsProperty.arraySize; i++)
        {
            FontIconSet iconSet = fontIconSetsProperty.GetArrayElementAtIndex(i).objectReferenceValue as FontIconSet;

            Dictionary<string, uint> glyphs = new(iconSet.GlyphIconsByName);
            usedNames.UnionWith(iconSet.GlyphIconsByName.Keys);

            int column = 0;

            if (editToggled)
            {
                EditorGUILayout.BeginHorizontal();

                foreach (KeyValuePair<string, uint> kv in glyphs)
                {
                    if (column >= numColumns)
                    {
                        column = 0;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    EditorGUILayout.BeginVertical(GUILayout.Width(TileSize));

                    if (GUILayout.Button(string.Empty,
                        GUILayout.Height(TileSize),
                        GUILayout.Width(TileSize)))
                    {
                        //AddIcon(fontIconSet, fontAsset.characterTable[i].unicode);
                        //EditorUtility.SetDirty(target);
                    }

                    if (setDefinition != null && setDefinition.IconNames != null)
                    {
                        validNames.Clear();
                        foreach (string name in setDefinition.IconNames)
                        {
                            if (!glyphs.Keys.Contains(name))
                            {
                                validNames.Add(name);
                            }
                        }
                        validNames.Add(kv.Key);
                        string[] validNamesArray = validNames.ToArray();

                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            int selected = System.Array.IndexOf(validNamesArray, kv.Key);
                            selected = EditorGUILayout.Popup(string.Empty, selected, validNamesArray, GUILayout.MaxWidth(TileSize));
                            if (check.changed)
                            {
                                iconSet.GlyphIconsByName.Remove(kv.Key);
                                iconSet.GlyphIconsByName.Add(validNamesArray[selected], kv.Value);
                                serializedObject.Update();
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

                    Rect textureRect = GUILayoutUtility.GetLastRect();
                    textureRect.width = TileSize;
                    textureRect.height = TileSize;
                    FontIconSetInspector.EditorDrawTMPGlyph(textureRect, kv.Value, iconSet.IconFontAsset);
                }

                EditorGUILayout.EndHorizontal();
                column++;
            }
            else
            {
                foreach (KeyValuePair<string, uint> kv in glyphs)
                {
                    if (iconMatches.TryGetValue(kv.Key, out List<FontIconSet> icons))
                    {
                        icons.Add(iconSet);
                    }
                    else
                    {
                        iconMatches.Add(kv.Key, new List<FontIconSet> { iconSet });
                    }
                }

                foreach (KeyValuePair<string, List<FontIconSet>> kv in iconMatches)
                {
                    if (setDefinition != null && setDefinition.IconNames != null)
                    {
                        validNames.Clear();
                        foreach (string name in setDefinition.IconNames)
                        {
                            if (!glyphs.Keys.Contains(name))
                            {
                                validNames.Add(name);
                            }
                        }
                        validNames.Add(kv.Key);
                        string[] validNamesArray = validNames.ToArray();

                        EditorGUILayout.BeginVertical();

                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            int selected = System.Array.IndexOf(validNamesArray, kv.Key);
                            selected = EditorGUILayout.Popup(string.Empty, selected, validNamesArray, GUILayout.MaxWidth(TileSize));
                            if (check.changed)
                            {
                                foreach (FontIconSet set in kv.Value)
                                {
                                    uint value = set.GlyphIconsByName[kv.Key];
                                    _ = set.GlyphIconsByName.Remove(kv.Key);
                                    set.GlyphIconsByName.Add(validNamesArray[selected], value);
                                }
                                serializedObject.Update();
                            }
                        }

                        EditorGUILayout.BeginHorizontal();

                        foreach (FontIconSet set in kv.Value)
                        {
                            if (GUILayout.Button(string.Empty,
                            GUILayout.Height(TileSize),
                            GUILayout.Width(TileSize)))
                            {
                                //AddIcon(fontIconSet, fontAsset.characterTable[i].unicode);
                                //EditorUtility.SetDirty(target);
                            }

                            Rect textureRect = GUILayoutUtility.GetLastRect();
                            textureRect.width = TileSize;
                            textureRect.height = TileSize;
                            FontIconSetInspector.EditorDrawTMPGlyph(textureRect, set.GlyphIconsByName[kv.Key], set.IconFontAsset);
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                    }
                }

            }
        }

        if (Event.current.type == EventType.Repaint)
        {
            float editorWindowWidth = GUILayoutUtility.GetLastRect().width;
            numColumns = (int)Mathf.Floor(editorWindowWidth / (TileSize + GUI.skin.button.margin.right));
        }

        serializedObject.ApplyModifiedProperties();
    }
}

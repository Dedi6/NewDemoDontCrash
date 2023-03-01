using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Spikes))]
public class SpikesScriptEditor : Editor
{

    [Range(0, 20)]
    float xSize = 1;

    private void OnEnable()
    {
        Spikes spikeScript = (Spikes)target;
        xSize = spikeScript.GetComponent<SpriteRenderer>().size.x;
    }

    public override void OnInspectorGUI()
    {
        Spikes spikeScript = (Spikes)target;

        xSize = (int)EditorGUILayout.Slider(xSize, 0, 20);
        
        SpriteRenderer _sprite = spikeScript.GetComponent<SpriteRenderer>();
        _sprite.size = new Vector2(xSize, _sprite.size.y);
        spikeScript._xSize = xSize;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Up"))
        {
            spikeScript.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if (GUILayout.Button("Down"))
        {
            spikeScript.transform.localRotation = Quaternion.Euler(0, 0, 180f);
        }
        if (GUILayout.Button("Right"))
        {
            spikeScript.transform.localRotation = Quaternion.Euler(0, 0, 270f);
        }
        if (GUILayout.Button("Left"))
        {
            spikeScript.transform.localRotation = Quaternion.Euler(0, 0, 90f);
        }

        GUILayout.EndHorizontal();

        spikeScript.dontDealDamage = EditorGUILayout.Toggle("Don't Deal Damage", spikeScript.dontDealDamage);
       // EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());   // make changes requiring a save
    }

}

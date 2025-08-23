using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SongInfo", menuName = "ScriptableObject/SongInfo")]
public class SongInfo : ScriptableObject
{
    public string songName;
    public AudioClip audioClip;
    public float bpm;
}

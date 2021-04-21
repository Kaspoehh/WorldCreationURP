using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New World Prop Manager", menuName = "World Creation/New World Prop Manager")]
public class WorldPropManager : ScriptableObject
{
    public List<PropData> PropsForInWorld = new List<PropData>();
    public List<PropData> TreesForInWorld = new List<PropData>();
}

[Serializable]
public class PropData
{
    public GameObject PropPrefab;
    public int PropIndex;
}
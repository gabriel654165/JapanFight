using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapMetaData : MonoBehaviour
{
    private int m_indexPlace = 0;

    private Tuple<int, Vector3>[] m_spawnPosP1 = new Tuple<int, Vector3>[] {
        new Tuple<int, Vector3>(0, new Vector3(-5, 1, 0)),
        new Tuple<int, Vector3>(0, new Vector3(-40, 1, 50)),
        new Tuple<int, Vector3>(1, new Vector3(28, 1, 75)),
        new Tuple<int, Vector3>(1, new Vector3(21, 1, 113)),
    };

    private Tuple<int, Vector3>[] m_spawnPosP2 = new Tuple<int, Vector3>[] {
        new Tuple<int, Vector3>(0, new Vector3(5, 1, 0)),
        new Tuple<int, Vector3>(0, new Vector3(-35, 1, 45)),
        new Tuple<int, Vector3>(1, new Vector3(28, 1, 67)),
        new Tuple<int, Vector3>(1, new Vector3(12, 1, 113)),
    };

    private Tuple<int, Vector3>[] m_spawnPosCam = new Tuple<int, Vector3>[] {
        new Tuple<int, Vector3>(0, new Vector3(0, 0, 10)),
        new Tuple<int, Vector3>(0, new Vector3(-37.5f, 0, 47.5f)),
        new Tuple<int, Vector3>(1, new Vector3(76, 4, 71)),
        new Tuple<int, Vector3>(1, new Vector3(16.5f, 4, 80)),
    };

    private Tuple<int, Vector3>[] m_cameraOffset = new Tuple<int, Vector3>[] {
        new Tuple<int, Vector3>(0, new Vector3(0, 2.5f, -30)),
        new Tuple<int, Vector3>(0, new Vector3(-20, 2.5f, -20)),
        new Tuple<int, Vector3>(1, new Vector3(-30, 2.5f, 0)),
        new Tuple<int, Vector3>(1, new Vector3(0, 2.5f, 25)),
    };

    // @note: should be overriden
    virtual public int GetMapIndex()
    {
        return 1;
    }

    private Vector3 GetSpawnPosByMapIndex(Tuple<int, Vector3>[] tupleArray, int placeIndex, int mapIndex)
    {
        List<Vector3> arraySpawnPos = new List<Vector3>();

        foreach (var tupleSpawnPos in tupleArray) {
            if (tupleSpawnPos.Item1 == mapIndex) {
                arraySpawnPos.Add(tupleSpawnPos.Item2);
            }
        }
        return arraySpawnPos[placeIndex];
    }

    public Vector3 GetSpawnPosP1(int placeIndex)
    {
        return GetSpawnPosByMapIndex(m_spawnPosP1, placeIndex, GetMapIndex());
    }
    public Vector3 GetSpawnPosP2(int placeIndex)
    {
        return GetSpawnPosByMapIndex(m_spawnPosP2, placeIndex, GetMapIndex());
    }
    public Vector3 GetSpawnPosCam(int placeIndex)
    {
        return GetSpawnPosByMapIndex(m_spawnPosCam, placeIndex, GetMapIndex());
    }
    public Vector3 GetCamoffset(int placeIndex)
    {
        return GetSpawnPosByMapIndex(m_cameraOffset, placeIndex, GetMapIndex());
    }

}
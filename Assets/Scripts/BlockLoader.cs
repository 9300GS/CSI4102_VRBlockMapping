using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class BlockLoader : MonoBehaviour
{
    public string filePath;
    public GameObject roomPrefab;
    public GameObject resetPointPrefab;

    [HideInInspector] public int roomNum;
    [HideInInspector] public List<Vector3> roomPos;

    [HideInInspector] public int resetNum;
    [HideInInspector] public List<Vector3> resetPos;
    [HideInInspector] public List<string> resetInfo;

    [HideInInspector] public Room startingRoom;
    private Vector3 startingRoomPos;

    void Awake()
    {
        startingRoomPos = Vector3.zero;
    }

    public bool LoadInfo()
    {
        FileInfo f = new FileInfo(filePath);
        if(!f.Exists) return false;

        string[] lines = File.ReadAllLines(@filePath);

        if(!Int32.TryParse(lines[0], out roomNum)) return false;

        for(int i = 1; i <= roomNum; i++)
        {
            float x, z;
            string[] line = lines[i].Split(' ');

            if(!float.TryParse(line[0], out x)) return false;
            if(!float.TryParse(line[1], out z)) return false;

            if(i == 1)
            {
                startingRoomPos = new Vector3(x, 0f, z);
                GameObject.Find("Terrain").gameObject.transform.position -= startingRoomPos;
            }
            roomPos.Add(new Vector3(x, 0f, z) - startingRoomPos);
        }

        if(!Int32.TryParse(lines[roomNum + 1], out resetNum)) return false;

        for(int i = roomNum + 2; i <= roomNum + resetNum + 1; i++)
        {
            float x, z;
            string[] line = lines[i].Split(' ');

            if(!float.TryParse(line[0], out x)) return false;
            if(!float.TryParse(line[1], out z)) return false;
            resetPos.Add(new Vector3(x, 0f, z));

            resetInfo.Add(line[2] + " " + line[3]);
        }
        
        return true;
    }

    public void LoadBlocks()
    {
        for(int i = 0; i < roomNum; i++)
        {
            GameObject room = Instantiate(roomPrefab);
            room.name = "Room" + i.ToString();
            room.transform.SetParent(transform.Find("Rooms"));
            room.transform.position = roomPos[i];

            if(i == 0) startingRoom = room.GetComponent<Room>();
        }

        for(int i = 0; i < resetNum; i++)
        {
            GameObject resetPoint = Instantiate(resetPointPrefab);
            resetPoint.name = "ResetPoint" + i.ToString();
            resetPoint.transform.SetParent(transform.Find("ResetPoints"));
            resetPoint.transform.position = resetPos[i];
            
            string[] info = resetInfo[i].Split(' ');
            int rm1 = Int32.Parse(info[0]);
            int rm2 = Int32.Parse(info[1]);
            
            Room room1 = transform.Find("Rooms").Find("Room" + rm1.ToString()).GetComponent<Room>();
            Room room2 = transform.Find("Rooms").Find("Room" + rm2.ToString()).GetComponent<Room>();

            ResetPoint rp = resetPoint.GetComponent<ResetPoint>();
            rp.connectedRooms.Add(room1);
            rp.connectedRooms.Add(room2);
            room1.connectedResetPoints.Add(rp);
            room2.connectedResetPoints.Add(rp);
        }
    }

}

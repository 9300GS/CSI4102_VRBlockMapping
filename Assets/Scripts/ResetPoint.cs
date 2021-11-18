using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPoint : MonoBehaviour
{
    [SerializeField] public List<Room> connectedRooms;

    void Initialize()
    {
        if(connectedRooms.Count != 2)
            Debug.Log("Error : Reset Point (" + this.gameObject.name + ") has invalid number of Rooms Connected");
    }

    public Room GetNextRoom(Room previousRoom)
    {
        if(previousRoom == connectedRooms[0])
            return connectedRooms[1];
        else
            return connectedRooms[0];
    }
}

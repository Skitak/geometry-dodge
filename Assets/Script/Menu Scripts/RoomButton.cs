using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomButton : MonoBehaviour
{
    //This class represents all the infos in our "room button".
    //So this class should update infos on the room with the UI texts or elements.

    private RoomInfo roomInfo;

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }
    /// <summary>
    /// Modifie the UI of the button
    /// </summary>
    /// <param name="room"></param>
    public void SetRoom(RoomInfo room){
        roomInfo = room;
    }

}
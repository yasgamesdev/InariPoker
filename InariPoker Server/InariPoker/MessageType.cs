using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YasGameLib
{
    public enum MessageType
    {
        Connect,
        Disconnect,
        Debug,
        SendMessage,
        BroadcastMessage,
        SendUserName,
        ReplyUserID,
        ReplyLobbyState,
        RequestLobbyState,
        EnterRoom,
        ReplySuccessEnterRoom,
        ReplyRoomState,
        LeaveRoom,
        ReplySuccessLeaveRoom,
        ChangeReady,
        BattleStart,
        BattleEnd,
        SendTrunData,
        DeclareCard,
        RemoveCardType,
        RemoveCard,
        RequestPass,
        ReplyPass,
        RequestTrueOrLie,
        ReplyTrueOrLie,
        LeavePlayer,
    }
}

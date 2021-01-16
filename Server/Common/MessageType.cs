using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public enum MessageType
    {
        PlayerName,
        NewPlayer,
        PlayerMovement,
        BallMovement, //
        ChangeTurn,  //
        EndGame,    //
        FinishedSync,
        Information,
        Warning,
        Error
    }
}

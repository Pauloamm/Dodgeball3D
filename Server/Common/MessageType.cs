﻿using System;
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
        BallMovement, 
        EndGame,    
        FinishedSync,
        Information,
        Warning,
        Error
    }
}

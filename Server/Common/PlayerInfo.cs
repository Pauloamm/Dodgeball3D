using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class PlayerInfo
    {
        public Guid Id { get; set; }
        public Guid BallId { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public int Score { get; set; }

        
        public float directionX { get; set; }
        public float directionY { get; set; }
        public float directionZ { get; set; }
      
    }
}

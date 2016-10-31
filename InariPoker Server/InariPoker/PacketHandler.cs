using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace YasGameLib
{
    public class PacketHandler
    {
        public DataType dataType { get; private set; }
        public delegate void Handle(NetConnection connection, object data);
        public Handle handleFunc { get; private set; }

        public PacketHandler(DataType dataType, Handle handleFunc)
        {
            this.dataType = dataType;
            this.handleFunc = handleFunc;
        }
    }
}

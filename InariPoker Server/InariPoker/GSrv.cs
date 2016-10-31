using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YasGameLib
{
    class GSrv
    {
        static PacketHandler[] handlers;
        static NetServer server;

        public static void Init()
        {
            int length = Enum.GetNames(typeof(MessageType)).Length;
            handlers = new PacketHandler[length];
        }

        public static void SetPacketHandler(MessageType messageType, DataType dataType, PacketHandler.Handle handleFunc)
        {
            handlers[(int)messageType] = new PacketHandler(dataType, handleFunc);
        }

        public static void UnsetPacketHandler(MessageType messageType)
        {
            handlers[(int)messageType] = null;
        }

        public static void SetConnectPacketHandler(PacketHandler.Handle handleFunc)
        {
            SetPacketHandler(MessageType.Connect, DataType.Null, handleFunc);
        }

        public static void SetDisconnectPacketHandler(PacketHandler.Handle handleFunc)
        {
            SetPacketHandler(MessageType.Disconnect, DataType.Null, handleFunc);
        }

        public static void SetDebugPacketHandler(PacketHandler.Handle handleFunc)
        {
            SetPacketHandler(MessageType.Debug, DataType.String, handleFunc);
        }

        public static void Listen(string appIdentifier, int port)
        {
            NetPeerConfiguration config = new NetPeerConfiguration(appIdentifier);
            config.Port = port;
            server = new NetServer(config);
            server.Start();
        }

        public static void Shutdown()
        {
            if (server != null)
            {
                server.Shutdown("bye");
            }
        }

        public static byte[] Serialize<T>(T data)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ProtoBuf.Serializer.Serialize<T>(ms, data);
            byte[] result = ms.ToArray();
            ms.Close();
            return result;
        }

        public static T Deserialize<T>(byte[] data)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(data);
            T result = ProtoBuf.Serializer.Deserialize<T>(ms);
            ms.Close();
            return result;
        }

        public static void Receive()
        {
            NetIncomingMessage message;
            while ((message = server.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        ReceiveData(message);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        if (message.SenderConnection.Status == NetConnectionStatus.Connected)
                        {
                            if (handlers[(int)MessageType.Connect] != null)
                            {
                                handlers[(int)MessageType.Connect].handleFunc(message.SenderConnection, null);
                            }
                        }
                        else if (message.SenderConnection.Status == NetConnectionStatus.Disconnected)
                        {
                            if (handlers[(int)MessageType.Disconnect] != null)
                            {
                                handlers[(int)MessageType.Disconnect].handleFunc(message.SenderConnection, null);
                            }
                        }
                        break;
                    default:
                        if (handlers[(int)MessageType.Debug] != null)
                        {
                            handlers[(int)MessageType.Debug].handleFunc(message.SenderConnection, message.ReadString());
                        }
                        break;
                }
            }
        }

        static void ReceiveData(NetIncomingMessage message)
        {
            try
            {
                byte messageType = message.ReadByte();

                if (handlers[(int)messageType] == null)
                {
                    return;
                }

                DataType dataType = handlers[(int)messageType].dataType;
                if (dataType == DataType.Null)
                {
                    handlers[(int)messageType].handleFunc(message.SenderConnection, null);
                }
                else if (dataType == DataType.String)
                {
                    handlers[(int)messageType].handleFunc(message.SenderConnection, message.ReadString());
                }
                else if (dataType == DataType.Bytes)
                {
                    int numberOfBytes = message.ReadInt32();
                    handlers[(int)messageType].handleFunc(message.SenderConnection, message.ReadBytes(numberOfBytes));
                }
                else if (dataType == DataType.Int32)
                {
                    handlers[(int)messageType].handleFunc(message.SenderConnection, message.ReadInt32());
                }
            }
            catch
            {
                return;
            }
        }

        public static void Send(MessageType messageType, NetConnection connect, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            server.SendMessage(message, connect, method);
        }

        public static void SendToAll(MessageType messageType, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            server.SendToAll(message, method);
        }

        public static void Send(MessageType messageType, string data, NetConnection connect, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            message.Write(data);
            server.SendMessage(message, connect, method);
        }

        public static void SendToAll(MessageType messageType, string data, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            message.Write(data);
            server.SendToAll(message, method);
        }

        public static void Send(MessageType messageType, byte[] data, NetConnection connect, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            message.Write(data.Length);
            message.Write(data);
            server.SendMessage(message, connect, method);
        }

        public static void SendToAll(MessageType messageType, byte[] data, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            message.Write(data.Length);
            message.Write(data);
            server.SendToAll(message, method);
        }

        public static void Send(MessageType messageType, int data, NetConnection connect, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            message.Write(data);
            server.SendMessage(message, connect, method);
        }

        public static void SendToAll(MessageType messageType, int data, NetDeliveryMethod method)
        {
            NetOutgoingMessage message = server.CreateMessage();
            message.Write((byte)messageType);
            message.Write(data);
            server.SendToAll(message, method);
        }
    }
}

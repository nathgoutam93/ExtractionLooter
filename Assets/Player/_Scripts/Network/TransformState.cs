using UnityEngine;
using Unity.Netcode;


namespace Assets.Player.Network
{
    public class TransformState: INetworkSerializable
    {
        public int Tick;
        public Vector2 Position;
        public Quaternion Rotation;
        public bool HasStartedMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var _reader = serializer.GetFastBufferReader();
                _reader.ReadValueSafe(out Tick);
                _reader.ReadValueSafe(out Position);
                _reader.ReadValueSafe(out Rotation);
                _reader.ReadValueSafe(out HasStartedMoving);
            }
            else
            {
                var _writer = serializer.GetFastBufferWriter();
                _writer.WriteValueSafe(Tick);
                _writer.WriteValueSafe(Position);
                _writer.WriteValueSafe(Rotation);
                _writer.WriteValueSafe(HasStartedMoving);
            }
        }
    }
}
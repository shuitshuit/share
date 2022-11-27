using System.Runtime.Serialization;

namespace WebSocketServer
{
    [Serializable()]
    public class ParametersException : Exception
    {
        public ParametersException()
        : base()
        {
        }

        public ParametersException(string message)
            : base(message)
        {
        }

        public ParametersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        //逆シリアル化コンストラクタ。このクラスの逆シリアル化のために必須。
        //アクセス修飾子をpublicにしないこと！（詳細は後述）
        protected ParametersException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
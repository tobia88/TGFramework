public class TGException : System.Exception
{
    public TGException() : base() {}
    public TGException(string message) : base(message) {}
}

public class InvalidPortNumberException : TGException
{
    private const string FORMAT = "端口{0}不存在，请将game.txt的端口设置成正确的端口号";
    public InvalidPortNumberException(string port) : base(string.Format(FORMAT, port)) { }
}

public class NoDataReceivedException : TGException
{
    private const string FORMAT = "获取不到数据，请确认设备是否正确安装及开启";
    public NoDataReceivedException() : base(FORMAT) { }
}
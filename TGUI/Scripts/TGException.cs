

public class InvalidPortNumberException : System.Exception
{
    private const string FORMAT = "端口{0}不存在，请将game.txt的端口设置成正确的端口号";
    public InvalidPortNumberException(string port) : base(string.Format(FORMAT, port)) { }
}
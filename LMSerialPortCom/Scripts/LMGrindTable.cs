using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

public struct GrindNode {
    public int x;
    public int y;

    public override bool Equals( object obj ) {
        if( obj is GrindNode ) {
            var n = ( GrindNode )obj;
            return this.x == n.x && this.y == n.y;
        }

        return false;
    }

    public static bool operator ==( GrindNode left, GrindNode right ) {
        return left.Equals( right );
    }

    public static bool operator !=( GrindNode left, GrindNode right ) {
        return !left.Equals( right );
    }

    public override int GetHashCode() {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public override string ToString() {
        return string.Format( "X:{0}, Y:{1}", this.x, this.y );
    }
}

public class LMGrindTable: LMInput_Port {
    public struct GrindEvent {
        public string key;
        public string value;

        public GrindEvent( string key, string value ) {
            this.key = key;
            this.value = value;
        }

        public override string ToString() {
            return string.Format( "Key:{0}, Value:{1}", key, value );
        }
    }

    public const string CLEAR_PATH = "CB01FD";
    public const string PATH_FORMAT = "Y{0}Z";
    public const int SKIP_LENGTH = 8; //Skip along from code 88 to 96

    // public Leadiy_M7B m7bResolver;
    // public LMBasePortInput m7bPort;

    public static int ColumnCount { get; private set; }
    public static int RowCount { get; private set; }

    public override bool IsReconnectable { get { return false; } }

    private string m_currentKey;
    private string m_currentValue;
    // private Rect m_worldBound;
    // private bool m_worldAvailable;

    public static Action<bool> onTestFinished;
    public static Action onTestStarted;
    public static Action<GrindNode> onTurnOffLight;
    public Queue<GrindEvent> eventQueue = new Queue<GrindEvent>();
    public LMGrindTableEmulator GrindTableEmu { get { return Emulator as LMGrindTableEmulator; } }

    // public override bool OpenPort()
    // {
    //     if (base.OpenPort())
    //     {
    //         int udp = controller.gameConfig.GetValue("UDP", -1);

    //         //如果监测udp开启了，则使用udp做为3D传感器的端口
    //         if (udp >= 0)
    //         {
    //             m7bPort = new LMInput_UDP();
    //             (m7bPort as LMInput_UDP).Init(controller, KeyportData, udp);
    //         }
    //         //否则使用serial port做为3D传感器端口
    //         else
    //         {
    //             m7bPort = new LMInput_Port();
    //             (m7bPort as LMInput_Port).Init(controller,
    //                 KeyportData,
    //                 controller.gameConfig.GetValue("端口2", -1));
    //         }
    //         return true;
    //     }
    //     return false;
    // }

    public static GrindNode[] PathToNodes( Vector3[] _path, Rect _bound, bool _is3D ) {
        // 用来保存最后一次的编号，以避免重复发送同样的编号
        string lastCode = string.Empty;

        // 记录最新的编号
        string newCode = string.Empty;

        GrindNode lastNode = new GrindNode();
        List<GrindNode> nodes = new List<GrindNode>();

        for( int i = 0; i < _path.Length; i++ ) {
            var tmpNode = GetNode( _path[i], _bound, _is3D );
            Debug.Log( "Get Node: " + tmpNode.x + ", " + tmpNode.y );

            if( tmpNode == lastNode ) {
                continue;
            }

            nodes.Add( tmpNode );
            lastNode = tmpNode;
            // // 测试时候发送到模拟器
            // if( TGData.IsTesting ) {
            //     GrindTableEmu.SetBtnEnable( tmpNode.x, tmpNode.y );
            // }

        }

        return nodes.ToArray();
    }

    private static GrindNode GetNode( Vector3 p, Rect _bound, bool _is3D ) {
        float ratioX = 0f;
        float ratioY = 0f;

        if( _is3D ) {
            ratioX = ( p.x - _bound.x ) / ( _bound.width );
            ratioY = ( p.z - _bound.y ) / ( _bound.height );

        } else {
            ratioX = ( p.x - _bound.x ) / ( _bound.width );
            ratioY = ( p.y - _bound.y ) / ( _bound.height );
        }
        // 逆转Y值
        ratioY = 1f - ratioY;

        ratioX = Mathf.Clamp01( ratioX );
        ratioY = Mathf.Clamp01( ratioY );

        GrindNode n = new GrindNode();

        n.x = Mathf.RoundToInt( ratioX * ( ColumnCount - 1 ) );
        n.y = Mathf.RoundToInt( ratioY * ( RowCount - 1 ) );

        return n;
    }

    public override void Init( TGController _controller, KeyPortData keyportData, int _com ) { base.Init( _controller, keyportData, _com ); 
        //获取keyInputConfig.json里的行列数
        ColumnCount = keyportData.width;
        RowCount = keyportData.height;
    }

    //清除所有磨砂版的灯
    public void ClearLights() {
        Debug.Log( "Clear Lights" );

        Write( CLEAR_PATH, false );
        eventQueue.Clear();

        if( IsTesting && GrindTableEmu != null )
            GrindTableEmu.Reset();
    }

    // 由于发送清除指令的时候，上位机不会返回数据
    // 因此与其像之前开协程等待上位机重新发送数据，不如直接发送数据给上位机妥当
    public override void Write( byte[] bytes ) {
        if( Port != null && Port.IsOpen ) {
            Port.Write(bytes, 0, bytes.Length);
        }
    }

    public override bool OnUpdate() {
        if( IsTesting )
            GrindTableEmu.OnUpdate();

        if( base.OnUpdate() ) {
            //如果存在排序中的事件，则按顺序触发该事件
            if( eventQueue.Count > 0 ) {
                var evt = eventQueue.Dequeue();
                TriggerEvent( evt );
            }

            return true;
        }
        return false;
    }

    private void TriggerEvent( GrindEvent evt ) {
        switch( evt.key ) {
            case "CJ":
                // 任务开始事件
                Debug.Log( "Train Start" );
                if( onTestStarted != null )
                    onTestStarted();

                break;

            case "CB":
                // 如果没有数值则直接跳过
                if( string.IsNullOrEmpty( evt.value ) )
                    return;

                // 如果获取CB:0001则表示顺利完成任务
                // 反之获取CB:0002则表示任务失败，需要重来
                bool result = evt.value == "1";
                Debug.Log( "Train Finished, Result: " + result );
                if( onTestFinished != null )
                    onTestFinished( result );

                break;

            case "CC":
                // 砂磨板消除灯之后获取的事件
                Debug.Log( "On Turn Off Light: " + evt.value );

                if( onTurnOffLight != null )
                    onTurnOffLight( CodeToNode( evt.value ) );
                break;
        }
    }

    public void Write( GrindNode[] nodes ) {
        string newCode = string.Empty;
        string content = string.Empty;
        string lastCode = string.Empty;

        for( int i = 0; i < nodes.Length; i++ ) {
            var node = nodes[i];

            Debug.Log( "Get Node: " + node.x + ", " + node.y );

            // 将位置转译成磨砂版接收的编号
            newCode = NodeToCode( node );

            if( lastCode == newCode )
                continue;

            content += newCode;
            lastCode = newCode;

            // 测试时候发送到模拟器
            if( IsTesting ) {
                GrindTableEmu.SetBtnEnable( node.x, node.y );
            }
        }

        Write( string.Format( PATH_FORMAT, content ), false );
    }

    public void DrawLine( GrindNode from, GrindNode to ) {
        Write( GetLines( from, to ) );
    }

    public void DrawLine( int x0, int y0, int x1, int y1 ) {
        var startNode = new GrindNode() { x = x0, y = y0 };
        var endNode = new GrindNode() { x = x1, y = y1 };
        Write( GetLines( startNode, endNode ) );
    }

    private GrindNode[] GetLines( GrindNode a, GrindNode b ) {
        List<GrindNode> nodes = new List<GrindNode>();

        int x0 = a.x;
        int y0 = a.y;
        int x1 = b.x;
        int y1 = b.y;

        // 检测是否大斜率
        bool steep = Mathf.Abs( y1 - y0 ) > Mathf.Abs( x1 - x0 );

        // 如果大斜率，则把xy掉转，小斜率比较好计算，在取值时候调过来就好了
        if( steep ) {
            Swap( ref x0, ref y0 );
            Swap( ref x1, ref y1 );
        }

        // 只从左往右画线
        // 如果发现起点在右手边，则把两个点调换
        // 最后再把列表倒转就好了
        bool mirror = x0 > x1;

        if( mirror ) {
            Swap( ref x0, ref x1 );
            Swap( ref y0, ref y1 );
        }

        int dy = y1 - y0;
        int dx = x1 - x0;

        // 检测往上画还是往下画
        int yStep = ( y1 > y0 ) ? 1 : -1;

        // 表示是一条竖线
        if( dx == 0 ) {
            int yLength = Mathf.Abs( dy );

            for( int y = 0; y < yLength; y++ ) {
                int fy = y0 + y * yStep;
                nodes.Add( new GrindNode() { x = x0, y = fy } );
            }
        } else {
            int de = 2 * Mathf.Abs( dy );
            int e = 0;

            int ty = y0;

            for( int tx = x0; tx <= x1; tx++ ) {
                if( steep ) {
                    // 如果是大斜率的，记得把xy调转回来
                    nodes.Add( new GrindNode() { x = ty, y = tx } );
                } else {
                    Debug.LogWarning( x0 );
                    nodes.Add( new GrindNode() { x = tx, y = ty } );
                }

                e += de;

                if( e > dx ) {
                    ty += yStep;
                    e -= 2 * dx;
                }
            }
        }
        if( mirror || steep ) {
            nodes.Reverse();
        }
        return nodes.ToArray();
    }

    private void Swap( ref int x, ref int y ) {
        int tmp = x;
        x = y;
        y = tmp;
    }

    public void DrawArc( int cx, int cy, int r, int fromDeg, int toDeg, bool counterClockwise = false ) {

        // 把所有角度调整到[0, 360]
        fromDeg = ConvertDeg( fromDeg );
        toDeg = ConvertDeg( toDeg );

        int d = ( 5 - r * 4 ) / 4;
        int x = 0;
        int y = r;

        var nodes = new List<GrindNode>();

        // Bresenham Mid Point Circle Drawing Algorithm
        while( x <= y ) {
            // 八个角度同步绘画相互映射，其中加入对角度值的判定
            // 如果审核不通过则不绘制该点
            DrawCirc( cx, cy, x, y, fromDeg, toDeg, ref nodes, counterClockwise );
            DrawCirc( cx, cy, x, -y, fromDeg, toDeg, ref nodes, counterClockwise );
            DrawCirc( cx, cy, -x, y, fromDeg, toDeg, ref nodes, counterClockwise );
            DrawCirc( cx, cy, -x, -y, fromDeg, toDeg, ref nodes, counterClockwise );
            DrawCirc( cx, cy, y, x, fromDeg, toDeg, ref nodes, counterClockwise );
            DrawCirc( cx, cy, y, -x, fromDeg, toDeg, ref nodes, counterClockwise );
            DrawCirc( cx, cy, -y, x, fromDeg, toDeg, ref nodes, counterClockwise );
            DrawCirc( cx, cy, -y, -x, fromDeg, toDeg, ref nodes, counterClockwise );

            if( d < 0 ) {
                d += 2 * x + 1;
            } else {
                d += 2 * ( x - y ) + 1;
                y--;
            }

            x++;
        }

        // 把点从新排序
        nodes.Sort( ( GrindNode a, GrindNode b ) => {
            // 计算比对点的角度
            float degA = Mathf.Atan2( a.y - cy, a.x - cx ) * Mathf.Rad2Deg;
            float degB = Mathf.Atan2( b.y - cy, b.x - cx ) * Mathf.Rad2Deg;

            // 把角度调整到方便比对的角度，其中包括对经过角度0的负数调整
            degA = ConvertDegOverZero( ( int )degA, fromDeg, toDeg, counterClockwise );
            degB = ConvertDegOverZero( ( int )degB, fromDeg, toDeg, counterClockwise );

            int retval = degA.CompareTo( degB );

            return ( counterClockwise ) ? retval * -1 : retval;
        } );


        Write( nodes.ToArray() );
    }

    // 检测点是否在有效范围以内
    private bool Within( int x, int y ) {
        return x >= 0 && x < ColumnCount && y >= 0 && y < RowCount;
    }

    // 把角度调整到[0, 360]
    private int ConvertDeg( int deg ) {
        return ( deg + 360 ) % 360;
    }

    // 把角度根据顺时钟逆时钟调整到相应区域，好方便比较
    // 例如从角度270画到90度，因为会经过0度，造成0度前后数值无法比对的问题，因此创建此方法
    private int ConvertDegOverZero( int deg, int fromDeg, int toDeg, bool counterClockwise ) {
        // 把角度映射到[0, 360]
        int retval = ConvertDeg( deg );

        // 如果是顺时针旋转，但是起始角度比终点角度大，因此判断会经过0度
        if( !counterClockwise && fromDeg > toDeg ) {

            // 如果角度比起始角度大，则-360度，变为负值
            if( retval >= fromDeg )
                retval -= 360;
        }

        // 如果是逆时钟转，并且起始角度小于终点角度，因此也会经过0度
        else if( counterClockwise && fromDeg < toDeg ) {

            // 如果角度比终点角度大，则-360度，变为负值
            if( retval >= toDeg ) {
                retval -= 360;
            }
        }
        return retval;
    }

    private void DrawCirc( int cx, int cy, int tx, int ty, int fromDeg, int toDeg, ref List<GrindNode> nodes, bool counterClockwise ) {
        // 判断要画的点是否在有效区域，并且角度是否在起始角度与终点角度之间
        if( Within( cx + tx, cy + ty ) && WithinAngle( cx, cy, cx + tx, cy + ty, fromDeg, toDeg, counterClockwise ) ) {
            var n = new GrindNode() { x = cx + tx, y = cy + ty };

            if( !nodes.Contains( n ) ) {
                // Debug.Log( "Add Node: " + n );
                nodes.Add( n );
            }
        }
    }

    // 判断要画的点是否在有效角度之间
    private bool WithinAngle( int cx, int cy, int tx, int ty, int fromDeg, int toDeg, bool counterClockwise ) {
        var dy = ty - cy;
        var dx = tx - cx;

        var deg = Mathf.Atan2( dy, dx ) * Mathf.Rad2Deg;

        // 把角度变换到[0, 360]
        deg = ConvertDeg( Mathf.FloorToInt( deg ) );

        // Debug.Log( string.Format( "TX: {0}, TY: {1}, Deg: {2}", tx, ty, deg ));

        var nf = fromDeg;
        var nt = toDeg;

        // 如果顺时钟旋转并且起始角度大于终点角度，则表示会经过零点
        if( !counterClockwise && nf > nt ) {
            // 把起始角度变为负值
            nf -= 360;

            // 如果角度比起始角度大，则-360度，变为负值
            if( deg >= fromDeg )
                deg -= 360;

        } else if( counterClockwise && nf < nt ) {
            // 把终点角度变为负值
            nt -= 360;

            // 如果角度比终点角度大，则-360度，变为负值
            if( deg >= toDeg ) {
                deg -= 360;
            }
        }

        // 如果起始角度比终点角度大，则调转过来，以方便测试区间
        if( nf > nt )
            Swap( ref nf, ref nt );

        return deg >= nf && deg <= nt;
    }

    public override void Close() {
        ClearLights();

        // if (m7bPort != null)
        //     m7bPort.Close();

        base.Close();
    }

    private string NodeToCode( GrindNode node ) {
        int colIndex = 65 + node.x;
        int rowIndex = 65 + node.y;

        // 直接跳过从Y开始到a之前的字符
        if( colIndex >= 89 ) colIndex += SKIP_LENGTH;
        if( rowIndex >= 89 ) rowIndex += SKIP_LENGTH;

        char colChar = ( char )colIndex;
        char rowChar = ( char )rowIndex;

        string retval = colChar.ToString() + rowChar.ToString();

        Debug.Log( string.Format( "Writing code x={0} y={1}, return {2}", colIndex, rowIndex, retval ) );

        return retval;
    }


    // 将砂磨板接收到的编号转译映射成世界空间坐标系
    public static GrindNode CodeToNode( string code ) {
        Debug.Log( "Code To Vector: " + code );

        int colIndex = ( int )code[0];
        int rowIndex = ( int )code[1];

        if( colIndex >= 89 ) colIndex -= SKIP_LENGTH;
        if( rowIndex >= 89 ) rowIndex -= SKIP_LENGTH;

        colIndex -= 65;
        rowIndex -= 65;

        return new GrindNode() { x = colIndex, y = rowIndex };
    }

    public static Vector3 NodeToVector( GrindNode node, Rect bound, bool is3D ) {
        float ratioX = ( float )node.x / ( ColumnCount - 1 );
        float ratioY = 1f - ( float )node.y / ( RowCount - 1 );

        float rx = ratioX * bound.width + bound.x;
        float ry = ratioY * bound.height + bound.y;

        Debug.Log( string.Format( "Rx: {0}. Ry: {1}", ratioX, ratioY ) );

        if( is3D )
            return new Vector3( rx, 0f, ry );

        return new Vector3( rx, ry );
    }

    // public override IEnumerator OnStart(LMBasePortResolver resolver = null)
    // {
    //     yield return controller.StartCoroutine(base.OnStart(resolver));

    //     // 没有任何错误信息则开启3D传感器的解析器
    //     if (string.IsNullOrEmpty(ErrorTxt) && m7bPort != null)
    //     {
    //         Debug.Log("m7b is started");
    //         m7bResolver = new Leadiy_M7B();
    //         yield return controller.StartCoroutine(m7bPort.OnStart(m7bResolver));
    //         ErrorTxt = m7bPort.ErrorTxt;
    //     }
    // }

    // public Vector3 GetAccelerations()
    // {
    //     if (m7bResolver == null)
    //         return Vector3.zero;

    //     return m7bResolver.Acceleration;
    // }

    // public override float GetValue(int index)
    // {
    //     if (m7bResolver == null)
    //         return 0f;

    //     return m7bResolver.GetValue(index);
    // }

    // public override float GetRawValue(int index)
    // {
    //     if (m7bResolver == null)
    //         return 0f;

    //     return m7bResolver.GetRawValue(index);
    // }

    private string m_getString;

    protected override void ResolveBytes( byte[] bytes ) {
        // 消除字节中的空值
        bytes = LMUtility.RemoveSpacing( bytes );

        // 把字节转化为字符串并叠加起来
        m_getString += Encoding.UTF8.GetString( bytes );

        // 砂磨板的值会以分号为区分，如CB:0001;CC:00JK;等
        // 因此当分号出现时候，则判断可能获取到数值了
        if( m_getString.IndexOf( ';' ) >= 0 ) {
            // 按分号分割成各别的数组，如[CB:0001,CC:00JK]
            string[] split = m_getString.Split( ';' );

            // 倒序迭代每个字符串
            for( int i = split.Length - 1; i >= 0; i-- ) {
                if( split[i].IndexOf( ':' ) < 0 )
                    continue;

                string[] splitKey = split[i].Split( ':' );

                string key = splitKey[0] ?? string.Empty;

                // 如果键值为空或者是个空格，则直接跳过
                if( string.IsNullOrWhiteSpace( key ) )
                    continue;

                // 如果获取到位置代号但是不完整，则跳过
                if( key == "CC" && split[i].Length != 7 )
                    continue;

                // 如果有值，则把开始的0全部去掉
                string value = splitKey[1].TrimStart( '0' ) ?? string.Empty;

                // 存到序列里
                SetValue( key, value );

                // 只要获取任何有效键值，就清空叠加的字符串
                m_getString = string.Empty;
                break;
            }
        }
    }

    private void SetValue( string key, string value ) {
        // 由于上位机会重复发送当前的事件，因此需要判断
        // 如果获得的时间存在重复，则跳过
        if( m_currentKey == key && m_currentValue == value )
            return;

        // 如果没有数值，则直接返回
        if( string.IsNullOrEmpty( value ) )
            return;

        Debug.Log( "捕捉到事件: Key: " + key + "，Value: " + value );

        // 把该事件存到事件序列里，依序触发
        var evt = new GrindEvent( key, value );
        eventQueue.Enqueue( evt );

        Debug.Log( "加入触发事件: " + evt );

        m_currentKey = key;
        m_currentValue = value;
    }
}
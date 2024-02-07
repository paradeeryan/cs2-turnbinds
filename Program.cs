using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using cs2_turnbinds;

class Config
{
    public List<int> Yaw { get; set; }
    public double Sensitivity { get; set; }
    public double M_Yaw { get; set; }
    public Keys Left { get; set; }
    public Keys Right { get; set; }
    public Keys Toggle { get; set; }
    public Keys Pause { get; set; }
    public Keys Inc { get; set; }
    public Keys Dec { get; set; }
    public Keys Exit { get; set; }
}

public class KeysConverter : JsonConverter<Keys>
{
    public override Keys Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string keyName = reader.GetString();
        return (Keys)Enum.Parse(typeof(Keys), keyName);
    }

    public override void Write(Utf8JsonWriter writer, Keys value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

class Program
{
    [StructLayout(LayoutKind.Explicit)]
    struct LARGE_INTEGER
    {
        [FieldOffset(0)] public long QuadPart;
    }
    
    
    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);
    
    [DllImport("ntdll.dll")]
    private static extern uint NtDelayExecution([In] bool Alertable, [In] ref LARGE_INTEGER DelayInterval);
    
    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);
    
    // [DllImport("ntdll.dll")]
    // private static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint CurrentResolution);
    
    
    const int MOUSEEVENTF_MOVE = 0x0001;
    const int MOUSEEVENTF_ABSOLUTE = 0x8000;
    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;
    
    static void MoveMouse(int deltaX, int deltaY)
    {
        mouse_event(MOUSEEVENTF_MOVE, deltaX, deltaY, 0, (int)IntPtr.Zero);
    }
    
    static void DelayExecutionBy(long hns)
    {
        // Convert the delay from 100-nanosecond intervals to a LARGE_INTEGER
        LARGE_INTEGER interval;
        interval.QuadPart = -1 * hns; // Negative value for relative delay

        // Call NtDelayExecution
        NtDelayExecution(false, ref interval);
    }
    
    public class MouseMoveCalculator
    {
        private long lastTime;
        private double remainingMovement;
        private long frequency;

        public MouseMoveCalculator()
        {
            QueryPerformanceCounter(out lastTime);
            QueryPerformanceFrequency(out frequency);
        }

        public int CalculateMovement(double yawspeed, double sensitivity, double m_yaw)
        {
            long currentTime;
            QueryPerformanceCounter(out currentTime);
            
            long elapsedTime = currentTime - lastTime;
            

            // Calculate movement basedp
            double movement = ((yawspeed / (sensitivity * m_yaw)) * elapsedTime) / frequency;

            // Accumulate remaining movement fractions to ensure precision
            remainingMovement += movement;
            long movementAmount = (long)remainingMovement;

            // Subtract the integer part used, keep the fractional part for next calculation
            remainingMovement -= movementAmount;

            // Update last time for next call
            lastTime = currentTime;

            return (int) movementAmount;
        }
    }
    
    static bool IsKeyDown(int vk)
    {
        return (GetAsyncKeyState(vk) & 0x8000) != 0;
    }


    static void Main(string[] args)
    {
        
        var configFilePath = "config.json";
        var configFileExists = File.Exists(configFilePath);
        
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
        serializerOptions.Converters.Add(new KeysConverter());
        
        if (!configFileExists)
        {
            Console.WriteLine("Config file not found, making default config");
            var defaultConfig = new Config
            {
                Yaw = new List<int> { 80, 160, 240 },
                Sensitivity = 0.8,
                M_Yaw = 0.022,
                Left = Keys.LBUTTON,
                Right = Keys.RBUTTON,
                Toggle = Keys.XBUTTON1,
                Pause = Keys.F1,
                Inc = Keys.OEM_PLUS,
                Dec = Keys.OEM_MINUS,
                Exit = Keys.F2
            };
            File.WriteAllText(configFilePath, JsonSerializer.Serialize(defaultConfig, serializerOptions));
        }
        
        var configFile = File.ReadAllText(configFilePath);
        
        // use system.json
        var config = JsonSerializer.Deserialize<Config>(configFile, serializerOptions);
        if (config == null)
        {
            throw new Exception("Config file is empty");
        }
        
        int currentYawIndex = 0;
        
        bool paused = true; // Pause state for left mouse button
        int yawSpeed = config.Yaw[currentYawIndex];
        double sensitivity = config.Sensitivity;
        double m_yaw = config.M_Yaw;

        var calculator = new MouseMoveCalculator();

        while (true)
        {
            var movement = calculator.CalculateMovement( yawSpeed, sensitivity, m_yaw);
            
            // Check if the left mouse button is pressed and not paused
            if (IsKeyDown((int) config.Left) && !paused)
            {
                // Move the mouse left by the specified speed
                MoveMouse(-movement, 0);
            }

            // Check if the right mouse button is pressed and not paused
            if (IsKeyDown((int) config.Right) && !paused)
            {
                // Move the mouse right by the specified speed
                MoveMouse(movement, 0);
            }

            // Check if the 'P' key is pressed to toggle pause state for left mouse button
            if (IsKeyDown((int) config.Pause))
            {
                paused = !paused;
                Console.WriteLine($"Yaw speed {(paused ? "paused" : "resumed")}");
                Thread.Sleep(200); // Delay to avoid multiple toggles with one key press
            }

            // Check if the '+' key is pressed to increase yaw speed
            if (IsKeyDown((int) config.Inc))
            {
                yawSpeed += 10; // Increase yaw speed by 10
                Thread.Sleep(200); // Delay to avoid multiple increments with one key press
                Console.WriteLine($"Yaw speed increased to {yawSpeed}");
            }

            // Check if the '-' key is pressed to decrease yaw speed
            if (IsKeyDown((int) config.Dec))
            {
                yawSpeed -= 10; // Decrease yaw speed by 10
                Thread.Sleep(200); // Delay to avoid multiple decrements with one key press
                Console.WriteLine($"Yaw speed decreased to {yawSpeed}");
            }
            
            if (IsKeyDown((int) config.Toggle))
            {
                currentYawIndex = (currentYawIndex + 1) % config.Yaw.Count;
                yawSpeed = config.Yaw[currentYawIndex];
                Thread.Sleep(200); // Delay to avoid multiple toggles with one key press
            }

            // Check if the END key is pressed to exit the program
            if (IsKeyDown((int) config.Exit))
            {
                Console.WriteLine("Exiting program...");
                break; // Exit the loop
            }
            
            DelayExecutionBy(3500);
        }
    }
}

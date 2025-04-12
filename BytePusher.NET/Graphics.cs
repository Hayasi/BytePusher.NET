using BytePusher.NET.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BytePusher.NET;
internal class Graphics
{
    private readonly IWindow window;
    private GL? gl;
    private IInputContext? input;

    private uint shaderProgram;
    private uint texture;
    private uint vao;

    private const int Width = 768;
    private const int Height = 768;

    private const int ScreenWidth = 256;
    private const int ScreenHeight = 256;

    private readonly Audio audio;
    private readonly Core.BytePusher bytePusher;

    public Graphics(Core.BytePusher bytePusher)
    {
        this.bytePusher = bytePusher;

        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(Width, Height);
        options.Title = "BytePusher";
        options.WindowBorder = WindowBorder.Fixed;
        options.FramesPerSecond = 60;
        options.UpdatesPerSecond = 60;
        options.VSync = false;

        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Closing += OnClose;
        window.Update += OnUpdate;

        audio = new Audio();
    }

    public void Run()
    {
        window.Run();
    }

    private void OnLoad()
    {
        input = window.CreateInput();

        gl = window.CreateOpenGL();

        gl.Viewport(0, 0, Width, Height);
        gl.ClearColor(0, 0, 0, 0);

        InitializeShaders();
        InitializeTexture();
        InitializeQuad();
    }

    private void InitializeShaders()
    {
        const string vertShaderSource = @"
                #version 330 core
                layout (location = 0) in vec2 aPos;
                layout (location = 1) in vec2 aTexCoord;
                out vec2 TexCoord;
                
                void main()
                {
                    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0);
                    TexCoord = aTexCoord;
                }";

        const string fragShaderSource = @"
                #version 330 core
                in vec2 TexCoord;
                out vec4 FragColor;
                uniform sampler2D uTexture;
                
                void main()
                {
                    FragColor = texture(uTexture, TexCoord);
                }";

        uint vertShader = gl!.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertShader, vertShaderSource);
        gl.CompileShader(vertShader);

        uint fragShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragShader, fragShaderSource);
        gl.CompileShader(fragShader);

        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertShader);
        gl.AttachShader(shaderProgram, fragShader);
        gl.LinkProgram(shaderProgram);

        gl.DeleteShader(vertShader);
        gl.DeleteShader(fragShader);
    }

    private void InitializeTexture()
    {
        texture = gl!.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);

        gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgb,
                     (uint)ScreenWidth, (uint)ScreenHeight, 0, PixelFormat.Rgb,
                     PixelType.UnsignedByte, ReadOnlySpan<byte>.Empty);

        gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    private unsafe void InitializeQuad()
    {
        float[] vertices = {
            -1.0f,  1.0f,    0.0f, 0.0f,
            -1.0f, -1.0f,    0.0f, 1.0f,
             1.0f, -1.0f,    1.0f, 1.0f,

            -1.0f,  1.0f,    0.0f, 0.0f,
             1.0f, -1.0f,    1.0f, 1.0f,
             1.0f,  1.0f,    1.0f, 0.0f
        };

        vao = gl!.GenVertexArray();
        uint vbo = gl.GenBuffer();

        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertices, BufferUsageARB.StaticDraw);

        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);
    }

    private void OnUpdate(double deltaTime)
    {
        bool IsKeyPressed(params Key[] keys)
        {
            return keys.Any(a => input!.Keyboards.Any(b => b.IsKeyPressed(a)));
        }

        Keys keys = 0;
        if (IsKeyPressed(Key.Number0, Key.Keypad0)) keys |= Keys.Key0; else keys &= ~Keys.Key0;
        if (IsKeyPressed(Key.Number1, Key.Keypad1)) keys |= Keys.Key1; else keys &= ~Keys.Key1;
        if (IsKeyPressed(Key.Number2, Key.Keypad2)) keys |= Keys.Key2; else keys &= ~Keys.Key2;
        if (IsKeyPressed(Key.Number3, Key.Keypad3)) keys |= Keys.Key3; else keys &= ~Keys.Key3;
        if (IsKeyPressed(Key.Number4, Key.Keypad4)) keys |= Keys.Key4; else keys &= ~Keys.Key4;
        if (IsKeyPressed(Key.Number5, Key.Keypad5)) keys |= Keys.Key5; else keys &= ~Keys.Key5;
        if (IsKeyPressed(Key.Number6, Key.Keypad6)) keys |= Keys.Key6; else keys &= ~Keys.Key6;
        if (IsKeyPressed(Key.Number7, Key.Keypad7)) keys |= Keys.Key7; else keys &= ~Keys.Key7;
        if (IsKeyPressed(Key.Number8, Key.Keypad8)) keys |= Keys.Key8; else keys &= ~Keys.Key8;
        if (IsKeyPressed(Key.Number9, Key.Keypad9)) keys |= Keys.Key9; else keys &= ~Keys.Key9;
        if (IsKeyPressed(Key.A)) keys |= Keys.A; else keys &= ~Keys.A;
        if (IsKeyPressed(Key.B)) keys |= Keys.B; else keys &= ~Keys.B;
        if (IsKeyPressed(Key.C)) keys |= Keys.C; else keys &= ~Keys.C;
        if (IsKeyPressed(Key.D)) keys |= Keys.D; else keys &= ~Keys.D;
        if (IsKeyPressed(Key.E)) keys |= Keys.E; else keys &= ~Keys.E;
        if (IsKeyPressed(Key.F)) keys |= Keys.F; else keys &= ~Keys.F;

        bytePusher.KeysState = keys;

        bytePusher.Clock();
    }

    private void OnRender(double deltaTime)
    {
        byte[] samples = bytePusher.AudioSamples.Select(a => (byte)(a+128)).ToArray();
        audio.Update(samples);
        gl!.BindTexture(TextureTarget.Texture2D, texture);
        gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)ScreenWidth, (uint)ScreenHeight,
                        PixelFormat.Rgb, PixelType.UnsignedByte, new ReadOnlySpan<byte>(bytePusher.Screen));

        gl.Clear(ClearBufferMask.ColorBufferBit);

        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    private void OnClose()
    {
        audio.Close();
        gl!.DeleteVertexArray(vao);
        gl.DeleteTexture(texture);
        gl.DeleteProgram(shaderProgram);
    }
}

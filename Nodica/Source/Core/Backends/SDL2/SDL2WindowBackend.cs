using SDL2;

namespace Cherris.Backends;

public sealed class SDL2WindowBackend : IWindowBackend
{
    private IntPtr window;

    public override void InitializeWindow(int width, int height, int minWidth, int minHeight, int maxWidth, int maxHeight, string title, string iconPath)
    {
        _ = SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO);

        window = SDL.SDL_CreateWindow(
            title,
            SDL.SDL_WINDOWPOS_CENTERED,
            SDL.SDL_WINDOWPOS_CENTERED,
            width,
            height,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
        );

        SDL.SDL_SetWindowMinimumSize(window, minWidth, minHeight);
        SDL.SDL_SetWindowMaximumSize(window, maxWidth, maxHeight);

        if (!string.IsNullOrEmpty(iconPath))
        {
            IntPtr icon = SDL_image.IMG_Load(iconPath);
            SDL.SDL_SetWindowIcon(window, icon);
            SDL.SDL_FreeSurface(icon);
        }

        ((SDL2DrawingBackend)((SDL2Backend)App.Instance.Backend).Drawing).Renderer = SDL.SDL_CreateRenderer(
            window,
            -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC
        );

        _ = SDL.SDL_InitSubSystem(SDL.SDL_INIT_AUDIO);

        _ = SDL_ttf.TTF_Init();
    }

    public override void SetWindowFlags(bool resizable, bool antiAliasing)
    {
        if (antiAliasing)
        {
            _ = SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1");
        }

        SDL.SDL_SetWindowResizable(window, resizable ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
    }

    public override void SetWindowMinSize(int width, int height)
    {
        SDL.SDL_SetWindowMinimumSize(window, width, height);
    }

    public override bool WindowShouldClose()
    {
        SDL.SDL_Event e;
        while (SDL.SDL_PollEvent(out e) != 0)
        {
            if (e.type == SDL.SDL_EventType.SDL_QUIT)
                return true;
        }

        return false;
    }

    public override Vector2 GetWindowSize()
    {
        SDL.SDL_GetWindowSize(window, out int width, out int height);
        return new(width, height);
    }

    public override void SetWindowSize(Vector2 size)
    {
        SDL.SDL_SetWindowSize(window, (int)size.X, (int)size.Y);
    }

    public override bool IsWindowFullscreen()
    {
        // Explicitly cast the flags to uint for proper bitwise operation
        uint windowFlags = (uint)SDL.SDL_GetWindowFlags(window);
        return (windowFlags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0;
    }

    public override void ToggleFullscreen()
    {
        uint currentFlags = (uint)SDL.SDL_GetWindowFlags(window);
        if ((currentFlags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) == 0)
        {
            SDL.SDL_SetWindowFullscreen(window, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
        }
        else
        {
            SDL.SDL_SetWindowFullscreen(window, 0); // Switch to windowed mode
        }
    }
}
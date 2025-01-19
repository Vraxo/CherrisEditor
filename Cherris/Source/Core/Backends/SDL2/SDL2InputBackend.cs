using System.Runtime.InteropServices;
using SDL2;

namespace Cherris.Backends;

public sealed class SDL2InputBackend : IInputBackend
{
    public override bool IsKeyPressed(KeyCode key)
    {
        //SDL.SDL_PumpEvents();
        //IntPtr state = SDL.SDL_GetKeyboardState(out _);
        //return Marshal.ReadByte(state, (int)key) == 1;
        return false;
    }

    public override bool IsKeyReleased(KeyCode key)
    {
        return false;
    }

    public override bool IsKeyDown(KeyCode key)
    {
        IntPtr keyState = SDL.SDL_GetKeyboardState(out _);
        byte keycode = (byte)SDL.SDL_GetScancodeFromKey(GetSdlKeycode(key));
        return Marshal.ReadByte(keyState, keycode) != 0;
    }

    public override bool IsMouseButtonPressed(MouseButtonCode button)
    {
        SDL.SDL_PumpEvents();
        uint state = SDL.SDL_GetMouseState(out _, out _);
        return (state & SDL.SDL_BUTTON((uint)button)) != 0;
        return false;
    }

    public override bool IsMouseButtonReleased(MouseButtonCode button)
    {
        return false; // Placeholder
    }

    public override bool IsMouseButtonDown(MouseButtonCode button)
    {
        int x, y;
        uint mouseState = SDL.SDL_GetMouseState(out x, out y);
        byte buttonMask = (byte)(1 << (int)button);
        return (mouseState & buttonMask) != 0;
    }

    public override Vector2 GetMousePosition()
    {
        _ = SDL.SDL_GetMouseState(out int x, out int y);
        return new Vector2(x, y);
    }

    public override float GetMouseWheelMovement()
    {
        //SDL.SDL_Event e;
        //while (SDL.SDL_PollEvent(out e) != 0)
        //{
        //    if (e.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
        //    {
        //        return e.wheel.y;
        //    }
        //}

        return 0.0f;
    }

    private static SDL.SDL_Keycode GetSdlKeycode(KeyCode key)
    {
        return key switch
        {
            KeyCode.A => SDL.SDL_Keycode.SDLK_a,
            KeyCode.B => SDL.SDL_Keycode.SDLK_b,
            KeyCode.C => SDL.SDL_Keycode.SDLK_c,
            KeyCode.D => SDL.SDL_Keycode.SDLK_d,
            KeyCode.E => SDL.SDL_Keycode.SDLK_e,
            KeyCode.F => SDL.SDL_Keycode.SDLK_f,
            KeyCode.G => SDL.SDL_Keycode.SDLK_g,
            KeyCode.H => SDL.SDL_Keycode.SDLK_h,
            KeyCode.I => SDL.SDL_Keycode.SDLK_i,
            KeyCode.J => SDL.SDL_Keycode.SDLK_j,
            KeyCode.K => SDL.SDL_Keycode.SDLK_k,
            KeyCode.L => SDL.SDL_Keycode.SDLK_l,
            KeyCode.M => SDL.SDL_Keycode.SDLK_m,
            KeyCode.N => SDL.SDL_Keycode.SDLK_n,
            KeyCode.O => SDL.SDL_Keycode.SDLK_o,
            KeyCode.P => SDL.SDL_Keycode.SDLK_p,
            KeyCode.Q => SDL.SDL_Keycode.SDLK_q,
            KeyCode.R => SDL.SDL_Keycode.SDLK_r,
            KeyCode.S => SDL.SDL_Keycode.SDLK_s,
            KeyCode.T => SDL.SDL_Keycode.SDLK_t,
            KeyCode.U => SDL.SDL_Keycode.SDLK_u,
            KeyCode.V => SDL.SDL_Keycode.SDLK_v,
            KeyCode.W => SDL.SDL_Keycode.SDLK_w,
            KeyCode.X => SDL.SDL_Keycode.SDLK_x,
            KeyCode.Y => SDL.SDL_Keycode.SDLK_y,
            KeyCode.Z => SDL.SDL_Keycode.SDLK_z,
            KeyCode.Space => SDL.SDL_Keycode.SDLK_SPACE,
            KeyCode.Escape => SDL.SDL_Keycode.SDLK_ESCAPE,
            KeyCode.Enter => SDL.SDL_Keycode.SDLK_RETURN,
            KeyCode.Tab => SDL.SDL_Keycode.SDLK_TAB,
            KeyCode.Backspace => SDL.SDL_Keycode.SDLK_BACKSPACE,
            KeyCode.Insert => SDL.SDL_Keycode.SDLK_INSERT,
            KeyCode.Delete => SDL.SDL_Keycode.SDLK_DELETE,
            KeyCode.Right => SDL.SDL_Keycode.SDLK_RIGHT,
            KeyCode.Left => SDL.SDL_Keycode.SDLK_LEFT,
            KeyCode.Down => SDL.SDL_Keycode.SDLK_DOWN,
            KeyCode.Up => SDL.SDL_Keycode.SDLK_UP,
            KeyCode.F1 => SDL.SDL_Keycode.SDLK_F1,
            KeyCode.F2 => SDL.SDL_Keycode.SDLK_F2,
            KeyCode.F3 => SDL.SDL_Keycode.SDLK_F3,
            KeyCode.F4 => SDL.SDL_Keycode.SDLK_F4,
            KeyCode.F5 => SDL.SDL_Keycode.SDLK_F5,
            KeyCode.F6 => SDL.SDL_Keycode.SDLK_F6,
            KeyCode.F7 => SDL.SDL_Keycode.SDLK_F7,
            KeyCode.F8 => SDL.SDL_Keycode.SDLK_F8,
            KeyCode.F9 => SDL.SDL_Keycode.SDLK_F9,
            KeyCode.F10 => SDL.SDL_Keycode.SDLK_F10,
            KeyCode.F11 => SDL.SDL_Keycode.SDLK_F11,
            KeyCode.F12 => SDL.SDL_Keycode.SDLK_F12,
            KeyCode.LeftShift => SDL.SDL_Keycode.SDLK_LSHIFT,
            KeyCode.LeftControl => SDL.SDL_Keycode.SDLK_LCTRL,
            KeyCode.LeftAlt => SDL.SDL_Keycode.SDLK_LALT,
            KeyCode.RightShift => SDL.SDL_Keycode.SDLK_RSHIFT,
            KeyCode.RightControl => SDL.SDL_Keycode.SDLK_RCTRL,
            KeyCode.RightAlt => SDL.SDL_Keycode.SDLK_RALT,
            _ => SDL.SDL_Keycode.SDLK_UNKNOWN
        };
    }
}
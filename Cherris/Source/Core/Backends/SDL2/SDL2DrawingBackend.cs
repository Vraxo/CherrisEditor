using SDL2;

namespace Cherris.Backends;

public sealed class SDL2DrawingBackend : IDrawingBackend
{
    public IntPtr Renderer;

    public override void BeginDrawing()
    {
        _ = SDL.SDL_RenderClear(Renderer);
    }

    public override void ClearBackground(Color color)
    {
        _ = SDL.SDL_SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
        _ = SDL.SDL_RenderClear(Renderer);
    }

    public override void EndDrawing()
    {
        SDL.SDL_RenderPresent(Renderer);
    }

    public override void DrawCircleOutline(Vector2 position, float radius, Color color)
    {
        const int segments = 100; // The number of line segments to use
        double angleStep = Math.PI * 2 / segments;

        _ = SDL.SDL_SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);

        for (int i = 0; i < segments; i++)
        {
            double angle1 = i * angleStep;
            double angle2 = (i + 1) * angleStep;

            int x1 = (int)(position.X + Math.Cos(angle1) * radius);
            int y1 = (int)(position.Y + Math.Sin(angle1) * radius);
            int x2 = (int)(position.X + Math.Cos(angle2) * radius);
            int y2 = (int)(position.Y + Math.Sin(angle2) * radius);

            _ = SDL.SDL_RenderDrawLine(Renderer, x1, y1, x2, y2);
        }
    }

    public override void DrawRectangleOutline(Vector2 position, Vector2 size, Color color)
    {
        SDL.SDL_Rect rect = new SDL.SDL_Rect { x = (int)position.X, y = (int)position.Y, w = (int)size.X, h = (int)size.Y };
        _ = SDL.SDL_SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
        _ = SDL.SDL_RenderDrawRect(Renderer, ref rect);
    }

    public override void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        SDL.SDL_Rect rect = new SDL.SDL_Rect { x = (int)position.X, y = (int)position.Y, w = (int)size.X, h = (int)size.Y };
        _ = SDL.SDL_SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
        _ = SDL.SDL_RenderFillRect(Renderer, ref rect);
    }

    public override void DrawRoundedRectangle(Vector2 position, Vector2 size, float roundness, int segments, Color color)
    {
        // Calculate the corner radius based on the roundness factor
        float cornerRadius = Math.Min(size.X, size.Y) * roundness;

        // Draw the four corner arcs (top-left, top-right, bottom-left, bottom-right)
        DrawArc(new(position.X + cornerRadius, position.Y + cornerRadius), cornerRadius, 180, 270, segments, color); // top-left
        DrawArc(new(position.X + size.X - cornerRadius, position.Y + cornerRadius), cornerRadius, 270, 360, segments, color); // top-right
        DrawArc(new(position.X + cornerRadius, position.Y + size.Y - cornerRadius), cornerRadius, 90, 180, segments, color); // bottom-left
        DrawArc(new(position.X + size.X - cornerRadius, position.Y + size.Y - cornerRadius), cornerRadius, 0, 90, segments, color); // bottom-right

        // Draw the four sides (left, right, top, bottom)
        DrawRectangle(new(position.X + cornerRadius, position.Y), new(size.X - 2 * cornerRadius, size.Y), color); // Middle horizontal part
        DrawRectangle(new(position.X, position.Y + cornerRadius), new(size.X, size.Y - 2 * cornerRadius), color); // Middle vertical part
    }

    public void DrawArc(Vector2 position, float radius, float startAngle, float endAngle, int segments, Color color)
    {
        float angleStep = (endAngle - startAngle) / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = startAngle + i * angleStep;
            float angle2 = startAngle + (i + 1) * angleStep;

            float x1 = position.X + radius * MathF.Cos(MathF.PI * angle1 / 180);
            float y1 = position.Y + radius * MathF.Sin(MathF.PI * angle1 / 180);
            float x2 = position.X + radius * MathF.Cos(MathF.PI * angle2 / 180);
            float y2 = position.Y + radius * MathF.Sin(MathF.PI * angle2 / 180);

            DrawLine(new(x1, y1), new(x2, y2), color);  // Draw the line between each segment
        }
    }

    public override void DrawTexture(Texture texture, Vector2 position, float rotation, Vector2 scale, Color tint)
    {
        _ = SDL.SDL_SetTextureColorMod(texture, tint.R, tint.G, tint.B);

        SDL.SDL_Rect destRect = new()
        {
            x = (int)position.X,
            y = (int)position.Y,
            w = (int)(texture.Size.X * scale.X),
            h = (int)(texture.Size.Y * scale.Y)
        };

        Console.WriteLine(destRect.x);

        _ = SDL.SDL_RenderCopyEx(Renderer, texture, IntPtr.Zero, ref destRect, rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
    }

    public override void DrawText(string text, Vector2 position, Font font, float fontSize, float spacing, Color color)
    {
        // SDL_ttf font rendering
        IntPtr sdlFont = font;  // Assuming Font class stores SDL_ttf font pointer

        if (sdlFont == IntPtr.Zero)
        {
            return;
        }

        // Set font size
        if (SDL_ttf.TTF_SetFontSize(sdlFont, (int)fontSize) != 0)
        {
            return;
        }

        // Measure text size (use the proper size for width and height)
        _ = SDL_ttf.TTF_SizeText(sdlFont, text, out int textWidth, out int textHeight);

        // Render the text with improved blending for better quality
        IntPtr textSurface = SDL_ttf.TTF_RenderText_Blended(sdlFont, text, new() { r = color.R, g = color.G, b = color.B, a = color.A });
        if (textSurface == IntPtr.Zero)
        {
            Console.WriteLine($"Failed to render text: {SDL.SDL_GetError()}");
            return;
        }

        // Create texture from surface
        IntPtr textTexture = SDL.SDL_CreateTextureFromSurface(Renderer, textSurface);
        if (textTexture == IntPtr.Zero)
        {
            Console.WriteLine($"Failed to create texture from surface: {SDL.SDL_GetError()}");
            SDL.SDL_FreeSurface(textSurface);
            return;
        }

        SDL.SDL_FreeSurface(textSurface);

        // Set up the rectangle for rendering
        SDL.SDL_Rect textRect = new()
        {
            x = (int)position.X,
            y = (int)position.Y,
            w = textWidth,  // Use measured width
            h = textHeight  // Use measured height
        };

        // Render the text
        if (SDL.SDL_RenderCopy(Renderer, textTexture, IntPtr.Zero, ref textRect) != 0)
        {
            Console.WriteLine($"Failed to copy texture to renderer: {SDL.SDL_GetError()}");
        }

        // Clean up the texture
        SDL.SDL_DestroyTexture(textTexture);
    }

    public override void DrawLine(Vector2 from, Vector2 to, Color color)
    {
        _ = SDL.SDL_SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);
        _ = SDL.SDL_RenderDrawLine(Renderer, (int)from.X, (int)from.Y, (int)to.X, (int)to.Y);
    }

    public override void DrawCircle(Vector2 position, float radius, Color color)
    {
        const int segments = 100;
        double angleStep = Math.PI * 2 / segments;

        _ = SDL.SDL_SetRenderDrawColor(Renderer, color.R, color.G, color.B, color.A);

        for (int i = 0; i < segments; i++)
        {
            double angle1 = i * angleStep;
            double angle2 = (i + 1) * angleStep;

            int x1 = (int)(position.X + Math.Cos(angle1) * radius);
            int y1 = (int)(position.Y + Math.Sin(angle1) * radius);
            int x2 = (int)(position.X + Math.Cos(angle2) * radius);
            int y2 = (int)(position.Y + Math.Sin(angle2) * radius);

            _ = SDL.SDL_RenderDrawLine(Renderer, x1, y1, x2, y2);
        }
    }

    public void DrawGrid(Vector2 size, float cellSize, Color color)
    {
        for (float x = 0; x < size.X; x += cellSize)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), color);
        }

        for (float y = 0; y < size.Y; y += cellSize)
        {
            DrawLine(new Vector2(0, y), new Vector2(size.X, y), color);
        }
    }

    public override void DrawScaledTexture(Texture texture, Vector2 position, Vector2 origin, float rotation, Vector2 scale, bool flipH = false, bool flipV = false)
    {
        _ = SDL.SDL_SetTextureColorMod(texture, 255, 255, 255);

        SDL.SDL_Rect destRect = new()
        {
            x = (int)(position.X - origin.X),
            y = (int)(position.Y - origin.Y),
            w = (int)(texture.Size.X * scale.X),
            h = (int)(texture.Size.Y * scale.Y)
        };

        SDL.SDL_RendererFlip flip = SDL.SDL_RendererFlip.SDL_FLIP_NONE;

        if (flipH && flipV)
        {
            flip = SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL | SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL;
        }
        else if (flipH)
        {
            flip = SDL.SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
        }
        else if (flipV)
        {
            flip = SDL.SDL_RendererFlip.SDL_FLIP_VERTICAL;
        }

        SDL.SDL_Point sdlOrigin = new()
        {
            x = (int)origin.X,
            y = (int)origin.Y
        };

        _ = SDL.SDL_RenderCopyEx(Renderer, texture, IntPtr.Zero, ref destRect, rotation, ref sdlOrigin, flip);
    }

    public override void DrawRectangleOutlineRounded(Vector2 position, Vector2 size, float roundness, int segments, float thickness, Color color)
    {
        throw new NotImplementedException();
    }
}
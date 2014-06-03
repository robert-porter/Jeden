using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jeden.Engine.Object;
using SFML.Graphics;
using SFML.Window;
using Jeden.Game;

namespace Jeden.Engine.Render
{
    class Lifebar : RenderComponent 
    {
        Vector2f Offset; // from top left corner of screen

        Texture BorderTexture;
        Vertex[] BorderVertices;

        Texture HealthTexture;
        Vertex[] HealthVertices;

        Texture ShieldTexture;
        Vertex[] ShieldVertices;

        HealthComponent HealthComponent;

        float HealthLeftX = 74;
        float HealthBottomRightX = 222 + 1;
        float HealthTopRightX = 239;
        float HealthTopY = 20;
        float HealthBottomY = 54 + 1;

        float ShieldTopLeftX = 250;
        float ShieldBottomLeftX = 233;
        float ShieldRightX = 398 + 1;
        float ShieldTopY = 20;
        float ShieldBottomY = 54 + 1;

        public Lifebar(HealthComponent healthComponent, RenderManager renderMgr, GameObject parent)
            : base(renderMgr, parent)
        {

            HealthComponent = healthComponent;

            AlwaysVisible = true;

            BorderVertices = new Vertex[4];
            HealthVertices = new Vertex[4];
            ShieldVertices = new Vertex[4];

            Offset = new Vector2f(30, 20);

            for (int i = 0; i < 4; i++)
            {
                BorderVertices[i] = new Vertex();
                BorderVertices[i].Color = new Color(255, 255, 255, 255);
                HealthVertices[i] = new Vertex();
                HealthVertices[i].Color = new Color(255, 255, 255, 255);
                ShieldVertices[i] = new Vertex();
                ShieldVertices[i].Color = new Color(255, 255, 255, 255);
            }

            BorderTexture = new Texture("assets/lifebar.png");
            ShieldTexture = new Texture("assets/shieldbar_gradient.png");
            HealthTexture = new Texture("assets/healthbar_gradient.png");

            BorderVertices[0].Position = new Vector2f(0, 0);
            BorderVertices[1].Position = new Vector2f(BorderTexture.Size.X, 0);
            BorderVertices[2].Position = new Vector2f(BorderTexture.Size.X, BorderTexture.Size.Y);
            BorderVertices[3].Position = new Vector2f(0, BorderTexture.Size.Y);
            BorderVertices[0].TexCoords = new Vector2f(0, 0);
            BorderVertices[1].TexCoords = new Vector2f(BorderTexture.Size.X, 0);
            BorderVertices[2].TexCoords = new Vector2f(BorderTexture.Size.X, BorderTexture.Size.Y);
            BorderVertices[3].TexCoords = new Vector2f(0, BorderTexture.Size.Y);


            HealthVertices[0].Position = new Vector2f(HealthLeftX, HealthTopY);
            HealthVertices[1].Position = new Vector2f(HealthLeftX, HealthBottomY);
            HealthVertices[2].Position = new Vector2f(HealthBottomRightX, HealthBottomY);
            HealthVertices[3].Position = new Vector2f(HealthTopRightX, HealthTopY);
            HealthVertices[0].TexCoords = new Vector2f(0, 0);
            HealthVertices[1].TexCoords = new Vector2f(0, HealthTexture.Size.Y);
            HealthVertices[2].TexCoords = new Vector2f(0, HealthTexture.Size.Y);
            HealthVertices[3].TexCoords = new Vector2f(0, 0);

            ShieldVertices[0].Position = new Vector2f(ShieldTopLeftX, ShieldTopY);
            ShieldVertices[1].Position = new Vector2f(ShieldBottomLeftX, ShieldBottomY);
            ShieldVertices[2].Position = new Vector2f(ShieldRightX, ShieldBottomY);
            ShieldVertices[3].Position = new Vector2f(ShieldRightX, ShieldTopY);
            ShieldVertices[0].TexCoords = new Vector2f(0, 0);
            ShieldVertices[1].TexCoords = new Vector2f(0, HealthTexture.Size.Y);
            ShieldVertices[2].TexCoords = new Vector2f(0, HealthTexture.Size.Y);
            ShieldVertices[3].TexCoords = new Vector2f(0, 0);


            for (int i = 0; i < 4; i++)
            {
                BorderVertices[i].Position = BorderVertices[i].Position + Offset;
                HealthVertices[i].Position = HealthVertices[i].Position + Offset;
                ShieldVertices[i].Position = ShieldVertices[i].Position + Offset;
            }

        }

        public override void Draw(RenderManager renderMgr, Camera camera)
        {
            float healthFactor = HealthComponent.CurrentHealth / HealthComponent.MaxHealth;
         
            if (healthFactor < 0)
                healthFactor = 0;

            float shieldFactor = HealthComponent.CurrentShield / HealthComponent.MaxShield;
            if (shieldFactor < 0)
                shieldFactor = 0;

            HealthVertices[0].Position = new Vector2f(HealthLeftX, HealthTopY);
            HealthVertices[1].Position = new Vector2f(HealthLeftX, HealthBottomY);
            HealthVertices[2].Position = new Vector2f(HealthLeftX + (HealthBottomRightX - HealthLeftX) * healthFactor, HealthBottomY);
            HealthVertices[3].Position = new Vector2f(HealthLeftX + (HealthTopRightX - HealthLeftX) * healthFactor, HealthTopY);

            ShieldVertices[0].Position = new Vector2f(ShieldTopLeftX, ShieldTopY);
            ShieldVertices[1].Position = new Vector2f(ShieldBottomLeftX, ShieldBottomY);
            ShieldVertices[2].Position = new Vector2f(ShieldBottomLeftX + (ShieldRightX - ShieldBottomLeftX) * shieldFactor, ShieldBottomY);
            ShieldVertices[3].Position = new Vector2f(ShieldTopLeftX + (ShieldRightX - ShieldTopLeftX) * shieldFactor, ShieldTopY);

            for (int i = 0; i < 4; i++)
            {
                HealthVertices[i].Position = HealthVertices[i].Position + Offset;
                ShieldVertices[i].Position = ShieldVertices[i].Position + Offset;
            }


            renderMgr.SetOverlayView();

            RenderStates renderStates = new RenderStates(BorderTexture);
            RenderStates healthGradientRenderStates = new RenderStates(HealthTexture);
            RenderStates shieldGradientRenderStates = new RenderStates(ShieldTexture);

            renderMgr.Target.Draw(BorderVertices, PrimitiveType.Quads, renderStates);
            renderMgr.Target.Draw(HealthVertices, PrimitiveType.Quads, healthGradientRenderStates);
            renderMgr.Target.Draw(ShieldVertices, PrimitiveType.Quads, shieldGradientRenderStates);

            renderMgr.SetCameraView();
        }
    }
}

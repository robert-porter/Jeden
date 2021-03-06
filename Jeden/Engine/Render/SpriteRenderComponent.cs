﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.Graphics;
using Jeden.Engine.Object;
using SFML.Window;

namespace Jeden.Engine.Render
{
    public class SpriteRenderComponent : RenderComponent
    {
        protected Texture Texture;
        protected IntRect SubImageRect;

        public SpriteRenderComponent(RenderManager renderMgr, GameObject parent, Texture texture)
            : base(renderMgr, parent)
        
        {
            Texture = texture;
            WorldPosition = parent.Position;
            SubImageRect.Top = 0;
            SubImageRect.Left = 0;
            SubImageRect.Width = (int)texture.Size.X;
            SubImageRect.Height = (int)texture.Size.Y;
            WorldWidth = SubImageRect.Width;
            WorldHeight = SubImageRect.Height;
            FlipX = false;
            FlipY = false;
            Tint = new Color(255, 255, 255, 255);
        }

        public SpriteRenderComponent(RenderManager renderMgr, GameObject parent, Texture texture, IntRect subImageRect)
            : base(renderMgr, parent)
        {
            Texture = texture;
            WorldPosition = parent.Position;
            SubImageRect = subImageRect;
            WorldWidth = SubImageRect.Width;
            WorldHeight = SubImageRect.Height;
            FlipX = false;
            FlipY = false;
            Tint = new Color(255, 255, 255, 255);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //Update position from parent
            
            WorldPosition = Parent.Position;
        }

        public override void Draw(RenderManager renderMgr, Camera camera)
        {
            renderMgr.DrawSprite(Texture, SubImageRect, WorldPosition, WorldWidth, WorldHeight, FlipX, FlipY, Tint, ZIndex);
        }
    }
}

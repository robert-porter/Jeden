﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML;
using SFML.Window;
using SFML.Audio;
using SFML.Graphics;
using System.Diagnostics;
using LuaInterface;
using System.Threading;

namespace Jeden.Engine
{
    /// <summary>
    /// Represents the Project Jeden game engine.
    /// </summary>
    public class GameEngine : IDisposable
    {
        /// <summary>
        /// The RenderWindow of the GameEngine.
        /// </summary>
        public RenderWindow Window { get; set; }

        /// <summary>
        /// The GameTime of the game.
        /// </summary>
        private GameTime DeltaTime;

        /// <summary>
        /// The stack of usable GameStates.
        /// </summary>
        private Stack<GameState> GameStates;

        /// <summary>
        /// The InputManager of the game.
        /// </summary>
        private InputManager InputMgr;



        double AccumulatedTime;
        double TimeStep = 1.0f / 60.0f;

        /// <summary>
        /// A new instance of GameEngine.
        /// </summary>
        public GameEngine()
        {
            GameStates = new Stack<GameState>();
            InputMgr = new InputManager(this);
            DeltaTime = new GameTime();
            Window = new RenderWindow(new VideoMode(1280, 720), "Jeden");

        }

        /// <summary>
        /// Pushes a new state to the GameState stack.
        /// </summary>
        /// <param name="state">The pushed GameState</param>
        public void PushState(GameState state)
        {
            state.SetInputManager(InputMgr);
            state.SetRenderTarget(Window);
            GameStates.Push(state);
        }

        /// <summary>
        /// Pop the current state off the GameState stack.
        /// </summary>
        public void PopState()
        {
            //Non-empty
            if (GameStates.Count > 0)
            {
                //TODO: allow state to call transition code
                GameStates.Pop();
            }
        }

        /// <summary>
        /// Runs the GameEngine.
        /// </summary>
        public void Run() 
        {

            Stopwatch stopwatch = new Stopwatch();

            Window.Closed += Window_Closed;

            stopwatch.Start();

            double TotalTime = 0;
            GameTime step = new GameTime();

            while (Window.IsOpen())
            {
                Window.DispatchEvents();

                DeltaTime.ElapsedGameTime = stopwatch.Elapsed - DeltaTime.TotalGameTime;
                DeltaTime.TotalGameTime = stopwatch.Elapsed;
                AccumulatedTime += DeltaTime.ElapsedGameTime.TotalSeconds;

                while (AccumulatedTime > TimeStep)
                {
                    TotalTime += TimeStep;

                    step.ElapsedGameTime = new TimeSpan((long)(TimeStep * TimeSpan.TicksPerSecond));
                    step.TotalGameTime = new TimeSpan((long)(TotalTime * TimeSpan.TicksPerSecond));

                    Update(step);
                    Draw();
                    AccumulatedTime -= TimeStep;
                }

                
            }
            stopwatch.Stop();
        }

        /// <summary>
        /// Handles the closing of the window.
        /// </summary>
        /// <param name="sender">The event's sender.</param>
        /// <param name="e">The EventArgs of the even.</param>
        void Window_Closed(object sender, EventArgs e)
        {
            Window.Close();
        }

        /// <summary>
        /// Updates the GameEngine.
        /// </summary>
        /// <param name="gameTime">The time difference to the last frame.</param>
        void Update(GameTime gameTime)
        {
            InputMgr.Update(this);
            GameStates.Peek().Update(gameTime);
        }

        /// <summary>
        /// Draws the active GameState
        /// </summary>
        void Draw()
        {
            Window.Clear(Color.Magenta);
            GameStates.Peek().Render(Window);
            Window.Display();
        }

        /// <summary>
        /// Disposes the GameEngine.
        /// </summary>
        public void Dispose()
        {

        }
    }
}

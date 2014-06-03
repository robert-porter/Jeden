using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jeden.Engine;
using Jeden.Game;
using System.Globalization;
using System.Threading;

namespace Jeden
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US"); // for .Parse functions

            using (GameEngine engine = new GameEngine()) 
            {
                engine.PushState(new JedenGameState());
                engine.Run();
            }
        }
    }
}

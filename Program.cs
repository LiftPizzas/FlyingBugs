using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;


namespace SphereDivision
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			using (Game game = new Game())
			{

				game.Run(30, 30);

			}
			/*
		    GameWindow window = new GameWindow(1920, 1080);
			Game game = new Game(window);

			window.Run(30,30);
			*/
		}
	}
}

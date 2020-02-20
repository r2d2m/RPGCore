﻿using RPGCore.Demo.BoardGame.Models;
using System.Linq;

namespace RPGCore.Demo.BoardGame
{
	public class BuildBuildingAction : GameViewAction
	{
		public string BuildingIdentifier { get; set; }
		public Integer2 Offset { get; set; }
		public Integer2 BuildingPosition { get; set; }
		public BuildingOrientation Orientation { get; set; }

		public override ActionApplyResult Apply(GameView view)
		{
			var buildingTemplate = new BuildingTemplate()
			{
				Recipe = new string[,]
				{
					{ "x", "x", "x" },
					{ "x", "-", "-" },
				}
			};

			var rotatedBuilding = new RotatedBuilding(buildingTemplate, Orientation);

			var ownerPlayer = view.Players.Where(player => player.OwnerId == Client).FirstOrDefault();

			for (int x = 0; x < rotatedBuilding.Width; x++)
			{
				for (int y = 0; y < rotatedBuilding.Height; y++)
				{
					var position = Offset + new Integer2(x, y);

					string recipeTile = rotatedBuilding[x, y];
					var tile = ownerPlayer.Board[position];

					if (recipeTile != null)
					{
						tile.Resource = null;
					}
				}
			}

			var placeTile = ownerPlayer.Board[BuildingPosition];
			placeTile.Building = new Building(buildingTemplate, placeTile);

			return ActionApplyResult.Success;
		}
	}
}

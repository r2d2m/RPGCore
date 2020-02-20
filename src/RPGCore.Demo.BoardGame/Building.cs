﻿using RPGCore.Demo.BoardGame.Models;
using RPGCore.Traits;

namespace RPGCore.Demo.BoardGame
{
	public class Building
	{
		public BuildingTemplate Template;
		public StatInstance LocalReward;

		public GameTile Tile { get; }

		public Building(BuildingTemplate template, GameTile tile)
		{
			Template = template;
			Tile = tile;
		}
	}
}

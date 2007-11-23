using System;
using System.Collections.Generic;
using System.Text;
using Yad.Board.Common;
using System.Collections;
using System.Drawing;
using Yad.Net.Common;
using Yad.Board;

namespace Yad.Engine.Common {
	/// <summary>
	/// Player class holds information on player:
	/// his buildings, units, id, race, etc...
	/// Each player must have unique ID.
	/// </summary>
	public class Player : PlayerInfo {

		int _objectID = 0;

		//used for fast access to an object
        private Dictionary<ObjectID, Building> _buildingsDict = new Dictionary<ObjectID, Building>();
        private Dictionary<ObjectID, Unit> _unitsDict = new Dictionary<ObjectID, Unit>();

		//used for slow access to an object :D
		//but pretty useful for a turn processing
		private List<Building> _buildings = new List<Building>();
		private List<Unit> _units = new List<Unit>();

		#region constructor
		public Player(short playerID, string playerName, short raceID, Color playerColor) {
			this.Id = playerID;
			this.House = raceID;
			this.Name = playerName;
			this.Color = playerColor;
		}

		public Player(PlayerInfo pi) {
			this.Id = pi.Id;
			this.House = pi.House;
			this.Name = pi.Name;
			this.Color = pi.Color;
		}
		#endregion

		#region public methods
		/// <summary>
		/// Used for generating id's for player-created objects (units/buildings)
		/// </summary>
		public int GenerateObjectID() {
			return ++_objectID;
		}

		public void AddUnit(Unit u) {
			_unitsDict.Add(u.ObjectID, u);
			_units.Add(u);
			u.PlaceOnMap();
		}

		public void RemoveUnit(Unit u) {
			_unitsDict.Remove(u.ObjectID);
			_units.Remove(u);
		}

		public void AddBuilding(Building b) {
            try {
                _buildingsDict.Add(b.ObjectID, b);
                _buildings.Add(b);
                b.PlaceOnMap();
            } catch (ArgumentException ae) {
                //RS TODO - sometimes duplicated id happens :| 
            }
		}

		public void RemoveBuilding(Building b) {
			_buildingsDict.Remove(b.ObjectID);
			_buildings.Remove(b);
		}

		public List<Unit> GetAllUnits() {
			return new List<Unit>(this._units);
		}

		public List<Building> GetAllBuildings() {
			return new List<Building>(this._buildings);
		}

		public Unit GetUnit(ObjectID id) {
			return this._unitsDict[id];
		}

		public Building GetBuilding(ObjectID id) {
			return this._buildingsDict[id];
		}
		#endregion
	}
}

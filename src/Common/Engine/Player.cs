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
		bool[,] fogOfWar;
		bool[,] slabs; // they don't work as normal buildings

		//used for fast access to an object
		private Dictionary<ObjectID, Building> _buildingsDict = new Dictionary<ObjectID, Building>();
		private Dictionary<ObjectID, Unit> _unitsDict = new Dictionary<ObjectID, Unit>();

		//used for slow access to an object :D
		//but pretty useful for a turn processing
		private List<Building> _buildings = new List<Building>();
		private List<Unit> _units = new List<Unit>();
		private List<Ammo> _ammos = new List<Ammo>();
		private int _credits;
		private int _power;
        private bool _isDisconnected = false;

        public bool IsDisconnected {
            get { return _isDisconnected; }
            set { _isDisconnected = value; }
        }

		#region constructor
		public Player(short playerID, string playerName, short raceID, Color playerColor, Map map) {
			fogOfWar = new bool[map.Width, map.Height];
			slabs = new bool[map.Width, map.Height];
			for (int x = 0; x < map.Width; x++) {
				for (int y = 0; y < map.Height; y++) {
					fogOfWar[x, y] = true;
					slabs[x, y] = false;
				}
			}
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
		/// 
		//Object idLock = new Object();
		/// <summary>
		/// NIE UZYWAC POZA KLAS� SIMULATION/CLIENTSIMULATION!!!
		/// </summary>
		/// <returns></returns>
		public int GenerateObjectID() {
			return ++_objectID;
		}

		public void AddAmmo(Ammo a) {
			_ammos.Add(a);
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
			_buildingsDict.Add(b.ObjectID, b);
			_buildings.Add(b);
			b.PlaceOnMap();
		}

		public void RemoveBuilding(Building b) {
			_buildingsDict.Remove(b.ObjectID);
			_buildings.Remove(b);
		}

		public void RemoveAmmo(Ammo a) {
			_ammos.Remove(a);
		}

		public List<Unit> GetAllUnits() {
			return new List<Unit>(this._units);
		}

		public List<Building> GetAllBuildings() {
			return new List<Building>(this._buildings);
		}

		public List<Ammo> GetAllAmmos() {
			return new List<Ammo>(this._ammos);
		}

		public Unit GetUnit(ObjectID id) {
			Unit u;
			this._unitsDict.TryGetValue(id, out u);
			return u;
		}

		public Building GetBuilding(ObjectID id) {
			Building b;
			this._buildingsDict.TryGetValue(id, out b);
			return b;
		}

		public int GameObjectsCount {
			get {
				return (_buildings.Count + _units.Count);
			}
		}

		public int Credits {
			get { return _credits; }
			set { _credits = value; }
		}

		public int Power {
			get { return _power; }
			set { _power = value; }
		}

		public void ClearForOfWar() {
			for (int x = 0; x < fogOfWar.GetLength(0); ++x)
				for (int y = 0; y < fogOfWar.GetLength(1); ++y)
					fogOfWar[x, y] = false;
		}

		public bool[,] FogOfWar {
			get { return fogOfWar; }
		}

		public bool IsVisible(Building b) {
			for (int x = b.Position.X; x < b.Position.X + b.Width; ++x)
				for (int y = b.Position.Y; y < b.Position.Y + b.Height; ++y) {
					if (!fogOfWar[x, y])
						return true;
				}
			return false;
		}

		public bool IsVisible(Unit u) {
			return (!fogOfWar[u.Position.X, u.Position.Y]);
		}

		public bool[,] Slabs {
			get { return slabs; }
		}
		#endregion
	}
}

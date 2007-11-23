using System;
using System.Collections.Generic;
using System.Text;
using Yad.Config;
using Yad.Config.Common;

namespace Yad.Board.Common {
	public class UnitTrooper : Unit {
		UnitTrooperData _trooperData;

		public UnitTrooper(ObjectID id, UnitTrooperData ud, Position pos, Map map)
			: base(id, ud.TypeID, BoardObjectClass.UnitTrooper, pos, map) {
			_trooperData = ud;
			this.Speed = ud.Speed;
			this._viewRange = ud.ViewRange;
		}

		public override void Destroy() {
			base.Destroy();
		}

		public override bool Move() {
			return base.Move();
		}

		public override void DoAI() {
			base.DoAI();
		}

		public UnitTrooperData TrooperData {
			get { return _trooperData; }
		}
	}
}

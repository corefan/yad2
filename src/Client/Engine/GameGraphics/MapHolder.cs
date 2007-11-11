﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Client.Board;
using Yad.Board.Common;
using Client.Properties;
using Yad.Engine.GameGraphics.Client;

namespace Yad.Engine.GameGraphics.Client {
	static class MapTextureGenerator {
		static int textureSize = 16;
        static Bitmap[] bmps = null;
		private static ETextures[][] tileFrameMap = new ETextures[][]{
			new ETextures[]{ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, (ETextures)0}, //left, right, upper, lower
			new ETextures[]{ETextures.Mountain, ETextures.Rock, ETextures.Rock, ETextures.Rock, ETextures.Rock, (ETextures)1},
			new ETextures[]{ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, ETextures.Rock, ETextures.Rock, (ETextures)2},
			new ETextures[]{ETextures.Mountain, ETextures.Mountain,ETextures.Rock, ETextures.Rock,ETextures.Rock, (ETextures)3},
			new ETextures[]{ETextures.Mountain, ETextures.Rock,ETextures.Mountain,ETextures.Rock,ETextures.Rock, (ETextures)4},
			new ETextures[]{ETextures.Mountain, ETextures.Rock,ETextures.Rock, ETextures.Mountain, ETextures.Rock, (ETextures)5},
			new ETextures[]{ETextures.Mountain, ETextures.Rock,ETextures.Rock, ETextures.Rock,ETextures.Mountain, (ETextures)6},
			new ETextures[]{ETextures.Mountain, ETextures.Rock,ETextures.Rock, ETextures.Mountain, ETextures.Mountain, (ETextures)7},
			new ETextures[]{ETextures.Mountain, ETextures.Rock,ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, (ETextures)8},
			new ETextures[]{ETextures.Mountain, ETextures.Mountain,ETextures.Rock, ETextures.Mountain, ETextures.Mountain, (ETextures)9},
			new ETextures[]{ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, ETextures.Rock, ETextures.Mountain, (ETextures)10},
			new ETextures[]{ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, ETextures.Mountain, ETextures.Rock, (ETextures)11},
			new ETextures[]{ETextures.Mountain, ETextures.Rock, ETextures.Mountain, ETextures.Rock, ETextures.Mountain, (ETextures)12},
			new ETextures[]{ETextures.Mountain, ETextures.Mountain, ETextures.Rock, ETextures.Mountain, ETextures.Rock, (ETextures)13},
			new ETextures[]{ETextures.Mountain, ETextures.Mountain, ETextures.Rock, ETextures.Rock, ETextures.Mountain, (ETextures)14},
			new ETextures[]{ETextures.Mountain, ETextures.Rock, ETextures.Mountain, ETextures.Mountain, ETextures.Rock, (ETextures)15},

			
			new ETextures[]{ETextures.Rock, ETextures.Sand, ETextures.Sand, ETextures.Sand, ETextures.Sand, (ETextures)1},
			new ETextures[]{ETextures.Rock, ETextures.Rock, ETextures.Rock, ETextures.Sand, ETextures.Sand, (ETextures)2},
			new ETextures[]{ETextures.Rock, ETextures.Rock,ETextures.Sand, ETextures.Sand,ETextures.Sand, (ETextures)3},
			new ETextures[]{ETextures.Rock, ETextures.Sand,ETextures.Rock,ETextures.Sand,ETextures.Sand, (ETextures)4},
			new ETextures[]{ETextures.Rock, ETextures.Sand,ETextures.Sand, ETextures.Rock, ETextures.Sand, (ETextures)5},
			new ETextures[]{ETextures.Rock, ETextures.Sand,ETextures.Sand, ETextures.Sand,ETextures.Rock, (ETextures)6},
			new ETextures[]{ETextures.Rock, ETextures.Sand,ETextures.Sand, ETextures.Rock, ETextures.Rock, (ETextures)7},
			new ETextures[]{ETextures.Rock, ETextures.Sand,ETextures.Rock, ETextures.Rock, ETextures.Rock, (ETextures)8},
			new ETextures[]{ETextures.Rock, ETextures.Rock,ETextures.Sand, ETextures.Rock, ETextures.Rock, (ETextures)9},
			new ETextures[]{ETextures.Rock, ETextures.Rock, ETextures.Rock, ETextures.Sand, ETextures.Rock, (ETextures)10},
			new ETextures[]{ETextures.Rock, ETextures.Rock, ETextures.Rock, ETextures.Rock, ETextures.Sand, (ETextures)11},
			new ETextures[]{ETextures.Rock, ETextures.Sand, ETextures.Rock, ETextures.Sand, ETextures.Rock, (ETextures)12},
			new ETextures[]{ETextures.Rock, ETextures.Rock, ETextures.Sand, ETextures.Rock, ETextures.Sand, (ETextures)13},
			new ETextures[]{ETextures.Rock, ETextures.Rock, ETextures.Sand, ETextures.Sand, ETextures.Rock, (ETextures)14},
			new ETextures[]{ETextures.Rock, ETextures.Sand, ETextures.Rock, ETextures.Rock, ETextures.Sand, (ETextures)15},
			new ETextures[]{ETextures.Rock, ETextures.Whatever, ETextures.Whatever, ETextures.Whatever, ETextures.Whatever, (ETextures)0}, //left, right, upper, lower
			new ETextures[]{ETextures.Sand, ETextures.Whatever,ETextures.Whatever,ETextures.Whatever,ETextures.Whatever, (ETextures)0}
			};

		private static int FindFrame(int x, int y)
		{
			int result;
			if(x < 0 || y < 0 || x>Map.Width || y> Map.Height)
				throw new MapHolderException("Incorrect map position");
			ETextures tileLeft, tileRight, tileUpper, tileLower;
			if (x > 0)
				tileLeft = (ETextures) Map.Tiles[x - 1, y];
			else
				tileLeft = ETextures.Whatever;
			if (x < Map.Width-1)
				tileRight = (ETextures)Map.Tiles[x + 1, y];
			else
				tileRight = ETextures.Whatever;
			if (y > 0)
				tileUpper = (ETextures)Map.Tiles[x, y-1];
			else
				tileUpper = ETextures.Whatever;
			if (y < Map.Height-1)
				tileLower = (ETextures)Map.Tiles[x, y+1];
			else
				tileLower = ETextures.Whatever;
			for(int i=0; i<tileFrameMap.Length; i++){
				if((result=Match(tileFrameMap[i], (ETextures)Map.Tiles[x,y], tileLeft, tileRight, tileLower, tileUpper))>=0)
					return result;
			}
			throw new MapHolderException("Bitmap frame not found");

		}

		private static int Match(ETextures[] tileType, ETextures center ,ETextures tileLeft, ETextures tileRight, ETextures tileLower, ETextures tileUpper)
		{
			if ((int)tileLeft == 255)
				tileLeft = center;
			if ((int)tileRight == 255)
				tileRight = center;
			if ((int)tileUpper == 255)
				tileUpper = center;
			if ((int)tileLower == 255)
				tileLower = center;
			if (tileType[0] == center && (tileType[1] == tileLeft || tileType[1] == ETextures.Whatever) && (tileType[2] == tileRight || tileType[2] == ETextures.Whatever) && (tileType[3] == tileUpper || tileType[3] == ETextures.Whatever) && (tileType[4] == tileLower || tileType[4] == ETextures.Whatever))
				return (int)tileType[5];
			else
				return -1;
		}

        private static void LoadTextures()
        {
			try
			{
				if (bmps != null)
					return;
				bmps = new Bitmap[TextureFiles.Count];
				for (int i = 0; i < TextureFiles.Count; i++)
				{
					bmps[i] = new Bitmap(Path.Combine(Path.GetFullPath(Settings.Default.Terrain), TextureFiles.getFileName((ETextures)i)));
				}
			}
			catch (Exception e)
			{
				throw new MapHolderException(e);
			}
         }

        
        public static Bitmap GenerateBitmap()
        {
			if (!Map.CheckConststency())
				throw new MapHolderException("Map is inconsistent");
			LoadTextures();

			int oldWidth = Map.Width * textureSize, oldHeight = Map.Height * textureSize;
			Bitmap bmp = new Bitmap(oldWidth, oldHeight, PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(bmp);
			for (int y = 0; y < Map.Height; y++) {
				for (int x = 0; x < Map.Width; x++) {
					g.DrawImage(bmps[(int)Map.Tiles[x, y]], new Rectangle(textureSize * x, textureSize * y, textureSize, textureSize), new Rectangle(bmps[(int)Map.Tiles[x, y]].Height * FindFrame(x, y), 0, bmps[(int)Map.Tiles[x, y]].Height-1, bmps[(int)Map.Tiles[x, y]].Height-1), GraphicsUnit.Pixel);
				}
			}
			g.Dispose();

			//scale bitmap to whole bitmap with dimensions = 2^y
			int newWidth = Correct(oldWidth), newHeight = Correct(oldHeight);
			Bitmap res = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
			Graphics g2 = Graphics.FromImage(res);
			g2.DrawImage(bmp, 0, 0, newWidth, newHeight);
			return res;
		}

		private static int Correct(int size) {
			double y = Math.Ceiling(Math.Log(size, 2));
			return (int)Math.Pow(2, y);
		}
	}
}

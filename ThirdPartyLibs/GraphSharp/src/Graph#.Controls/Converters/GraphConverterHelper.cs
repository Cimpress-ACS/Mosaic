using System;
using System.Windows;

namespace GraphSharp.Converters
{
	static class GraphConverterHelper
	{
		// ---------------------------------------------------------
		// New custom algorithm with parallel alignment capabilities 
		// ---------------------------------------------------------
		public static Point CalculateParallelAttachPoint(Point sourcePosition, Point targetPosition,
																		 Size sourceSize, Size targetSize = default(Size),
																		 Point firstPosition = default(Point),
																		 Size p1Size = default(Size))
		{
			double x;
			double y;

			//+----------------------
			//+ Calculate X-Position
			//+----------------------
			if (sourcePosition.X <= targetPosition.X - targetSize.Width / 2.0)
				x = sourcePosition.X + sourceSize.Width / 2.0;
			else if (sourcePosition.X >= targetPosition.X + targetSize.Width / 2.0)
				x = sourcePosition.X - sourceSize.Width / 2.0;
			else
				x = sourcePosition.X;

			if (firstPosition == new Point( Double.NaN, Double.NaN ) || firstPosition == default( Point ))
			{
				//+-------------------------------------------------------------------------------
				//+ Calculate Y-Position for the Start Point (when Source is SMALLER than Target)
				//+-------------------------------------------------------------------------------
				if (targetSize.Height > sourceSize.Height)
				{
					//if (sourcePosition.Y <= targetPosition.Y - targetSize.Height / 2.0 &&
					//    x >= targetPosition.X - targetSize.Width / 4.0 &&
					//    x <= targetPosition.X + targetSize.Width / 4.0)
					//   y = sourcePosition.Y + sourceSize.Height / 2.0;
					//else
					y = sourcePosition.Y;
				}
				else
				{
					//+-------------------------------------------------------------------------------
					//+ Calculate Y-Position for the Start Point (when Source is BIGGER than Target)
					//+-------------------------------------------------------------------------------
					y = targetPosition.Y;

					//+-------------------------------------------------------------------------------
					//+ Re-Calculate Y-Position depending on scope area between source and target
					//+-------------------------------------------------------------------------------
					if (y <= (sourcePosition.Y - sourceSize.Height / 2.0) && sourceSize.Height > targetSize.Height * 1.5)
						y = sourcePosition.Y - sourceSize.Height / 2.0;
					else if (y >= (sourcePosition.Y + sourceSize.Height / 2.0) && sourceSize.Height > targetSize.Height * 1.5)
						y = sourcePosition.Y + sourceSize.Height / 2.0;
					else if (y <= (sourcePosition.Y - sourceSize.Height / 2.0) ||
								y >= (sourcePosition.Y + sourceSize.Height / 2.0) ||
								sourceSize.Height < targetSize.Height * 1.5)
						y = sourcePosition.Y;
				}
			}
			else
			{
				//+-------------------------
				//+ Re-Calculate X-Position
				//+-------------------------
				if (sourcePosition.X <= firstPosition.X - p1Size.Width / 2.0)
					x += 10;
				else if (sourcePosition.X >= firstPosition.X + p1Size.Width / 2.0)
					x -= 10;

				//+-----------------------------------------------------------------------------
				//+ Calculate Y-Position for the End Point (when Source is SMALLER than Target)
				//+-----------------------------------------------------------------------------
				if (sourceSize.Height > p1Size.Height)
				{
					if (firstPosition.Y <= (sourcePosition.Y - sourceSize.Height / 2.0))
					{
						//Debug.WriteLine( "1" );
						y = (sourcePosition.Y - sourceSize.Height / 2.0);
					}
					else if (firstPosition.Y >= (sourcePosition.Y + sourceSize.Height / 2.0))
					{
						//Debug.WriteLine( "2" );
						y = (sourcePosition.Y + sourceSize.Height / 2.0);
					}
					else
					{
						//Debug.WriteLine( "3" );
						y = firstPosition.Y;
					}
				}
				else
				{
					//+--------------------------------------------------------------------------------------
					//+ Re-Calculate Y-Position for the End Point (when Source is SMALLER than Target * 1.5)
					//+--------------------------------------------------------------------------------------
					if (sourceSize.Height * 1.5 > p1Size.Height)
						if (firstPosition.Y < sourcePosition.Y - sourceSize.Height / 2.0)
							y = sourcePosition.Y - sourceSize.Height / 4.0;
						else if (firstPosition.Y > sourcePosition.Y + sourceSize.Height / 2.0)
							y = sourcePosition.Y + sourceSize.Height / 4.0;
						else
							y = sourcePosition.Y;
					else
						y = sourcePosition.Y;
				}
			}

			return new Point( x, y );
		}

		// -------------------------
		// Old GraphSharp algorithm
		// -------------------------
		public static Point CalculateAttachPoint(Point sourcePoint, Size sourceSize, Point targetPoint)
		{
			var sides = new double[4];

			sides[0] = (sourcePoint.X - sourceSize.Width / 2.0 - targetPoint.X) / (sourcePoint.X - targetPoint.X);
			sides[1] = (sourcePoint.Y - sourceSize.Height / 2.0 - targetPoint.Y) / (sourcePoint.Y - targetPoint.Y);
			sides[2] = (sourcePoint.X + sourceSize.Width / 2.0 - targetPoint.X) / (sourcePoint.X - targetPoint.X);
			sides[3] = (sourcePoint.Y + sourceSize.Height / 2.0 - targetPoint.Y) / (sourcePoint.Y - targetPoint.Y);

			double fi = 0;
			for (var i = 0; i < 4; i++)
			{
				if (sides[i] <= 1)
					fi = Math.Max( fi, sides[i] );
			}

			var targetAttachPoint = targetPoint + fi * (sourcePoint - targetPoint);

			return targetAttachPoint;
		}

		// ------------------------------
		// Miscellaneous test algorithms
		// ------------------------------
		// ------------------------------------------------------------------------------------------------------
		public static Point CalculateStartAttachPoint(Point sourcePosition, Point targetPosition, Size sourceSize,
																	 Size targetSize)
		{
			double x;

			var targetUpperBound = targetPosition.Y - targetSize.Height / 2.0;

			if (sourcePosition.Y >= targetUpperBound)
				if (sourcePosition.X <= (targetPosition.X - targetSize.Width / 2.0))
					x = sourcePosition.X + sourceSize.Width / 2.0 + 10;
				else
					x = sourcePosition.X - sourceSize.Width / 2.0 - 10;
			else
				x = sourcePosition.X;

			var y = sourcePosition.Y;

			var targetAttachPoint = new Point( x, y );

			return targetAttachPoint;
		}

		public static Point CalculateEndAttachPoint(Point sourcePosition, Point targetPosition, Size sourceSize,
																  Size targetSize)
		{
			double x;
			double y;

			if (sourcePosition.X <= (targetPosition.X - targetSize.Width / 2.0))
				x = targetPosition.X - targetSize.Width / 2.0 - 15;
			else if (sourcePosition.X >= (targetPosition.X + targetSize.Width / 2.0))
				x = targetPosition.X + targetSize.Width / 2.0 + 15;
			else
				x = sourcePosition.X;

			if (targetSize.Height > sourceSize.Height)
			{
				var targetUpperBound = targetPosition.Y - targetSize.Height / 2.0;
				var targetLowerBound = targetPosition.Y + targetSize.Height / 2.0;

				if (sourcePosition.Y <= targetUpperBound)
					y = targetUpperBound - 10;
				else if (sourcePosition.Y >= targetLowerBound)
					y = targetLowerBound + 10;
				else
					y = sourcePosition.Y;
			}
			else
			{
				y = targetPosition.Y;
			}

			var targetAttachPoint = new Point( x, y );

			return targetAttachPoint;
		}
		// ------------------------------------------------------------------------------------------------------
	}
}
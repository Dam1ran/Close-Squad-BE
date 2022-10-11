using System.Numerics;
using CS.Core.Enums;

namespace CS.Core.Entities;
public class Position {
  public const float ArrivingEdgePercent = 8.0F;
  public const float originX = 50;
  public const float originY = 50;

  public Vector2 Location = new(originX, originY);
  public Vector2 Destination = new(originX, originY);
  public Vector2 NormalizedHeading = new(0, 0);

  public void SetDestination(float x, float y) {
    Destination.X = x;
    Destination.Y = y;
    NormalizedHeading = Vector2.Normalize(Vector2.Subtract(Location, Destination));
  }

  public void SetLocationAndDestination(TravelDirection travelDirection) {
    NormalizedHeading *= 0;
    switch (travelDirection) {
      case TravelDirection.NW : {
        Location.X = Destination.X = 100 - ArrivingEdgePercent;
        Location.Y = Destination.Y = 100 - ArrivingEdgePercent;
        break;
      }
      case TravelDirection.N : {
        Location.X = Destination.X = originX;
        Location.Y = Destination.Y = 100 - ArrivingEdgePercent;
        break;
      }
      case TravelDirection.NE : {
        Location.X = Destination.X = ArrivingEdgePercent;
        Location.Y = Destination.Y = 100 - ArrivingEdgePercent;
        break;
      }
      case TravelDirection.E : {
        Location.X = Destination.X = ArrivingEdgePercent;
        Location.Y = Destination.Y = originY;
        break;
      }
      case TravelDirection.SE : {
        Location.X = Destination.X = ArrivingEdgePercent;
        Location.Y = Destination.Y = ArrivingEdgePercent;
        break;
      }
      case TravelDirection.S : {
        Location.X = Destination.X = originX;
        Location.Y = Destination.Y = ArrivingEdgePercent;
        break;
      }
      case TravelDirection.SW : {
        Location.X = Destination.X = 100 - ArrivingEdgePercent;
        Location.Y = Destination.Y = ArrivingEdgePercent;
        break;
      }
      case TravelDirection.W : {
        Location.X = Destination.X = 100 - ArrivingEdgePercent;
        Location.Y = Destination.Y = originY;
        break;
      }
    }

  }

  public void Move(float speed) => Location -= NormalizedHeading * speed;
  public bool IsAtDestination(float speed) => Vector2.Subtract(Location, Destination).LengthSquared() <= Math.Pow(speed, 2);
  public void Stop() {
    NormalizedHeading *= 0;
    Destination.X = Location.X;
    Destination.Y = Location.Y;
  }

}

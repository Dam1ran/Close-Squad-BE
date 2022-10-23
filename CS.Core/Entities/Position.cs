using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Enums;

namespace CS.Core.Entities;
public class Position {

  public const double ArrivingEdgePercent = 8.0F;
  public const double originX = 50;
  public const double originY = 50;

  public double LocationX { get; private set; } = originX;
  public double LocationY { get; private set; } = originY;

  protected double DestinationX { get; private set; } = originX;
  protected double DestinationY { get; private set; } = originY;

  private double normalizedHeadingX;
  private double normalizedHeadingY;

  [NotMapped]
  public bool IsAtDestination { get; set; }

  public void SetDestination(double x, double y) {
    IsAtDestination = false;

    DestinationX = x;
    DestinationY = y;

    var headingX = LocationX - DestinationX;
    var headingY = LocationY - DestinationY;
    var headingLength = Math.Sqrt(headingX * headingX + headingY * headingY);
    if (headingLength == 0.0F) {
      normalizedHeadingX = 0;
      normalizedHeadingY = 0;
    }

    normalizedHeadingX = headingX / headingLength;
    normalizedHeadingY = headingY / headingLength;

  }

  public void SetLocationAndDestination(TravelDirection travelDirection) {
    normalizedHeadingX = 0;
    normalizedHeadingY = 0;
    switch (travelDirection) {
      case TravelDirection.NW : {
        LocationX = DestinationX = 100 - ArrivingEdgePercent;
        LocationY = DestinationY = 100 - ArrivingEdgePercent;
        break;
      }
      case TravelDirection.N : {
        LocationX = DestinationX = originX;
        LocationY = DestinationY = 100 - ArrivingEdgePercent;
        break;
      }
      case TravelDirection.NE : {
        LocationX = DestinationX = ArrivingEdgePercent;
        LocationY = DestinationY = 100 - ArrivingEdgePercent;
        break;
      }
      case TravelDirection.E : {
        LocationX = DestinationX = ArrivingEdgePercent;
        LocationY = DestinationY = originY;
        break;
      }
      case TravelDirection.SE : {
        LocationX = DestinationX = ArrivingEdgePercent;
        LocationY = DestinationY = ArrivingEdgePercent;
        break;
      }
      case TravelDirection.S : {
        LocationX = DestinationX = originX;
        LocationY = DestinationY = ArrivingEdgePercent;
        break;
      }
      case TravelDirection.SW : {
        LocationX = DestinationX = 100 - ArrivingEdgePercent;
        LocationY = DestinationY = ArrivingEdgePercent;
        break;
      }
      case TravelDirection.W : {
        LocationX = DestinationX = 100 - ArrivingEdgePercent;
        LocationY = DestinationY = originY;
        break;
      }
    }

  }

  public void MoveTick(double distancePerTick) {

    var firstCathetus = (LocationX - DestinationX);
    var secondCathetus = (LocationY - DestinationY);
    var lengthSquared = firstCathetus * firstCathetus + secondCathetus * secondCathetus;
    if (lengthSquared >= distancePerTick * distancePerTick)
    {
      LocationX -= normalizedHeadingX * distancePerTick;
      LocationY -= normalizedHeadingY * distancePerTick;
      IsAtDestination = false;
    }
    else if (normalizedHeadingX != 0 || normalizedHeadingY != 0)
    {
      normalizedHeadingX = 0;
      normalizedHeadingY = 0;
      LocationX = DestinationX;
      LocationY = DestinationY;
      IsAtDestination = true;
    }
  }

  public void Stop() {
    normalizedHeadingX = 0;
    normalizedHeadingY = 0;
    DestinationX = LocationX;
    DestinationY = LocationY;
  }

  public double GetDistance(Position position) =>
    Math.Sqrt(Math.Pow(LocationX - position.LocationX, 2) + Math.Pow(LocationY - position.LocationY, 2));

  public Position GetDifference(Position position) =>
    new() {
      LocationX = position.LocationX - LocationX,
      LocationY = position.LocationY - LocationY,
    };

  public Position SetLength(double newLength) {
    var length = Math.Sqrt(LocationX * LocationX + LocationY * LocationY);
    var normalizedX = LocationX * (newLength / length);
    var normalizedY = LocationY * (newLength / length);

    return
      new() {
        LocationX = normalizedX,
        LocationY = normalizedY,
      };
  }

  public void CopyLocationFrom(Position position) {
    LocationX = position.LocationX;
    LocationY = position.LocationY;
  }


}

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

  public void SetDestination(float x, float y) {
    DestinationX = x;
    DestinationY = y;

    var headingX = LocationX - DestinationX;
    var headingY = LocationY - DestinationY;
    var headingLength = (float)Math.Sqrt(headingX * headingX + headingY * headingY);
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

  public void Move(double distancePerTick) {

    var firstCathetus = (LocationX - DestinationX);
    var secondCathetus = (LocationY - DestinationY);
    var lengthSquared = firstCathetus * firstCathetus + secondCathetus * secondCathetus;
    if (lengthSquared >= distancePerTick * distancePerTick)
    {
      LocationX -= normalizedHeadingX * distancePerTick;
      LocationY -= normalizedHeadingY * distancePerTick;
    }
    else if (normalizedHeadingX != 0 || normalizedHeadingY != 0)
    {
      normalizedHeadingX = 0;
      normalizedHeadingY = 0;
      LocationX = DestinationX;
      LocationY = DestinationY;
    }
  }

  public void Stop() {
    normalizedHeadingX = 0;
    normalizedHeadingY = 0;
    DestinationX = LocationX;
    DestinationY = LocationY;
  }

}

using CS.Application.Options;

namespace CS.Application.Support.Utils;
public static class ArrayDimensions {
  public static Tuple<ushort, ushort> ColIndexRowIndex(this uint quadrantIndex, WorldMapSettings worldMapSettings) {
    var nrOfCols = (uint)worldMapSettings.NrOfCols;
    unchecked {
      ushort colIndex = (ushort)(quadrantIndex % nrOfCols);
      ushort rowIndex = (ushort)(quadrantIndex / nrOfCols);

      return new Tuple<ushort, ushort>(colIndex, rowIndex);
    }
  }
  public static uint GetIndex(ushort colIndex, ushort rowIndex, WorldMapSettings worldMapSettings) =>
    (uint)worldMapSettings.NrOfCols * (uint)rowIndex + (uint)colIndex;

}

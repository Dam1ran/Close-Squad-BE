using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;
using CS.Application.Options;
using CS.Application.Options.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Utils;

namespace CS.Api.Services;
public class CaptchaService : ICaptchaService {
  private readonly CaptchaOptions _captchaOptions;
  private readonly Color _backgroundColor;
  private readonly Color _foregroundColor;
  public CaptchaService(IOptions<CaptchaOptions> captchaOptions) {
    _captchaOptions = Check.NotNull(captchaOptions?.Value, nameof(captchaOptions))!;
    _backgroundColor = ColorTranslator.FromHtml(_captchaOptions.BackgroundHexColor);
    _foregroundColor = ColorTranslator.FromHtml(_captchaOptions.ForegroundHexColor);
  }
  public string GetCode() {

    var captchaCodeLength = RandomNumberGenerator.GetInt32(_captchaOptions.MinLength, _captchaOptions.MaxLength + 1);
    var sb = new StringBuilder();
    for(var index = 0; index < captchaCodeLength; index++) {
      var randomIndex = RandomNumberGenerator.GetInt32(0, _captchaOptions.AllowedCharacters.Length);
      var character = _captchaOptions.AllowedCharacters[randomIndex];

      sb.Append(character);
    }

    return sb.ToString();
  }

  public ICsFile GetImage(string code) {
    Bitmap bitmap = new Bitmap(_captchaOptions.WidthPx, _captchaOptions.HeightPx, PixelFormat.Format32bppArgb);
    Graphics graphics = Graphics.FromImage(bitmap);
    Rectangle rectangle = new Rectangle(0, 0, _captchaOptions.WidthPx, _captchaOptions.HeightPx);

    AddBackground(graphics, rectangle);
    RotateTransform(graphics);
    AddText(graphics, code, rectangle);
    graphics.ResetTransform();
    AddBlobs(graphics);
    AddLines(graphics);

    graphics.Dispose();

    var memoryStream = new MemoryStream();
    bitmap.Save(memoryStream, ImageFormat.Png);
    return new Application.Services.Image("captcha.png", memoryStream);
  }

  private void AddLines(Graphics graphics) {

    var solidBrush = new SolidBrush(_backgroundColor);
    var linePen = new Pen(solidBrush, 3);
    var linesCount = RandomNumberGenerator.GetInt32(3, 6);
    for (int i = 0; i < linesCount; i++) {
      Point startPoint = new Point(RandomNumberGenerator.GetInt32(0, _captchaOptions.WidthPx), RandomNumberGenerator.GetInt32(0, _captchaOptions.HeightPx));
      Point endPoint = new Point(RandomNumberGenerator.GetInt32(0, _captchaOptions.WidthPx), RandomNumberGenerator.GetInt32(0, _captchaOptions.HeightPx));

      if (i % 2 == 0) {
        solidBrush.Color = _backgroundColor;
      } else {
        solidBrush.Color = _foregroundColor;
      }
      linePen = new Pen(solidBrush, 3);

      graphics.DrawLine(linePen, startPoint, endPoint);
    }

    solidBrush.Dispose();
    linePen.Dispose();
  }

  private void AddBackground(Graphics graphics, Rectangle rectangle) {

    var solidBrush = new SolidBrush(_backgroundColor);
    graphics.FillRectangle(solidBrush, rectangle);

    solidBrush.Dispose();
  }

  private void AddBlobs(Graphics graphics) {

    var hatchBrush = new HatchBrush(HatchStyle.Percent20, _backgroundColor, _foregroundColor);
    for (var i = 0; i < (int)((_captchaOptions.WidthPx * _captchaOptions.HeightPx) * 0.0075); i++) {
      var x = RandomNumberGenerator.GetInt32(0, _captchaOptions.WidthPx);
      var y = RandomNumberGenerator.GetInt32(0, _captchaOptions.HeightPx);
      var w = RandomNumberGenerator.GetInt32(1, 11);
      var h = RandomNumberGenerator.GetInt32(1, 11);
      graphics.FillEllipse(hatchBrush, x, y, w, h);
    }

    hatchBrush.Dispose();
  }

  private void RotateTransform(Graphics graphics) {
    var matrix = new Matrix();
    matrix.Reset();

    matrix.RotateAt(RandomNumberGenerator.GetInt32(-12, 13), new PointF(_captchaOptions.WidthPx / 2F, _captchaOptions.HeightPx / 2F));
    graphics.Transform = matrix;

    matrix.Dispose();
  }

  private void AddText(Graphics graphics, string code, Rectangle boundingRectangle) {
    code = new String(' ', (int)Math.Ceiling((_captchaOptions.MaxLength - code.Length) / 2D)) + code;

    GraphicsPath graphicPath = new GraphicsPath();
    graphicPath.AddString(code, FontFamily.GenericMonospace, (int)FontStyle.Bold, (int)(_captchaOptions.HeightPx * 0.75), boundingRectangle, null);
    var hatchBrush = new HatchBrush(HatchStyle.Percent20, _backgroundColor, _foregroundColor);

    graphics.FillPath(hatchBrush, graphicPath);

    graphicPath.Dispose();
    hatchBrush.Dispose();
  }
}
